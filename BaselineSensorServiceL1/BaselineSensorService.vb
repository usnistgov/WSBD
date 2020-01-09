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

Imports System.Threading
Imports System.Windows.Media.Imaging
Imports System.Windows
Imports System.Windows.Media
Imports System.Globalization


Namespace Nist.Bcl.Wsbd

    Public Class BaselineSensorService
        Inherits SensorService
        Implements IDisposable

        Private Sub New()
            ' Forbid explicit construction
        End Sub

        Private Shared smSessionIdOverrideLock As New ReaderWriterLockSlim
        Private Shared smSessionIdOverride As Guid = Guid.Empty
        Public Shared WriteOnly Property OverrideSessionId As Guid
            Set(ByVal value As Guid)
                smSessionIdOverrideLock.EnterWriteLock()
                smSessionIdOverride = value
                smSessionIdOverrideLock.ExitWriteLock()
            End Set
        End Property

        Protected Overrides Function GenerateSessionId() As System.Guid
            Dim sessionId As Guid
            smSessionIdOverrideLock.EnterReadLock()
            If Not smSessionIdOverride.Equals(Guid.Empty) Then
                sessionId = smSessionIdOverride
            Else
                sessionId = MyBase.GenerateSessionId()
            End If
            smSessionIdOverrideLock.ExitReadLock()
            Return sessionId
        End Function

        Public Shared Sub Reset()

            ResetActiveSessions()
            ResetStagnantSessionDetection()
            ResetRegisteredSessions()
            ResetLock()
            ResetSensorJob()
            ResetServiceInfo()
            ResetConfiguration()

            LockStealingForbidden = False

            ForceSensorFailure = False
            ForceSensorTimeout = False
            ForceGeneralFailure = False

            ForceInitializationRequired = False
            ForceConfigurationRequired = False

            StorageProvider = New FileStorageProvider(DefaultStorageLocation, smServiceInfoDictionary.MaximumStorageCapacity, DefaultStorageCountCapacity)

            OverrideSessionId = Guid.Empty

            InitializationTime = DefaultInitializationTime
            ConfigurationTime = DefaultConfigurationTime

            PendingDownloadTime = DefaultPendingDownloadTime

            ConfigurationValidator = Nothing
            ThrifyDownloadValidator = Nothing

        End Sub

        Public Overrides Function Unregister(ByVal userInputSessionId As String) As Result
            Dim result As Result
            If smForceGeneralFailure Then
                result = New Result(Status.Failure)
            Else
                result = MyBase.Unregister(userInputSessionId)
            End If
            Return result
        End Function

#Region " Forbid Lock Stealing "

        ' During testing, it is useful to forbit lock stealing.
        ' A real service should not offer this feature.

        Private Shared smLockStealingForbiddenLock As New ReaderWriterLockSlim
        Private Shared smLockStealingForbidden As Boolean = False
        Public Shared WriteOnly Property LockStealingForbidden As Boolean
            Set(ByVal value As Boolean)
                smLockStealingForbiddenLock.EnterWriteLock()
                smLockStealingForbidden = value
                smLockStealingForbiddenLock.ExitWriteLock()
            End Set
        End Property

#End Region

#Region " Force Sensor Failure "

        ' During testing, it is useful to simulate a sensor failure
        ' It is unlikely that a real service would offer this feature.

        Private Shared smForceSensorFailureLock As New ReaderWriterLockSlim
        Private Shared smForceSensorFailure As Boolean = False
        Public Shared WriteOnly Property ForceSensorFailure As Boolean
            Set(ByVal value As Boolean)
                smForceSensorFailureLock.EnterWriteLock()
                smForceSensorFailure = value
                smForceSensorFailureLock.ExitWriteLock()
            End Set
        End Property

#End Region

#Region " Force General Failure "

        ' During testing, it is useful to simulate a general failure.
        ' It is unlikely that a real service would offer this feature.


        Private Shared smForceGeneralFailureLock As New ReaderWriterLockSlim
        Private Shared smForceGeneralFailure As Boolean = False
        Public Shared WriteOnly Property ForceGeneralFailure As Boolean
            Set(ByVal value As Boolean)
                smForceGeneralFailureLock.EnterWriteLock()
                smForceGeneralFailure = value
                smForceGeneralFailureLock.ExitWriteLock()
            End Set
        End Property

#End Region

