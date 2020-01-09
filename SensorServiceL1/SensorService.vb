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

Imports System.ComponentModel
Imports System.ServiceModel
Imports System.Timers
Imports System.Threading

Imports Nist.Bcl.Threading

' Internally, this class uses the interfaces for certain private members. We make an aliases here
' in case we want to refactor the implmentation later. (I.e., this is a cheap form of dependency injection.)

Imports JobFactory = Nist.Bcl.Threading.ThreadJobFactory
'Imports StorageProvider = Nist.Bcl.Wsbd.FileStorageProvider

Namespace Nist.Bcl.Wsbd

    <ServiceBehavior(InstanceContextMode:=InstanceContextMode.PerCall)>
    Public MustInherit Class SensorService
        Implements ISensorService

        Public Sub New()
            RestartStagnantSessionDetectionTimer()
        End Sub

#Region "  Activity Logging  "

        Private Shared smActiveSessions As New Dictionary(Of Guid, DateTime)
        Private Shared smActiveSessionsLock As New ReaderWriterLockSlim

        Protected Shared Sub ResetActiveSessions()
            smActiveSessionsLock.EnterWriteLock()
            smActiveSessions.Clear()
            smActiveSessionsLock.ExitWriteLock()
        End Sub

        Private Shared Sub LogSessionActivity(ByVal sessionId As Guid, ByVal timeStamp As DateTime)
            smActiveSessionsLock.EnterWriteLock()
            If Not smActiveSessions.ContainsKey(sessionId) Then
                smActiveSessions.Add(sessionId, timeStamp)
            Else
                smActiveSessions(sessionId) = timeStamp
            End If
            smActiveSessionsLock.ExitWriteLock()
        End Sub

        Private Shared Sub RemoveStaleSessions(ByVal now As DateTime)

            smActiveSessionsLock.EnterUpgradeableReadLock()

            ' Get the list of stale sessions
            Dim inactivePairs = From pair In smActiveSessions Select pair
                             Where Not IsActive(pair.Key, now)

            ' Remove them from the current session list
            smActiveSessionsLock.EnterWriteLock()
            For Each pair In inactivePairs.ToArray
                smActiveSessions.Remove(pair.Key)
            Next
            smActiveSessionsLock.ExitWriteLock()

            smActiveSessionsLock.ExitUpgradeableReadLock()

        End Sub

        Private Shared Sub RemoveLeastRecentlyUsedSession()
            smActiveSessionsLock.EnterUpgradeableReadLock()

            Dim lru = From pair In smActiveSessions Order By pair.Value Ascending Select pair Take 1

            For Each pair In lru.ToArray()
                RemoveSession(pair.Key)
            Next

            smActiveSessionsLock.ExitUpgradeableReadLock()
        End Sub

        Private Shared Sub RemoveSession(ByVal id As Guid)
            smActiveSessionsLock.EnterUpgradeableReadLock()

            If smActiveSessions.ContainsKey(id) Then
                smActiveSessionsLock.EnterWriteLock()
                smActiveSessions.Remove(id)
                smActiveSessionsLock.ExitWriteLock()
            End If

            smActiveSessionsLock.ExitUpgradeableReadLock()
        End Sub

        Private Shared Function IsActive(ByVal sessionId As Guid, ByVal now As DateTime) As Boolean
            Return TimeSpan.Compare(now.Subtract(smActiveSessions(sessionId)), New TimeSpan(0, 0, CInt(smServiceInfoDictionary.InactivityTimeout))) <= 0
        End Function


        Private Shared Sub StaleSessionDetectionHandler(ByVal sender As Object, ByVal e As EventArgs) _
            Handles smStagnantSessionDetectionTimer.Elapsed
            RemoveStaleSessions(DateTime.Now)
        End Sub


        Public Shared Function IsActive(ByVal sessionId As Guid) As Boolean
            Dim stale As Boolean
            smActiveSessionsLock.EnterReadLock()
            stale = smActiveSessions.ContainsKey(sessionId)
            smActiveSessionsLock.ExitReadLock()
            Return stale
        End Function

        Private Shared smStagnantSessionLock As New Object

        Private Shared WithEvents smStagnantSessionDetectionTimer As New Timers.Timer

        Protected Shared Sub ResetStagnantSessionDetection()
            SyncLock smStagnantSessionLock
                smStagnantSessionDetectionTimer = New Timers.Timer
            End SyncLock
        End Sub

        Private Shared Sub UnsafeRestartStagnantSessionDetectionTimer()
            smStagnantSessionDetectionTimer.Stop()
            smStagnantSessionDetectionTimer.AutoReset = True
            smStagnantSessionDetectionTimer.Start()
        End Sub

        Public Shared Sub RestartStagnantSessionDetectionTimer()
            SyncLock smStagnantSessionLock
                UnsafeRestartStagnantSessionDetectionTimer()
            End SyncLock
        End Sub

        Public Shared Property StagnantSessionDetectionRefreshRate As Integer
            Get
                SyncLock smStagnantSessionLock
                    Return CInt(smStagnantSessionDetectionTimer.Interval)
                End SyncLock
            End Get
            Set(ByVal value As Integer)
                SyncLock smStagnantSessionLock
                    smStagnantSessionDetectionTimer.Interval = value
                End SyncLock
            End Set
        End Property

