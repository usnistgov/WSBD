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

Imports System.ServiceModel
Imports System.ServiceModel.Web
Imports System.Threading.Tasks
Imports BaselineSensorServiceTestsExtentionMethods

Imports Nist.Bcl.Wsbd


<TestClass()>
Public Class BaselineSensorServiceLockStealingTests
    Inherits BaselineSensorServiceTests

    <TestMethod(), TestCategory(StealLock), TestCategory(InvalidId)>
    Sub LockStealingWithAnUnregisteredIdGivesAnInvalidId()
        Dim client = CreateClient()
        Dim result = client.StealLock(Guid.NewGuid.ToString).WsbdResult
        Assert.AreEqual(Status.InvalidId, result.Status)
    End Sub

    <TestMethod(), TestCategory(StealLock), TestCategory(BadValue)>
    Sub LockStealingWithAnUnparseableIdGivesABadValue()
        Dim client = CreateClient()
        Dim result = client.StealLock("this_is_not_a_guid").WsbdResult
        Assert.AreEqual(Status.BadValue, result.Status)
    End Sub


    <TestMethod(), TestCategory(StealLock)>
    Sub TimeSinceStartOfLastSensorOperationIsAccurate()

        '' Test parameters
        '' 
        Dim activityCount As Integer = 10
        Dim inactivityTimes = {25, 100, 500} 'ms
        Dim inactivityTimeTolerance As Integer = 50 'ms (be careful using tolerances less than this)
        BaselineSensorService.InitializationTime = 1000 'ms

        ' 
        ' This is a test to see if 'TimeSinceStartOfLastSensorOperation' gives accurately reflects
        ' time that has passed between when a sensor operation is started, and when 'TimeSinceStartOfLastSensorOperation' 
        ' is called.
        '

        Dim client = CreateClient()
        Dim sessionId = client.Register.WsbdResult.SessionId.Value.ToString
        client.Lock(sessionId)

        Dim totalTimeSinceLastSensorOperation As Long
        For Each inactivityTime In inactivityTimes

            totalTimeSinceLastSensorOperation = 0

            For j As Integer = 0 To activityCount - 1

                ' Initialize the sensor
                Dim result As Result = client.Initialize(sessionId).WsbdResult

                ' Simulate some time passing by after initialization
                Threading.Thread.Sleep(inactivityTime)

                ' Get how much time has passed since we called .Init()
                Dim elapsedTime = BaselineSensorService.TimeSinceStartOfLastSensorOperation

                ' Make sure we performed initialization successfully; otherwise the timing will
                ' be completely incorrect.
                '
                Assert.AreEqual(Status.Success, result.Status)

                ' Accumulate the elapsed time
                totalTimeSinceLastSensorOperation += elapsedTime

            Next

            Dim meanTime = CInt(totalTimeSinceLastSensorOperation / activityCount)

            Dim expectedTime = BaselineSensorService.InitializationTime + inactivityTime
            Dim minTime = expectedTime - inactivityTimeTolerance
            Dim maxTime = expectedTime + inactivityTimeTolerance

            Assert.IsTrue(minTime <= meanTime AndAlso meanTime <= maxTime,
                          String.Format("Mean time of {0} does not fall within [{1},{2}]", meanTime, minTime, maxTime))

        Next

    End Sub

    <TestMethod(), TestCategory(StealLock)>
    Sub LockIsForbiddenIfNotEnoughTimeHasPassed()

        '' Test paramters
        ''
        Dim activityCount As Integer = 10
        Dim lockoutWindows = {500, 250, 100} 'ms

        '
        ' This is a test to make sure that the server correctly prevents lock stealing
        ' for the lockout window specified. Before any sensor operation is performed,
        ' lock stealing is permitted. Immediately after a sensor operation starts, lock
        ' stealing is forbidden, and then after the lock stealing window passes, lock
        ' stealing is permitted again.
        '

        Dim client = CreateClient()
        Dim sessionId = client.Register.WsbdResult.SessionId.Value.ToString
        client.Lock(sessionId)

        For Each lockoutWindow In lockoutWindows

            ' Using a lockout window of much less than 100 ms is not recommended
            lockoutWindow = Math.Max(100, lockoutWindow)

            ' Typically, the lockout window will be much longer than any sensor operation itself.
            ' However, the test will work either way. 
            '
            BaselineSensorService.InitializationTime = CInt(lockoutWindow / 2)
            BaselineSensorService.ForbidLockStealingWindow = lockoutWindow

            ' Before we start, make sure lock stealing is permitted.
            '
            Assert.IsTrue(BaselineSensorService.LockStealingPermitted)

            For j As Integer = 0 To activityCount - 1

                ' Initializing is a sensor operation. 
                Dim init As New Threading.Thread(Sub()
                                                     client.Initialize(sessionId)
                                                 End Sub)
                init.Start()

                ' Wait long enough for the sensor operation to 'kick in.' Without a wait
                ' here, the sensor operation may not have started, and the server will
                ' not have had time to start forbidding lock stealing
                '
                Threading.Thread.Sleep(50)

                ' Immediately after the operation, lock stealing should be forbidden
                Dim elapsed1 = BaselineSensorService.TimeSinceStartOfLastSensorOperation
                Assert.IsFalse(BaselineSensorService.LockStealingPermitted)

                ' Wait long enough for the lockout window to pass
                Threading.Thread.Sleep(lockoutWindow + 100)

                ' Now, lockout should be permitted
                Dim elapsed2 = BaselineSensorService.TimeSinceStartOfLastSensorOperation
                Assert.IsTrue(BaselineSensorService.LockStealingPermitted)

                ' Wait for initialization to complete before continuing
                init.Join()
            Next
        Next

    End Sub

    <TestMethod(), TestCategory(StealLock), TestCategory(Success)>
    Sub StealingALockNotHeldSucceeds()
        Dim client = CreateClient()
        Dim sessionId = client.Register.WsbdResult.SessionId.ToString
        Dim result = client.StealLock(sessionId).WsbdResult
        Assert.AreEqual(Status.Success, result.Status)
    End Sub

    <TestMethod(), TestCategory(StealLock), TestCategory(Success), TestCategory(Idempotent)>
    Sub StealingALockAlreadyHeldSucceeds()
        Dim client = CreateClient()
        Dim sessionId = client.Register.WsbdResult.SessionId.ToString
        client.Lock(sessionId)
        Dim result = client.StealLock(sessionId).WsbdResult
        Assert.AreEqual(Status.Success, result.Status)
    End Sub


    <TestMethod(), TestCategory(StealLock), TestCategory(Failure)>
    Sub StealingALockFromAnotherClientTooSoonFails()
        Dim client1 = CreateClient()
        Dim sessionId1 = client1.Register.WsbdResult.SessionId.ToString
        client1.Lock(sessionId1)
        client1.Initialize(sessionId1)

        Dim client2 = CreateClient()
        Dim sessionId2 = client2.Register.WsbdResult.SessionId.ToString
        Dim result = client2.StealLock(sessionId2).WsbdResult

        Assert.AreEqual(Status.Failure, result.Status)
    End Sub

    <TestMethod(), TestCategory(StealLock), TestCategory(Success), TestCategory(Failure)>
    Sub StealingALockFromAnotherClientCanSucceed()

        ' Test parameters
        Dim initTime = 500 ' ms (how long initialization should take)


        Dim lockStealingDelta = 100 'ms (how much extra time to wait to make sure the window passed)

        ' -- Begin test
        BaselineSensorService.InitializationTime = initTime
        Dim lockStealingWindow = initTime * 2
        BaselineSensorService.ForbidLockStealingWindow = lockStealingWindow


        ' Client 1 starts the initializaiton
        Dim client1 = CreateClient()
        Dim sessionId1 = client1.Register.WsbdResult.SessionId.ToString
        client1.Lock(sessionId1)
        client1.Initialize(sessionId1)

        ' Within the lock stealing window, lock stealing should fail
        Dim client2 = CreateClient()
        Dim sessionId2 = client2.Register.WsbdResult.SessionId.ToString
        Dim result = client2.StealLock(sessionId2).WsbdResult
        Assert.AreEqual(Status.Failure, result.Status)

        ' Cancelation should result in 'LockHeldByAnother'
        result = client2.Cancel(sessionId2).WsbdResult
        Assert.AreEqual(Status.LockHeldByAnother, result.Status)

        ' Wait for the lock stealing window to pass
        Threading.Thread.Sleep(lockStealingWindow + lockStealingDelta)

        ' Now, stealing should succeed
        result = client2.StealLock(sessionId2).WsbdResult
        Assert.AreEqual(Status.Success, result.Status)

    End Sub

    <TestMethod(), TestCategory(StealLock), TestCategory(Success)>
    Sub OneClientCanCancelAnotherClientsOperation()

        ' This is a test where client1 initializes, and before it is finished,
        ' another client, client2, steals the lock and then cancel the operation.
        ' For this to occur, the initialization time must be greater than the lock stealing time

        Dim lockStealingWindow = 1000 'ms (how long to forbid lock stealing)
        Dim lockStealingDelta = 100 'ms (how much extra time to wait to make sure the window is passed)

        ' -- Begin test
        BaselineSensorService.InitializationTime = 10 * lockStealingWindow
        BaselineSensorService.ForbidLockStealingWindow = lockStealingWindow

        ' Start a client that calls initialization
        Dim client1 = CreateClient()
        Dim sessionId1 = client1.Register.WsbdResult.SessionId.ToString
        Dim initResult As Result = Nothing
        Dim init As New Threading.Thread(Sub()
                                             client1.Lock(sessionId1)
                                             initResult = client1.Initialize(sessionId1).WsbdResult
                                         End Sub)
        init.Start()

        ' Wait long enough for the sensor operation to 'kick in.'
        '
        Threading.Thread.Sleep(100)

        Dim client2 = CreateClient()
        Dim sessionId2 = client2.Register.WsbdResult.SessionId.ToString

        ' Immediately after initialization, lock stealing should fail
        Dim result = client2.StealLock(sessionId2).WsbdResult
        Assert.AreEqual(Status.Failure, result.Status)

        ' Wiat for the lockout window to pass
        Threading.Thread.Sleep(lockStealingWindow + lockStealingDelta)

        ' There still should be no initialzation result yet.
        Assert.IsNull(initResult)

        ' Cancelation should still result in 'LockHeldByAnother'
        result = client2.Cancel(sessionId2).WsbdResult
        Assert.AreEqual(Status.LockHeldByAnother, result.Status)

        ' Lock stealing should now succeed
        result = client2.StealLock(sessionId2).WsbdResult
        Assert.AreEqual(Status.Success, result.Status)

        ' With the lock client2 can cancel client1's initiailzation
        Dim cancelResult = client2.Cancel(sessionId2).WsbdResult

        ' Make sure client1 is done 
        init.Join()

        ' Cancelation should succeed, the initialization will be Canceled
        Assert.AreEqual(Status.Canceled, initResult.Status)
        Assert.AreEqual(Status.Success, cancelResult.Status)




    End Sub

  
End Class