#Region " Force Sensor Timeout "

        ' During testing, it is useful to simulate a sensor timeout
        ' It is unlikely that a real service would offer this feature.

        Private Shared smForceSensorTimeoutLock As New ReaderWriterLockSlim
        Private Shared smForceSensorTimeout As Boolean = False
        Public Shared WriteOnly Property ForceSensorTimeout As Boolean
            Set(ByVal value As Boolean)
                smForceSensorTimeoutLock.EnterWriteLock()
                smForceSensorTimeout = value
                smForceSensorTimeoutLock.ExitWriteLock()
            End Set
        End Property

#End Region

#Region " Force Initialization Required "

        ' During testing, it is useful to simulate an initialization prerequisite.
        ' A real service would not offer this feature.

        Private Shared smForceInitializationRequiredLock As New ReaderWriterLockSlim
        Private Shared smForceInitializationRequired As Boolean = False
        Public Shared WriteOnly Property ForceInitializationRequired As Boolean
            Set(ByVal value As Boolean)
                smForceInitializationRequiredLock.EnterWriteLock()
                smForceInitializationRequired = value
                smForceInitializationRequiredLock.ExitWriteLock()
            End Set
        End Property


#End Region

#Region " Force Configuration Required "

        ' During testing, it is useful to simulate an configuration prerequisite.
        ' A real service would not offer this feature.

        Private Shared smForceConfigurationRequiredLock As New ReaderWriterLockSlim
        Private Shared smForceConfigurationRequired As Boolean = False
        Public Shared WriteOnly Property ForceConfigurationRequired As Boolean
            Set(ByVal value As Boolean)
                smForceConfigurationRequiredLock.EnterWriteLock()
                smForceConfigurationRequired = value
                smForceConfigurationRequiredLock.ExitWriteLock()
            End Set
        End Property


#End Region

#Region " Initialization "

        Public Shared Property InitializationTime As Integer = DefaultInitializationTime

        ' This is the default value for 'InitializationTime'
        Public Const DefaultInitializationTime As Integer = 10000 ' ms

        Protected Overrides Function Initialize() As Result

            Dim initResult As New Result
            Try
                Thread.Sleep(InitializationTime)
                If smForceGeneralFailure Then
                    initResult.Status = Status.Failure
                    initResult.Message = "Simulated general failure"
                ElseIf smForceSensorFailure Then
                    initResult.Status = Status.SensorFailure
                    initResult.Message = "Simulated sensor failure"
                ElseIf smForceSensorTimeout Then
                    initResult.Status = Status.SensorTimeout
                    initResult.Message = "Simulated sensor timeout"
                Else
                    initResult.Status = Status.Success
                End If

            Catch ex As ThreadInterruptedException
                initResult.Status = Status.Canceled
                If smForceSensorFailure Then
                    initResult.Status = Status.CanceledWithSensorFailure
                    initResult.Message = "Interrupted"
                End If
            End Try
            Return initResult
        End Function
#End Region

#Region " Info "
        Public Overrides Function GetServiceInfo() As Result
            Dim result As New Result

            If smForceGeneralFailure = True Then
                result.Status = Status.Failure
            Else
                result.Metadata = smServiceInfoDictionary
                result.Status = Status.Success
            End If

            Return result
        End Function


#End Region

