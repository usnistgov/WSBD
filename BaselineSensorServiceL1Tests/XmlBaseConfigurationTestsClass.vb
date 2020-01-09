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
Public Class XmlBaseConfigurationTestsClass
    Inherits BaseConfigurationTestsClass

    <TestMethod()>
    Sub SetConfigurationExample()
        Dim client = CreateClient()
        Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString
        client.Lock(sessionId)
        Dim configXml As XElement
        Dim configString As String

        configXml = _
        <configuration xmlns:i="http://www.w3.org/2001/XMLSchema-instance" xmlns="urn:oid:2.16.840.1.101.3.9.3.1" xmlns:xs="http://www.w3.org/2001/XMLSchema">
            <item><key>submodality</key><value i:type="xs:string">leftMiddle</value></item></configuration>
        configString = configXml.ToString

        Dim result = client.SetConfiguration(sessionId, configString).WsbdResult
        Assert.AreEqual(Status.Success, result.Status)
    End Sub


    <TestMethod(), TestCategory(SetConfiguration), ExpectedException(GetType(Net.WebException))>
    Sub SetConfigurationRequiresValidXML()
        Dim client = CreateClient()
        Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString
        client.Lock(sessionId)
        Dim result = client.SetConfiguration(sessionId, "this is not xml").WsbdResult
    End Sub


    Public Overrides Function CreateClient() As TestClient
        Dim client As TestClient = New TestClient(ServiceUri)
        client.ClientType = WebMessageFormat.Xml
        Return client
    End Function

End Class
