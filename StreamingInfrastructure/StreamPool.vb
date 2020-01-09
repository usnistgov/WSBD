' •—————————————————————————————————————————————————————————————————————————————————————————————————————•
'                              National Institute of Standards and Technology
'                                          Biometric Clients Lab
' •—————————————————————————————————————————————————————————————————————————————————————————————————————•
'  File author(s):
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
Option Explicit On

Imports System.IO
Imports System.Text
Imports System.Threading
Imports System.ServiceModel
Imports System.ServiceModel.Web

Namespace Nist.Bcl.Wsbd.Streaming
    Public Class StreamPool
        Protected Property StreamSource As IStreamable
        Protected Property Streams As New List(Of WsbdStream)

        Protected Property LatestFrame As Byte()
        Protected Property LatestContentType As String

        Public Sub RegisterStream(Stream As WsbdStream)
            Streams.Add(Stream)
            StreamSource.SignalStartStream()
        End Sub

        Public Sub UnregisterStream(Stream As WsbdStream)
            Streams.Remove(Stream)

            If Streams.Count = 0 Then
                StreamSource.SignalStopStream()
                'Stop streaming should also clear the latest frame in the Stream pool.
                Monitor.Enter(Me)
                LatestFrame = Nothing
                LatestContentType = Nothing
                Monitor.Exit(Me)
            End If

        End Sub

        Public Sub New(StreamSource As IStreamable)
            Me.StreamSource = StreamSource
            StreamSource.RegisterTargetPool(Me)
        End Sub

        Public Function FrameReady() As Boolean
            Dim Ready As Boolean = False
            Monitor.Enter(Me)
            Ready = LatestFrame IsNot Nothing
            Monitor.Exit(Me)
            Return Ready
        End Function

        Public Sub RegisterStreamSource(StreamSource As IStreamable)
            If Me.StreamSource IsNot Nothing Then
                Me.StreamSource.UnregisterTargetPool()
            End If

            Me.StreamSource = StreamSource
            StreamSource.RegisterTargetPool(Me)
        End Sub

        Public Sub UpdateLatestFrame(NewFrame As Byte(), NewContentType As String)
            Monitor.Enter(Me)
            LatestFrame = NewFrame
            LatestContentType = NewContentType
            Monitor.Exit(Me)
        End Sub

        Public Sub CopyLatestFrame(s As WsbdStream)
            Monitor.Enter(Me)
            s.SetLatestFrame(LatestFrame, LatestContentType)
            Monitor.Exit(Me)
        End Sub

    End Class
End Namespace