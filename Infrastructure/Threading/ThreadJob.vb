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

Imports System.Threading
Imports System.Reflection


Namespace Nist.Bcl.Threading

    Public Class ThreadJob
        Implements IJob
        Implements IDisposable

        Friend Sub New(ByVal targetDelegate As System.Delegate, ByVal args() As Object,
                       ByVal timeout As Integer, ByVal endInvokeWaitsForHandlers As Boolean,
                       ByVal eventHandlerTimeout As Integer)

            Dim exceptionHandlers = targetDelegate.Method.GetMethodBody.ExceptionHandlingClauses
            For Each handler In exceptionHandlers
                If handler.Flags = ExceptionHandlingClauseOptions.Clause AndAlso handler.CatchType = GetType(ThreadInterruptedException) Then
                    mTargetDelegateCatchesInterrupt = True
                End If
            Next

            mJobId = Guid.NewGuid
            mTargetDelegate = targetDelegate
            mTargetArgs = args
            mTimeout = timeout
            mEndInvokeWaitsForEventHandlers = endInvokeWaitsForHandlers
            mEventHandlerTimeout = eventHandlerTimeout

        End Sub

        ' Thread that runs the target delegate
        Private mTargetInvocationThread As Thread

        ' Thread that runs the event handler
        Private mEventHandlerThread As Thread

        ' True if the target delegate has a catch clause for the thread interrupt exception
        Private mTargetDelegateCatchesInterrupt As Boolean


        Private mTargetArgs As Object()
        Private mTargetDelegate As System.Delegate
        Private mReturnValue As Object
        Private mTargetException As Exception

        Private mTimeout As Integer 'ms

        Private mTimeInvoked As DateTime ' Timestamp when the job was actually invoked (started)
        Private mInvocationStopwatch As Stopwatch ' Stopwatch for instrumenting invocation time
        Private mTimeCompleted As DateTime ' Timestap of when the job completed
        Private mTargetDelegateInvocationTime As Long ' How long the target delegate was running

        Private mJobId As Guid ' Unique identifier for job

        Private mEndInvokeWaitsForEventHandlers As Boolean ' If a call to EndInvoke() should block until event handlers complete
        Private mEventHandlerTimeout As Integer ' How long an EndInvoke() call will wait until it stops blocking until event handler(s) complete
        Private mEventHandlerInvocationTime As Long ' How long the job's event handlers took to invoke

        Private mWasEnded As Boolean ' True if the job successfully ended via call to Invoke() or EndInvoke()
        Private mWasTimedOut As Boolean ' True if the job experienced a time out
        Private mWasCanceled As Boolean ' True if the job was explicitly Canceled via call to Cancel()
        Private mWasHandled As Boolean ' True if the job's event handlers completed

        Private mNeverInvoked As Boolean

        ' The target delegate should wait for this handle before starting
        Private mEventHandlerThreadStarted As ManualResetEventSlim

        Private Sub InvokeTargetDelegate()
            Try
                If mEventHandlerThreadStarted IsNot Nothing Then mEventHandlerThreadStarted.Wait()
                mReturnValue = mTargetDelegate.DynamicInvoke(mTargetArgs)
            Catch ex As TargetInvocationException
                mTargetException = ex.InnerException
            Catch ex As ThreadInterruptedException
                '
                ' If cancel is called immediately after BeginInvoke, it's possible that the job gets interuppted
                ' before the target delegate was even invoked. 
                '
                If mTargetDelegateCatchesInterrupt Then
                    ' If the target delegate has an exception handler, then mark job as 'never invoked.'
                    mNeverInvoked = True
                Else
                    ' If the target delegate does NOT have a handler, we just pass the exception back to the job.
                    mTargetException = ex
                End If
            End Try
        End Sub