#Region "  Configuration  "

        Private Shared smCurrentConfigurationLock As New ReaderWriterLockSlim
        Private Shared smCurrentConfiguration As Dictionary

        Public Const DefaultConfigurationTime As Integer = 10000 ' ms
        Public Shared Property ConfigurationTime As Integer = DefaultInitializationTime

        Protected Overrides Function SetConfiguration(ByVal configuration As Configuration) As Result
            Dim result As New Result
            Try
                Thread.Sleep(ConfigurationTime)

                If smForceGeneralFailure Then
                    result.Status = Status.Failure
                    result.Message = "Simulated general failure"
                ElseIf smForceSensorFailure Then
                    result.Status = Status.SensorFailure
                    result.Message = "Simulated sensor failure"
                ElseIf smForceSensorTimeout Then
                    result.Status = Status.SensorTimeout
                    result.Message = "Simulated sensor timeout"
                ElseIf smForceInitializationRequired Then
                    result.Status = Status.InitializationNeeded
                    result.Message = "Simulation of initialization prerequisite"
                Else

                    If ConfigurationValidator Is Nothing Then
                        smCurrentConfigurationLock.EnterWriteLock()
                        smCurrentConfiguration = configuration
                        smCurrentConfigurationLock.ExitWriteLock()
                        result.Status = Status.Success
                    Else
                        result = ConfigurationValidator(configuration)
                    End If

                End If

            Catch ex As ThreadInterruptedException
                result.Status = Status.Canceled
                If smForceSensorFailure Then
                    result.Status = Status.CanceledWithSensorFailure
                End If
            End Try
            Return result

        End Function

        Public Shared Property Configuration As Dictionary
            Get
                Dim currentConfiguration As Dictionary = Nothing
                smCurrentConfigurationLock.EnterReadLock()
                If smCurrentConfiguration IsNot Nothing Then currentConfiguration = smCurrentConfiguration.DeepCopy()
                smCurrentConfigurationLock.ExitReadLock()
                Return currentConfiguration
            End Get
            Set(ByVal value As Dictionary)
                smCurrentConfigurationLock.EnterWriteLock()
                smCurrentConfiguration = value.DeepCopy
                smCurrentConfigurationLock.ExitWriteLock()
            End Set

        End Property

        Private Shared Sub ResetConfiguration()
            smCurrentConfigurationLock.EnterWriteLock()
            smCurrentConfiguration = Nothing
            smCurrentConfigurationLock.ExitWriteLock()
        End Sub


        Protected Overrides Function GetConfiguration() As Result

            Dim result As New Result
            Try

                Thread.Sleep(ConfigurationTime)

                If smForceGeneralFailure Then
                    result.Status = Status.Failure
                    result.Message = "Simulated general failure"
                ElseIf smForceSensorFailure Then
                    result.Status = Status.SensorFailure
                    result.Message = "Simulated sensor failure"
                ElseIf smForceSensorTimeout Then
                    result.Status = Status.SensorTimeout
                    result.Message = "Simulated sensor timeout"
                ElseIf smForceInitializationRequired Then
                    result.Status = Status.InitializationNeeded
                    result.Message = "Simulation of initialization required"
                ElseIf smForceConfigurationRequired Then
                    result.Status = Status.ConfigurationNeeded
                    result.Message = "Simulation of configuration required"
                Else
                    smCurrentConfigurationLock.EnterReadLock()
                    If smCurrentConfiguration IsNot Nothing Then
                        result.Metadata = smCurrentConfiguration.DeepCopy
                    End If
                    smCurrentConfigurationLock.ExitReadLock()
                    result.Status = Status.Success
                    result.Metadata = smCurrentConfiguration.DeepCopy
                End If
            Catch ex As ThreadInterruptedException
                result.Status = Status.Canceled
                If smForceSensorFailure Then
                    result.Status = Status.CanceledWithSensorFailure
                End If
            End Try
            Return result

        End Function

        Private Shared smConfigurationValidatorLock As New ReaderWriterLockSlim
        Private Shared smConfigurationValidator As ValidateConfigurationDelegate

        Public Delegate Function ValidateConfigurationDelegate(ByVal configuration As Dictionary) As Result

        Public Shared Property ConfigurationValidator As ValidateConfigurationDelegate
            Get
                Dim validator As ValidateConfigurationDelegate
                smConfigurationValidatorLock.EnterReadLock()
                validator = smConfigurationValidator
                smConfigurationValidatorLock.ExitReadLock()
                Return validator
            End Get
            Set(ByVal value As ValidateConfigurationDelegate)
                smConfigurationValidatorLock.EnterWriteLock()
                smConfigurationValidator = value
                smConfigurationValidatorLock.ExitWriteLock()
            End Set
        End Property
#End Region

#Region "  Storage Provider "

        Public Const DefaultStorageLocation As String = "Storage"
        Public Const DefaultStorageCountCapacity As Integer = 1024

        Private Shared smStorageProviderLock As New ReaderWriterLockSlim
        Private Shared smStorageProvider As IStorageProvider = _
            New FileStorageProvider(DefaultStorageLocation, smServiceInfoDictionary.MaximumStorageCapacity, DefaultStorageCountCapacity)

        Public Shared Property StorageProvider() As IStorageProvider
            Get
                Dim provider As IStorageProvider
                smStorageProviderLock.EnterReadLock()
                provider = smStorageProvider
                smStorageProviderLock.ExitReadLock()
                Return provider
            End Get
            Protected Set(ByVal storageProvider As IStorageProvider)
                smStorageProviderLock.EnterWriteLock()
                smStorageProvider = storageProvider
                smStorageProviderLock.ExitWriteLock()
            End Set
        End Property

#End Region

