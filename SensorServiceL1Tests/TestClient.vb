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

Imports System.IO
Imports System.Net
Imports System.Runtime.Serialization
Imports System.Xml
Imports System.Runtime.Serialization.Json
Imports System.ServiceModel.Web

Namespace Nist.Bcl.Wsbd

    Public Class TestClient
        Private mServiceUri As Uri
        Protected mClientType As [Enum] = WebMessageFormat.Xml  'set to xml or json. 

        Public Sub New(ByVal uri As String)
            mServiceUri = New Uri(uri)
        End Sub
      

        Public Property ClientType As [Enum]
            Get
                Return mClientType
            End Get
            Set(value As [Enum])
                mClientType = value
            End Set

        End Property

        Public Enum TestClientRequest As Integer
            xml = 0
            json = 1
        End Enum

        Protected Overridable Function GetContentTypeAsString() As String
            If ClientType.Equals(WebMessageFormat.Xml) Then
                Return "application/xml"
            Else
                Return "application/json"
            End If

        End Function
      
        Public Function ToWsbdResult(ByVal webResult As WebResponse) As Result
            Dim wResult As Result
            If ClientType.Equals(WebMessageFormat.Json) Then
                wResult = DirectCast(New DataContractJsonSerializer(GetType(Result)).ReadObject(webResult.GetResponseStream()), Result)
            Else
                wResult = DirectCast(New DataContractSerializer(GetType(Result)).ReadObject(webResult.GetResponseStream()), Result)
            End If
            Return wResult
        End Function

        Public ReadOnly Property ServiceUri As Uri
            Get
                Return mServiceUri
            End Get
        End Property

        Public ReadOnly Property ResourceUri(ByVal resource As String) As Uri
            Get
                Return New Uri(String.Format("{0}/{1}", mServiceUri.ToString,
                                             resource.TrimStart({"/"c})))
            End Get
        End Property


        Public Function Register() As WebResponse
            Dim request = HttpWebRequest.Create(ResourceUri("register"))
            request.Method = "POST"
            request.ContentType = GetContentTypeAsString()
            request.ContentLength = 0
            Return request.GetResponse
        End Function

        Public Function Unregister(ByVal sessionId As String) As WebResponse
            Dim resource As String = String.Format("register/{0}", sessionId)
            Dim request = HttpWebRequest.Create(ResourceUri(resource))
            request.Method = "DELETE"
            request.ContentType = GetContentTypeAsString()
            request.ContentLength = 0
            Return request.GetResponse()
        End Function

        Public Function Lock(ByVal sessionId As String) As WebResponse
            Dim resource As String = String.Format("lock/{0}", sessionId)
            Dim request = HttpWebRequest.Create(ResourceUri(resource))
            request.Method = "POST"
            request.ContentType = GetContentTypeAsString()
            request.ContentLength = 0
            Return request.GetResponse()
        End Function

        Public Function Unlock(ByVal sessionId As String) As WebResponse
            Dim resource As String = String.Format("lock/{0}", sessionId)
            Dim request = HttpWebRequest.Create(ResourceUri(resource))
            request.Method = "DELETE"
            request.ContentType = GetContentTypeAsString()
            request.ContentLength = 0
            Return request.GetResponse()
        End Function

        Public Function StealLock(ByVal sessionId As String) As WebResponse
            Dim resource As String = String.Format("lock/{0}", sessionId)
            Dim request = HttpWebRequest.Create(ResourceUri(resource))
            request.Method = "PUT"
            request.ContentType = GetContentTypeAsString()
            request.ContentLength = 0
            Return request.GetResponse()
        End Function

        Public Function ServiceInfo() As WebResponse
            Dim request = HttpWebRequest.Create(ResourceUri("info"))
            request.Method = "GET"
            request.ContentType = GetContentTypeAsString()
            request.ContentLength = 0
            Return request.GetResponse()
        End Function

        Public Function Initialize(ByVal sessionId As String) As WebResponse
            Dim resource As String = String.Format("initialize/{0}", sessionId)
            Dim request = HttpWebRequest.Create(ResourceUri(resource))
            request.Method = "POST"
            request.ContentType = GetContentTypeAsString()
            request.ContentLength = 0
            Return request.GetResponse()
        End Function

        Public Function Cancel(ByVal sessionId As String) As WebResponse
            Dim resource As String = String.Format("cancel/{0}", sessionId)
            Dim request = HttpWebRequest.Create(ResourceUri(resource))
            request.Method = "POST"
            request.ContentType = GetContentTypeAsString()
            request.ContentLength = 0
            Return request.GetResponse()
        End Function

        Public Function GetConfiguration(ByVal sessionId As String) As WebResponse
            Dim resource As String = String.Format("configure/{0}", sessionId)
            Dim request = HttpWebRequest.Create(ResourceUri(resource))
            request.Method = "GET"
            request.ContentType = GetContentTypeAsString()
            request.ContentLength = 0
            Return request.GetResponse()
        End Function

        Public Function SetConfiguration(ByVal sessionId As String, ByVal configuration As Configuration) As WebResponse
            If configuration Is Nothing Then Throw New ArgumentNullException("configuration")

            Dim resource As String = String.Format("configure/{0}", sessionId)
            Dim request = HttpWebRequest.Create(ResourceUri(resource))
            request.Method = "POST"
            request.ContentType = GetContentTypeAsString()


            Dim payload As String
            If ClientType.Equals(TestClientRequest.json) Then
                Dim serializer As New DataContractJsonSerializer(GetType(Configuration))
                Dim jsonStream As New MemoryStream   '(for json output)
                serializer.WriteObject(jsonStream, configuration)
                jsonStream.Position = 0
                Dim sr As New StreamReader(jsonStream)
                payload = sr.ReadToEnd()
            Else
                Dim serializer As New DataContractSerializer(GetType(Configuration))
                Dim stringWriter As New StringWriter
                Using xmlWriter = New XmlTextWriter(stringWriter)
                    serializer.WriteObject(xmlWriter, configuration)
                    payload = stringWriter.ToString
                End Using
            End If
           
            Using requestStream As New StreamWriter(request.GetRequestStream)
                requestStream.Write(payload)
                requestStream.Flush()
            End Using

            Return request.GetResponse()
        End Function

        Public Function SetConfiguration(ByVal sessionId As String, ByVal rawPayload As String) As WebResponse

            Dim resource As String = String.Format("configure/{0}", sessionId)
            Dim request = HttpWebRequest.Create(ResourceUri(resource))
            request.Method = "POST"
            request.ContentType = GetContentTypeAsString()

            Using requestStream As New StreamWriter(request.GetRequestStream)
                requestStream.Write(rawPayload)
                requestStream.Flush()
            End Using

            Return request.GetResponse()

        End Function
        Public Function Capture(ByVal sessionId As String) As WebResponse
            Dim resource As String = String.Format("capture/{0}", sessionId)
            Dim request = HttpWebRequest.Create(ResourceUri(resource))
            request.Method = "POST"
            request.ContentType = GetContentTypeAsString()
            request.ContentLength = 0
            Return request.GetResponse()
        End Function

        Public Function Download(ByVal captureId As String) As WebResponse
            Dim resource As String = String.Format("download/{0}", captureId)
            Dim request = HttpWebRequest.Create(ResourceUri(resource))
            request.Method = "GET"
            request.ContentType = GetContentTypeAsString()
            request.ContentLength = 0
            Return request.GetResponse()
        End Function

        Public Function ThriftyDownload(ByVal captureId As String, ByVal maxSize As String) As WebResponse
            Dim resource As String = String.Format("download/{0}/{1}", captureId, maxSize)
            Dim request = HttpWebRequest.Create(ResourceUri(resource))
            request.Method = "GET"
            request.ContentType = GetContentTypeAsString()
            request.ContentLength = 0
            Return request.GetResponse()
        End Function

        Public Function GetDownloadInfo(ByVal captureId As String) As WebResponse
            Dim resource As String = String.Format("download/{0}/info", captureId)
            Dim request = HttpWebRequest.Create(ResourceUri(resource))
            request.Method = "GET"
            request.ContentType = GetContentTypeAsString()
            request.ContentLength = 0
            Return request.GetResponse()
        End Function

    End Class

End Namespace
