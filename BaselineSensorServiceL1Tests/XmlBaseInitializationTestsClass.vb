﻿Option Strict On
Option Infer On

Imports System.Runtime.Serialization
Imports System.ServiceModel
Imports System.ServiceModel.Web
Imports System.Threading
Imports System.Threading.Tasks
Imports Nist.Bcl.Wsbd
Imports System.Net

<TestClass()>
Public Class XmlBaseInitializationTestsClass
    Inherits BaseInitializationTestsClass

    Public Overrides Function CreateClient() As TestClient
        Dim client As TestClient = New TestClient(ServiceUri)
        client.ClientType = WebMessageFormat.Xml
        Return client
    End Function
    
End Class




