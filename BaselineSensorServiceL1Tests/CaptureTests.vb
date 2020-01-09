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
Imports System.Threading
Imports System.Threading.Tasks

Imports BaselineSensorServiceTestsExtentionMethods

Imports Nist.Bcl.Wsbd


<TestClass()>
Public Class BaselineSensorCaptureTests
    Inherits BaselineSensorServiceTests

    <TestMethod(), TestCategory(Capture), TestCategory(InvalidId)> _
    Sub CapturingForAnUnregisteredIdGivesAnInvalidId()
        Dim client = CreateClient()
        Dim result = client.Capture(Guid.NewGuid.ToString).WsbdResult
        Assert.AreEqual(Status.InvalidId, result.Status)
    End Sub

    <TestMethod(), TestCategory(Capture), TestCategory(BadValue)> _
    Sub CapturingForAnUnparseableIdGivesABadValue()
        Dim client = CreateClient()
        Dim result = client.Capture("this_is_not_a_guid").WsbdResult
        Assert.AreEqual(Status.BadValue, result.Status)
    End Sub

    <TestMethod(), TestCategory(Capture), TestCategory(LockNotHeld)> _
    Sub CapturingRequiresALock()
        Dim client = CreateClient()
        Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString
        Dim result = client.Capture(sessionId).WsbdResult
        Assert.AreEqual(Status.LockNotHeld, result.Status)
    End Sub

    <TestMethod(), TestCategory(Capture), TestCategory(SensorFailure)>
    Sub CapturingCanResultInSensorFailure()
        BaselineSensorService.ForceSensorFailure = True
        Dim client = CreateClient()
        Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString
        client.Lock(sessionId)
        Dim result = client.GetConfiguration(sessionId).WsbdResult
        Assert.AreEqual(Status.SensorFailure, result.Status)
    End Sub

    <TestMethod(), TestCategory(Capture), TestCategory(InitializationRequired)>
    Sub CaptureMayRequireInitialization()
        BaselineSensorService.ForceInitializationRequired = True
        Dim client = CreateClient()
        Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString
        client.Lock(sessionId)
        Dim result = client.Capture(sessionId).WsbdResult
        Assert.AreEqual(Status.InitializationNeeded, result.Status)
    End Sub

    <TestMethod(), TestCategory(Capture), TestCategory(ConfigurationRequired)>
    Sub CaptureMayRequireConfiguration()
        BaselineSensorService.ForceConfigurationRequired = True
        Dim client = CreateClient()
        Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString
        client.Lock(sessionId)
        Dim result = client.Capture(sessionId).WsbdResult
        Assert.AreEqual(Status.ConfigurationNeeded, result.Status)
    End Sub

    <TestMethod(), TestCategory(Capture), TestCategory(Failure)>
    Sub CaptureCanResultInGeneralFailure()
        BaselineSensorService.ForceGeneralFailure = True
        Dim client = CreateClient()
        Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString
        client.Lock(sessionId)
        Dim result = client.Capture(sessionId).WsbdResult
        Assert.AreEqual(Status.Failure, result.Status)
    End Sub

    <TestMethod(), TestCategory(Capture), TestCategory(SensorTimeout)>
    Sub CaptureCanResultInSensorTimeout()
        BaselineSensorService.ForceSensorTimeout = True
        Dim client = CreateClient()
        Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString
        client.Lock(sessionId)
        Dim result = client.Capture(sessionId).WsbdResult
        Assert.AreEqual(Status.SensorTimeout, result.Status)
    End Sub

    <TestMethod(), TestCategory(Capture), TestCategory(LockHeldByAnother)>
    Sub CannotCaptureWhenLockHeldByAnother()
        Dim client1 = CreateClient()
        Dim client2 = CreateClient()
        Dim session1 = client1.Register().WsbdResult.SessionId.Value.ToString
        Dim session2 = client2.Register().WsbdResult.SessionId.Value.ToString
        client1.Lock(session1)
        Dim result = client2.Capture(session2).WsbdResult
        Assert.AreEqual(Status.LockHeldByAnother, result.Status)
    End Sub


    <TestMethod(), TestCategory(Capture), TestCategory(Success)>
    Sub CaptureCanBeSuccessful()

        BaselineSensorService.CaptureTime = 1000 'ms 
        ' Normally, initialization and configuration would be required before capture

        Dim client = CreateClient()
        Dim sessionId = client.Register.WsbdResult.SessionId.Value.ToString
        client.Lock(sessionId)
        Dim result = client.Capture(sessionId).WsbdResult
        Assert.IsNotNull(result.CaptureIds)
        Assert.AreEqual(Status.Success, result.Status)
    End Sub

    <TestMethod(), TestCategory(Capture), TestCategory(Reentrant), TestCategory(Success), TestCategory(SensorBusy)>
    Sub WithSimultaneousCallsOnlyOneCallCapturesSuccessfully()

        Dim busyClients As Integer = 64

        '' ----------
        '' Begin test
        '' ----------

        BaselineSensorService.InitializationTime = 100 'ms. 
        BaselineSensorService.CaptureTime = 10000 'ms. 

        ' Make the timeout so long that the server never Cancels the operation
        SensorService.ServiceInfo.InitializationTimeout = BaselineSensorService.InitializationTime * 1000

        Dim mainClient = CreateClient()
        Dim mainResult As Result


        Dim sessionId = mainClient.Register.WsbdResult.SessionId.Value.ToString
        mainClient.Lock(sessionId)
        Dim runMainClient As New Thread(Sub()
                                            mainResult = mainClient.Capture(sessionId).WsbdResult
                                        End Sub)

        ' Start a sensor operation and wait for the thread to really start
        runMainClient.Start()
        Threading.Thread.Sleep(500)

        For i As Integer = 0 To busyClients - 1
            Dim client = CreateClient()
            Dim result = client.Capture(sessionId).WsbdResult
            Assert.AreEqual(Status.SensorBusy, result.Status)
        Next

        runMainClient.Join()

    End Sub

End Class