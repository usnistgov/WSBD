' •—————————————————————————————————————————————————————————————————————————————————————————————————————•
'                              National Institute of Standards and Technology
'                                          Biometric Clients Lab
' •—————————————————————————————————————————————————————————————————————————————————————————————————————•
'  File author(s):
'       Ross J. Micheals (ross.micheals@nist.gov)
'
' •—————————————————————————————————————————————————————————————————————————————————————————————————————•
' | NOTICE & DISCLAIMER                                                                                 |
' |                                                                                                     |
' | The research software provided on this web site (“software”) is provided by NIST as a public 		|
' | service. You may use, copy and distribute copies of the software in any medium, provided that you 	|
' | keep intact this entire notice. You may improve, modify and create derivative works of the software	|
' | or any portion of the software, and you may copy and distribute such modifications or works.  		|
' | Modified works should carry a notice stating that you changed the software and should note the date	|
' | and nature of any such change.  Please explicitly acknowledge the National Institute of Standards	|
' | and Technology as the source of the software.														|
' | 																									|
' | The software is expressly provided “AS IS.”  NIST MAKES NO WARRANTY OF ANY KIND, EXPRESS, IMPLIED, 	|
' | IN FACT OR ARISING BY OPERATION OF LAW, INCLUDING, WITHOUT LIMITATION, THE IMPLIED WARRANTY OF 		|
' | MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE, NON-INFRINGEMENT AND DATA ACCURACY.  NIST 		|
' | NEITHER REPRESENTS NOR WARRANTS THAT THE OPERATION OF THE SOFTWARE WILL BE UNINTERRUPTED OR 		|
' | ERROR-FREE, OR THAT ANY DEFECTS WILL BE CORRECTED.  NIST DOES NOT WARRANT OR MAKE ANY 				|
' | REPRESENTATIONS REGARDING THE USE OF THE SOFTWARE OR THE RESULTS THEREOF, INCLUDING BUT NOT LIMITED	|
' | TO THE CORRECTNESS, ACCURACY, RELIABILITY, OR USEFULNESS OF THE SOFTWARE.							|
' | 																									|
' | You are solely responsible for determining the appropriateness of using and distributing the 		|
' | software and you assume all risks associated with its use, including but not limited to the risks	|
' | and costs of program errors, compliance with applicable laws, damage to or loss of data, programs	|
' | or equipment, and the unavailability or interruption of operation.  This software is not intended	|
' | to be used in any situation where a failure could cause risk of injury or damage to property.  The	|
' | software was developed by NIST employees.  NIST employee contributions are not subject to copyright	|
' | protection within the United States.  																|
' | 																									|
' | Specific hardware and software products identified in this open source project were used in order   |
' | to perform technology transfer and collaboration. In no case does such identification imply         |
' | recommendation or endorsement by the National Institute of Standards and Technology, nor            |
' | does it imply that the products and equipment identified are necessarily the best available for the |
' | purpose.                                                                                            |
' •—————————————————————————————————————————————————————————————————————————————————————————————————————•

Option Strict On
Option Infer On

Imports System.ServiceModel
Imports System.ServiceModel.Web
Imports System.Threading.Tasks
Imports BaselineSensorServiceTestsExtentionMethods

Imports Nist.Bcl.Wsbd