#End Region

#Region "  Registration  "
        Private Shared smRegisteredSessions As New SortedSet(Of Guid)
        Private Shared smRegistrationLock As New ReaderWriterLockSlim

        Private Const DefaultMaximumAutomaticRegistrationRetry As Integer = 2

        Public Shared Property MaximumAutomaticRegistrationRetry As Integer = DefaultMaximumAutomaticRegistrationRetry

        Protected Shared Sub ResetRegisteredSessions()
            smRegistrationLock.EnterWriteLock()
            smRegisteredSessions.Clear()
            smRegistrationLock.ExitWriteLock()
        End Sub


        Public Function Register() As Result Implements ISensorService.Register
            ' It is extremely unlikely that we will not be able to create a unique GUID, but
            ' we protect against it anyway.

            Dim sessionId As Guid
            Dim guidGenerationCount As Integer = 0
            Dim generationFailed As Boolean

            ' If we are already at or above the maximum number of concurrent sessions
            ' then will will not be able to add another.
            '
            Dim tooManySessions As Result = Nothing
            smRegistrationLock.EnterReadLock()
            If smRegisteredSessions.Count >= smServiceInfoDictionary.MaximumConcurrentSessions Then
                If smServiceInfoDictionary.LeastRecentlyUsedSessionsAutomaticallyDropped Then
                    RemoveLeastRecentlyUsedSession()
                Else
                    tooManySessions = New Result
                    tooManySessions.Status = Status.Failure
                    tooManySessions.Message = My.Resources.MaximumConcurrentSessionsExceeded
                End If
            End If
            smRegistrationLock.ExitReadLock()
            If tooManySessions IsNot Nothing Then Return tooManySessions

            ' Generate a new ID and add it to the list of registered sessions. It's extremely
            ' unlikely, but it's possible that a unique ID cannot be generated, so we retry
            ' in order to prevent that.
            '
            smRegistrationLock.EnterWriteLock()
            Do
                sessionId = GenerateSessionId()
                guidGenerationCount += 1
                generationFailed = guidGenerationCount > MaximumAutomaticRegistrationRetry
            Loop While smRegisteredSessions.Contains(sessionId) AndAlso Not generationFailed
            If Not generationFailed Then
                smRegisteredSessions.Add(sessionId)
            End If
            smRegistrationLock.ExitWriteLock()


            Dim result As New Result

            If Not generationFailed Then
                LogSessionActivity(sessionId, DateTime.Now)
                result.Status = Status.Success
                result.SessionId = sessionId
            Else
                result.Status = Status.Failure
                result.Message = My.Resources.CouldNotGenerateUniqueSessionId
            End If

            Return result

        End Function

        Protected Overridable Function GenerateSessionId() As Guid
            Return Guid.NewGuid
        End Function


        Public Overridable Function Unregister(ByVal userInputSessionId As String) As Result Implements ISensorService.Unregister

            ' Verify that the user has given us a valid id.
            '
            Dim sessionId As Guid
            Dim parseError As Result = ValidateIdIsParsable(userInputSessionId, Constants.SessionIdParameterName,
                                                      sessionId)
            If parseError IsNot Nothing Then Return parseError

            ' Even though we might delete this session from the list of active sessions, we log
            ' the activity in case there is some instrumentation or logging facilities inside
            ' LogActivity() that might do something useful.
            '
            LogSessionActivity(sessionId, DateTime.Now)

            If HasLock(sessionId) Then

                ' If we have the lock, then we release it.
                smSensorOwnerLock.EnterWriteLock()
                smSensorOwner = Nothing
                smSensorOwnerLock.ExitWriteLock()

            End If

            smRegistrationLock.EnterUpgradeableReadLock()
            If smRegisteredSessions.Contains(sessionId) Then

                ' First, we remove the id from the list of active sessions
                RemoveSession(sessionId)

                ' Remove the id from the current list of registered sessions
                smRegistrationLock.EnterWriteLock()
                smRegisteredSessions.Remove(sessionId)
                smRegistrationLock.ExitWriteLock()
            End If
            smRegistrationLock.ExitUpgradeableReadLock()

            Dim result As New Result(Status.Success)
            Return result

        End Function

        Public Shared Function IsRegistered(ByVal sessionId As Guid) As Boolean
            Dim registered As Boolean = False
            smRegistrationLock.EnterReadLock()
            registered = smRegisteredSessions.Contains(sessionId)
            smRegistrationLock.ExitReadLock()
            Return registered
        End Function


