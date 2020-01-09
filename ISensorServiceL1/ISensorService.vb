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

Namespace Nist.Bcl.Wsbd

    Public Class Constants

        Public Const WsbdNamespace As String = "urn:oid:2.16.840.1.101.3.9.3.1"

        Public Const SessionIdParameterName As String = "sessionId"
        Public Const CaptureIdParameterName As String = "captureId"
        Public Const MaxSizeParameterName As String = "maxSize"

        Public Const ContentTypeKey As String = "contentType"
        Public Const CaptureDateKey As String = "captureDate"
        Public Const ModalityKey As String = "modality"
        Public Const SubModalityKey As String = "submodality"

    End Class

    <ServiceContract(Namespace:=Constants.WsbdNamespace)> _
    Public Interface ISensorService


#Region "Register"

        <WebInvoke(BodyStyle:=WebMessageBodyStyle.Bare,
            UriTemplate:="/register",
            Method:="POST")>
        Function Register() As Result

        <OperationContract(), WebInvoke(
            BodyStyle:=WebMessageBodyStyle.Bare,
            UriTemplate:="/register/{sessionId}",
            Method:="DELETE")>
        Function Unregister(ByVal sessionId As String) As Result

#End Region

#Region "Lock"

        <OperationContract(), WebInvoke(
            BodyStyle:=WebMessageBodyStyle.Bare,
            UriTemplate:="/lock/{sessionId}",
            Method:="POST")>
        Function Lock(ByVal sessionId As String) As Result

        <OperationContract(), WebInvoke(
            BodyStyle:=WebMessageBodyStyle.Bare,
            UriTemplate:="/lock/{sessionId}",
            Method:="PUT")>
        Function StealLock(ByVal sessionId As String) As Result

        <OperationContract(), WebInvoke(
           BodyStyle:=WebMessageBodyStyle.Bare,
           UriTemplate:="/lock/{sessionId}",
           Method:="DELETE")>
        Function Unlock(ByVal sessionId As String) As Result

#End Region

#Region "Info"

        <OperationContract(), WebInvoke(
            BodyStyle:=WebMessageBodyStyle.Bare,
            UriTemplate:="/info",
            Method:="GET")>
        Function GetServiceInfo() As Result

#End Region

#Region "Initialize"

        <OperationContract(), WebInvoke(
            BodyStyle:=WebMessageBodyStyle.Bare,
            UriTemplate:="/initialize/{sessionId}",
            Method:="POST")>
        Function Initialize(ByVal sessionId As String) As Result

#End Region

#Region "Configure"

        <OperationContract(), WebInvoke(
           BodyStyle:=WebMessageBodyStyle.Bare,
           UriTemplate:="/configure/{sessionId}",
           Method:="GET")>
        Function GetConfiguration(ByVal sessionId As String) As Result


        <OperationContract(), WebInvoke(
            BodyStyle:=WebMessageBodyStyle.Bare,
            UriTemplate:="/configure/{sessionId}",
            Method:="POST")>
        Function SetConfiguration(ByVal sessionId As String, ByVal configuration As Configuration) As Result


#End Region

#Region "Capture"

        <OperationContract(), WebInvoke(
            BodyStyle:=WebMessageBodyStyle.Bare,
            UriTemplate:="/capture/{sessionId}",
            Method:="POST")>
        Function Capture(ByVal sessionId As String) As Result

#End Region

#Region "Download"

        <OperationContract(), WebInvoke(
          BodyStyle:=WebMessageBodyStyle.Bare,
          UriTemplate:="/download/{captureId}",
          Method:="GET")>
        Function Download(ByVal captureId As String) As Result

        <OperationContract(), WebInvoke(
            BodyStyle:=WebMessageBodyStyle.Bare,
            UriTemplate:="/download/{captureId}/info",
            Method:="GET")>
        Function GetDownloadInfo(ByVal captureId As String) As Result

        <OperationContract(), WebInvoke(
          BodyStyle:=WebMessageBodyStyle.Bare,
          UriTemplate:="/download/{captureId}/{maxSize}",
          Method:="GET")>
        Function ThriftyDownload(ByVal captureId As String, ByVal maxSize As String) As Result

#End Region

#Region "Cancel"

        <OperationContract(), WebInvoke(
         BodyStyle:=WebMessageBodyStyle.Bare,
         UriTemplate:="/cancel/{sessionId}",
         Method:="POST")>
        Function Cancel(ByVal sessionId As String) As Result

#End Region

    End Interface


End Namespace