#Region "Implmentation of IJob"

        Public ReadOnly Property WasTimedOut As Boolean Implements IJob.WasTimedOut
            Get
                Return mWasTimedOut
            End Get
        End Property

        Public ReadOnly Property WasCanceled As Boolean Implements IJob.WasCanceled
            Get
                Return mWasCanceled
            End Get
        End Property

        Public ReadOnly Property ReturnValue As Object Implements IJob.ReturnValue
            Get
                Return mReturnValue
            End Get
        End Property

        Private Sub EventHandlerThread()

            If mEventHandlerThreadStarted IsNot Nothing Then mEventHandlerThreadStarted.Set()

            mTargetInvocationThread.Join(mTimeout)
            InterruptTargetInvocationThreadIfNeeded()

            mTargetDelegateInvocationTime = mInvocationStopwatch.ElapsedMilliseconds

            Dim eventHandlerStopwatch = Stopwatch.StartNew
            RaiseEvent Done(Me, New JobDoneEventArgs(Me))
            mEventHandlerInvocationTime = eventHandlerStopwatch.ElapsedMilliseconds
            mWasHandled = True

        End Sub


        Private Sub InterruptTargetInvocationThreadIfNeeded()

            If mTargetInvocationThread.IsAlive Then
                mWasTimedOut = True
                mTargetInvocationThread.Interrupt()
                mTargetInvocationThread.Join()
            End If

        End Sub

        Private Sub BeginInvokeImplmentation(ByVal withEventHandlerThread As Boolean)

            ' A job cannot be started more than once.
            If mTargetInvocationThread IsNot Nothing Then
                Throw New JobAlreadyStartedException
            End If

            ' Set up the event handler thread. Note we embed the job's id in the thread name
            If withEventHandlerThread Then
                mEventHandlerThreadStarted = New ManualResetEventSlim
                mEventHandlerThread = New Thread(AddressOf EventHandlerThread) With {
                    .Name = "Event Handler " & Id.ToString,
                    .IsBackground = True
                }
            End If

            ' Set up the target invocation thread. Note we embed the job's id in the thread name
            mTargetInvocationThread = New Thread(AddressOf InvokeTargetDelegate) With {
                .Name = "Job " & Id.ToString(),
                .IsBackground = True
            }

            mTimeInvoked = DateTime.Now
            mInvocationStopwatch = Diagnostics.Stopwatch.StartNew

            ' Start the target invocation thread. In the case that this is just an Invoke() call
            ' (i.e., withEventHandlerThread is false), then the target invocation thread can go 
            ' be started and the routine exited. If this is a BeginInvoke() call then there is
            ' an event handler thread. However, since the target invocation thread blocks on
            ' a reset event, we don't have to worry about the target invocation thread finishing
            ' before the event handler thread is truly started, since the event handler thread
            ' is what sets that reset event.             '
            '
            mTargetInvocationThread.Start()
            If withEventHandlerThread Then
                mEventHandlerThread.Start()

                ' Don't allow BeginInvoke() to return until the event handler thread is fully started.
                ' This can interfere with our ability to use Cancel() successfully, since an immediate
                ' call to Cancel() after BeginInvoke() would cancel threads not yet fully started.
                '
                mEventHandlerThreadStarted.Wait()
            End If


        End Sub

        Public Sub BeginInvoke() Implements IJob.BeginInvoke

            ' BeginInvoke() attempts to start invocation of the target delegate. This
            ' cannot be guaranteed, since we do not offer (or require) a mechanism 
            ' for the target thread to notify the job that the target invocation has actually 
            ' started. 
            '
            BeginInvokeImplmentation(withEventHandlerThread:=True)
        End Sub

        Public Sub EndInvokeImplementation(ByVal calledFromEndInvoke As Boolean)

            ' Block until the TargetInvocation thread either completes, or times out
            '
            Dim timeout As Integer
            If calledFromEndInvoke Then
                '
                ' Some time may have passed between the call to BeginInvoke() and
                ' the manual call to EndInvoke(). The timeout needs to be reduced by
                ' this amount of time, but certainly no more than the timeout itself.
                '
                timeout = Math.Max(0, CInt(mTimeout - mInvocationStopwatch.ElapsedMilliseconds))
            Else
                timeout = mTimeout
            End If
            mTargetInvocationThread.Join(timeout)

            ' If we're here, and the thread is still alive, then we need to kill it. It's 
            ' entirely possible that the thread is already dead, particularly if the EndInvoke()
            ' is called after the EventHandler thread has timed out the TargetDelegateInvocation 
            ' thread.
            '
            InterruptTargetInvocationThreadIfNeeded()


            ' If we're called from EndInvoke() (as opposed to Invoke()) then we are
            ' not responsible for measuring timing --- the EventHandler thread
            ' will take care of that
            '
            If Not calledFromEndInvoke Then
                mTargetDelegateInvocationTime = mInvocationStopwatch.ElapsedMilliseconds
                mInvocationStopwatch.Stop()
            End If

            JoinWithTimeoutToEventHandlerThread()

            mWasEnded = True
            mTimeCompleted = mTimeInvoked.AddMilliseconds(mTargetDelegateInvocationTime)

        End Sub

        Private Sub JoinWithTimeoutToEventHandlerThread()

            If mEndInvokeWaitsForEventHandlers AndAlso mEventHandlerThread IsNot Nothing Then
                mEventHandlerThread.Join(mEventHandlerTimeout)
                If mEventHandlerThread.IsAlive Then
                    mEventHandlerThread.Interrupt()
                    mEventHandlerThread.Join()
                End If
            End If

        End Sub

        Public Sub EndInvoke() Implements IJob.EndInvoke

            ' Note that when EndInvoke() is called, we want the watchdog thread to 
            ' timestamp the end of the invocation time, not the EndInvoke() call. 
            ' This is because some time may pass between (a) the end of the execution of the target
            ' delegate, and (b) the actual call to EndInvoke(). If EndInvoke() compted the
            ' invocation time based on this, then the job's invocation time would incorrectly
            ' include any lag time.
            '
            EndInvokeImplementation(calledFromEndInvoke:=True)
        End Sub

        Public Sub Invoke() Implements IJob.Invoke

            BeginInvokeImplmentation(withEventHandlerThread:=False)
            EndInvokeImplementation(calledFromEndInvoke:=False)

        End Sub

        Public Sub Cancel() Implements IJob.Cancel

            If mTargetInvocationThread Is Nothing Then Throw New JobNotStartedException
            If mWasCanceled Then Throw New JobAlreadyCanceledException

            ' Any call to cancel should also wait for the event handler thread to be started, 
            ' to minimize the chance that a Cancel() call will occur before the target delegate
            ' can be invoked.
            '
            If mEventHandlerThreadStarted IsNot Nothing Then
                mEventHandlerThreadStarted.Wait()
            End If

            If mTargetInvocationThread.IsAlive Then

                ' First, mark this thread as Canceled
                mWasCanceled = True

                ' Note that this will send a ThreadInterruptedException to the target invocation
                ' thread only if it is fully running. If Cancel() is called immmediately after
                ' a call to BeginInvoke() then it is possible that this object's InvokeTargetDelegate()
                ' method would be sent the ThreadInterruptedException, as opposed to the target
                ' delegate itself. The target delegate will not run; so it would behave as if it was Canceled
                ' before it was even started.
                '
                mTargetInvocationThread.Interrupt()

                ' Note that after an interruption, we wait indefinitely for the target invocation thread
                ' to finish. 
                mTargetInvocationThread.Join()
            End If

            JoinWithTimeoutToEventHandlerThread()

        End Sub


        Public ReadOnly Property TargetException As System.Exception _
            Implements IJob.TargetException
            Get
                Return mTargetException
            End Get
        End Property

        Public ReadOnly Property TimeCompleted As Date Implements IJob.TimeCompleted
            Get
                Return mTimeCompleted
            End Get
        End Property

        Public ReadOnly Property TimeInvoked As Date Implements IJob.TimeInvoked
            Get
                Return mTimeInvoked
            End Get
        End Property


        Public ReadOnly Property TargetArgs As Object() _
            Implements IJob.TargetArgs
            Get
                Return mTargetArgs
            End Get
        End Property

        Public ReadOnly Property TargetDelegate As System.Delegate _
            Implements IJob.TargetDelegate
            Get
                Return mTargetDelegate
            End Get
        End Property

        Public ReadOnly Property Timeout As Integer Implements IJob.Timeout
            Get
                Return mTimeout
            End Get
        End Property

        Public ReadOnly Property Id As Guid Implements IJob.Id
            Get
                Return mJobId
            End Get
        End Property

        Public ReadOnly Property NevenInvoked As Boolean Implements IJob.NeverInvoked
            Get
                Return mNeverInvoked
            End Get
        End Property


        Public ReadOnly Property TotalInvocationTime As Long _
            Implements IJob.TargetDelegateInvocationTime
            Get
                Return mTargetDelegateInvocationTime
            End Get
        End Property

        Public ReadOnly Property EventHandlerInvocationTime As Long _
            Implements IJob.EventHandlerInvocationTime
            Get
                Return mEventHandlerInvocationTime
            End Get
        End Property

        Public Event Done(ByVal sender As Object, ByVal e As JobDoneEventArgs) Implements IJob.Done

        Public ReadOnly Property EventHandlerTimeout As Integer _
            Implements IJob.EndInvokeEventHandlerTimeout
            Get
                Return mEventHandlerTimeout
            End Get
        End Property

        Public ReadOnly Property EndInvokeWaitsForEventHandlers As Boolean _
            Implements IJob.EndInvokeWaitsForEventHandlers
            Get
                Return mEndInvokeWaitsForEventHandlers
            End Get
        End Property

        Public ReadOnly Property IsEnded As Boolean Implements IJob.WasEnded
            Get
                Return mWasEnded
            End Get
        End Property

        Public ReadOnly Property WasHandled As Boolean Implements IJob.WasHandled
            Get
                Return mWasHandled
            End Get
        End Property

#End Region

#Region "IDisposable Support"
        Private mDisposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(ByVal disposing As Boolean)
            If Not mDisposedValue Then
                If disposing AndAlso mEventHandlerThreadStarted IsNot Nothing Then
                    mEventHandlerThreadStarted.Dispose()
                End If
            End If
            mDisposedValue = True
        End Sub

        ' This code added by Visual Basic to correctly implement the disposable pattern.
        Public Sub Dispose() Implements IDisposable.Dispose
            ' Do not change this code.  Put cleanup code in Dispose(ByVal disposing As Boolean) above.
            Dispose(True)
            GC.SuppressFinalize(Me)
        End Sub
#End Region

    End Class

End Namespace