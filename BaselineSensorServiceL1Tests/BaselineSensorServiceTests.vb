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

Imports Nist.Bcl.Wsbd

Imports System.Net
Imports System.Runtime.CompilerServices
Imports System.Runtime.Serialization
Imports System.ServiceModel
Imports System.ServiceModel.Description
Imports System.ServiceModel.Web
Imports System.Text.RegularExpressions
Imports System.Runtime.Serialization.Json
Imports System.IO
Imports System.Text


<TestClass()>
Public MustInherit Class BaselineSensorServiceTests

    Public Const Activity As String = "Activity"
    Public Const BadValue As String = "Bad Value"
    Public Const Cancel As String = "Cancel"
    Public Const CanceledWithSensorFailure As String = "Canceled with Sensor Failure"
    Public Const Capture As String = "Capture"
    Public Const ConfigurationRequired As String = "Configuration Required"
    Public Const Download As String = "Download"
    Public Const Failure As String = "Failure"
    Public Const GetConfiguration As String = "Get Configuration"
    Public Const GetDownloadInfo As String = "Get Download Information"
    Public Const Idempotent As String = "Idempotent"
    Public Const Initialize As String = "Initialize"
    Public Const InitializationRequired As String = "Initialization Required"
    Public Const InvalidId As String = "Invalid Id"
    Public Const LockHeldByAnother As String = "Lock Held by Another"
    Public Const LockNotHeld As String = "Lock Not Held"
    Public Const NoSuchParameter As String = "No Such Parameter"
    Public Const PreparingDownload As String = "Preparing Download"
    Public Const Reentrant As String = "Reentrant"
    Public Const Register As String = "Register"
    Public Const SensorBusy As String = "Sensor Busy"
    Public Const SensorFailure As String = "Sensor Failure"
    Public Const SensorTimeout As String = "Sensor Timeout"
    Public Const ServiceInfo As String = "Service Information"
    Public Const SetConfiguration As String = "Set Configuration"
    Public Const StealLock As String = "Steal Lock"
    Public Const Success As String = "Success"
    Public Const ThriftyDownload As String = "Thrify Download"
    Public Const TryLock As String = "Try Lock"
    Public Const Unlock As String = "Unlock"
    Public Const Unregister As String = "Unregister"
    Public Const Unsupported As String = "Unsupported"

    Private Shared smHost As ServiceHost
    Private Shared smRunOneTestAtATime As New Object
    Private Shared LockTaken As Boolean


    Public Shared Property ServiceUri As String = "http://localhost:7000/BaselineSensorService"

    Public Shared Property DataFormat As String
    Public Shared Property configString As String
    Public Shared Property PopupErrorOpened As Boolean = False

    <TestInitialize()>
    Public Sub TestInitialize()
        LockTaken = False
        Threading.Monitor.Enter(smRunOneTestAtATime, LockTaken)

        BaselineSensorService.Reset()

        smHost = New ServiceHost(GetType(BaselineSensorService), New Uri(ServiceUri))
        Dim endpoint = smHost.AddServiceEndpoint(GetType(ISensorService), New WebHttpBinding, "")
        Dim whb As WebHttpBehavior = New WebHttpBehavior
        whb.DefaultOutgoingResponseFormat = WebMessageFormat.Xml
        whb.AutomaticFormatSelectionEnabled = True
        endpoint.Behaviors.Add(whb)
        endpoint.Behaviors.Add(New TidierInspector)

        Try
            smHost.Open()
        Catch ex As System.ServiceModel.AddressAccessDeniedException

            ' This exception handling code can be safely removed, as it is intended mostly as
            ' a clear way of presenting a solution to a common error that may be come
            ' across when running these unit tests.
            If Not PopupErrorOpened Then
                Dim username = System.Security.Principal.WindowsIdentity.GetCurrent().Name
                Dim command = "netsh http add urlacl url=" & GetUrlString(ex.Message) & " user=""" & username & """"
                Dim message =
                       "The WS-BD tests could not be run because a web service could not be started at the URL """ & ServiceUri & """. " &
                       "This is most likely due to a permissions error. To fix this, run the command" & vbNewLine &
                       vbNewLine &
                       command &
                       vbNewLine & vbNewLine &
                       "from an elevated command prompt (run cmd.exe as 'Administrator'). Would you like to copy this command to your clipboard?"

                Dim result = System.Windows.Forms.MessageBox.Show(message, "WS-BD Tests Error", Windows.Forms.MessageBoxButtons.YesNo, Windows.Forms.MessageBoxIcon.Exclamation, Windows.Forms.MessageBoxDefaultButton.Button1, Windows.Forms.MessageBoxOptions.DefaultDesktopOnly)
                If result = Windows.Forms.DialogResult.Yes Then
                    My.Computer.Clipboard.SetText(command)
                End If
                PopupErrorOpened = True
            End If
        End Try

    End Sub

    <TestCleanup()>
    Public Sub TestCleanup()

        ' After each test, close the connection to the host
        smHost.Close()

        'Static methods must be manually cleared between runs. :(
        BaselineSensorService.Reset()

        If BaselineSensorService.StorageProvider IsNot Nothing Then
            '
            ' If the test touched the storage repository, then clear it. Notice that we take advantage
            ' of our a priori knowledge that the BaselineSensorService uses the FileStorageProvider.
            '
            Dim storageLocation = DirectCast(BaselineSensorService.StorageProvider, FileStorageProvider).Location
            If IO.Directory.Exists(storageLocation) Then
                IO.Directory.Delete(storageLocation, True)
            End If
        End If

        If LockTaken Then
            Threading.Monitor.Exit(smRunOneTestAtATime)
        End If
    End Sub

    Public ReadOnly Property Host As ServiceHost
        Get
            Return smHost
        End Get
    End Property

    Public Overridable Function CreateClient() As TestClient
        Return New TestClient(ServiceUri)
    End Function

    Public Shared Function ToWsbdResult(ByVal responseStream As IO.Stream) As Result
        If String.Compare(DataFormat, TestClient.TestClientRequest.json.ToString, True) = 0 Then
            Dim responseJsonSerializer As DataContractJsonSerializer = New DataContractJsonSerializer(GetType(Result))
            Return DirectCast(responseJsonSerializer.ReadObject(responseStream), Result)
        Else
            Dim responseSerializer As DataContractSerializer = New DataContractSerializer(GetType(Result))
            Return DirectCast(responseSerializer.ReadObject(responseStream), Result)
        End If
    End Function

    Public Shared Function ToWsbdResult(ByVal webResult As WebResponse) As Result
        GetDataFormat(webResult)
        Return ToWsbdResult(webResult.GetResponseStream)
    End Function
    Public Shared Sub GetDataFormat(xResponse As WebResponse)
        Dim contentType As String = xResponse.ContentType
        Dim pattern As String = "^application/xml"
        If Regex.IsMatch(contentType, pattern) Then
            DataFormat = TestClient.TestClientRequest.xml.ToString
        Else
            DataFormat = TestClient.TestClientRequest.json.ToString
        End If
    End Sub

    Public Shared Function SummarizeResults(ByVal results() As Result) As String
        Dim histogram As New Dictionary(Of Status, Integer)
        For Each r In results
            If Not histogram.ContainsKey(r.Status) Then
                histogram.Add(r.Status, 1)
            Else
                histogram(r.Status) += 1
            End If
        Next

        Dim builder As New Text.StringBuilder
        For Each pair In histogram
            builder.Append(pair.Key.ToString & "=" & pair.Value.ToString & " ")
        Next
        Return builder.ToString
    End Function

    Private Shared Function GetUrlString(ByVal text As String) As String
        Dim r As New Regex("\b(https?|ftp|file)://[-A-Z0-9+&@#/%?=~_|$!:,.;]*[A-Z0-9+&@#/%=~_|$]", RegexOptions.IgnoreCase)
        GetUrlString = r.Match(text).Value
    End Function
End Class

Public Module BaselineSensorServiceTestsExtentionMethods

    <Extension()> Public Function WsbdResult(ByVal response As WebResponse) As Result
        Return BaselineSensorServiceTests.ToWsbdResult(response)
    End Function

End Module

