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

Imports System.IO
Imports System.Threading
Imports System.Runtime.Serialization

Namespace Nist.Bcl.Wsbd

    Public Class FileStorageProvider
        Implements IStorageProvider

        Protected Sub New()
        End Sub

        Public Sub New(ByVal location As String, ByVal dataCapacity As Long, ByVal countCapacity As Integer)
            Initialize(location, dataCapacity, countCapacity, True)
        End Sub

        Public Sub New(ByVal location As String, ByVal dataCapacity As Long, ByVal countCapacity As Integer, autoDropLRUData As Boolean)
            Initialize(location, dataCapacity, countCapacity, autoDropLRUData)
        End Sub

        Private Sub Initialize(ByVal location As String, ByVal dataCapacity As Long, ByVal countCapacity As Integer, autoDropLRUData As Boolean)
            mLocation = Location
            mDataCapacity = dataCapacity
            mCountCapacity = countCapacity
            Me.AutoDropLRUData = autoDropLRUData

            If IO.Directory.Exists(location) Then
                IO.Directory.Delete(location, True)
            End If

            IO.Directory.CreateDirectory(location)

            ValidateLocation()
            CalculatedDirectorySize = ValidateSizeCapacity(True)
            ValidateCountCapacity()
        End Sub

        Public Function RetrieveData(ByVal id As System.Guid) As Byte() Implements IStorageProvider.RetrieveData
            Try
                UpdateActivity(id)
                Return IO.File.ReadAllBytes(DataFileName(id))
            Catch ex As IOException
                Throw New StorageProviderIOException(ex.Message, ex)
            End Try
        End Function

        Public Function RetrieveMetadata(ByVal id As System.Guid) As Dictionary Implements IStorageProvider.RetrieveMetadata
            Dim fs As New FileStream(MetadataFileName(id), FileMode.Open)
            UpdateActivity(id)

            Try
                Dim dcs As New DataContractSerializer(GetType(Dictionary))
                Dim metadata As Dictionary = CType(dcs.ReadObject(fs), Dictionary)

                Return metadata
            Catch ex As IOException
                Throw New StorageProviderIOException(ex.Message, ex)
            Finally
                fs.Close()
                fs = Nothing
            End Try
        End Function

        Public Sub Delete(id As System.Guid) Implements IStorageProvider.Delete
            Dim Filenames As String() = {DataFileName(id), MetadataFileName(id), PendingFileName(id)}

            mDataActivityLock.EnterUpgradeableReadLock()

            If mDataActivity.ContainsKey(id) Then
                mDataActivityLock.EnterWriteLock()
                mDataActivity.Remove(id)
                mDataActivityLock.ExitWriteLock()
            End If

            mDataActivityLock.ExitUpgradeableReadLock()

            For Each Filename As String In Filenames
                If File.Exists(Filename) Then
                    Dim Info As New FileInfo(Filename)
                    CalculatedDirectorySize -= Info.Length
                    File.Delete(Filename)
                End If
            Next Filename
        End Sub

        Public Sub StoreData(ByVal id As System.Guid, ByVal metadata As Dictionary, ByVal data() As Byte) Implements IStorageProvider.StoreData
            ValidateIdIsNeitherFullNorCorrupt(id)

            If AutoDropLRUData Then
                While Not IsValidCountCapacity()
                    RemoveLeastRecentlyUsedData()
                End While

                While Not IsValidSizeCapacity()
                    RemoveLeastRecentlyUsedData()
                End While
            Else
                ValidateCountCapacity()
                ValidateSizeCapacity()
            End If

            Dim fs As New FileStream(MetadataFileName(id), FileMode.OpenOrCreate)

            Try
                UpdateActivity(id)
                CalculatedDirectorySize += data.Length
                IO.File.WriteAllBytes(DataFileName(id), data)

                Dim dcs As New System.Runtime.Serialization.DataContractSerializer(GetType(Dictionary))


                dcs.WriteObject(fs, metadata)
                CalculatedDirectorySize += fs.Length

                IO.File.Delete(PendingFileName(id))
            Catch ex As IOException
                Throw New StorageProviderIOException(ex.Message, ex)
            Finally
                fs.Close()
                fs = Nothing
            End Try
        End Sub

        Public Sub ReserveId(ByVal id As System.Guid) Implements IStorageProvider.ReserveId

            ValidateIdIsNeitherFullNorCorrupt(id)

            If GetState(id) = StorageIdState.Pending Then Return

            Try
                IO.File.WriteAllText(PendingFileName(id), String.Empty)
            Catch ex As IOException
                Throw New StorageProviderIOException(ex.Message, ex)
            End Try

        End Sub

        Private Sub ValidateIdIsNeitherFullNorCorrupt(ByVal id As Guid)

            Dim idState = GetState(id)
            If idState = StorageIdState.OK Then
                Throw New StorageProviderDataCollisionException
            ElseIf idState = StorageIdState.Corrupt Then
                Throw New StorageProviderIOException(String.Format(My.Resources.IncompleteDataStoreRecord, id))
            End If

        End Sub

        Private mLocation As String
        Private mDataCapacity As Long
        Private mCountCapacity As Integer

        Private mContentTypes As Dictionary(Of Guid, String)

        Private Const DataFileExtention As String = "data"
        Private Const ContentFileExtention As String = "mime"
        Private Const PendingFileExtention As String = "pending"

        Public ReadOnly Property Location As String
            Get
                Return mLocation
            End Get
        End Property

        Public ReadOnly Property DataCapacity As Long
            Get
                Return mDataCapacity
            End Get
        End Property


        Public ReadOnly Property CountCapacity As Integer
            Get
                Return mCountCapacity
            End Get
        End Property

        Private Sub ValidateLocation()
            If Not IO.Directory.Exists(Location) Then
                Throw New IO.DirectoryNotFoundException(Location)
            End If
        End Sub

        Private Function ValidateSizeCapacity(Recalculate As Boolean) As Long
            If Recalculate Then
                Dim Size As Long = DirectorySize(New DirectoryInfo(mLocation))

                CalculatedDirectorySize = Size
            End If

            If CalculatedDirectorySize >= DataCapacity Then
                Throw New StorageProviderCapacityExceededException
            End If

            Return CalculatedDirectorySize
        End Function

        Private Function IsValidSizeCapacity() As Boolean
            Try
                ValidateSizeCapacity()
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Function

        Private Function IsValidCountCapacity() As Boolean
            Try
                ValidateCountCapacity()
                Return True
            Catch ex As Exception
                Return False
            End Try
        End Function

        Private Function ValidateSizeCapacity() As Long
            Return ValidateSizeCapacity(False)
        End Function

        Private Sub ValidateCountCapacity()
            If ChildCount(New DirectoryInfo(mLocation)) >= CountCapacity Then
                Throw New StorageProviderCapacityExceededException
            End If
        End Sub

        Public Function GetState(ByVal id As System.Guid) As StorageIdState Implements IStorageProvider.GetState

            Dim hasDataFile = IO.File.Exists(DataFileName(id))
            Dim hasMetadataFile = IO.File.Exists(MetadataFileName(id))
            Dim hasPendingFile = IO.File.Exists(PendingFileName(id))

            ' By default, the state will be considered corrupt
            Dim state As StorageIdState = StorageIdState.Corrupt

            If Not hasPendingFile AndAlso Not hasMetadataFile AndAlso Not hasDataFile Then
                state = StorageIdState.Empty
            End If

            If hasPendingFile AndAlso Not hasMetadataFile AndAlso Not hasDataFile Then
                state = StorageIdState.Pending
            End If

            If Not hasPendingFile AndAlso hasMetadataFile AndAlso hasDataFile Then
                state = StorageIdState.OK
            End If

            Return state
        End Function

        Public Property AutoDropLRUData As Boolean

        Private mDataActivity As New Dictionary(Of Guid, DateTime)
        Private mDataActivityLock As New ReaderWriterLockSlim(LockRecursionPolicy.SupportsRecursion)

        Public Sub UpdateActivity(id As Guid)
            mDataActivityLock.EnterUpgradeableReadLock()

            If Not mDataActivity.ContainsKey(id) Then
                mDataActivityLock.EnterWriteLock()
                mDataActivity.Add(id, DateTime.Now)
                mDataActivityLock.ExitWriteLock()
            End If

            mDataActivityLock.EnterWriteLock()
            mDataActivity(id) = DateTime.Now
            mDataActivityLock.ExitWriteLock()
            mDataActivityLock.ExitUpgradeableReadLock()
        End Sub

        Private Sub RemoveLeastRecentlyUsedData()
            mDataActivityLock.EnterUpgradeableReadLock()

            Dim lru = From pair In mDataActivity Order By pair.Value Ascending Select pair Take 1

            For Each pair In lru.ToArray()
                Delete(pair.Key)
            Next

            mDataActivityLock.ExitUpgradeableReadLock()
        End Sub

        Public Function DataFileName(ByVal id As Guid) As String
            Return IO.Path.Combine(Location, id.ToString & "." & DataFileExtention)
        End Function

        Public Function MetadataFileName(ByVal id As Guid) As String
            Return IO.Path.Combine(Location, id.ToString & "." & ContentFileExtention)
        End Function

        Public Function PendingFileName(ByVal id As Guid) As String
            Return IO.Path.Combine(Location, id.ToString & "." & PendingFileExtention)
        End Function

        Private Property CalculatedDirectorySize() As Long = -1

        Private Shared Function DirectorySize(ByVal dirInfo As DirectoryInfo) As Long

            ' Accumulate the size of the files
            Dim size As Long = 0
            For Each fileInfo In dirInfo.GetFiles()
                size += fileInfo.Length
            Next

            For Each childDirInfo In dirInfo.GetDirectories()
                size += DirectorySize(childDirInfo)
            Next

            Return size
        End Function

        Private Shared Function ChildCount(ByVal dirInfo As DirectoryInfo) As Long
            Dim count As Long = 0
            count = dirInfo.GetFiles("*." & DataFileExtention).Count()
            Return count
        End Function

    End Class

End Namespace