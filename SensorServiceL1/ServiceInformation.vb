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

Imports Nist.Bcl.Wsbd

<Serializable()>
Partial Public NotInheritable Class ServiceInformation
    Inherits Configuration

    Public Sub New()
    End Sub

    'Private Shared serviceConfiguration As New ServiceInformation()

    'Public Shared Function GetInstance() As ServiceInformation
    '    Return serviceConfiguration
    'End Function

    Private Const LastUpdatedKey As String = "lastUpdated"
    Private Const LastUpdatedDefault As DateTime = Nothing

    Private Const InactivityTimeoutKey As String = "inactivityTimeout"

    Private Const InactivityTimeoutDefault As Int64 = 60 * 60    '1 hour

    Private Const MaximumConcurrentSessionsKey As String = "maximumConcurrentSessions"
    Private Const MaximumConcurrentSessionsDefault As Integer = 256

    Private Const LeastRecentlyUsedSessionsAutomaticallyDroppedKey As String = "autoDropLRUSessions"
    Private Const LeastRecentlyUsedSessionsAutomaticallyDroppedDefault As Boolean = True

    Private Const InitializationTimeoutKey As String = "initializationTimeout"
    Public Const InitializationTimeoutDefault As Integer = 30 * 1000

    Private Const GetConfigurationTimeoutKey As String = "getConfigurationTimeout"
    Private Const GetConfigurationTimeoutDefault As Integer = 30 * 1000   '30 seconds

    Private Const SetConfigurationTimeoutKey As String = "setConfigurationTimeout"
    Private Const SetConfigurationTimeoutDefault As Integer = 30 * 1000   '30 seconds

    Private Const CaptureTimeoutKey As String = "captureTimeout"
    Private Const CaptureTimeoutDefault As Integer = 30 * 1000    '30 seconds

    Private Const PostAcquisitionProcessingTimeKey As String = "postAcquisitionProcessingTime"
    Private Const PostAcquisitionProcessingTimeDefault As Int64 = 0

    Private Const LockStealingPreventionPeriodKey As String = "lockStealingPreventionPeriod"
    Private Const LockStealingPreventionPeriodDefault As Int64 = 1024 * 60  '60 seconds = 1 minute

    Private Const MaximumStorageCapacityKey As String = "maximumStorageCapacity"
    Private Const MaximumStorageCapacityDefault As Int64 = Int64.MaxValue

    Private Const LeastRecentlyUsedCaptureDataAutomaticallyDroppedKey As String = "lruCaptureDataAutomaticallyDropped"
    Private Const LeastRecentlyUsedCaptureDataAutomaticallyDroppedDefault As Boolean = True

    Private Const ModalityKey As String = "modality"
    Private Const ModalityDefault As String = Nothing

    Private Const SubmodalityKey As String = "submodality"
    Private Const SubmodalityDefault As String = Nothing

    Public Sub RestoreDefaults()
        LastUpdated = LastUpdatedDefault
        InactivityTimeout = InactivityTimeoutDefault
        MaximumConcurrentSessions = MaximumConcurrentSessionsDefault
        LeastRecentlyUsedSessionsAutomaticallyDropped = LeastRecentlyUsedSessionsAutomaticallyDroppedDefault
        InitializationTimeout = InitializationTimeoutDefault
        GetConfigurationTimeout = GetConfigurationTimeoutDefault
        SetConfigurationTimeout = SetConfigurationTimeoutDefault
        CaptureTimeout = CaptureTimeoutDefault
        PostAcquisitionProcessingTime = PostAcquisitionProcessingTimeDefault
        LockStealingPreventionPeriod = LockStealingPreventionPeriodDefault
        MaximumStorageCapacity = MaximumStorageCapacityDefault
        LeastRecentlyUsedCaptureDataAutomaticallyDropped = LeastRecentlyUsedCaptureDataAutomaticallyDroppedDefault
        Modality = ModalityDefault
        Submodality = SubmodalityDefault
    End Sub

    Public Property LastUpdated As DateTime
        Get
            If Me.ContainsKey(LastUpdatedKey) Then
                Return CType(Me(LastUpdatedKey), DateTime)
            Else
                Return LastUpdatedDefault
            End If
        End Get
        Set(value As DateTime)
            Me(LastUpdatedKey) = value
        End Set
    End Property

    Public Property InactivityTimeout As Int64
        Get
            If Me.ContainsKey(InactivityTimeoutKey) Then
                Return CType(Me(InactivityTimeoutKey), Int64)
            Else
                Return InactivityTimeoutDefault
            End If
        End Get
        Set(value As Int64)
            Me(InactivityTimeoutKey) = value
        End Set
    End Property

    Public Property MaximumConcurrentSessions As Integer
        Get
            If Me.ContainsKey(MaximumConcurrentSessionsKey) Then
                Return CInt(Me(MaximumConcurrentSessionsKey))
            Else
                Return MaximumConcurrentSessionsDefault
            End If
        End Get
        Set(value As Integer)
            Me(MaximumConcurrentSessionsKey) = value
        End Set
    End Property

    Public Property LeastRecentlyUsedSessionsAutomaticallyDropped As Boolean
        Get
            If Me.ContainsKey(LeastRecentlyUsedSessionsAutomaticallyDroppedKey) Then
                Return CBool(Me(LeastRecentlyUsedSessionsAutomaticallyDroppedKey))
            Else
                Return LeastRecentlyUsedSessionsAutomaticallyDroppedDefault
            End If
        End Get
        Set(value As Boolean)
            Me(LeastRecentlyUsedSessionsAutomaticallyDroppedKey) = value
        End Set
    End Property

    Public Property InitializationTimeout As Integer
        Get
            If Me.ContainsKey(InitializationTimeoutKey) Then
                Return CInt(Me(InitializationTimeoutKey))
            Else
                Return InitializationTimeoutDefault
            End If
        End Get
        Set(value As Integer)
            Me(InitializationTimeoutKey) = value
        End Set
    End Property

    Public Property GetConfigurationTimeout As Integer
        Get
            If Me.ContainsKey(GetConfigurationTimeoutKey) Then
                Return CInt(Me(GetConfigurationTimeoutKey))
            Else
                Return GetConfigurationTimeoutDefault
            End If
        End Get
        Set(value As Integer)
            Me(GetConfigurationTimeoutKey) = value
        End Set
    End Property

    Public Property SetConfigurationTimeout As Integer
        Get
            If Me.ContainsKey(SetConfigurationTimeoutKey) Then
                Return CInt(Me(SetConfigurationTimeoutKey))
            Else
                Return SetConfigurationTimeoutDefault
            End If
        End Get
        Set(value As Integer)
            Me(SetConfigurationTimeoutKey) = value
        End Set
    End Property

    Public Property CaptureTimeout As Integer
        Get
            If Me.ContainsKey(CaptureTimeoutKey) Then
                Return CInt(Me(CaptureTimeoutKey))
            Else
                Return CaptureTimeoutDefault
            End If
        End Get
        Set(value As Integer)
            Me(CaptureTimeoutKey) = value
        End Set
    End Property

    Public Property PostAcquisitionProcessingTime As Int64
        Get
            If Me.ContainsKey(PostAcquisitionProcessingTimeKey) Then
                Return CType(Me(PostAcquisitionProcessingTimeKey), Int64)
            Else
                Return PostAcquisitionProcessingTimeDefault
            End If
        End Get
        Set(value As Int64)
            Me(PostAcquisitionProcessingTimeKey) = value
        End Set
    End Property

    Public Property LockStealingPreventionPeriod As Int64
        Get
            If Me.ContainsKey(LockStealingPreventionPeriodKey) Then
                Return CType(Me(LockStealingPreventionPeriodKey), Int64)
            Else
                Return LockStealingPreventionPeriodDefault
            End If
        End Get
        Set(value As Int64)
            Me(LockStealingPreventionPeriodKey) = value
        End Set
    End Property

    Public Property MaximumStorageCapacity As Int64
        Get
            If Me.ContainsKey(MaximumStorageCapacityKey) Then
                Return CType(Me(MaximumStorageCapacityKey), Int64)
            Else
                Return MaximumStorageCapacityDefault
            End If
        End Get
        Set(value As Int64)
            Me(MaximumStorageCapacityKey) = value
        End Set
    End Property

    Public Property LeastRecentlyUsedCaptureDataAutomaticallyDropped As Boolean
        Get
            If Me.ContainsKey(LeastRecentlyUsedCaptureDataAutomaticallyDroppedKey) Then
                Return CBool(Me(LeastRecentlyUsedCaptureDataAutomaticallyDroppedKey))
            Else
                Return LeastRecentlyUsedCaptureDataAutomaticallyDroppedDefault
            End If
        End Get
        Set(value As Boolean)
            Me(LeastRecentlyUsedCaptureDataAutomaticallyDroppedKey) = value
        End Set
    End Property

    Public Property Modality As String
        Get
            If Me.ContainsKey(ModalityKey) Then
                Return CStr(Me(ModalityKey))
            Else
                Return ModalityDefault
            End If
        End Get
        Set(value As String)
            Me(ModalityKey) = value
        End Set
    End Property

    Public Property Submodality As String
        Get
            If Me.ContainsKey(SubmodalityKey) Then
                Return CStr(Me(SubmodalityKey))
            Else
                Return SubmodalityDefault
            End If
        End Get
        Set(value As String)
            Me(SubmodalityKey) = value
        End Set
    End Property
End Class