#End Region

#Region "  Locking  "

        Private Shared smSensorOwner As Guid?
        Private Shared smSensorOwnerLock As New ReaderWriterLockSlim

        Public Function Lock(ByVal userInputSessionId As String) As Result Implements ISensorService.Lock

            Dim sessionId As Guid
            Dim parseError As Result _
                = ValidateIdIsParsable(userInputSessionId, Constants.SessionIdParameterName, sessionId)
            If parseError IsNot Nothing Then Return parseError

            Dim unregisteredError As Result _
                = ValidateRegistration(sessionId)
            If unregisteredError IsNot Nothing Then Return unregisteredError

            LogSessionActivity(sessionId, DateTime.Now)

            Dim result As New Result

            smSensorOwnerLock.EnterUpgradeableReadLock()
            If smSensorOwner IsNot Nothing Then
                ' If there is an owner...
                If smSensorOwner.Value.Equals(sessionId) Then
                    ' ... and that owner is already that session, then idempotency 
                    ' dictates that the lock succeeds
                    result.Status = Status.Success
                Else
                    ' ... and that owner is not that session, then the lock is held by another
                    result.Status = Status.LockHeldByAnother
                End If

            Else

                ' If there is no owner, then the session can take ownership of the lock
                smSensorOwnerLock.EnterWriteLock()
                smSensorOwner = sessionId
                smSensorOwnerLock.ExitWriteLock()
                result.Status = Status.Success
            End If
            smSensorOwnerLock.ExitUpgradeableReadLock()

            Return result
        End Function

        Public Function StealLock(ByVal userInputSessionId As String) As Result Implements ISensorService.StealLock

            Dim sessionId As Guid
            Dim parseError As Result _
                = ValidateIdIsParsable(userInputSessionId, Constants.SessionIdParameterName, sessionId)
            If parseError IsNot Nothing Then Return parseError

            Dim unregisteredError As Result _
                = ValidateRegistration(sessionId)
            If unregisteredError IsNot Nothing Then Return unregisteredError

            LogSessionActivity(sessionId, DateTime.Now)

            Dim result As New Result
            smSensorOwnerLock.EnterUpgradeableReadLock()


            If smSensorOwner IsNot Nothing Then

                ' If there is a sensor owner ...
                If smSensorOwner.Value.Equals(sessionId) Then

                    ' ... and that owner is already that session, then a session that steals the 
                    ' lock from itself will just succeed.
                    '
                    result.Status = Status.Success
                Else
                    ' ... and that owner is not that session, then the lock is held by another.

                    If LockStealingPermitted Then

                        ' If lock stealing is permitted, then go ahead and steal the lock.
                        smSensorOwnerLock.EnterWriteLock()
                        smSensorOwner = sessionId
                        smSensorOwnerLock.ExitWriteLock()
                        result.Status = Status.Success

                    Else
                        ' Without lock stealing, the attempt fails.
                        result.Status = Status.Failure
                    End If

                End If

            Else
                ' If there is no owner, then the session can 'steal' the lock from no one
                smSensorOwnerLock.EnterWriteLock()
                smSensorOwner = sessionId
                smSensorOwnerLock.ExitWriteLock()
                result.Status = Status.Success
            End If
            smSensorOwnerLock.ExitUpgradeableReadLock()

            Return result

        End Function

        Public Shared ReadOnly Property LockStealingPermitted As Boolean
            Get
                Dim stealingPermitted As Boolean
                smSensorJobLock.EnterReadLock()
                ' If there is no stopwatch, there has been no sensor job, so stealing is okay.
                stealingPermitted = smSensorJobActivityStopwatch Is Nothing
                If smSensorJobActivityStopwatch IsNot Nothing Then
                    Dim elapsed = smSensorJobActivityStopwatch.ElapsedMilliseconds
                    stealingPermitted = elapsed >= smForbidLockStealingWindow
                End If
                smSensorJobLock.ExitReadLock()
                Return stealingPermitted
            End Get
        End Property



        Public Function Unlock(ByVal userInputSessionId As String) As Result Implements ISensorService.Unlock

            Dim sessionId As Guid
            Dim parseError As Result _
                = ValidateIdIsParsable(userInputSessionId, Constants.SessionIdParameterName, sessionId)
            If parseError IsNot Nothing Then Return parseError

            Dim unregisteredError As Result _
             = ValidateRegistration(sessionId)
            If unregisteredError IsNot Nothing Then Return unregisteredError

            LogSessionActivity(sessionId, DateTime.Now)

            Dim result As New Result
            smSensorOwnerLock.EnterUpgradeableReadLock()
            If smSensorOwner IsNot Nothing Then

                If smSensorOwner.Value.Equals(sessionId) Then
                    result.Status = Status.Success
                    smSensorOwnerLock.EnterWriteLock()
                    smSensorOwner = Nothing
                    smSensorOwnerLock.ExitWriteLock()
                Else
                    result.Status = Status.LockHeldByAnother
                End If

            Else
                ' If the sensor owner is not locked, then idempotency dictates that 
                ' unlock should succeed
                '
                result.Status = Status.Success

            End If
            smSensorOwnerLock.ExitUpgradeableReadLock()

            Return result

        End Function

        Protected Shared Sub ResetLock()
            smSensorOwnerLock.EnterWriteLock()
            smSensorOwner = Nothing
            smSensorOwnerLock.ExitWriteLock()
        End Sub

        Public Shared Function ValidateLock(ByVal sessionId As Guid) As Result

            Dim result As Result = Nothing
            Dim sensorHasOwner As Boolean = False
            Dim sensorOwnerIsSession As Boolean = False

            smSensorOwnerLock.EnterReadLock()
            sensorHasOwner = smSensorOwner IsNot Nothing
            sensorOwnerIsSession = sensorHasOwner AndAlso smSensorOwner.Equals(sessionId)
            smSensorOwnerLock.ExitReadLock()

            If Not sensorOwnerIsSession Then
                ' If the owner is not this session...
                result = New Result

                If sensorHasOwner Then
                    ' ... but it has an owner, then the lock is held be another.
                    result.Status = Status.LockHeldByAnother
                Else
                    ' ... and it has no owner, then the lock is not held.
                    result.Status = Status.LockNotHeld
                End If

            End If

            Return result
        End Function

        Public Shared Function HasLock(ByVal sessionId As Guid) As Boolean
            Dim result As Boolean
            smSensorOwnerLock.EnterReadLock()
            result = smSensorOwner.Equals(sessionId)
            smSensorOwnerLock.ExitReadLock()
            Return result
        End Function