#Region "  Capture  "

        Public Const DefaultCaptureTime As Integer = 1000 ' ms
        Public Shared Property CaptureTime As Integer = DefaultCaptureTime
        Private mCaptureTimer As Timer

        Protected Overrides Function Capture() As Result

            Dim result As New Result
            Try
                Thread.Sleep(CaptureTime)
                If smForceGeneralFailure Then
                    result.Status = Status.Failure
                    result.Message = "Simulated general failure"
                ElseIf smForceSensorFailure Then
                    result.Status = Status.SensorFailure
                    result.Message = "Simulated sensor failure"
                ElseIf smForceSensorTimeout Then
                    result.Status = Status.SensorTimeout
                    result.Message = "Simulated sensor timeout"
                ElseIf smForceInitializationRequired Then
                    result.Status = Status.InitializationNeeded
                    result.Message = "Simulation of initialization required"
                ElseIf smForceConfigurationRequired Then
                    result.Status = Status.ConfigurationNeeded
                    result.Message = "Simulation of configuration required"
                Else

                    Dim id = Guid.NewGuid

                    StorageProvider.ReserveId(id)

                    ' We use a thread instead of a Timer, since the Timer uses a thread
                    ' from the ThreadPool.
                    '

                    Dim metadata As New Dictionary()
                    metadata.Add(Constants.ContentTypeKey, "image/png")
                    metadata.Add(Constants.CaptureDateKey, System.DateTime.Now)

                    Dim storeDataThread = New Thread(Sub()
                                                         Thread.Sleep(PendingDownloadTime)
                                                         Dim imageData = GenerateCapturedImage(id)
                                                         StorageProvider.StoreData(id, metadata, imageData)
                                                     End Sub)
                    storeDataThread.Start()

                    result.CaptureIds = New GuidArray
                    result.CaptureIds.Add(id)
                    result.Status = Status.Success
                End If

            Catch ex As ThreadInterruptedException
                result.Status = Status.Canceled
                If smForceSensorFailure Then
                    result.Status = Status.CanceledWithSensorFailure
                End If
            End Try

            Return result
        End Function



        Public Const DefaultPendingDownloadTime As Integer = 100 ' ms
        Private Shared smPendingDownloadTime As Integer = DefaultPendingDownloadTime
        Public Shared Property PendingDownloadTime As Integer
            Get
                Return smPendingDownloadTime
            End Get
            Set(ByVal value As Integer)
                If value < 1 Then Throw New ArgumentException
                smPendingDownloadTime = value
            End Set
        End Property

#End Region

