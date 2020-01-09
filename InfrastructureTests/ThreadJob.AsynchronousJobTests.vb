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

Imports System
Imports System.Threading
Imports Nist.Bcl.Threading


<TestClass()>
Public Class AsynchronousThreadJobTests

    Private Shared smRunOneTestAtATime As New Object
    Private Shared LockTaken As Boolean

    <TestInitialize()>
    Public Sub TestInitialize()
        LockTaken = False
        Threading.Monitor.Enter(smRunOneTestAtATime, LockTaken)
    End Sub

    <TestCleanup()>
    Public Sub TestCleanup()
        If LockTaken Then
            Threading.Monitor.Exit(smRunOneTestAtATime)
        End If
    End Sub

    Public Shared Function AverageInvocationTimeOkay(ByVal jobs As List(Of IJob),
                                                     ByVal targetAverageTime As Integer,
                                                     ByVal plusOrMinus As Integer) As Tuple(Of Boolean, String)

        If jobs Is Nothing Then Return New Tuple(Of Boolean, String)(False, String.Empty)
        Dim averageTime As Integer = CInt((From j As IJob In jobs Select j.TargetDelegateInvocationTime).Sum() / jobs.Count)
        Dim minimumTime As Integer = targetAverageTime - plusOrMinus
        Dim maximumTime As Integer = targetAverageTime + plusOrMinus

        Dim okay As Boolean = minimumTime <= averageTime AndAlso averageTime <= maximumTime
        Dim message As String = String.Format("Average invocation time = {0} total ms. Expected range is {1}-{2} ms",
                                              averageTime, minimumTime, maximumTime)

        Return New Tuple(Of Boolean, String)(okay, message)

    End Function



    Private Shared smCreateJobLock As New Object
    Public Shared Function CreateJob(ByVal sleepTime As Integer, ByVal timeout As Integer, ByVal scenario As ActiveObjectScenario,
                                     Optional ByVal endInvokeWaitsForEventHandlers As Boolean = True,
                                     Optional ByVal endInvokeEventHandlerTimeout As Integer = 500) As IJob
        SyncLock smCreateJobLock
            Dim ao As New ActiveObject
            Dim factory As New ThreadJobFactory

            With factory
                .TargetDelegate = [Delegate].CreateDelegate(GetType(ActiveObject.RunDelegate), ao, "Run")
                .TargetArgs = New Object() {Math.Max(0, sleepTime), scenario}
                .Timeout = timeout
                .EndInvokeWaitsForEventHandlers = endInvokeWaitsForEventHandlers
                .EndInvokeEventHandlerTimeout = endInvokeEventHandlerTimeout
            End With
            Dim job = factory.Create()
            Return job
        End SyncLock
    End Function

    Public Shared Function CreateJobWithoutHandler(ByVal sleepTime As Integer, ByVal timeout As Integer, ByVal scenario As ActiveObjectScenario,
                                 Optional ByVal endInvokeWaitsForEventHandlers As Boolean = True,
                                 Optional ByVal endInvokeEventHandlerTimeout As Integer = 500) As IJob
        SyncLock smCreateJobLock
            Dim ao As New ActiveObject
            Dim factory As New ThreadJobFactory

            With factory
                .TargetDelegate = [Delegate].CreateDelegate(GetType(ActiveObject.RunDelegate), ao, "NoHandlerRun")
                .TargetArgs = New Object() {Math.Max(0, sleepTime), scenario}
                .Timeout = timeout
                .EndInvokeWaitsForEventHandlers = endInvokeWaitsForEventHandlers
                .EndInvokeEventHandlerTimeout = endInvokeEventHandlerTimeout
            End With
            Dim job = factory.Create()
            Return job
        End SyncLock
    End Function

    Public Shared Function CreateJobWithExceptionFilter(ByVal sleepTime As Integer, ByVal timeout As Integer, ByVal scenario As ActiveObjectScenario,
                                 Optional ByVal endInvokeWaitsForEventHandlers As Boolean = True,
                                 Optional ByVal endInvokeEventHandlerTimeout As Integer = 500) As IJob
        SyncLock smCreateJobLock
            Dim ao As New ActiveObject
            Dim factory As New ThreadJobFactory

            With factory
                .TargetDelegate = [Delegate].CreateDelegate(GetType(ActiveObject.RunDelegate), ao, "RunWithExceptionFilter")
                .TargetArgs = New Object() {Math.Max(0, sleepTime), scenario}
                .Timeout = timeout
                .EndInvokeWaitsForEventHandlers = endInvokeWaitsForEventHandlers
                .EndInvokeEventHandlerTimeout = endInvokeEventHandlerTimeout
            End With
            Dim job = factory.Create()
            Return job
        End SyncLock
    End Function


    Public Shared Function CreateJobWithFinallyBlock(ByVal sleepTime As Integer, ByVal timeout As Integer, ByVal scenario As ActiveObjectScenario,
                             Optional ByVal endInvokeWaitsForEventHandlers As Boolean = True,
                             Optional ByVal endInvokeEventHandlerTimeout As Integer = 500) As IJob
        SyncLock smCreateJobLock
            Dim ao As New ActiveObject
            Dim factory As New ThreadJobFactory

            With factory
                .TargetDelegate = [Delegate].CreateDelegate(GetType(ActiveObject.RunDelegate), ao, "RunWithFinallyOnly")
                .TargetArgs = New Object() {Math.Max(0, sleepTime), scenario}
                .Timeout = timeout
                .EndInvokeWaitsForEventHandlers = endInvokeWaitsForEventHandlers
                .EndInvokeEventHandlerTimeout = endInvokeEventHandlerTimeout
            End With
            Dim job = factory.Create()
            Return job
        End SyncLock
    End Function

    Public Const DefaultJobCount As Integer = 25

    <TestMethod()>
    Public Sub TestSuccessfulJobsHandledBeforeEndInvoke()

        Dim sleeptimes = {0, 1, 10, 100, 500}
        Dim jobCount As Integer = AsynchronousThreadJobTests.DefaultJobCount

        For Each sleepTime In sleeptimes

            Dim jobs As New List(Of IJob)


            For i As Integer = 0 To jobCount - 1
                Dim job = CreateJob(sleepTime, Math.Max(100, CInt(10 * sleepTime)), ActiveObjectScenario.DoNothing)
                jobs.Add(job)

                job.BeginInvoke()

                ' Make sure the event handler has plenty of time to complete. This sleep time should have
                ' no effect on job's TargetDelegationInvocationTime.
                '
                Thread.Sleep(Math.Max(100, CInt(2 * sleepTime)))
                Assert.IsTrue(job.WasHandled)

                job.EndInvoke()

                Assert.AreEqual(ActiveObjectReturnState.Completed, job.ReturnValue)
                Assert.IsFalse(job.WasTimedOut)
                Assert.IsFalse(job.WasCanceled)
                Assert.IsTrue(job.WasEnded)
                Assert.IsNull(job.TargetException)
            Next

            Dim timeOkay = AverageInvocationTimeOkay(jobs, sleepTime, Math.Max(100, CInt(5 * sleepTime)))
            Assert.IsTrue(timeOkay.Item1, timeOkay.Item2)

        Next

    End Sub


    <TestMethod()>
    Public Sub TestSuccessfulJobsHandledAfterEndInvoke()

        ' This test may be ill behaved for sleep times under 10 ms, since the event handler 
        ' might have been called (and complete) successfully between the call to job.BeginInvoke()
        ' and the Assert.IsFalse(job.WasHandled)
        '
        Dim sleeptimes = {100, 500}

        Dim jobCount As Integer = AsynchronousThreadJobTests.DefaultJobCount

        For Each sleepTime In sleeptimes

            Dim jobs As New List(Of IJob)

            For i As Integer = 0 To jobCount - 1
                Dim job = CreateJob(sleepTime, Math.Max(100, CInt(10 * sleepTime)), ActiveObjectScenario.DoNothing)
                jobs.Add(job)

                ' Immediately after BeginInvoke() the job should not be handled, but because EndInvoke()
                ' waits for the job handler to be finished, the job should be marked as handled after that call
                '
                job.BeginInvoke()
                Assert.IsFalse(job.WasHandled)
                job.EndInvoke()
                Assert.IsTrue(job.WasHandled)

                Assert.AreEqual(ActiveObjectReturnState.Completed, job.ReturnValue)
                Assert.IsFalse(job.WasTimedOut)
                Assert.IsFalse(job.WasCanceled)
                Assert.IsTrue(job.WasEnded)
                Assert.IsNull(job.TargetException)
            Next

            Dim timeOkay = AverageInvocationTimeOkay(jobs, sleepTime, 25)
            Assert.IsTrue(timeOkay.Item1, timeOkay.Item2)

        Next


    End Sub

    <TestMethod()>
    Public Sub TestTimedOutJobs()


        Dim sleeptimes = {100, 500}
        Dim jobCount As Integer = AsynchronousThreadJobTests.DefaultJobCount

        For Each sleepTime In sleeptimes

            Dim jobs As New List(Of IJob)

            For i As Integer = 0 To jobCount - 1
                Dim job = CreateJob(2 * sleepTime, sleepTime, ActiveObjectScenario.DoNothing)
                jobs.Add(job)

                AddHandler job.Done, Sub(sender As Object, e As JobDoneEventArgs)
                                         ' In the event handler, the job should be tagged as timed out
                                         Assert.IsTrue(e.Job.WasTimedOut)
                                     End Sub

                job.BeginInvoke()
                Assert.IsFalse(job.WasHandled)
                job.EndInvoke()
                Assert.IsTrue(job.WasHandled)

                Assert.AreEqual(ActiveObjectReturnState.Interrupted, job.ReturnValue)
                Assert.IsTrue(job.WasTimedOut)
                Assert.IsFalse(job.WasCanceled)
                Assert.IsTrue(job.WasEnded)
                Assert.IsNull(job.TargetException)
            Next

            Dim timeOkay = AverageInvocationTimeOkay(jobs, sleepTime, Math.Max(100, CInt(5 * sleepTime)))
            Assert.IsTrue(timeOkay.Item1, timeOkay.Item2)

        Next

    End Sub

    <TestMethod()>
    Public Sub TestManuallyCanceledAsynchronousJobs()

        Dim sleeptimes = {100, 250, 500}
        Dim jobCount As Integer = 500 ' AsynchronousThreadJobTests.DefaultJobCount

        For Each sleepTime In sleeptimes


            For i As Integer = 0 To jobCount - 1
                Dim job = CreateJob(sleepTime, Math.Max(sleepTime, CInt(10 * sleepTime)), ActiveObjectScenario.DoNothing)



                AddHandler job.Done, Sub(sender As Object, e As JobDoneEventArgs)
                                         ' In the event handler, the job should be tagged as Canceled
                                         Assert.IsTrue(e.Job.WasCanceled)
                                     End Sub

                job.BeginInvoke()
                Assert.IsFalse(job.WasEnded) : Assert.IsFalse(job.WasHandled) : Assert.IsFalse(job.WasCanceled)

                job.Cancel()
                Assert.IsTrue(job.WasCanceled)

                job.EndInvoke()
                Assert.IsTrue(job.WasEnded) : Assert.IsTrue(job.WasHandled) : Assert.IsTrue(job.WasCanceled)

                If Not job.NeverInvoked Then
                    ' If the target delegate was actually invoked, check the state of the target
                    Assert.AreEqual(ActiveObjectReturnState.Interrupted, DirectCast(job.TargetDelegate.Target, ActiveObject).State)
                End If

                Assert.IsNull(job.TargetException)
                Assert.IsFalse(job.WasTimedOut)
            Next

        Next

    End Sub

    <TestMethod()>
    Public Sub TestManuallyCanceledAsynchronousJobsWithNoInterruptHandler()

        Dim sleeptimes = {100, 250, 500}
        Dim jobCount As Integer = 500

        For Each sleepTime In sleeptimes


            For i As Integer = 0 To jobCount - 1
                Dim job = CreateJobWithoutHandler(sleepTime, Math.Max(sleepTime, CInt(10 * sleepTime)), ActiveObjectScenario.DoNothing)


                AddHandler job.Done, Sub(sender As Object, e As JobDoneEventArgs)
                                         ' In the event handler, the job should be tagged as Canceled
                                         Assert.IsTrue(e.Job.WasCanceled)
                                     End Sub

                job.BeginInvoke()
                Assert.IsFalse(job.WasEnded) : Assert.IsFalse(job.WasHandled) : Assert.IsFalse(job.WasCanceled)

                job.Cancel()
                Assert.IsTrue(job.WasCanceled)

                job.EndInvoke()
                Assert.IsTrue(job.WasEnded) : Assert.IsTrue(job.WasHandled) : Assert.IsTrue(job.WasCanceled)

                Assert.AreEqual(GetType(ThreadInterruptedException), job.TargetException.GetType)
                Assert.IsFalse(job.NeverInvoked)
                Assert.IsFalse(job.WasTimedOut)
            Next

        Next

    End Sub


    <TestMethod()>
    Public Sub TestEndInvokeWithoutWaitingForEventsToBeHandled()

        ' Don't use an event handler sleep time much less than 250 ms, 
        ' just to be sure that the handler cannot complete before
        ' we check to see if the job was actually handled.
        '
        Dim eventHandlerSleeptimes = {250, 500}



        Dim jobCount As Integer = AsynchronousThreadJobTests.DefaultJobCount

        For Each sleepTime In eventHandlerSleeptimes

            Dim jobs As New List(Of IJob)

            Dim sleep As Integer = sleepTime ' Prevent compiler warning
            For i As Integer = 0 To jobCount - 1
                Dim job = CreateJob(0, 100, ActiveObjectScenario.DoNothing, False)
                AddHandler job.Done, Sub()
                                         Threading.Thread.Sleep(sleep)
                                     End Sub

                jobs.Add(job)

                job.BeginInvoke()
                job.EndInvoke()
                Assert.IsFalse(job.WasHandled)

                ' Wait plenty of time for the job to be handled, and then check
                Threading.Thread.Sleep(sleep * 2)
                Assert.IsTrue(job.WasHandled)
            Next

        Next


    End Sub


    <TestMethod()>
    Public Sub TestEventHandlerTimeout()

        ' Don't use an event handler sleep time much less than 50 ms, the
        ' job might be handleded before we can test for whether or not it
        ' was actually handled.
        '
        Dim eventHandlerSleeptimes = {100, 500}
        Dim jobCount As Integer = AsynchronousThreadJobTests.DefaultJobCount
        Dim invocationTimeTolerance = 25

        For Each eventHandlerSleeptime In eventHandlerSleeptimes

            Dim sleep As Integer = eventHandlerSleeptime ' Prevent compiler warning
            Dim totalEventHandlerTime As Long = 0
            For i As Integer = 0 To jobCount - 1
                Dim job = CreateJob(0, 100, ActiveObjectScenario.DoNothing, True, sleep)
                Dim jobInterrupted As Boolean = False
                AddHandler job.Done, Sub()
                                         Try
                                             Threading.Thread.Sleep(sleep * 2)
                                         Catch ex As ThreadInterruptedException
                                             jobInterrupted = True
                                         End Try
                                     End Sub

                ' Note that Invoke() does not start an event handler thread
                job.BeginInvoke()
                job.EndInvoke()
                Assert.IsTrue(jobInterrupted)
                totalEventHandlerTime += job.EventHandlerInvocationTime
            Next

            Dim meanEventHandlerTime = CInt(totalEventHandlerTime / jobCount)
            Dim minEventHandlerTime = sleep - invocationTimeTolerance
            Dim maxEventHandlerTime = sleep + invocationTimeTolerance

            Assert.IsTrue(minEventHandlerTime <= meanEventHandlerTime _
                          AndAlso meanEventHandlerTime <= maxEventHandlerTime)

        Next
    End Sub


    <TestMethod(), ExpectedException(GetType(JobNotStartedException))>
    Public Sub OnlyStartedJobsCanBeCanceled()
        Dim job = CreateJob(100, 100, ActiveObjectScenario.DoNothing)
        job.Cancel()
    End Sub

    <TestMethod(), ExpectedException(GetType(JobAlreadyCanceledException))>
    Public Sub JobsCanOnlyBeCanceledOnce()
        Dim job = CreateJob(100, 200, ActiveObjectScenario.DoNothing)
        job.BeginInvoke()
        job.Cancel()
        job.EndInvoke()
        job.Cancel()
    End Sub


    <TestMethod()>
    Public Sub TargetDelegatesCanHaveOnlyAFinally()
        '
        ' This is a test of the ThreadJob's ability to properly detect if the target delegate
        ' has an exception handler.
        '
        Dim job = CreateJobWithFinallyBlock(100, 200, ActiveObjectScenario.DoNothing)
        job.Invoke()
    End Sub

End Class