#End Region

        Private Delegate Function ImplementationDelegate() As Result
        Private Delegate Function ImplementationDelegate(Of T)(ByVal arg As T) As Result


        Private Function CreateImplementationDelegate(ByVal methodName As String) As System.Delegate
            Return [Delegate].CreateDelegate(GetType(ImplementationDelegate), Me, methodName)
        End Function

        Private Function CreateImplementationDelegate(Of T)(ByVal methodName As String) As System.Delegate
            Return [Delegate].CreateDelegate(GetType(ImplementationDelegate(Of T)), Me, methodName)
        End Function



#Region " Initialization "

        Private Shared smInitializationJobFactory As IJobFactory
        Protected Overridable ReadOnly Property InitializationJobFactory As IJobFactory
            Get
                If smInitializationJobFactory Is Nothing Then
                    smInitializationJobFactory = New JobFactory
                    smInitializationJobFactory.TargetDelegate = CreateImplementationDelegate("Initialize")
                End If

                smInitializationJobFactory.Timeout = smServiceInfoDictionary.InitializationTimeout
                Return smInitializationJobFactory
            End Get
        End Property

        Public Function InitializeWrapper(ByVal userInputSessionId As String) As Result Implements ISensorService.Initialize
            Return RunSensorJob(userInputSessionId, InitializationJobFactory)
        End Function

        Protected MustOverride Function Initialize() As Result