#Region "  Download  "

        Public Overrides Function GetDownloadInfo(ByVal captureId As Guid) As Result
            Dim result As New Result
            Try
                Dim idState = StorageProvider.GetState(captureId)
                Select Case idState
                    Case StorageIdState.OK
                        result.Status = Status.Success
                        result.Metadata = StorageProvider.RetrieveMetadata(captureId)
                    Case StorageIdState.Pending
                        result.Status = Status.PreparingDownload
                    Case StorageIdState.Empty
                        result.Status = Status.InvalidId
                    Case Else
                        result.Status = Status.Failure
                        result.Message = idState.ToString
                End Select
            Catch ex As Exception
                result.Status = Status.Failure
                result.Message = ex.Message
            End Try
            Return result
        End Function

        Public Overrides Function Download(ByVal captureId As Guid) As Result
            Dim result As New Result
            Try
                Dim idState = StorageProvider.GetState(captureId)
                Select Case idState
                    Case StorageIdState.OK
                        result.Status = Status.Success
                        result.Metadata = StorageProvider.RetrieveMetadata(captureId)
                        result.SensorData = StorageProvider.RetrieveData(captureId)
                    Case StorageIdState.Pending
                        result.Status = Status.PreparingDownload
                    Case StorageIdState.Empty
                        result.Status = Status.InvalidId
                    Case Else
                        result.Status = Status.Failure
                        result.Message = idState.ToString
                End Select

            Catch ex As StorageProviderException
                result.Status = Status.Failure
                result.Message = ex.Message
            End Try
            Return result

        End Function


        Public Overrides Function ThriftyDownload(ByVal captureId As System.Guid, ByVal maxSize As Integer) As Result


            Dim result As New Result
            Try
                If maxSize <= 0 Then
                    result.Status = Status.BadValue
                    result.BadFields = New StringArray
                    result.BadFields.Add(Constants.MaxSizeParameterName)
                ElseIf maxSize >= Math.Max(My.Resources.SampleImage.Width, My.Resources.SampleImage.Height) Then
                    ' If the size of the image is larger than the original image, then we forbid this. 
                    result.Status = Status.Unsupported
                Else


                    Dim idState = StorageProvider.GetState(captureId)
                    Select Case idState
                        Case StorageIdState.OK
                            result.Status = Status.Success
                            result.Metadata = New Dictionary()
                            result.Metadata.Add(Constants.ContentTypeKey, CStr(StorageProvider.RetrieveMetadata(captureId)(Constants.ContentTypeKey)))
                            result.SensorData = GenerateCapturedImage(captureId, maxSize)
                        Case StorageIdState.Pending
                            result.Status = Status.PreparingDownload
                        Case StorageIdState.Empty
                            result.Status = Status.InvalidId
                        Case Else
                            result.Status = Status.Failure
                            result.Message = idState.ToString
                    End Select


                End If



            Catch ex As StorageProviderException
                result.Status = Status.Failure
                result.Message = ex.Message
            End Try
            Return result
        End Function

        Private Shared smGenerateCapturedImageLock As New Object
        Public Shared Function GenerateCapturedImage(ByVal id As Guid, Optional ByVal maxDimension As Integer = -1) As Byte()

            Dim imageData As Byte()

            SyncLock smGenerateCapturedImageLock

                Dim source = BitmapToBitmapSource(My.Resources.SampleImage)




                Dim visual As New DrawingVisual
                Dim context = visual.RenderOpen()

                Dim width, height As Integer
                width = source.PixelWidth
                height = source.PixelHeight

                Dim emSize = Math.Max(4.0, maxDimension * 30.0 / Math.Max(width, height))

                Dim aspect = (width * 1.0) / (height * 1.0)
                If maxDimension <> -1 Then
                    If width >= height Then
                        width = maxDimension
                        height = CInt(maxDimension * 1.0 / aspect)
                    Else
                        height = maxDimension
                        width = CInt(maxDimension * 1.0 * aspect)
                    End If
                End If


                Dim text = New FormattedText(
                               id.ToString, New CultureInfo("en-us"), FlowDirection.LeftToRight,
                               New Typeface("Courier"), emSize, Brushes.White)

                context.DrawImage(source, New Rect(0, 0, width, height))
                context.DrawText(text, New Point(0, 0))
                context.Close()






                Dim render As New RenderTargetBitmap(width, height,
                                                     source.DpiX, source.DpiY,
                                                     PixelFormats.Pbgra32)
                render.Render(visual)

                Dim png = New PngBitmapEncoder
                png.Frames.Add(BitmapFrame.Create(render))


                Using stream = New IO.MemoryStream
                    png.Save(stream)
                    imageData = stream.ToArray()
                End Using

            End SyncLock

            Return imageData
        End Function


        Private Shared Function BitmapToBitmapSource(ByVal source As System.Drawing.Bitmap) As BitmapSource
            Return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap( _
                source.GetHbitmap, IntPtr.Zero, Int32Rect.Empty, BitmapSizeOptions.FromEmptyOptions)
        End Function

        Private Shared smThrifyDownloadValidatorLock As New ReaderWriterLockSlim
        Private Shared smThrifyDownloadValidator As ValidateThrifyDownloadDelegate

        Public Delegate Function ValidateThrifyDownloadDelegate(ByVal size As Integer) As Result

        Public Shared Property ThrifyDownloadValidator As ValidateThrifyDownloadDelegate
            Get
                Dim validator As ValidateThrifyDownloadDelegate
                smThrifyDownloadValidatorLock.EnterReadLock()
                validator = smThrifyDownloadValidator
                smConfigurationValidatorLock.ExitReadLock()
                Return validator
            End Get
            Set(ByVal value As ValidateThrifyDownloadDelegate)
                smThrifyDownloadValidatorLock.EnterWriteLock()
                smThrifyDownloadValidator = value
                smThrifyDownloadValidatorLock.ExitWriteLock()
            End Set
        End Property

#End Region

#Region "IDisposable Support"
        Private disposedValue As Boolean ' To detect redundant calls

        ' IDisposable
        Protected Overridable Sub Dispose(disposing As Boolean)
            If Not Me.disposedValue Then
                If disposing Then
                    If mCaptureTimer IsNot Nothing Then mCaptureTimer.Dispose()
                End If

            End If
            Me.disposedValue = True
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


