' •—————————————————————————————————————————————————————————————————————————————————————————————————————•
'                              National Institute of Standards and Technology
'                                          Biometric Clients Lab
' •—————————————————————————————————————————————————————————————————————————————————————————————————————•
'  File author(s):
'       Kevin Mangold (kevin.mangold@nist.gov)
'       Kayee Kwong (kayee@nist.gov)
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
Option Explicit On

Imports System.IO
Imports System.Text
Imports System.Threading
Imports System.ServiceModel
Imports System.ServiceModel.Web

Namespace Nist.Bcl.Wsbd.Streaming
    Public Class WsbdStream
        Inherits Stream

        Public Property MaxFramesPerSecond As Integer = 20

        Protected Property TargetPool As StreamPool
        Protected Property MaxBufferSize As Integer = 1024 * 5
        Protected Property Boundary As String

        Protected Property Frame As Byte()
        Protected Property ContentType As String
        Protected Property FramePosition As Integer

        Private mId As Guid = Guid.NewGuid()
        Public ReadOnly Property Id As Guid
            Get
                Return mId
            End Get
        End Property

        Public Sub New(TargetPool As StreamPool, Boundary As String)
            System.Console.WriteLine("{1} opened at {0}", DateTime.Now, Id)

            Me.Boundary = Boundary
            Me.TargetPool = TargetPool

            TargetPool.RegisterStream(Me)
        End Sub

        Protected Sub CopyLatestFrame()
            Thread.Sleep(CInt(1024 / MaxFramesPerSecond))
            TargetPool.CopyLatestFrame(Me)
        End Sub

        Public Sub SetLatestFrame(Frame As Byte(), ContentType As String)
            Me.Frame = Frame
            Me.ContentType = ContentType
            FramePosition = 0
        End Sub

        Public Overrides Function Read(Buffer() As Byte, Offset As Integer, Count As Integer) As Integer
            If Frame Is Nothing AndAlso Not TargetPool.FrameReady Then
                Return 1
            End If

            If Frame Is Nothing Then
                CopyLatestFrame()

                If Frame Is Nothing Then
                    Return 1
                End If

                Dim Header As Byte() = System.Text.Encoding.UTF8.GetBytes(GetHeader())
                Array.Copy(Header, Buffer, Header.Length)
                Return Header.Length
            Else
                Dim AllowedBufferSize = Math.Min(Count, MaxBufferSize)
                Dim BytesLeft As Integer = Frame.Length - FramePosition
                Dim BytesRead As Integer = 0

                If BytesLeft < AllowedBufferSize Then
                    Array.Copy(Frame, FramePosition, Buffer, 0, BytesLeft)
                    BytesRead = BytesLeft
                    Frame = Nothing
                Else
                    Array.Copy(Frame, FramePosition, Buffer, 0, AllowedBufferSize)
                    BytesRead = AllowedBufferSize
                End If

                FramePosition += BytesRead

                Return BytesRead
            End If
        End Function

        Private Function GetHeader() As String
            Return String.Format("{0}--{1}{0}Content-type: {2}{0}Content-Length: {3}{0}{0}", vbCrLf, Boundary, ContentType, Frame.Length)
        End Function

        Public Overrides ReadOnly Property CanRead As Boolean
            Get
                Return True
            End Get
        End Property

        Public Overrides ReadOnly Property CanSeek As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides ReadOnly Property CanWrite As Boolean
            Get
                Return False
            End Get
        End Property

        Public Overrides Sub Flush()

        End Sub

        Public Overrides ReadOnly Property Length As Long
            Get
                Throw New Exception("This Stream does not support length property.")
            End Get
        End Property

        Public Overrides Property Position As Long
            Get
                Return FramePosition
            End Get
            Set(value As Long)
                Throw New Exception("This Stream does not support setting the position property.")
            End Set
        End Property

        Public Overrides Function Seek(offset As Long, origin As System.IO.SeekOrigin) As Long
            Throw New Exception("This Stream does not support seeking.")
        End Function

        Public Overrides Sub SetLength(value As Long)
            Throw New Exception("This Stream does not support set length.")
        End Sub

        Public Overrides Sub Write(buffer() As Byte, offset As Integer, count As Integer)

        End Sub

        Public Overrides Sub Close()
            MyBase.Close()
            System.Console.WriteLine("{1} closed at {0}", DateTime.Now, Id)
            TargetPool.UnregisterStream(Me)
        End Sub
    End Class
End Namespace