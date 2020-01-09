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

Namespace Nist.Bcl.Threading


    Public Interface IJob
        Inherits IDisposable

        Sub Invoke()
        Sub BeginInvoke() ' Cannot guarantee that the target delegate has started
        Sub EndInvoke()

        Sub Cancel()

        ReadOnly Property Timeout As Integer
        ReadOnly Property TargetDelegate As [Delegate]
        ReadOnly Property TargetArgs As Object()

        ReadOnly Property Id As Guid
        ReadOnly Property TargetException As Exception
        ReadOnly Property ReturnValue As Object
        ReadOnly Property TimeInvoked As DateTime
        ReadOnly Property TimeCompleted As DateTime
        ReadOnly Property TargetDelegateInvocationTime As Long
        ReadOnly Property EventHandlerInvocationTime As Long

        ReadOnly Property WasEnded As Boolean ' True if EndInvoke() was called on this job
        ReadOnly Property WasTimedOut As Boolean ' True if the job timed out
        ReadOnly Property WasCanceled As Boolean ' True if the job was Canceled via Cancel()
        ReadOnly Property WasHandled As Boolean ' If true, then event handler is guaranteed to be completed (converse not necessarily true --- there could be a small lag)

        ReadOnly Property NeverInvoked As Boolean ' If true, then Invoke()/BeginInvoke() was interrupted before the target delegate started

        ReadOnly Property EndInvokeWaitsForEventHandlers As Boolean
        ReadOnly Property EndInvokeEventHandlerTimeout As Integer

        Event Done As EventHandler(Of JobDoneEventArgs)

    End Interface


    Public Interface IJobFactory

        Function Create() As IJob

        Property Timeout As Integer
        Property TargetDelegate As [Delegate]
        Property TargetArgs As Object()

        Property EndInvokeWaitsForEventHandlers As Boolean
        Property EndInvokeEventHandlerTimeout As Integer


    End Interface

    Public Class JobDoneEventArgs
        Inherits EventArgs

        Public Sub New(ByVal job As IJob)
            mJob = job
        End Sub

        Public ReadOnly Property Job As IJob
            Get
                Return mJob
            End Get
        End Property

        Private mJob As IJob
    End Class

    Public MustInherit Class JobException
        Inherits Exception

        Public Sub New()
        End Sub

        Public Sub New(ByVal message As String)
            MyBase.new(message)
        End Sub

        Public Sub New(ByVal message As String, ByVal innerException As Exception)
            MyBase.new(message, innerException)
        End Sub

    End Class

    Public Class JobAlreadyStartedException
        Inherits JobException

        Public Sub New()
        End Sub

        Public Sub New(ByVal message As String)
            MyBase.new(message)
        End Sub

        Public Sub New(ByVal message As String, ByVal innerException As Exception)
            MyBase.new(message, innerException)
        End Sub

    End Class

    Public Class JobNotStartedException
        Inherits JobException

        Public Sub New()
        End Sub

        Public Sub New(ByVal message As String)
            MyBase.new(message)
        End Sub

        Public Sub New(ByVal message As String, ByVal innerException As Exception)
            MyBase.new(message, innerException)
        End Sub


    End Class

    Public Class JobAlreadyCanceledException
        Inherits JobException

        Public Sub New()
        End Sub

        Public Sub New(ByVal message As String)
            MyBase.new(message)
        End Sub

        Public Sub New(ByVal message As String, ByVal innerException As Exception)
            MyBase.new(message, innerException)
        End Sub

    End Class


End Namespace