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

Public Enum ActiveObjectReturnState
    Unstarted = 0
    Started
    Interrupted
    Completed
End Enum

Public Enum ActiveObjectScenario
    DoNothing
    ThrowNullReferenceException
End Enum

Public Class ActiveObject

    Public Sub New()
        mId = Guid.NewGuid
    End Sub

    Private mId As Guid
    Private mState As ActiveObjectReturnState = ActiveObjectReturnState.Unstarted

    Public Delegate Function RunDelegate(ByVal sleepTime As Integer, ByVal scenario As ActiveObjectScenario) As ActiveObjectReturnState

    Function Run(ByVal sleepTime As Integer, ByVal scenario As ActiveObjectScenario) As ActiveObjectReturnState
        'Dim state As ActiveObjectReturnState = ActiveObjectReturnState.Unstarted

        Try
            mTimeStarted = DateTime.Now
            mState = ActiveObjectReturnState.Started
            Dim stopwatch = Diagnostics.Stopwatch.StartNew()
            Threading.Thread.Sleep(sleepTime)
            Dim timeSlept = stopwatch.ElapsedMilliseconds

            If scenario = ActiveObjectScenario.ThrowNullReferenceException Then
                Dim x As List(Of Integer) = Nothing
                x(0) = 0
            End If

            mState = ActiveObjectReturnState.Completed
            mTimeCompleted = DateTime.Now

        Catch ex As Threading.ThreadInterruptedException
            'state = ActiveObjectReturnState.Interrupted
            mWasInterrupred = True
            mTimeInterrupted = DateTime.Now
            mState = ActiveObjectReturnState.Interrupted
        Finally
            ' Do nothing
            Dim x = 0
        End Try

        Return mState
    End Function

    Function NoHandlerRun(ByVal sleepTime As Integer, ByVal scenario As ActiveObjectScenario) As ActiveObjectReturnState

        mTimeStarted = DateTime.Now
        mState = ActiveObjectReturnState.Started
        Dim stopwatch = Diagnostics.Stopwatch.StartNew()
        Threading.Thread.Sleep(sleepTime)
        Dim timeSlept = stopwatch.ElapsedMilliseconds

        If scenario = ActiveObjectScenario.ThrowNullReferenceException Then
            Dim x As List(Of Integer) = Nothing
            x(0) = 0
        End If

        mState = ActiveObjectReturnState.Completed
        mTimeCompleted = DateTime.Now

        Return mState
    End Function



    Function RunWithFinallyOnly(ByVal sleepTime As Integer, ByVal scenario As ActiveObjectScenario) As ActiveObjectReturnState

        ' This is a clause with a 'finally' block to test the ability of the job factory to correctly use reflection

        Try
            Dim x = 0
        Finally
            Dim y = 0
        End Try



        Return mState
    End Function


    Private mTimeStarted As DateTime
    Private mTimeCompleted As DateTime
    Private mTimeInterrupted As DateTime

    Private mWasInterrupred As Boolean = False
    Public ReadOnly Property WasInterrupted As Boolean
        Get
            Return mWasInterrupred
        End Get
    End Property


    Public ReadOnly Property TimeStarted As DateTime
        Get
            Return mTimeStarted
        End Get
    End Property

    Public ReadOnly Property TimeCompleted As DateTime
        Get
            Return mTimeCompleted
        End Get
    End Property

    Public ReadOnly Property TimeInterrupted As DateTime
        Get
            Return mTimeInterrupted
        End Get
    End Property

    Public ReadOnly Property Id As Guid
        Get
            Return mId
        End Get
    End Property

    Public ReadOnly Property State As ActiveObjectReturnState
        Get
            Return mState
        End Get
    End Property

End Class
