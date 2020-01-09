
Option Strict On
Option Infer On

Imports System.Runtime.Serialization
Imports System.ServiceModel
Imports System.ServiceModel.Web
Imports System.Threading
Imports System.Threading.Tasks

Imports Nist.Bcl.Wsbd


<TestClass()>
Public MustInherit Class BaseInitializationTestsClass
    Inherits BaselineSensorServiceTests

    <TestMethod(), TestCategory(Initialize), TestCategory(Success)>
    Sub InitializationCanBeSuccessfull()
        Dim client = CreateClient()
        Dim session = client.Register.WsbdResult.SessionId.Value.ToString
        client.Lock(session)
        Dim result = client.Initialize(session).WsbdResult
        Assert.AreEqual(Status.Success, result.Status)
    End Sub

    <TestMethod(), TestCategory(Initialize), TestCategory(InvalidId)>
    Sub InitializingAnUnregisteredIdGivesAnInvalidId()
        Dim client = CreateClient()
        Dim result = client.Initialize(Guid.NewGuid.ToString).WsbdResult
        Assert.AreEqual(Status.InvalidId, result.Status)
    End Sub

    <TestMethod(), TestCategory(Initialize), TestCategory(BadValue)>
    Sub InitializingAnUnparseableSessionIdGivesABadValue()
        Dim client = CreateClient()
        client.Register()
        Dim result = client.Initialize("this_is_not_a_guid").WsbdResult
        Assert.AreEqual(Status.BadValue, result.Status)
        Assert.IsTrue(result.BadFields.Contains(Constants.SessionIdParameterName))
    End Sub

    <TestMethod(), TestCategory(Initialize), TestCategory(LockNotHeld)>
    Sub InitializationRequiresALock()
        Dim client = CreateClient()
        Dim session = client.Register.WsbdResult.SessionId.Value.ToString
        Dim result = client.Initialize(session).WsbdResult
        Assert.AreEqual(Status.LockNotHeld, result.Status)
    End Sub

    <TestMethod(), TestCategory(Initialize), TestCategory(SensorTimeout)>
    Sub InitializationCanResultInSensorTimeout()
        Dim client = CreateClient()
        Dim session = client.Register.WsbdResult.SessionId.Value.ToString
        client.Lock(session)
        BaselineSensorService.ForceSensorTimeout = True
        Dim result = client.Initialize(session).WsbdResult
        Assert.AreEqual(Status.SensorTimeout, result.Status)
    End Sub

    <TestMethod(), TestCategory(Initialize), TestCategory(Reentrant),
    TestCategory(Success), TestCategory(SensorBusy)>
    Sub WithSimultaneousCallsOnlyOneCallInitializesSuccessfully()

        Dim busyClients As Integer = 128

        '' ----------
        '' Begin test
        '' ----------

        BaselineSensorService.InitializationTime = 10000 'ms. 

        ' Make the timeout so long that the server never Cancels the operation
        SensorService.ServiceInfo.InitializationTimeout = BaselineSensorService.InitializationTime * 1000

        Dim mainClient = CreateClient()
        Dim mainResult As Result


        Dim sessionId = mainClient.Register.WsbdResult.SessionId.Value.ToString
        mainClient.Lock(sessionId)
        Dim runMainClient As New Thread(Sub()
                                            mainResult = mainClient.Initialize(sessionId).WsbdResult
                                        End Sub)

        ' Start a sensor operation and wait for the thread to really start
        runMainClient.Start()
        Threading.Thread.Sleep(500)


        For i As Integer = 0 To busyClients - 1
            Dim client = CreateClient()
            Dim result = client.Initialize(sessionId).WsbdResult
            Assert.AreEqual(Status.SensorBusy, result.Status)
        Next

        runMainClient.Join()

    End Sub

    <TestMethod(), TestCategory(Initialize), TestCategory(Cancel)>
    Sub InitializationCanBeCanceledSuccessfuly()
        Dim timeBeforeCancelation As Integer = 1000

        Dim client = CreateClient()
        Dim initResult As Result = Nothing
        Dim cancelResult As Result = Nothing

        Dim sessionId = client.Register.WsbdResult.SessionId.Value.ToString
        client.Lock(sessionId)

        Dim init As New Thread(Sub()
                                   initResult = client.Initialize(sessionId).WsbdResult
                               End Sub)

        Dim cancel As New Thread(Sub()
                                     Thread.Sleep(timeBeforeCancelation)
                                     cancelResult = client.Cancel(sessionId).WsbdResult

                                 End Sub)
        init.Start()
        cancel.Start()

        init.Join()
        cancel.Join()

        Assert.AreEqual(Status.Canceled, initResult.Status)
        Assert.AreEqual(Status.Success, cancelResult.Status)

    End Sub

    <TestMethod(), TestCategory(Initialize), TestCategory(Cancel)>
    Sub InitializationCanBeCanceledIdempotently()

        Dim jobCount As Integer = 100

        BaselineSensorService.InitializationTime = 2 * 1000
        Dim timeBeforeCancelation As Integer = 25

        For i As Integer = 0 To jobCount - 1

            BaselineSensorService.Reset()

            ' Performing this test a variety of times should produce a unique blend of thread interleaving

            Dim initClient = CreateClient()
            Dim cancelClient1 = CreateClient()
            Dim cancelClient2 = CreateClient()
            Dim initResult As Result = Nothing

            Dim cancelResult1 As Result = Nothing
            Dim cancelResult2 As Result = Nothing

            Dim sessionId = initClient.Register.WsbdResult.SessionId.Value.ToString
            initClient.Lock(sessionId)

            Dim init As New Thread(Sub()
                                       initResult = initClient.Initialize(sessionId).WsbdResult
                                   End Sub)

            Dim cancel1 As New Thread(Sub()
                                          Thread.Sleep(timeBeforeCancelation)
                                          cancelResult1 = cancelClient1.Cancel(sessionId).WsbdResult
                                      End Sub)

            Dim cancel2 As New Thread(Sub()
                                          Thread.Sleep(timeBeforeCancelation)
                                          cancelResult2 = cancelClient2.Cancel(sessionId).WsbdResult
                                      End Sub)
            init.Start()
            cancel1.Start()
            cancel2.Start()

            init.Join()
            cancel1.Join()
            cancel2.Join()

            initClient.Unlock(sessionId)

            Assert.AreEqual(Status.Canceled, initResult.Status)
            Assert.AreEqual(Status.Success, cancelResult1.Status)
            Assert.AreEqual(Status.Success, cancelResult2.Status)
        Next

    End Sub

    <TestMethod(), TestCategory(Initialize), TestCategory(Cancel), TestCategory(CanceledWithSensorFailure)>
    Sub CancelingInitializationCanCauseSensorFailure()

        Dim timeBeforeCancelation As Integer = 1000

        Dim client = CreateClient()
        Dim initResult As Result = Nothing
        Dim cancelResult As Result = Nothing


        ' For testing, we simulate a sensor failure by setting this 'secret' property, 
        ' and set the initialization time for much longer than we will wait to cancel it 
        ' (otherwise, the initializaiton would complete and there would be nothing to 
        ' cancel.'
        '
        BaselineSensorService.ForceSensorFailure = True
        BaselineSensorService.InitializationTime = 10 * timeBeforeCancelation

        Dim sessionId = client.Register.WsbdResult.SessionId.Value.ToString
        client.Lock(sessionId)

        Dim init As New Thread(Sub()
                                   initResult = client.Initialize(sessionId).WsbdResult
                               End Sub)

        Dim cancel As New Thread(Sub()
                                     Thread.Sleep(timeBeforeCancelation)
                                     cancelResult = client.Cancel(sessionId).WsbdResult
                                 End Sub)
        init.Start()
        cancel.Start()

        init.Join()
        cancel.Join()

        Assert.AreEqual(Status.CanceledWithSensorFailure, initResult.Status)
        Assert.AreEqual(Status.Success, cancelResult.Status)

    End Sub

    <TestMethod(), TestCategory(Initialize), TestCategory(LockHeldByAnother)>
    Sub CannotInitializeWhenLockHeldByAnother()
        Dim client1 = CreateClient()
        Dim client2 = CreateClient()
        Dim session1 = client1.Register().WsbdResult.SessionId.Value.ToString
        Dim session2 = client2.Register().WsbdResult.SessionId.Value.ToString
        client1.Lock(session1)
        Dim result = client2.Initialize(session2).WsbdResult
        Assert.AreEqual(Status.LockHeldByAnother, result.Status)
    End Sub


    <TestMethod(), TestCategory(Initialize), TestCategory(Cancel)>
    Sub ServersCanCancelInitialization()
        BaselineSensorService.InitializationTime = 1000 'ms
        SensorService.ServiceInfo.InitializationTimeout = 50 'ms

        Dim client = CreateClient()
        Dim session = client.Register().WsbdResult.SessionId.Value.ToString
        client.Lock(session)
        Dim result = client.Initialize(session).WsbdResult
        Assert.AreEqual(Status.Canceled, result.Status)

    End Sub


End Class



