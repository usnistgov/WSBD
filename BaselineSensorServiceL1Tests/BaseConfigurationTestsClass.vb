Imports System.Runtime.Serialization
Imports System.ServiceModel
Imports System.ServiceModel.Web
Imports System.Threading
Imports System.Threading.Tasks

Imports Nist.Bcl.Wsbd
Imports System.Net
Imports System.Runtime.CompilerServices

<TestClass()>
Public MustInherit Class BaseConfigurationTestsClass
    Inherits BaselineSensorServiceTests

    <TestMethod(), TestCategory(GetConfiguration), TestCategory(InvalidId)> _
    Sub GettingConfigurationForAnUnregisterdIdGivesAnInvalidId()
        Dim client = CreateClient()
        Dim result = client.GetConfiguration(Guid.NewGuid.ToString).WsbdResult
        Assert.AreEqual(Status.InvalidId, result.Status)
    End Sub


    <TestMethod(), TestCategory(GetConfiguration), TestCategory(BadValue)> _
    Sub GettingConfigurationForAnUnparseableIdGivesABadValue()
        Dim client = CreateClient()
        Dim result = client.GetConfiguration("this_is_not_a_guid").WsbdResult
        Assert.AreEqual(Status.BadValue, result.Status)
    End Sub

    <TestMethod(), TestCategory(GetConfiguration), TestCategory(LockNotHeld)> _
    Sub GettingConfigurationRequiresALock()
        Dim client = CreateClient()
        Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString
        Dim result = client.GetConfiguration(sessionId).WsbdResult
        Assert.AreEqual(Status.LockNotHeld, result.Status)
    End Sub

    <TestMethod(), TestCategory(GetConfiguration), TestCategory(SensorFailure)>
    Sub GettingConfigurationCanResultInSensorFailure()
        BaselineSensorService.ForceSensorFailure = True
        Dim client = CreateClient()
        Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString
        client.Lock(sessionId)
        Dim result = client.GetConfiguration(sessionId).WsbdResult
        Assert.AreEqual(Status.SensorFailure, result.Status)
    End Sub

    <TestMethod(), TestCategory(GetConfiguration), TestCategory(InitializationRequired)>
    Sub GettingConfigurationMayRequireInitialization()
        BaselineSensorService.ForceInitializationRequired = True
        Dim client = CreateClient()
        Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString
        client.Lock(sessionId)
        Dim result = client.GetConfiguration(sessionId).WsbdResult
        Assert.AreEqual(Status.InitializationNeeded, result.Status)
    End Sub

    <TestMethod(), TestCategory(GetConfiguration), TestCategory(ConfigurationRequired)>
    Sub GettingConfigurationMayRequireConfiguration()
        BaselineSensorService.ForceConfigurationRequired = True
        Dim client = CreateClient()
        Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString
        client.Lock(sessionId)
        Dim result = client.GetConfiguration(sessionId).WsbdResult
        Assert.AreEqual(Status.ConfigurationNeeded, result.Status)
    End Sub

    <TestMethod(), TestCategory(GetConfiguration), TestCategory(Failure)>
    Sub GettingConfigurationCanResultInGeneralFailure()
        BaselineSensorService.ForceGeneralFailure = True
        Dim client = CreateClient()
        Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString
        client.Lock(sessionId)
        Dim result = client.GetConfiguration(sessionId).WsbdResult
        Assert.AreEqual(Status.Failure, result.Status)
    End Sub

    <TestMethod(), TestCategory(GetConfiguration), TestCategory(SensorTimeout)>
    Sub GettingConfigurationCanResultInSensorTimeout()
        BaselineSensorService.ForceSensorTimeout = True
        Dim client = CreateClient()
        Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString
        client.Lock(sessionId)
        Dim result = client.GetConfiguration(sessionId).WsbdResult
        Assert.AreEqual(Status.SensorTimeout, result.Status)
    End Sub

    <TestMethod(), TestCategory(GetConfiguration), TestCategory(LockHeldByAnother)>
    Sub CannotGetConfigurationWhenLockHeldByAnother()
        Dim client1 = CreateClient()
        Dim client2 = CreateClient()
        Dim session1 = client1.Register().WsbdResult.SessionId.Value.ToString
        Dim session2 = client2.Register().WsbdResult.SessionId.Value.ToString
        client1.Lock(session1)
        Dim result = client2.GetConfiguration(session2).WsbdResult
        Assert.AreEqual(Status.LockHeldByAnother, result.Status)
    End Sub

    <TestMethod(), TestCategory(GetConfiguration), TestCategory(Reentrant), TestCategory(Success), TestCategory(SensorBusy)>
    Sub WithSimultaneousCallsOnlyOneCallGetsConfigurationSuccessfully()

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
                                            mainResult = mainClient.GetConfiguration(sessionId).WsbdResult
                                        End Sub)

        ' Start a sensor operation and wait for the thread to really start
        runMainClient.Start()
        Threading.Thread.Sleep(500)


        For i As Integer = 0 To busyClients - 1
            Dim client = CreateClient()
            Dim result = client.GetConfiguration(sessionId).WsbdResult
            Assert.AreEqual(Status.SensorBusy, result.Status)
        Next

        runMainClient.Join()

    End Sub

    <TestMethod(), TestCategory(GetConfiguration), TestCategory(Cancel)>
    Sub GetConfigurationCanBeCanceledSuccessfully()

        ' Test parameters 
        Dim timeBeforeCancelation As Integer = 1000 ' ms

        '' Begin test

        Dim client = CreateClient()
        Dim infoResult As Result = Nothing
        Dim cancelResult As Result = Nothing


        Dim sessionId = client.Register.WsbdResult.SessionId.Value.ToString
        client.Lock(sessionId)

        Dim init As New Thread(Sub()
                                   infoResult = client.GetConfiguration(sessionId).WsbdResult
                               End Sub)

        Dim cancel As New Thread(Sub()
                                     Thread.Sleep(timeBeforeCancelation)
                                     cancelResult = client.Cancel(sessionId).WsbdResult
                                 End Sub)
        init.Start()
        cancel.Start()

        init.Join()
        cancel.Join()

        Assert.AreEqual(Status.Canceled, infoResult.Status)
        Assert.AreEqual(Status.Success, cancelResult.Status)

    End Sub

    <TestMethod(), TestCategory(GetConfiguration), TestCategory(CanceledWithSensorFailure)>
    Sub CancelingGetConfigurationCanCauseSensorFailure()

        ' Test parameters 
        Dim timeBeforeCancelation As Integer = 1000 ' ms

        '' Begin test

        Dim client = CreateClient()
        Dim infoResult As Result = Nothing
        Dim cancelResult As Result = Nothing

        BaselineSensorService.ForceSensorFailure = True

        Dim sessionId = client.Register.WsbdResult.SessionId.Value.ToString
        client.Lock(sessionId)

        Dim init As New Thread(Sub()
                                   infoResult = client.GetConfiguration(sessionId).WsbdResult
                               End Sub)

        Dim cancel As New Thread(Sub()
                                     Thread.Sleep(timeBeforeCancelation)
                                     cancelResult = client.Cancel(sessionId).WsbdResult
                                 End Sub)
        init.Start()
        cancel.Start()

        init.Join()
        cancel.Join()

        Assert.AreEqual(Status.CanceledWithSensorFailure, infoResult.Status)
        Assert.AreEqual(Status.Success, cancelResult.Status)

    End Sub


    <TestMethod(), TestCategory(GetConfiguration), TestCategory(Success)> _
    Sub GettingConfigurationCanSucceed()

        Dim configuration As New Dictionary
        configuration.Add("string1", "value")
        configuration.Add("integer1", 1)

        BaselineSensorService.Configuration = configuration

        Dim client = CreateClient()
        Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString
        client.Lock(sessionId)


        Dim result = client.GetConfiguration(sessionId).WsbdResult

        Assert.AreEqual(Status.Success, result.Status)
        Assert.IsNotNull(result.Metadata)
        Assert.AreEqual(result.Metadata.Count, 2)
        Assert.AreEqual(result.Metadata("string1"), "value")
        Assert.AreEqual(result.Metadata("integer1"), 1)
    End Sub