#End Region

#Region "  Info  "


        Public MustOverride Function GetServiceInfo() As Result
        Public Function GetServiceInfoWrapper() As Result Implements ISensorService.GetServiceInfo
            Return GetServiceInfo()
        End Function

        Protected Shared smServiceInfoDictionaryLock As New ReaderWriterLockSlim
        Protected Shared smServiceInfoDictionary As New ServiceInformation

        Public Shared ReadOnly Property ServiceInfo As ServiceInformation
            Get
                Return smServiceInfoDictionary
            End Get
        End Property

        Public Shared Sub AddToServiceInfoDictionary(ByVal key As String, ByVal value As Parameter)
            smServiceInfoDictionaryLock.EnterWriteLock()
            smServiceInfoDictionary.Add(key, value)
            smServiceInfoDictionaryLock.ExitWriteLock()
        End Sub

        Public Shared Sub ResetServiceInfo()
            smServiceInfoDictionaryLock.EnterWriteLock()
            smServiceInfoDictionary.Clear()
            smServiceInfoDictionary.RestoreDefaults()  '*******************
            smServiceInfoDictionaryLock.ExitWriteLock()
        End Sub


#End Region

#Region " GetConfiguration "

        Private Shared smGetConfigurationJobFactory As IJobFactory
        Protected Overridable ReadOnly Property GetConfigurationJobFactory As IJobFactory
            Get
                If smGetConfigurationJobFactory Is Nothing Then
                    smGetConfigurationJobFactory = New JobFactory
                    smGetConfigurationJobFactory.TargetDelegate = CreateImplementationDelegate("GetConfiguration")
                    smGetConfigurationJobFactory.Timeout = smServiceInfoDictionary.GetConfigurationTimeout
                End If
                Return smGetConfigurationJobFactory
            End Get
        End Property

        Public Function GetConfigurationWrapper(ByVal userInputSessionId As String) As Result Implements ISensorService.GetConfiguration
            Return RunSensorJob(userInputSessionId, GetConfigurationJobFactory)
        End Function

        Protected MustOverride Function GetConfiguration() As Result

#End Region

