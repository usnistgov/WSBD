Option Strict On
Option Infer On

Imports System.Runtime.Serialization
Imports System.ServiceModel
Imports System.ServiceModel.Web
Imports System.Threading
Imports System.Threading.Tasks
Imports Nist.Bcl.Wsbd
Imports System.Net


<TestClass()>
Public Class JsonBaseConfigurationTestsClass
    Inherits BaseConfigurationTestsClass

    <TestMethod()>
    Sub SetConfigurationExample()
        Dim client = CreateClient()
        Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString
        client.Lock(sessionId)
        Dim configString As String = "[{""Key"":""submodality"",""Value"":""leftMiddle""}]"
        Dim result = client.SetConfiguration(sessionId, configString).WsbdResult
        Assert.AreEqual(Status.Success, result.Status)
    End Sub


    Public Overrides Function CreateClient() As TestClient
        Dim client As TestClient = New TestClient(ServiceUri)
        client.ClientType = TestClient.TestClientRequest.json
        Return client
    End Function
End Class