<TestClass()>
Public Class BaselineSensorServiceLockingTests
    Inherits BaselineSensorServiceTests


    <TestMethod(), TestCategory(TryLock), TestCategory(BadValue)>
    Sub LockingAnUnparsableSessionIdGivesABadValue()

        '
        ' This test verifies that the server returns a 'BadValue' status if a client sends an unparsable 
        ' session id.
        '
        Dim client = CreateClient()
        Dim result = ToWsbdResult(client.Lock("this_is_not_a_guid"))
        Assert.AreEqual(Status.BadValue, result.Status)
        Assert.IsTrue(result.BadFields.Contains(Constants.SessionIdParameterName))
    End Sub

    <TestMethod(), TestCategory(TryLock), TestCategory(InvalidId)>
    Sub LockingAnUnregisteredSessionIdGivesAnInvalidId()
        Dim client = CreateClient()
        Dim sessionId = Guid.NewGuid
        Dim result = ToWsbdResult(client.Lock(sessionId.ToString))
        Assert.AreEqual(Status.InvalidId, result.Status)
        Assert.IsTrue(result.Message.Contains(sessionId.ToString))
    End Sub

    <TestMethod(), TestCategory(TryLock), TestCategory(Success)>
    Sub LockingCanBePeformedSuccessfully()
        Dim client = CreateClient()
        Dim sessionId = ToWsbdResult(client.Register()).SessionId
        Dim result = ToWsbdResult(client.Lock(sessionId.ToString))
        Assert.AreEqual(Status.Success, result.Status)
    End Sub

    <TestMethod(), TestCategory(TryLock), TestCategory(Success), TestCategory(Idempotent)>
    Sub SuccessfulLockingIsIdempotent()
        Dim client = CreateClient()
        Dim sessionId = ToWsbdResult(client.Register()).SessionId
        Dim result = ToWsbdResult(client.Lock(sessionId.ToString))
        Assert.AreEqual(Status.Success, result.Status)

        result = ToWsbdResult(client.Lock(sessionId.ToString))
        Assert.AreEqual(Status.Success, result.Status)
    End Sub

    <TestMethod(),
    TestCategory(TryLock), TestCategory(Success), TestCategory(Reentrant), TestCategory(LockHeldByAnother)>
    Sub WithSimultaneousClientsOnlyOneClientCanObtainTheLock()
        Dim clientCount As Integer = CInt(SensorService.ServiceInfo.MaximumConcurrentSessions / 2)

        Dim clients(clientCount - 1) As TestClient
        Dim results(clientCount - 1) As Result
        For i As Integer = 0 To clients.Length - 1
            clients(i) = CreateClient()
        Next

        Parallel.For(0, clientCount,
                     Sub(i As Integer)
                         Dim sessionId = clients(i).Register().WsbdResult.SessionId
                         results(i) = ToWsbdResult(clients(i).Lock(sessionId.ToString).GetResponseStream)
                     End Sub)


        Dim successfulCount As Integer = _
            (From r In results Select r Where r.Status = Status.Success).Count
        Dim lockHeldByAnotherCount As Integer = _
            (From r In results Select r Where r.Status = Status.LockHeldByAnother).Count

        Assert.AreEqual(1, successfulCount)
        Assert.AreEqual(clientCount - 1, lockHeldByAnotherCount)
    End Sub

    <TestMethod(), TestCategory(Unlock), TestCategory(Success)>
    Sub UnlockingCanBePeformedSuccessfully()
        Dim client = CreateClient()
        Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString
        client.Lock(sessionId)
        Dim result = client.Unlock(sessionId).WsbdResult
        Assert.AreEqual(Status.Success, result.Status)
    End Sub

    <TestMethod(), TestCategory(Unlock), TestCategory(Success), TestCategory(Idempotent)>
    Sub SuccessfulUnlockingIsIdempotent()
        Dim client = CreateClient()
        Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString
        Dim result = client.Unlock(sessionId).WsbdResult
        Assert.AreEqual(Status.Success, result.Status)

        client.Lock(sessionId)
        result = client.Unlock(sessionId).WsbdResult
        Assert.AreEqual(Status.Success, result.Status)

        result = client.Unlock(sessionId).WsbdResult
        Assert.AreEqual(Status.Success, result.Status)
    End Sub

    <TestMethod(), TestCategory(Unlock), TestCategory(BadValue)>
    Sub UnlockingAnUnparsableSessionIdGivesABadValue()
        Dim client = CreateClient()
        Dim result = ToWsbdResult(client.Unlock("this_is_not_a_guid"))
        Assert.AreEqual(Status.BadValue, result.Status)
        Assert.IsTrue(result.BadFields.Contains(Constants.SessionIdParameterName))
    End Sub


    <TestMethod(), TestCategory(Unlock), TestCategory(InvalidId)>
    Sub UnlockingAnUnregisteredSessionIdGivesAnInvalidId()
        Dim client = CreateClient()
        Dim sessionId = Guid.NewGuid
        Dim result = ToWsbdResult(client.Unlock(sessionId.ToString))
        Assert.AreEqual(Status.InvalidId, result.Status)
        Assert.IsTrue(result.Message.Contains(sessionId.ToString))
    End Sub

    <TestMethod(), TestCategory(Unlock), TestCategory(LockHeldByAnother)>
    Sub CannotUnlockWhenLockHeldByAnother()
        Dim client1 = CreateClient()
        Dim client2 = CreateClient()
        Dim session1 = client1.Register().WsbdResult.SessionId.Value.ToString
        Dim session2 = client2.Register().WsbdResult.SessionId.Value.ToString
        client1.Lock(session1)
        Dim result = client2.Unlock(session2).WsbdResult
        Assert.AreEqual(Status.LockHeldByAnother, result.Status)
    End Sub




End Class
