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

Imports System.IO
Imports System.ServiceModel.Channels
Imports System.Text
Imports System.Xml
Imports System.Xml.XPath

Namespace Nist.Bcl.Wsbd

    Public Module Tidier

        Public Const XsiPrefix = "i"
        Public Const XsdPrefix = "xs"
        Public Const XmlnsNamespace = "http://www.w3.org/2000/xmlns/"
        Public Const XsiNamespace = "http://www.w3.org/2001/XMLSchema-instance"
        Public Const XsdNamespace = "http://www.w3.org/2001/XMLSchema"


        Private Function ExtractXElement(ByRef message As Message) As XElement
            Dim result As XElement

            Dim ms As MemoryStream = Nothing
            Dim writer As XmlDictionaryWriter = Nothing
            Dim reader As XmlReader = Nothing

            Try
                ms = New MemoryStream
                writer = XmlDictionaryWriter.CreateTextWriter(ms)
                message.WriteMessage(writer)
                writer.Flush()
                ms.Seek(0, SeekOrigin.Begin)
                reader = XmlReader.Create(ms)
                result = XElement.Load(reader)
            Finally
                If ms IsNot Nothing Then ms.Dispose()
            End Try

            Return result
        End Function


        Private Sub InjectXElement(ByRef message As Message, ByVal xdoc As XElement)
            Dim ms = New MemoryStream
            xdoc.Save(ms)
            ms.Position = 0
            Dim reader = XmlReader.Create(ms)
            Dim newMessage = message.CreateMessage(reader, Integer.MaxValue, message.Version)
            newMessage.Properties.CopyProperties(message.Properties)
            message = newMessage
        End Sub


        Public Function TransformMessage(ByRef source As Message) As Message
            Dim xe = ExtractXElement(source)
            InjectXElement(source, xe)
            Return Nothing
        End Function

        Public Sub TidyXElement(ByRef xelement As XElement)

            ' This method replaces redundant attributes WCF uses for in-line datatyping.
            ' For example, the attribute combination
            '
            '   xmlns:d2p1="http://www.w3.org/2001/XMLSchema-instance" i:type="d2p1:int"
            '
            ' could by replaced by the more elegant
            '
            '   i:type="xs:int"
            '
            ' if the root element has the prefixes "i" for http://www.w3.org/2001/XMLSchema-instance
            ' and "xs" for http://www.w3.org/2001/XMLSchema-instance. 
            '

            Dim defaultNamespace As XNamespace = XmlnsNamespace

            Dim nodesTidied As Boolean

            For Each e In xelement.Descendants

                Dim xsdAttributes As New List(Of XAttribute)
                Dim typeAttributes As New List(Of XAttribute)
                For Each a In e.Attributes
                    If a.Value = XsdNamespace Then xsdAttributes.Add(a)
                    If a.Name = "{" & XsiNamespace & "}type" Then typeAttributes.Add(a)
                Next

                If Not (xsdAttributes.Count > 0 AndAlso typeAttributes.Count = 1) Then

                    ' Make sure we only tidy those elements that have an inline
                    ' reference to the XSD namespace, and describe the XSD datatype
                    '
                    Continue For
                End If

                If Not nodesTidied Then

                    ' If we haven't tidied any elements yet, then the root of the 
                    ' fragment needs to have the proper XSI and XSD prefixes.
                    '
                    Dim xsi = New XAttribute(defaultNamespace + XsiPrefix, XsiNamespace)
                    Dim xsd = New XAttribute(defaultNamespace + XsdPrefix, XsdNamespace)

                    xelement.ReplaceAttributes(New XAttribute() {xsi, xsd})
                    nodesTidied = True
                End If

                ' Create a "tidy" attribute           '
                Dim typeValue = typeAttributes.First.Value
                Dim datatype = typeValue.Substring(typeValue.IndexOf(":") + 1)
                Dim newTypeAttribute = New XAttribute("{" & XsiNamespace & "}type", XsdPrefix & ":" & datatype)

                ' Remove the "ugly" WCF attributes and replace it with a tidier one
                xsdAttributes.Remove()
                typeAttributes.Remove()
                e.Add(newTypeAttribute)

            Next


        End Sub

        Public Function GetMessageContentFormat(ByVal message As Message) As WebContentFormat

            Dim result = WebContentFormat.Default

            If message.Properties.ContainsKey(WebBodyFormatMessageProperty.Name) Then
                Dim bodyFormat = DirectCast(message.Properties(WebBodyFormatMessageProperty.Name), WebBodyFormatMessageProperty)
                result = bodyFormat.Format
            Else
                '
                ' After lenghtly debugging & testing, it was observed that if the contract operation has 
                ' been with a WebInvoke attribute where the Request or Reply format is set to WebMessageFormat.Xml,
                ' *and* the contents of message are not empty, then  the "WebBodyFormatMessageProperty" 
                ' property does *not* appear in the reply Message objects. Therefore, we use the absense of the
                ' property as a signal that the payload is indeed XML.
                '
                result = WebContentFormat.Xml
            End If
            Return result
        End Function

    End Module

End Namespace