#Region " Set Configuration "
    <TestMethod(), TestCategory(SetConfiguration), TestCategory(InvalidId)> _
    Sub SettingConfigurationForAnUnregisterdIdGivesAnInvalidId()
        Dim client = CreateClient()
        Dim result = client.SetConfiguration(Guid.NewGuid.ToString, New Configuration).WsbdResult
        Assert.AreEqual(Status.InvalidId, result.Status)
    End Sub

    <TestMethod(), TestCategory(SetConfiguration), TestCategory(BadValue)> _
    Sub SettingConfigurationForAnUnparseableIdGivesABadValue()
        Dim client = CreateClient()
        Dim result = client.SetConfiguration("this_is_not_a_guid", New Configuration).WsbdResult
        Assert.AreEqual(Status.BadValue, result.Status)
    End Sub

    <TestMethod(), TestCategory(SetConfiguration), TestCategory(LockNotHeld)> _
    Sub SettingConfigurationRequiresALock()
        Dim client = CreateClient()
        Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString
        Dim result = client.SetConfiguration(sessionId, New Configuration).WsbdResult
        Assert.AreEqual(Status.LockNotHeld, result.Status)
    End Sub

    <TestMethod(), TestCategory(SetConfiguration), TestCategory(SensorFailure)>
    Sub SettingConfigurationCanResultInSensorFailure()
        BaselineSensorService.ForceSensorFailure = True
        Dim client = CreateClient()
        Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString
        client.Lock(sessionId)
        Dim result = client.SetConfiguration(sessionId, New Configuration).WsbdResult
        Assert.AreEqual(Status.SensorFailure, result.Status)
    End Sub
    <TestMethod(), TestCategory(SetConfiguration), TestCategory(InitializationRequired)>
    Sub SettingConfigurationMayRequireInitialization()
        BaselineSensorService.ForceInitializationRequired = True
        Dim client = CreateClient()
        Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString
        client.Lock(sessionId)
        Dim result = client.SetConfiguration(sessionId, New Configuration).WsbdResult
        Assert.AreEqual(Status.InitializationNeeded, result.Status)
    End Sub
    <TestMethod(), TestCategory(SetConfiguration), TestCategory(Failure)>
    Sub SettingConfigurationCanResultInGeneralFailure()
        BaselineSensorService.ForceGeneralFailure = True
        Dim client = CreateClient()
        Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString
        client.Lock(sessionId)
        Dim result = client.SetConfiguration(sessionId, New Configuration).WsbdResult
        Assert.AreEqual(Status.Failure, result.Status)
    End Sub
    <TestMethod(), TestCategory(SetConfiguration), TestCategory(SensorTimeout)>
    Sub SettingConfigurationCanResultInSensorTimeout()
        BaselineSensorService.ForceSensorTimeout = True
        Dim client = CreateClient()
        Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString
        client.Lock(sessionId)
        Dim result = client.SetConfiguration(sessionId, New Configuration).WsbdResult
        Assert.AreEqual(Status.SensorTimeout, result.Status)
    End Sub
    <TestMethod(), TestCategory(SetConfiguration), TestCategory(Success)> _
    Sub SettingConfigurationCanSucceed()

        Dim configuration As New Configuration
        configuration.Add("string1", "value")
        configuration.Add("integer1", 1)

        Dim client = CreateClient()
        Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString
        client.Lock(sessionId)

        ' There should be no configuration before we call configure
        Assert.IsNull(BaselineSensorService.Configuration)

        Dim result = client.SetConfiguration(sessionId, configuration).WsbdResult

        ' After configuration, the service should be appropriately configured
        Assert.AreEqual(Status.Success, result.Status)
        Assert.AreEqual(BaselineSensorService.Configuration.Count, 2)
        Assert.AreEqual(BaselineSensorService.Configuration("string1"), "value")
        Assert.AreEqual(BaselineSensorService.Configuration("integer1"), 1)
    End Sub
    <TestMethod(), TestCategory(SetConfiguration), TestCategory(Unsupported)> _
    Sub SettingConfigurationCanYieldUnsupported()

        BaselineSensorService.ConfigurationValidator = Function(config As Dictionary) As Result
                                                           Dim r As New Result(Status.Unsupported)
                                                           r.BadFields = New StringArray
                                                           r.BadFields.Add("string1")
                                                           Return r
                                                       End Function


        Dim configuration As New Configuration
        configuration.Add("string1", "value")
        configuration.Add("integer1", 1)

        Dim client = CreateClient()
        Dim sessionId = client.Register.WsbdResult.SessionId.Value.ToString
        client.Lock(sessionId)
        Dim result = client.SetConfiguration(sessionId, configuration).WsbdResult
        Assert.AreEqual(Status.Unsupported, result.Status)

    End Sub
    <TestMethod(), TestCategory(SetConfiguration), TestCategory(NoSuchParameter)> _
    Sub SettingConfigurationCanYieldNoSuchParameter()

        BaselineSensorService.ConfigurationValidator = Function(config As Dictionary) As Result
                                                           Dim r As New Result(Status.NoSuchParameter)
                                                           r.BadFields = New StringArray
                                                           r.BadFields.Add("string1")
                                                           Return r
                                                       End Function


        Dim configuration As New Configuration
        configuration.Add("string1", "value")
        configuration.Add("integer1", 1)

        Dim client = CreateClient()
        Dim sessionId = client.Register.WsbdResult.SessionId.Value.ToString
        client.Lock(sessionId)
        Dim result = client.SetConfiguration(sessionId, configuration).WsbdResult
        Assert.AreEqual(Status.NoSuchParameter, result.Status)

    End Sub
    <TestMethod(), TestCategory(SetConfiguration), TestCategory(LockHeldByAnother)>
    Sub CannotSetConfigurationWhenLockHeldByAnother()
        Dim client1 = CreateClient()
        Dim client2 = CreateClient()
        Dim session1 = client1.Register().WsbdResult.SessionId.Value.ToString
        Dim session2 = client2.Register().WsbdResult.SessionId.Value.ToString
        client1.Lock(session1)
        Dim result = client2.SetConfiguration(session2, New Configuration).WsbdResult
        Assert.AreEqual(Status.LockHeldByAnother, result.Status)
    End Sub
    <TestMethod(), TestCategory(SetConfiguration), TestCategory(Reentrant), TestCategory(Success), TestCategory(SensorBusy)>
    Sub WithSimultaneousCallsOnlyOneCallSetsConfigurationSuccessfully()


        Dim busyClients As Integer = 128

        '' ----------
        '' Begin test
        '' ----------

        BaselineSensorService.InitializationTime = 10000 'ms. 

        ' Make the timeout so long that the server never cancels the operation
        SensorService.ServiceInfo.InitializationTimeout = BaselineSensorService.InitializationTime * 1000

        Dim mainClient = CreateClient()
        Dim mainResult As Result

        Dim configuration As New Configuration
        configuration.Add("string1", "value")
        configuration.Add("integer1", 1)

        Dim sessionId = mainClient.Register.WsbdResult.SessionId.Value.ToString
        mainClient.Lock(sessionId)
        Dim runMainClient As New Thread(Sub()
                                            mainResult = mainClient.SetConfiguration(sessionId, configuration).WsbdResult
                                        End Sub)

        ' Start a sensor operation and wait for the thread to really start
        runMainClient.Start()
        Threading.Thread.Sleep(500)


        For i As Integer = 0 To busyClients - 1
            Dim client = CreateClient()
            Dim result = client.SetConfiguration(sessionId, configuration).WsbdResult
            Assert.AreEqual(Status.SensorBusy, result.Status)
        Next

        runMainClient.Join()

    End Sub
    <TestMethod(), TestCategory(SetConfiguration), TestCategory(Cancel)>
    Sub SetConfigurationCanBeCanceledSuccessfuly()

        ' Test parameters 
        Dim timeBeforeCancelation As Integer = 1000 ' ms

        '' Begin test

        Dim client = CreateClient()
        Dim infoResult As Result = Nothing
        Dim cancelResult As Result = Nothing


        Dim sessionId = client.Register.WsbdResult.SessionId.Value.ToString
        client.Lock(sessionId)

        Dim init As New Thread(Sub()
                                   infoResult = client.SetConfiguration(sessionId, New Configuration).WsbdResult
                               End Sub)

        Dim cancel As New Thread(Sub()
                                     Thread.Sleep(timeBeforeCancelation)
                                     cancelResult = client.Cancel(sessionId).WsbdResult
                                 End Sub)
        init.Start()
        cancel.Start()

        init.Join()
        cancel.Join()

        Assert.AreEqual(Status.Canceled, infoResult.Status)
        Assert.AreEqual(Status.Success, cancelResult.Status)

    End Sub
    <TestMethod(), TestCategory(SetConfiguration), TestCategory(CanceledWithSensorFailure)>
    Sub CancelingSetConfigurationCanCauseSensorFailure()

        ' Test parameters 
        Dim timeBeforeCancelation As Integer = 1000 ' ms

        '' Begin test

        Dim client = CreateClient()
        Dim infoResult As Result = Nothing
        Dim cancelResult As Result = Nothing

        BaselineSensorService.ForceSensorFailure = True

        Dim sessionId = client.Register.WsbdResult.SessionId.Value.ToString
        client.Lock(sessionId)

        Dim init As New Thread(Sub()
                                   infoResult = client.SetConfiguration(sessionId, New Configuration).WsbdResult
                               End Sub)

        Dim cancel As New Thread(Sub()
                                     Thread.Sleep(timeBeforeCancelation)
                                     cancelResult = client.Cancel(sessionId).WsbdResult
                                 End Sub)
        init.Start()
        cancel.Start()

        init.Join()
        cancel.Join()

        Assert.AreEqual(Status.CanceledWithSensorFailure, infoResult.Status)
        Assert.AreEqual(Status.Success, cancelResult.Status)

    End Sub
#End Region

End Class