#Region " SetConfiguration "

        Private Shared smSetConfigurationJobFactory As IJobFactory
        Protected Overridable ReadOnly Property SetConfigurationJobFactory As IJobFactory
            Get
                If smSetConfigurationJobFactory Is Nothing Then
                    smSetConfigurationJobFactory = New JobFactory
                    smSetConfigurationJobFactory.TargetDelegate = CreateImplementationDelegate(Of Configuration)("SetConfiguration")
                    smSetConfigurationJobFactory.Timeout = smServiceInfoDictionary.SetConfigurationTimeout
                End If
                Return smSetConfigurationJobFactory
            End Get
        End Property

        Private smConfigurationArgLock As New ReaderWriterLockSlim
        Public Function SetConfigurationWrapper(ByVal userInputSessionId As String, ByVal configuration As Configuration) As Result _
            Implements ISensorService.SetConfiguration


            ' We manually push a copy of the configuration into the job factory, so that the configuration
            ' implementation will get a copy of the arguments.
            '
            smConfigurationArgLock.EnterWriteLock()
            SetConfigurationJobFactory.TargetArgs = New Object() {configuration.DeepCopy}
            smConfigurationArgLock.ExitWriteLock()

            Return RunSensorJob(userInputSessionId, SetConfigurationJobFactory)
        End Function

        Protected MustOverride Function SetConfiguration(ByVal configuration As Configuration) As Result

#End Region

#Region " Capture "

        Private Shared smCaptureJobFactory As IJobFactory
        Protected Overridable ReadOnly Property CaptureJobFactory As IJobFactory
            Get
                If smCaptureJobFactory Is Nothing Then
                    smCaptureJobFactory = New JobFactory
                    smCaptureJobFactory.TargetDelegate = CreateImplementationDelegate("Capture")
                    smCaptureJobFactory.Timeout = smServiceInfoDictionary.CaptureTimeout
                End If
                Return smCaptureJobFactory
            End Get
        End Property

        Public Function CaptureWrapper(ByVal userInputSessionId As String) As Result Implements ISensorService.Capture
            Return RunSensorJob(userInputSessionId, CaptureJobFactory)
        End Function

        Protected MustOverride Function Capture() As Result

#End Region

#Region " Common Support Methods  "

        Public Shared Function ValidateIdIsParsable(ByVal originalId As String, ByVal fieldName As String,
                                          ByRef newId As Guid) As Result

            Dim validGuid = Guid.TryParse(originalId, newId)
            Dim errorResult As Result = Nothing
            If Not validGuid Then
                errorResult = New Result
                errorResult.Status = Status.BadValue
                errorResult.BadFields = New StringArray
                errorResult.BadFields.Add(fieldName)
            End If
            Return errorResult
        End Function

        Public Shared Function ValidateRegistration(ByVal sessionId As Guid) As Result
            Dim errorResult As Result = Nothing

            If Not IsRegistered(sessionId) Then
                errorResult = New Result
                errorResult.Status = Status.InvalidId
                errorResult.Message = String.Format(My.Resources.SessionNotRegistered, sessionId.ToString)
            End If

            Return errorResult
        End Function



        Public Shared Function ValidateIntegerIsParsable(ByVal originalValue As String, ByVal fieldName As String,
                                                         ByRef newValue As Integer) As Result
            Dim validInteger = Integer.TryParse(originalValue, newValue)
            Dim errorResult As Result = Nothing
            If Not validInteger Then
                errorResult = New Result
                errorResult.Status = Status.BadValue
                errorResult.BadFields = New StringArray
                errorResult.BadFields.Add(fieldName)
            End If
            Return errorResult

        End Function


        Public Shared Function CombineBadValueResults(ByVal ParamArray results() As Result) As Result
            Dim combined As Result = Nothing

            For Each r In results

                If r Is Nothing Then Continue For

                If r.Status <> Status.BadValue Then Return Nothing

                If combined Is Nothing Then
                    combined = New Result(Status.BadValue)
                End If

                If combined.BadFields Is Nothing Then
                    combined.BadFields = New StringArray
                End If

                ' Add all of the bad fields into the combined result
                If r.BadFields IsNot Nothing Then
                    For Each f In r.BadFields
                        combined.BadFields.Add(f)
                    Next
                End If

            Next
            Return combined
        End Function

