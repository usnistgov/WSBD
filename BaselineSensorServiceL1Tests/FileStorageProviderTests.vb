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

Imports Nist.Bcl.Wsbd
Imports System.Runtime.Serialization


<TestClass()>
Public Class FileStorageProviderTests

    <TestInitialize()>
    Public Sub TestInitialize()
        CreateDataStore()
    End Sub

    <TestCleanup()>
    Public Sub TestCleanup()
        DeleteDataStore()
    End Sub

    Public Shared Property StoragePath As String = My.Computer.FileSystem.GetTempFileName

    Private Shared Sub CreateDataStore()
        ' This is cheating --- we delete the temporary file, so we can create a directory
        ' with the same name in it's place.
        If IO.File.Exists(StoragePath) Then IO.File.Delete(StoragePath)

        If Not IO.Directory.Exists(StoragePath) Then IO.Directory.CreateDirectory(StoragePath)
    End Sub

    Private Shared Sub DeleteDataStore()
        IO.Directory.Delete(StoragePath, True)
    End Sub

    Private Shared Sub WriteDummyFile(ByVal provider As FileStorageProvider, ByVal size As Integer)
        Dim data(size - 1) As Byte
        Dim filename As String = provider.DataFileName(Guid.NewGuid)
        provider.StoreData(Guid.NewGuid, Nothing, data)
    End Sub

    <TestMethod(), ExpectedException(GetType(IO.DirectoryNotFoundException))>
    Public Sub FileStorageProviderConstructorRequiresPathExists()
        Dim storageProvider As New FileStorageProvider("x:\directory_that_does_not_exist", 0, 0)
    End Sub

    <TestMethod(), ExpectedException(GetType(StorageProviderCapacityExceededException))>
    Public Sub FileStorageProviderEnforcesStorageCapacity()
        Dim testCapacity As Integer = 1024

        Dim storageProvider As New FileStorageProvider(StoragePath, testCapacity, 1, False)


        WriteDummyFile(storageProvider, testCapacity + 1)
        WriteDummyFile(storageProvider, testCapacity + 1)
    End Sub

    <TestMethod()>
    Public Sub FileStorageProviderDropsLRUDataOnMaximumCount()
        Dim LRUDroppedDefault = SensorService.ServiceInfo.LeastRecentlyUsedCaptureDataAutomaticallyDropped
        SensorService.ServiceInfo.LeastRecentlyUsedCaptureDataAutomaticallyDropped = True

        Dim testCount As Integer = 1024
        Dim storageProvider As New FileStorageProvider(StoragePath, Long.MaxValue, testCount - 1, True)

        For i As Integer = 0 To testCount + 1
            WriteDummyFile(storageProvider, 1024)
        Next

        SensorService.ServiceInfo.LeastRecentlyUsedCaptureDataAutomaticallyDropped = LRUDroppedDefault
    End Sub

    <TestMethod()>
    Public Sub FileStorageProviderDropsLRUDataOnMaximumCapacity()
        Dim testCapacity As Integer = 1024
        Dim storageProvider As New FileStorageProvider(StoragePath, Long.MaxValue, 1, True)

        WriteDummyFile(storageProvider, testCapacity + 1)
        WriteDummyFile(storageProvider, testCapacity + 1)
        WriteDummyFile(storageProvider, testCapacity + 1)
        WriteDummyFile(storageProvider, testCapacity + 1)
    End Sub

    <TestMethod(), ExpectedException(GetType(StorageProviderCapacityExceededException))>
    Public Sub FileStorageProviderEnforcesCountCapacity()
        Dim testCount As Integer = 1024
        Dim storageProvider As New FileStorageProvider(StoragePath, Long.MaxValue, testCount - 1, False)

        For i As Integer = 0 To testCount - 1
            WriteDummyFile(storageProvider, 1024)
        Next
    End Sub

    <TestMethod()>
    Public Sub GetStateCanCorrectlyReturnEmpty()
        Dim store As New FileStorageProvider(StoragePath, Long.MaxValue, Integer.MaxValue)
        Dim state = store.GetState(Guid.NewGuid)
        Assert.AreEqual(StorageIdState.Empty, state)
    End Sub


    <TestMethod()>
    Public Sub GetStateCanCorrectlyReturnPending()
        Dim store As New FileStorageProvider(StoragePath, Long.MaxValue, Integer.MaxValue)
        Dim id = Guid.NewGuid
        store.ReserveId(id)
        Dim state = store.GetState(id)
        Assert.AreEqual(StorageIdState.Pending, state)
    End Sub

    <TestMethod()>
    Public Sub GetStateCanCorrectlyReturnOK()

        Dim store As New FileStorageProvider(StoragePath, Long.MaxValue, Integer.MaxValue)

        ' Populate the data store to trigger an exception
        Dim id = Guid.NewGuid
        IO.File.WriteAllText(store.DataFileName(id), String.Empty)
        IO.File.WriteAllText(store.MetadataFileName(id), String.Empty)

        Dim state = store.GetState(id)
        Assert.AreEqual(StorageIdState.OK, state)
    End Sub

    <TestMethod(), ExpectedException(GetType(StorageProviderDataCollisionException))>
    Public Sub StoreDataCanThrowDataCollisionException()

        Dim storageProvider As New FileStorageProvider(StoragePath, Long.MaxValue, Integer.MaxValue)
        Dim id = Guid.NewGuid

        ' Populate the data store to trigger an exception
        IO.File.WriteAllText(storageProvider.DataFileName(id), String.Empty)
        IO.File.WriteAllText(storageProvider.MetadataFileName(id), String.Empty)

        storageProvider.StoreData(id, New Dictionary(), New Byte() {})

    End Sub


    <TestMethod(), ExpectedException(GetType(StorageProviderIOException))>
    Public Sub StoreDataCanThrowIOException()

        Dim storageProvider As New FileStorageProvider(StoragePath, Long.MaxValue, Integer.MaxValue)
        Dim id = Guid.NewGuid

        ' Populate only part of the data store to trigger an IO exception
        IO.File.WriteAllText(storageProvider.DataFileName(id), String.Empty)

        storageProvider.StoreData(id, New Dictionary(), New Byte() {})

    End Sub

    <TestMethod()>
    Public Sub StoreDataWithoutAReservationCanSaveDataSuccessfully()

        Dim storageProvider As New FileStorageProvider(StoragePath, Long.MaxValue, Integer.MaxValue)
        Dim id = Guid.NewGuid

        Dim dataToStore = New Byte() {1, 2, 3, 4}
        Dim metadata = New Dictionary()

        'Fill in with some arbitrary data
        metadata.Add("DataWidth", dataToStore.Length)
        metadata.Add(Constants.CaptureDateKey, System.DateTime.Now)

        storageProvider.StoreData(id, metadata, dataToStore)

        Dim storedData = IO.File.ReadAllBytes(storageProvider.DataFileName(id))
        Dim storedMetadata = storageProvider.RetrieveMetadata(id)

        Assert.AreEqual(StorageIdState.OK, storageProvider.GetState(id))
        Assert.IsTrue(AreIdentical(metadata, storedMetadata, GetType(Dictionary)))
        Assert.IsTrue(dataToStore.SequenceEqual(storedData))

    End Sub

    <TestMethod()>
    Public Sub StoreDataWithAReservationCanSaveDataSuccessfully()

        Dim storageProvider As New FileStorageProvider(StoragePath, Long.MaxValue, Integer.MaxValue)
        Dim id = Guid.NewGuid

        Dim dataToStore = New Byte() {1, 2, 3, 4}
        Dim metadata = New Dictionary()

        'Fill in with some arbitrary data
        metadata.Add("DataWidth", dataToStore.Length)
        metadata.Add(Constants.CaptureDateKey, System.DateTime.Now)

        storageProvider.ReserveId(id)
        storageProvider.StoreData(id, metadata, dataToStore)

        Dim storedData = IO.File.ReadAllBytes(storageProvider.DataFileName(id))
        Dim storedMetadata = storageProvider.RetrieveMetadata(id)

        Assert.AreEqual(StorageIdState.OK, storageProvider.GetState(id))
        Assert.IsTrue(AreIdentical(metadata, storedMetadata, GetType(Dictionary)))
        Assert.IsTrue(dataToStore.SequenceEqual(storedData))

    End Sub


    <TestMethod()>
    Public Sub DataCanBeStoredAndRetrievedSuccessfully()

        Dim storageProvider As New FileStorageProvider(StoragePath, Long.MaxValue, Integer.MaxValue)
        Dim id = Guid.NewGuid

        Dim dataToStore = New Byte() {1, 2, 3, 4}
        storageProvider.StoreData(id, New Dictionary(), dataToStore)

        Dim retrievedData = storageProvider.RetrieveData(id)
        Assert.IsTrue(dataToStore.SequenceEqual(retrievedData))

    End Sub

    <TestMethod()>
    Public Sub MetadataCanBeStoredAndRetrievedSuccessfully()

        Dim storageProvider As New FileStorageProvider(StoragePath, Long.MaxValue, Integer.MaxValue)
        Dim id = Guid.NewGuid

        Dim dataToStore = New Byte() {1, 2, 3, 4}
        Dim metadata As New Dictionary

        'Fill in with some arbitrary data
        metadata.Add("DataWidth", dataToStore.Length)
        metadata.Add(Constants.CaptureDateKey, System.DateTime.Now)

        storageProvider.StoreData(id, metadata, dataToStore)

        Dim retrievedMetadata = storageProvider.RetrieveMetadata(id)
        Assert.IsTrue(AreIdentical(metadata, retrievedMetadata, GetType(Dictionary)))

    End Sub

    Public Function AreIdentical(ByVal A As Object, ByVal B As Object, ByVal T As Type) As Boolean
        If A IsNot Nothing AndAlso B IsNot Nothing AndAlso A.GetType() = T AndAlso B.GetType() = T Then     'not null and same type
            Dim dcs As New DataContractSerializer(T)

            Dim msA As New System.IO.MemoryStream()
            Dim msB As New System.IO.MemoryStream()

            dcs.WriteObject(msA, A)
            dcs.WriteObject(msB, B)

            msA.Position = 0
            Dim reader As New System.IO.StreamReader(msA)
            Dim sA As String = reader.ReadToEnd

            msB.Position = 0
            reader = New System.IO.StreamReader(msB)
            Dim sB As String = reader.ReadToEnd

            Return sA.Equals(sB)
        Else
            Return False
        End If

    End Function
End Class