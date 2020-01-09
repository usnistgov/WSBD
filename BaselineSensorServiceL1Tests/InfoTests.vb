' •—————————————————————————————————————————————————————————————————————————————————————————————————————•
'                              National Institute of Standards and Technology
'                                          Biometric Clients Lab
' •—————————————————————————————————————————————————————————————————————————————————————————————————————•
'  File author(s):
'       Ross J. Micheals (ross.micheals@nist.gov)
'       Kevin Mangold (kevin.mangold@nist.gov)
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
Public Class BaselineSensorServiceInfoTests
    Inherits BaselineSensorServiceTests

    '<TestMethod(), TestCategory(ServiceInfo), TestCategory(Success)> _
    'Sub ServiceInfoCanBeQueriedSuccessfully()
    '    Dim VendorParameter As New Parameter()
    '    VendorParameter.Name = "vendor"
    '    VendorParameter.Type = New Xml.XmlQualifiedName("string", "http://www.w3.org/2001/XMLSchema")
    '    VendorParameter.IsReadOnly = True
    '    VendorParameter.SupportsMultiple = False
    '    VendorParameter.DefaultValue = "NIST"
    '    BaselineSensorService.AddToServiceInfoDictionary("vendor", VendorParameter)

    '    Dim client = CreateClient()
    '    Dim result = client.ServiceInfo().WsbdResult

    '    Assert.AreEqual(result.Status, Status.Success)
    '    Assert.AreEqual(CType(result.Metadata("vendor"), Parameter).DefaultValue, "NIST")
    'End Sub

    <TestMethod(), TestCategory(ServiceInfo), TestCategory(Failure)> _
    Sub ServiceInfoCanFail()
        Dim client = CreateClient()
        BaselineSensorService.ForceGeneralFailure = True
        Dim result = client.ServiceInfo().WsbdResult
        Assert.AreEqual(Status.Failure, result.Status)
    End Sub


    '<TestMethod(), TestCategory(DetailedInfo), TestCategory(InvalidId)>
    'Sub GettingDetailedInfoForAnUnregisteredIdGivesAnInvalidId()
    '    Dim client = CreateClient()
    '    Dim result = client.DetailedInfo(Guid.NewGuid.ToString).WsbdResult
    '    Assert.AreEqual(Status.InvalidId, result.Status)
    'End Sub

    '<TestMethod(), TestCategory(DetailedInfo), TestCategory(BadValue)>
    'Sub GettingDetailedInfoForAnUnparsableIdGivesABadValue()
    '    Dim client = CreateClient()
    '    Dim result = client.DetailedInfo("this_is_not_a_guid").WsbdResult
    '    Assert.AreEqual(Status.BadValue, result.Status)
    'End Sub

    '<TestMethod(), TestCategory(DetailedInfo), TestCategory(LockNotHeld)>
    'Sub GettingDetailedInfoRequiresALock()
    '    Dim client = CreateClient()
    '    Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString
    '    Dim result = client.DetailedInfo(sessionId).WsbdResult
    '    Assert.AreEqual(Status.LockNotHeld, result.Status)
    'End Sub

    '<TestMethod(), TestCategory(DetailedInfo), TestCategory(SensorFailure)>
    'Sub GettingDetailedInfoCanResultInSensorFailure()
    '    BaselineSensorService.ForceSensorFailure = True
    '    Dim client = CreateClient()
    '    Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString
    '    client.Lock(sessionId)
    '    Dim result = client.DetailedInfo(sessionId).WsbdResult
    '    Assert.AreEqual(Status.SensorFailure, result.Status)
    'End Sub

    '<TestMethod(), TestCategory(DetailedInfo), TestCategory(Failure)>
    'Sub GettingDetailedInfoCanResultInGeneralFailure()
    '    BaselineSensorService.ForceGeneralFailure = True
    '    Dim client = CreateClient()
    '    Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString
    '    client.Lock(sessionId)
    '    Dim result = client.DetailedInfo(sessionId).WsbdResult
    '    Assert.AreEqual(Status.Failure, result.Status)
    'End Sub

    '<TestMethod(), TestCategory(DetailedInfo), TestCategory(SensorTimeout)>
    'Sub GettingDetailedInfoCanResultInSensorTimeout()
    '    BaselineSensorService.ForceSensorTimeout = True
    '    Dim client = CreateClient()
    '    Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString
    '    client.Lock(sessionId)
    '    Dim result = client.DetailedInfo(sessionId).WsbdResult
    '    Assert.AreEqual(Status.SensorTimeout, result.Status)
    'End Sub


    '<TestMethod(), TestCategory(DetailedInfo), TestCategory(InitializationRequired)>
    'Sub GettingDetailedInfoMayRequireInitialization()
    '    BaselineSensorService.ForceInitializationRequired = True
    '    Dim client = CreateClient()
    '    Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString
    '    client.Lock(sessionId)
    '    Dim result = client.DetailedInfo(sessionId).WsbdResult
    '    Assert.AreEqual(Status.InitializationNeeded, result.Status)
    'End Sub


    '<TestMethod(), TestCategory(DetailedInfo), TestCategory(Success)>
    'Sub GettingDetailedInfoCanBeSuccessful()
    '    BaselineSensorService.AddToDetailedInfoDictionary("firmwareVersion", "1.0")

    '    Dim client = CreateClient()
    '    Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString
    '    client.Lock(sessionId)
    '    Dim result = client.DetailedInfo(sessionId).WsbdResult
    '    Assert.AreEqual(Status.Success, result.Status)
    '    Assert.AreEqual(result.Metadata("firmwareVersion"), "1.0")
    'End Sub

    '<TestMethod(), TestCategory(DetailedInfo), TestCategory(LockHeldByAnother)>
    'Sub CannotGetDetailedInfoWhenLockHeldByAnother()
    '    Dim client1 = CreateClient()
    '    Dim client2 = CreateClient()
    '    Dim session1 = client1.Register().WsbdResult.SessionId.Value.ToString
    '    Dim session2 = client2.Register().WsbdResult.SessionId.Value.ToString
    '    client1.Lock(session1)
    '    Dim result = client2.DetailedInfo(session2).WsbdResult
    '    Assert.AreEqual(Status.LockHeldByAnother, result.Status)
    'End Sub


    '<TestMethod(), TestCategory(DetailedInfo), TestCategory(Reentrant), TestCategory(Success), TestCategory(SensorBusy)>
    'Sub WithSimultaneousCallsOnlyOneCallGetsDetailedInfoSuccessfully()

    '    ' Test parameters
    '    Dim clientCount As Integer = 24


    '    '' Begin test



    '    Dim clients(clientCount - 1) As TestClient
    '    Dim results(clientCount - 1) As Result
    '    For i As Integer = 0 To clients.Length - 1
    '        clients(i) = CreateClient()
    '    Next

    '    ' Get the session id. We'll share it among different client instances to simulate multiple 
    '    ' initialization requests from the same client
    '    '
    '    Dim sessionId = clients(0).Register.WsbdResult.SessionId.Value.ToString
    '    clients(0).Lock(sessionId)

    '    Parallel.For(0, clientCount,
    '                 Sub(i As Integer)
    '                     results(i) = clients(i).DetailedInfo(sessionId).WsbdResult
    '                 End Sub)


    '    Dim successfulCount As Integer = _
    '        (From r In results Select r Where r.Status = Status.Success).Count
    '    Dim busyCount As Integer = _
    '        (From r In results Select r Where r.Status = Status.SensorBusy).Count

    '    Assert.AreEqual(clientCount - 1, busyCount)
    '    Assert.AreEqual(1, successfulCount)
    'End Sub


    '<TestMethod(), TestCategory(DetailedInfo), TestCategory(Cancel)>
    'Sub GettingDetailedInfoCanBeCanceledSuccessfuly()

    '    ' Test parameters 
    '    Dim timeBeforeCancelation As Integer = 1000 ' ms

    '    '' Begin test

    '    Dim client = CreateClient()
    '    Dim infoResult As Result = Nothing
    '    Dim cancelResult As Result = Nothing


    '    Dim sessionId = client.Register.WsbdResult.SessionId.Value.ToString
    '    client.Lock(sessionId)

    '    Dim init As New Thread(Sub()
    '                               infoResult = client.DetailedInfo(sessionId).WsbdResult
    '                           End Sub)

    '    Dim cancel As New Thread(Sub()
    '                                 Thread.Sleep(timeBeforeCancelation)
    '                                 cancelResult = client.Cancel(sessionId).WsbdResult
    '                             End Sub)
    '    init.Start()
    '    cancel.Start()

    '    init.Join()
    '    cancel.Join()

    '    Assert.AreEqual(Status.Canceled, infoResult.Status)
    '    Assert.AreEqual(Status.Success, cancelResult.Status)

    'End Sub

    '<TestMethod(), TestCategory(DetailedInfo), TestCategory(CanceledWithSensorFailure)>
    'Sub CancelingDetailedInfoCanCauseSensorFailure()

    '    ' Test parameters 
    '    Dim timeBeforeCancelation As Integer = 1000 ' ms

    '    '' Begin test

    '    Dim client = CreateClient()
    '    Dim infoResult As Result = Nothing
    '    Dim cancelResult As Result = Nothing

    '    BaselineSensorService.ForceSensorFailure = True

    '    Dim sessionId = client.Register.WsbdResult.SessionId.Value.ToString
    '    client.Lock(sessionId)

    '    Dim init As New Thread(Sub()
    '                               infoResult = client.DetailedInfo(sessionId).WsbdResult
    '                           End Sub)

    '    Dim cancel As New Thread(Sub()
    '                                 Thread.Sleep(timeBeforeCancelation)
    '                                 cancelResult = client.Cancel(sessionId).WsbdResult
    '                             End Sub)
    '    init.Start()
    '    cancel.Start()

    '    init.Join()
    '    cancel.Join()

    '    Assert.AreEqual(Status.CanceledWithSensorFailure, infoResult.Status)
    '    Assert.AreEqual(Status.Success, cancelResult.Status)

    'End Sub

End Class

