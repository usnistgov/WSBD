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

Imports Nist.Bcl.Threading
Imports System.Threading.Tasks

<TestClass()>
Public Class SynchronousThreadJobTests

    Public Shared Function GetActiveObjectRunDelegate(ByVal ao As ActiveObject) As [Delegate]
        Return [Delegate].CreateDelegate(GetType(ActiveObject.RunDelegate), ao, "Run")
    End Function

    Public Const DefaultJobCount As Integer = 25

    Public Shared Function CreateJob(ByVal sleepTime As Integer, ByVal timeout As Integer, ByVal scenario As ActiveObjectScenario) As IJob
        Dim ao As New ActiveObject
        Dim factory As New ThreadJobFactory

        With factory
            .TargetDelegate = GetActiveObjectRunDelegate(ao)
            .TargetArgs = New Object() {Math.Max(0, sleepTime), scenario}
            .Timeout = timeout
        End With
        Dim job = factory.Create()
        Return job
    End Function


    <TestMethod()>
    Public Sub JobFactoryCorrectlyPropogatesParameters()

        Dim ao As New ActiveObject
        Dim factory As New ThreadJobFactory

        Dim sleepTime As Integer = 100
        Dim scenario As ActiveObjectScenario = ActiveObjectScenario.DoNothing
        Dim timeout As Integer = 200
        Dim eventHandlerWaitsForCompletion As Boolean = False
        Dim eventHandlerTimeout As Integer = 300

        With factory
            .TargetDelegate = GetActiveObjectRunDelegate(ao)
            .TargetArgs = New Object() {Math.Max(0, sleepTime), scenario}
            .Timeout = timeout
            .EndInvokeWaitsForEventHandlers = eventHandlerWaitsForCompletion
            .EndInvokeEventHandlerTimeout = eventHandlerTimeout
        End With
        Dim job = factory.Create()

        Assert.AreEqual(job.TargetDelegate, factory.TargetDelegate)
        Assert.AreEqual(job.Timeout, timeout)
        Assert.AreEqual(job.TargetArgs, factory.TargetArgs)
        Assert.AreEqual(job.EndInvokeWaitsForEventHandlers, eventHandlerWaitsForCompletion)
        Assert.AreEqual(job.EndInvokeEventHandlerTimeout, eventHandlerTimeout)


    End Sub

    <TestMethod()>
    Public Sub SuccessfulJobsHaveCorrectReturnValue()

        Dim sleepTimes = {1, 10, 100, 500}

        For Each sleepTime In sleepTimes
            Dim job = CreateJob(sleepTime, Math.Max(100, CInt(10 * sleepTime)), ActiveObjectScenario.DoNothing)
            job.Invoke()
            Dim failureMessage = String.Format(
                "Job.ReturnValue was {0} when {1} was expected; ActiveObject sleep time {2}",
                job.ReturnValue, ActiveObjectReturnState.Completed, sleepTime)
            Assert.AreEqual(ActiveObjectReturnState.Completed, job.ReturnValue, failureMessage)
        Next

    End Sub

    <TestMethod()>
    Public Sub SuccessfulJobsHaveCorrectInvocationTime()

        Dim sleepTimes = {1, 10, 100, 500}
        Dim averageInvocationTimeTolerance As Double = 50
        Dim jobCount As Integer = SynchronousThreadJobTests.DefaultJobCount

        For Each sleepTime In sleepTimes
            Dim timeoutTime = CInt(5 * sleepTime)
            Dim totalInvocationTime As Long = 0

            For i As Integer = 0 To jobCount - 1
                Dim job = CreateJob(sleepTime, timeoutTime, ActiveObjectScenario.DoNothing)
                job.Invoke()
                totalInvocationTime += job.TargetDelegateInvocationTime
            Next

            Dim averageInvocationTime As Double = totalInvocationTime / jobCount

            Dim lowerBound = Math.Max(0, sleepTime - averageInvocationTimeTolerance)
            Dim upperBound = sleepTime + averageInvocationTimeTolerance

            Dim failureMessage = String.Format(
                "Average invocation time: {0}, Expected range was {1}-{2}",
                averageInvocationTime, lowerBound, upperBound)
            Assert.IsTrue(lowerBound <= averageInvocationTime AndAlso averageInvocationTime <= upperBound, failureMessage)

        Next

    End Sub

    <TestMethod()>
    Public Sub TimedOutJobsHaveCorrectInvocationTime()

        Dim sleepTimes = {100, 500}
        Dim averageInvocationTimeTolerance As Double = 50
        Dim elapsedTimeTolerance As Double = 2
        Dim jobCount As Integer = SynchronousThreadJobTests.DefaultJobCount

        For Each sleepTime In sleepTimes
            Assert.IsTrue(sleepTime >= 100)
            Dim timeoutTime As Integer = CInt(sleepTime / 2)
            Dim totalInvocationTime As Long = 0

            For i As Integer = 0 To jobCount - 1
                Dim job = CreateJob(sleepTime, timeoutTime, ActiveObjectScenario.DoNothing)
                job.Invoke()
                totalInvocationTime += job.TargetDelegateInvocationTime

                Dim timeStarted As DateTime = job.TimeInvoked
                Dim timeDone As DateTime = job.TimeCompleted

                Dim elapsedTime = timeDone.Subtract(timeStarted).TotalMilliseconds
                Dim minElapsedTime As Double = job.TargetDelegateInvocationTime - elapsedTimeTolerance
                Dim maxElapsedTime As Double = job.TargetDelegateInvocationTime + elapsedTimeTolerance
                Assert.IsTrue(minElapsedTime <= elapsedTime AndAlso elapsedTime <= maxElapsedTime)
            Next

            Dim averageInvocationTime As Double = totalInvocationTime / jobCount

            Dim lowerBound = Math.Max(0, timeoutTime - averageInvocationTimeTolerance)
            Dim upperBound = timeoutTime + averageInvocationTimeTolerance

            Dim failureMessage = String.Format(
                "Average invocation time: {0}, Expected range was {1}-{2}",
                averageInvocationTime, lowerBound, upperBound)
            Assert.IsTrue(lowerBound < averageInvocationTime AndAlso
                          averageInvocationTime < upperBound, failureMessage)



        Next

    End Sub

    <TestMethod()>
    Public Sub TimedOutJobsAreFlaggedAsTimedOut()

        Dim sleepTimes = {100, 500}
        Dim jobCount As Integer = SynchronousThreadJobTests.DefaultJobCount

        For Each sleepTime In sleepTimes
            ' Don't use small sleep times for this test (the job may finish before the timeout test)
            Assert.IsTrue(sleepTime >= 100)
            For i As Integer = 0 To jobCount - 1
                Dim job = CreateJob(sleepTime, CInt(sleepTime / 2), ActiveObjectScenario.DoNothing)
                job.Invoke()
                Assert.IsTrue(job.WasTimedOut)
            Next

        Next

    End Sub

    <TestMethod()>
    Public Sub TimedOutActiveObjectsHaveCorrectReturnValue()

        Dim sleepTimes = {100, 500}
        Dim averageInvocationTimeTolerance As Double = 50
        Dim jobCount As Integer = SynchronousThreadJobTests.DefaultJobCount

        For Each sleepTime In sleepTimes
            Dim timeoutTime As Integer = CInt(sleepTime / 2)
            For i As Integer = 0 To jobCount - 1
                Dim job = CreateJob(sleepTime, timeoutTime, ActiveObjectScenario.DoNothing)
                job.Invoke()
                Assert.AreEqual(ActiveObjectReturnState.Interrupted, job.ReturnValue)
            Next
        Next

    End Sub

    <TestMethod()>
    Public Sub TestJobThatThrowsDivideByZeroException()
        Dim job = CreateJob(100, 200, ActiveObjectScenario.ThrowNullReferenceException)
        job.Invoke()
        Assert.IsTrue(TypeOf job.TargetException Is NullReferenceException)
    End Sub

    <TestMethod()>
    Public Sub TestJobsWithSimilarSleepAndTimeouts()
        ' TODO: This test should be refactored to support multiple sleep times

        Dim sleep As Integer = 100

        ' For accurate results, the time between timeout and sleep should be 20 ms or greater
        Dim sleepDelta As Integer = 20
        Dim sleepDeltas As Integer() = {-sleepDelta, sleepDelta}
        Dim timeoutCounts(3) As Integer

        ' A smaller sleep delta will require a larger count tolerance
        Dim countTolerance = 0.05

        For i As Integer = 0 To sleepDeltas.Count - 1

            Dim timeoutCount As Integer = 0
            Dim timeout = sleep + sleepDeltas(i)

            For j As Integer = 0 To DefaultJobCount - 1

                Dim job = CreateJob(sleep, timeout, ActiveObjectScenario.DoNothing)
                job.Invoke()
                If job.WasTimedOut Then
                    timeoutCount += 1
                End If
            Next
            timeoutCounts(i) = timeoutCount
        Next

        ' TimeoutsCount(0) holds the number of timeouts when the timeout is less than sleep time
        Dim mininumNumberOfTimeouts = Math.Floor(DefaultJobCount * (1.0 - countTolerance))
        Assert.IsTrue(timeoutCounts(0) > mininumNumberOfTimeouts,
                      "Not enough timeouts. Expected {0} or more, but only recieved {1}",
                      mininumNumberOfTimeouts, timeoutCounts(0))

        ' TimeoutsCount(2) holds the number of timeouts when the timeout is greater than sleep time
        Dim maximumNumberOfTimeouts = Math.Ceiling(DefaultJobCount * countTolerance)
        Assert.IsTrue(timeoutCounts(1) < maximumNumberOfTimeouts,
                      "Too many timeouts. Expected {0} or less, but recieved {1}",
                      maximumNumberOfTimeouts, timeoutCounts(2))

    End Sub


    <TestMethod(), ExpectedException(GetType(JobAlreadyStartedException))>
    Public Sub JobsShouldOnlyBeInvokedOnce()
        Dim job = CreateJob(10, 10, ActiveObjectScenario.DoNothing)
        job.Invoke()
        job.Invoke()
    End Sub

    <TestMethod()>
    Public Sub ThreadJobDispose()
        Using job = CreateJob(10, 10, ActiveObjectScenario.DoNothing)
            job.Invoke()
        End Using
    End Sub





End Class