#End Region

#Region "  Sensor Operation  "

        Private Shared smSensorJob As IJob
        Private Shared smSensorJobActivityStopwatch As Stopwatch

        ' Determines how long to forbid lock stealing after a sensor operation has started (milliseconds)
        Public Const DefaultForbidLockStealingWindow As Long = 60000
        Private Shared smForbidLockStealingWindow As Long = DefaultForbidLockStealingWindow

        Private Shared smSensorJobLock As New ReaderWriterLockSlim

        Protected Shared Function RunSensorJob(ByVal userInputSessionId As String,
                                               ByVal factory As IJobFactory) As Result

            Dim result As New Result(Status.Failure)

            Dim sessionId As Guid
            Dim parseError As Result _
                = ValidateIdIsParsable(userInputSessionId, Constants.SessionIdParameterName, sessionId)
            If parseError IsNot Nothing Then Return parseError

            Dim unregisteredError As Result = ValidateRegistration(sessionId)
            If unregisteredError IsNot Nothing Then Return unregisteredError

            Dim lockError As Result = ValidateLock(sessionId)
            If lockError IsNot Nothing Then Return lockError


            LogSessionActivity(sessionId, DateTime.Now)

            Dim busy = IsBusy()

            If busy Then
                result.Status = Status.SensorBusy
            Else
                smSensorJobLock.EnterUpgradeableReadLock()

                ' To create the job and reset the job stopwatch, we grab a write lock
                smSensorJobLock.EnterWriteLock()

                ' Create the job
                smSensorJob = factory.Create()

                ' Create and start the activity stopwatch, otherwise, just restart it
                If smSensorJobActivityStopwatch Is Nothing Then
                    smSensorJobActivityStopwatch = Stopwatch.StartNew()
                Else
                    smSensorJobActivityStopwatch.Restart()
                End If

                ' Exit the write region. We have to do this so that calls to 'Busy'
                ' will not block.
                smSensorJobLock.ExitWriteLock()

                ' Invoke the actual sensor job
                smSensorJob.Invoke()

                smSensorJobLock.EnterWriteLock()
                If smSensorJob.TargetException IsNot Nothing Then
                    ' If the job ran with an exception, then return a failure
                    result = New Result(Status.Failure)
                    result.Message = smSensorJob.TargetException.ToString
                Else
                    ' Otherwise, return the result
                    result = DirectCast(smSensorJob.ReturnValue, Result)
                End If
                smSensorJob.Dispose()
                smSensorJob = Nothing
                smSensorJobLock.ExitWriteLock()

                smSensorJobLock.ExitUpgradeableReadLock()
            End If

            Return result
        End Function


        Public Function Cancel(ByVal userInputSessionId As String) As Result Implements ISensorService.Cancel

            Dim sessionId As Guid

            Dim parseError As Result _
                = ValidateIdIsParsable(userInputSessionId, Constants.SessionIdParameterName, sessionId)
            If parseError IsNot Nothing Then Return parseError

            Dim unregisteredError As Result = ValidateRegistration(sessionId)
            If unregisteredError IsNot Nothing Then Return unregisteredError

            Dim lockError As Result = ValidateLock(sessionId)
            If lockError IsNot Nothing Then Return lockError

            LogSessionActivity(sessionId, DateTime.Now)

            ' If the service is not busy, then there is nothing to cancel.
            ' Report out a successful cancelation.
            '
            If Not IsBusy() Then Return New Result(Status.Success)

            Dim jobCanceled As Boolean

            smSensorJobLock.EnterReadLock()
            Try
                smSensorJob.Cancel()
            Catch jacEx As JobAlreadyCanceledException
                jobCanceled = True
            Catch ex As JobNotStartedException
                jobCanceled = True
            End Try

            If Not jobCanceled Then
                jobCanceled = smSensorJob.WasCanceled
            End If

            smSensorJobLock.ExitReadLock()

            Dim result As Result
            If jobCanceled Then
                result = New Result(Status.Success)
            Else
                Dim failure As New Result(Status.SensorFailure)
                If smSensorJob.TargetException IsNot Nothing Then
                    failure.Message = smSensorJob.TargetException.ToString
                End If
                result = failure
            End If

            Return result
        End Function

        Public Shared WriteOnly Property ForbidLockStealingWindow As Long
            Set(ByVal value As Long)
                smSensorJobLock.EnterWriteLock()
                smForbidLockStealingWindow = value
                smSensorJobLock.ExitWriteLock()
            End Set
        End Property

        Public Shared Function TimeSinceStartOfLastSensorOperation() As Long
            Dim elapsedTimeInSeconds As Long
            smSensorJobLock.EnterReadLock()
            If smSensorJobActivityStopwatch Is Nothing Then
                elapsedTimeInSeconds = -1
            Else
                elapsedTimeInSeconds = smSensorJobActivityStopwatch.ElapsedMilliseconds
            End If
            smSensorJobLock.ExitReadLock()
            Return elapsedTimeInSeconds
        End Function


        Protected Shared Sub ResetSensorJob()
            smSensorJobLock.EnterWriteLock()

            If smSensorJob IsNot Nothing Then
                smSensorJob.Dispose()
                smSensorJob = Nothing
            End If

            smForbidLockStealingWindow = DefaultForbidLockStealingWindow
            smSensorJobActivityStopwatch = Nothing
            smSensorJobLock.ExitWriteLock()
        End Sub

        Protected Shared Function IsBusy() As Boolean
            Dim busy As Boolean
            smSensorJobLock.EnterReadLock()
            busy = smSensorJob IsNot Nothing
            smSensorJobLock.ExitReadLock()
            Return busy
        End Function

