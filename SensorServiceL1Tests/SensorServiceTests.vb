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

Imports Microsoft.VisualStudio.TestTools.UnitTesting

Imports System.ServiceModel
Imports Nist.Bcl.Wsbd
Imports System.ServiceModel.Description
Imports System.Text.RegularExpressions
Imports System.ServiceModel.Web

<TestClass()>
Public MustInherit Class SensorServiceTests(Of T)

    Public Shared Property ServiceUri As String = "http://localhost:7000/TestService"

    Private Shared smHost As ServiceHost
    Private Shared smRunOneTestAtATime As New Object
    Private Shared LockTaken As Boolean


    <TestInitialize()>
    Public Sub TestInitialize()
        LockTaken = False
        Threading.Monitor.Enter(smRunOneTestAtATime, LockTaken)
        smHost = New ServiceHost(GetType(T), New Uri(ServiceUri))
        Dim endpoint = smHost.AddServiceEndpoint(GetType(ISensorService), New WebHttpBinding, "WebServiceHost")
        endpoint.Behaviors.Add(New WebHttpBehavior)
        endpoint.Behaviors.Add(New TidierInspector)
        Try
            smHost.Open()
            

        Catch ex As System.ServiceModel.AddressAccessDeniedException

            ' This exception handling code can be safely removed, as it is intended mostly as
            ' a clear way of presenting a solution to a common error that may be come
            ' across when running these unit tests.

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
        End Try
    End Sub

    Private Shared Function GetUrlString(ByVal text As String) As String
        Dim r As New Regex("\b(https?|ftp|file)://[-A-Z0-9+&@#/%?=~_|$!:,.;]*[A-Z0-9+&@#/%=~_|$]", RegexOptions.IgnoreCase)
        GetUrlString = r.Match(text).Value
    End Function

    <TestCleanup()>
    Public Sub TestCleanup()
        smHost.Close()

        If LockTaken Then
            Threading.Monitor.Exit(smRunOneTestAtATime)
        End If
    End Sub

    Protected MustOverride Sub ResetService()

End Class
