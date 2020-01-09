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

Imports System.Runtime.Serialization
Imports System.ServiceModel
Imports System.ServiceModel.Web

Imports Nist.Bcl.Wsbd


<TestClass()>
Public Class BaselineSensorServiceRegistrationAndActivityIntegrationTests
    Inherits BaselineSensorServiceTests

    <TestMethod()>
    Sub BaselineSensorServiceCanBeHosted()
        ' The base class creates and opens the host automatically
        Assert.IsTrue(Host.State = CommunicationState.Opened)
    End Sub


    <TestMethod(), TestCategory(Register), TestCategory(Success)> _
    Sub RegistrationCanSucceeed()
        Dim client = CreateClient()
        Dim result = ToWsbdResult(client.Register().GetResponseStream)
        Assert.AreEqual(Status.Success, result.Status)
        Assert.AreNotEqual(Guid.Empty, result.SessionId)
    End Sub

    <TestMethod(), TestCategory(Register), TestCategory(Success)> _
    Sub MultipleRegistrationscanSucceed()
        Dim client1 = CreateClient()
        Dim result1 = ToWsbdResult(client1.Register().GetResponseStream)
        Assert.AreEqual(Status.Success, result1.Status)
        Assert.AreNotEqual(Guid.Empty, result1.SessionId)

        Dim client2 = CreateClient()
        Dim result2 = ToWsbdResult(client2.Register().GetResponseStream)
        Assert.AreEqual(Status.Success, result2.Status, result2.Message)
        Assert.AreNotEqual(Guid.Empty, result2.SessionId)

        Assert.IsFalse(Equals(result1.SessionId, result2.SessionId))

    End Sub

    <TestMethod(), TestCategory(Register), TestCategory(Failure)> _
    Sub FailedRegistration()
        Dim client = CreateClient()

        Dim sessionId = Guid.NewGuid()
        BaselineSensorService.OverrideSessionId = sessionId

        ' The first registration should succeed
        Dim result = ToWsbdResult(client.Register().GetResponseStream)
        Assert.AreEqual(Status.Success, result.Status)

        ' The second registration should fail because of the inability to create a unique sessionId
        result = ToWsbdResult(client.Register().GetResponseStream)
        Assert.AreEqual(Status.Failure, result.Status)
        Assert.IsNull(result.SessionId)

    End Sub

    <TestMethod(), TestCategory(Activity)>
    Sub RecentActivityDoesNotMakeAStaleSession()
        Dim client = CreateClient()
        Dim sessionId As Guid? = ToWsbdResult(client.Register()).SessionId   '******************************
        Assert.IsTrue(BaselineSensorService.IsActive(sessionId.Value))
    End Sub

    <TestMethod(), TestCategory(Activity)>
    Sub StagnantSessionDetectionRefreshRateHasValidGetterAndSetter()
        BaselineSensorService.StagnantSessionDetectionRefreshRate = 20
        Assert.AreEqual(20, BaselineSensorService.StagnantSessionDetectionRefreshRate)
    End Sub

    <TestMethod(), TestCategory(Activity)>
    Sub InactivityMakesASessionStale()
        Dim host = MyBase.Host()
        Dim uri = host.BaseAddresses(0).ToString
        Dim client = CreateClient()

        Dim sessionId As Guid = ToWsbdResult(client.Register().GetResponseStream).SessionId.Value
        Assert.IsTrue(BaselineSensorService.IsActive(sessionId))

        Dim OriginalInactivityTimeout As Long = SensorService.ServiceInfo.InactivityTimeout

        ' Speed up the stagnant session detection refresh rate, and reduce the inactivity
        ' threshold to something small enough that can be run in an automated test.
        SensorService.ServiceInfo.InactivityTimeout = 1
        BaselineSensorService.StagnantSessionDetectionRefreshRate = 2000
        BaselineSensorService.RestartStagnantSessionDetectionTimer()    'Essnetially, does nothing

        ' Wait for enough time to pass that the stagnant session detection timer fires at least
        ' twice
        Threading.Thread.Sleep(CInt(BaselineSensorService.StagnantSessionDetectionRefreshRate * 2.5))

        ' Now, the session should be inactive
        Assert.IsFalse(SensorService.IsActive(sessionId))

        SensorService.ServiceInfo.InactivityTimeout = OriginalInactivityTimeout
    End Sub


    <TestMethod(), TestCategory(Activity)>
    Sub SessionsThatDoNotExistAreNotActive()
        Assert.IsFalse(SensorService.IsActive(Guid.NewGuid))
    End Sub

    <TestMethod(), TestCategory(Unregister), TestCategory(BadValue)>
    Sub UnregisteringAnUnparsableSessionIdGivesAnError()
        Dim client = CreateClient()
        Dim result = client.Unregister("this_is_not_a_guid").WsbdResult
        Assert.AreEqual(Status.BadValue, result.Status)
        Assert.IsTrue(result.BadFields.Contains(Constants.SessionIdParameterName))
    End Sub


    <TestMethod(), TestCategory(Unregister), TestCategory(Success)>
    Sub RegisteredSessionsCanBeUnregisteredOkay()
        Dim client = CreateClient()
        Dim result = ToWsbdResult(client.Register().GetResponseStream)
        Assert.AreEqual(Status.Success, result.Status)
        result = ToWsbdResult(client.Unregister(result.SessionId.ToString).GetResponseStream)
        Assert.AreEqual(Status.Success, result.Status)
    End Sub

    <TestMethod(), TestCategory(Unregister), TestCategory(Failure)>
    Sub UnregistrationCanFail()
        Dim client = CreateClient()
        Dim result = ToWsbdResult(client.Register().GetResponseStream)
        BaselineSensorService.ForceGeneralFailure = True
        result = ToWsbdResult(client.Unregister(result.SessionId.ToString).GetResponseStream)
        Assert.AreEqual(Status.Failure, result.Status)
    End Sub

    <TestMethod(), TestCategory(Unregister), TestCategory(Activity)> _
    Sub UnregisteredSessionsAreNotActive()
        Dim client = CreateClient()
        Dim result = ToWsbdResult(client.Register().GetResponseStream)
        Assert.AreEqual(Status.Success, result.Status)

        Dim id As Guid = result.SessionId.Value
        Assert.IsTrue(SensorService.IsActive(id))
        result = ToWsbdResult(client.Unregister(id.ToString).GetResponseStream)
        Assert.IsFalse(SensorService.IsActive(id))


    End Sub

    <TestMethod(), TestCategory(Unregister), TestCategory(Idempotent)>
    Sub UnregistrationIsIdempotent()
        Dim client = CreateClient()
        Dim result = ToWsbdResult(client.Register().GetResponseStream)
        Dim id As Guid = result.SessionId.Value

        result = ToWsbdResult(client.Unregister(id.ToString).GetResponseStream)
        Assert.AreEqual(Status.Success, result.Status)

        result = ToWsbdResult(client.Unregister(id.ToString).GetResponseStream)
        Assert.AreEqual(Status.Success, result.Status)

    End Sub

    <TestMethod(), TestCategory(Unregister)> _
    Sub ExcessiveRegistrationFails()
        Dim OriginalValue As Boolean = SensorService.ServiceInfo.LeastRecentlyUsedSessionsAutomaticallyDropped
        SensorService.ServiceInfo.LeastRecentlyUsedSessionsAutomaticallyDropped = False

        For i As Integer = 0 To SensorService.ServiceInfo.MaximumConcurrentSessions - 1
            Dim goodClient = CreateClient()
            Dim goodResult = ToWsbdResult(goodClient.Register())
            Assert.AreEqual(Status.Success, goodResult.Status)
        Next

        Dim badClient = CreateClient()
        Dim badResult = ToWsbdResult(badClient.Register())
        Assert.AreEqual(Status.Failure, badResult.Status)

        SensorService.ServiceInfo.LeastRecentlyUsedSessionsAutomaticallyDropped = OriginalValue
    End Sub
End Class