#End Region

#Region " Download "

        Public MustOverride Function Download(ByVal captureId As Guid) As Result
        Public Function DownloadWrapper(ByVal userInputCaptureId As String) As Result Implements ISensorService.Download

            Dim captureId As Guid
            Dim parseError As Result _
                    = ValidateIdIsParsable(userInputCaptureId, Constants.SessionIdParameterName, captureId)
            If parseError IsNot Nothing Then Return parseError

            Return Download(captureId)
        End Function

        Public MustOverride Function ThriftyDownload(ByVal captureId As Guid, ByVal maxSize As Integer) As Result
        Public Overridable Function ThriftyDownloadWrapper(ByVal userInputCaptureId As String, ByVal userInputMaxSize As String) As Result Implements ISensorService.ThriftyDownload

            Dim captureId As Guid
            Dim idParseError As Result _
                    = ValidateIdIsParsable(userInputCaptureId, Constants.CaptureIdParameterName, captureId)

            Dim maxSize As Integer
            Dim maxSizeParseError As Result _
                = ValidateIntegerIsParsable(userInputMaxSize, Constants.MaxSizeParameterName, maxSize)


            Dim parseError = CombineBadValueResults(idParseError, maxSizeParseError)
            If parseError IsNot Nothing Then Return parseError

            Return ThriftyDownload(captureId, maxSize)

        End Function

        Public MustOverride Function GetDownloadInfo(ByVal captureId As Guid) As Result
        Public Overridable Function GetDownloadInfoWrapper(ByVal userInputCaptureId As String) As Result Implements ISensorService.GetDownloadInfo

            Dim captureId As Guid
            Dim idParseError As Result = ValidateIdIsParsable(userInputCaptureId, Constants.CaptureIdParameterName, captureId)

            Dim parseError = CombineBadValueResults(idParseError)
            If parseError IsNot Nothing Then Return parseError

            Return GetDownloadInfo(captureId)
        End Function

#End Region

    End Class

End Namespace
