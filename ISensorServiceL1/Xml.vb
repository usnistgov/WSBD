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

Imports System.Runtime.Serialization
Imports System.Reflection
Imports System.Text
Imports System.Xml
Imports System.Xml.Schema
Imports System.IO

'
' Note: This module is not part of Infrastructure so that the Infrastructure assembly 
' did not also need a reference to the System.Runtime.Serialization assembly.
'

Namespace Nist.Bcl.Wsbd

    Public Module XmlUtil

        Public Function ToXElement(ByVal source As Object) As XElement
            Return ToXElement(source, New DataContractSerializer(source.GetType()))
        End Function


        Public Function ToXElement(ByVal source As Object, ByVal serializer As XmlObjectSerializer) As XElement

            Dim result As XElement

            ' This is a fragile code block. The memory stream gets closed prematurely
            ' if (a) the 'ms' and 'writer' intialization is moved inside the 'try' block
            ' or (b) the XmlWriter is not flushed. Nested 'using' statements are not 
            ' recommended since they lead to a code analysis warning.

            Dim ms As MemoryStream = Nothing
            Dim writer As XmlWriter = Nothing

            ms = New MemoryStream
            writer = XmlWriter.Create(ms)
            Try
                serializer.WriteObject(writer, source)
                writer.Flush()
                ms.Seek(0, SeekOrigin.Begin)
                result = XElement.Load(ms)
            Finally
                If writer IsNot Nothing Then DirectCast(writer, IDisposable).Dispose()
                ' [CA2202] If ms IsNot Nothing Then DirectCast(ms, IDisposable).Dispose()
            End Try

            Return result
        End Function

        Public Function ToObject(Of T)(ByVal xe As XElement) As T
            Return ToObject(Of T)(xe, New DataContractSerializer(GetType(T)))
        End Function


        Public Function ToObject(Of T)(ByVal xe As XElement, ByVal serializer As XmlObjectSerializer) As T
            Return DirectCast(serializer.ReadObject(xe.CreateReader), T)
        End Function


        Public Function Validate(ByVal xml As XElement, ByVal schemas As XmlSchemaSet) As List(Of ValidationEventArgs)

            Dim result As New List(Of ValidationEventArgs)

            Dim readerSettings As New XmlReaderSettings
            readerSettings.Schemas.Add(schemas)
            readerSettings.ValidationType = ValidationType.Schema
            readerSettings.ConformanceLevel = ConformanceLevel.Auto
            readerSettings.ValidationFlags = XmlSchemaValidationFlags.ReportValidationWarnings

            Dim handler = Sub(sender As Object, e As ValidationEventArgs)
                              result.Add(e)
                          End Sub
            AddHandler readerSettings.ValidationEventHandler, handler

            Dim reader = XmlReader.Create(xml.CreateReader, readerSettings)
            Try
                While reader.Read
                    ' Read the entire document; the validation event handler
                    ' will add validation events to the result list.
                End While
            Finally
                If reader IsNot Nothing Then DirectCast(reader, IDisposable).Dispose()
            End Try

            Return result
        End Function

    End Module

    Public Module Schema

        Public ReadOnly Property WSBD As XmlSchemaSet
            Get
                Static ss As XmlSchemaSet = Nothing
                If ss Is Nothing Then
                    ss = New XmlSchemaSet
                    ss.Add(RetrieveSchema("WSBD"))
                End If
                Return ss
            End Get
        End Property

        Public ReadOnly Property Dictionary As XmlSchemaSet
            Get
                Static ss As XmlSchemaSet = Nothing
                If ss Is Nothing Then
                    ss = New XmlSchemaSet
                    ss.Add(RetrieveSchema("WSBD"))
                    ss.Add(DataTypeAsRootElement("Dictionary"))
                End If
                Return ss
            End Get
        End Property

        Public ReadOnly Property WsbdArray As XmlSchemaSet
            Get
                Static ss As XmlSchemaSet = Nothing
                If ss Is Nothing Then
                    ss = New XmlSchemaSet
                    ss.Add(RetrieveSchema("WSBD"))
                    ss.Add(DataTypeAsRootElement("Array"))
                End If
                Return ss
            End Get
        End Property

        Private Function DataTypeAsRootElement(ByVal typename As String) As XmlSchema
            Dim shim = <xs:schema elementFormDefault="qualified"
                           targetNamespace=<%= Constants.WsbdNamespace %>
                           xmlns:wsbd="urn:oid:2.16.840.1.101.3.9.3.1"
                           xmlns:xs="http://www.w3.org/2001/XMLSchema">
                           <xs:include schemaLocation=<%= "WSBD.xsd" %>/>
                           <xs:element name=<%= typename %> type=<%= "wsbd:" & typename %>/>
                       </xs:schema>
            Return XmlSchema.Read(shim.CreateReader(), Nothing)
        End Function

        Private Function RetrieveSchema(ByVal typeName As String) As XmlSchema
            Dim resourceNames = Assembly.GetExecutingAssembly.GetManifestResourceNames
            Dim nameOfSoughtResource = typeName & ".xsd"

            If Not resourceNames.Contains(nameOfSoughtResource) Then
                Dim exceptionMessage = String.Format(My.Resources.CouldNotRetrieveSchema, typeName, nameOfSoughtResource)
                Throw New ArgumentException(exceptionMessage, "typeName")
            End If

            Dim testSchemaStream As XmlSchema = XmlSchema.Read(Assembly.GetExecutingAssembly.GetManifestResourceStream(typeName & ".xsd"), Nothing)

            Return XmlSchema.Read(Assembly.GetExecutingAssembly.GetManifestResourceStream(typeName & ".xsd"), Nothing)

        End Function
    End Module
End Namespace