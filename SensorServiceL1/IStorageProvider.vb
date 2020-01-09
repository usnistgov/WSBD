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

Namespace Nist.Bcl.Wsbd

    Public Interface IStorageProvider

        Sub ReserveId(ByVal id As Guid)
        ' Notify the provider to expect a data store to the target id. (Useful for when capture does
        ' not immediately produce a result.). Throws a DataCollisionException if there already
        ' a reservation or data for that id.

        Function GetState(ByVal id As Guid) As StorageIdState
        ' Get the state of the data associated with a particular id.
        'Function HasData(ByVal id As Guid) As Boolean
        ' Returns 'true' if this id has data stored in storage provider; returns 'false' otherwise. 
        ' May throw a StorageProviderIOException if the data is corrupt or if the underlying IO fails.

        Sub StoreData(ByVal id As Guid, ByVal metadata As Dictionary, ByVal data As Byte())
        ' Stores data to be associated with a particular 'id.' Throws an DataCollisionException if there is already data there

        Function RetrieveData(ByVal id As Guid) As Byte()
        ' Retrieves the data associated with a particular id. Throws an excpetion if the id is invalid

        Function RetrieveMetadata(ByVal id As Guid) As Dictionary
        ' Retrieves the mime type associated with a particular id.

        Sub Delete(id As Guid)
        ' Deletes all artifacts associated with the specified id.

    End Interface

    Public Enum StorageIdState
        Empty ' The id has neither a reservation nor any associated data 
        Pending ' The id has a reservation, but no associated data
        Corrupt ' The data for the id has been corrupted
        OK
    End Enum

    <Serializable()> Public MustInherit Class StorageProviderException
        Inherits Exception

        Public Sub New()
        End Sub

        Public Sub New(ByVal message As String)
            MyBase.New(message)
        End Sub

        Public Sub New(ByVal message As String, ByVal innerException As Exception)
            MyBase.New(message, innerException)
        End Sub

    End Class

    <Serializable()> Public Class InvalidStorageProviderIdException
        Inherits StorageProviderException

        Public Sub New()
        End Sub

        Public Sub New(ByVal message As String)
            MyBase.New(message)
        End Sub

        Public Sub New(ByVal message As String, ByVal innerException As Exception)
            MyBase.New(message, innerException)
        End Sub

    End Class

    <Serializable()> Public Class StorageProviderCapacityExceededException
        Inherits StorageProviderException

        Public Sub New()
        End Sub

        Public Sub New(ByVal message As String)
            MyBase.New(message)
        End Sub

        Public Sub New(ByVal message As String, ByVal innerException As Exception)
            MyBase.New(message, innerException)
        End Sub

    End Class

    <Serializable()> Public Class StorageProviderDataCollisionException
        Inherits StorageProviderException

        Public Sub New()
        End Sub

        Public Sub New(ByVal message As String)
            MyBase.new(message)
        End Sub

        Public Sub New(ByVal message As String, ByVal innerException As Exception)
            MyBase.New(message, innerException)
        End Sub

    End Class

    <Serializable()> Public Class StorageProviderIOException
        Inherits StorageProviderException

        Public Sub New()
        End Sub

        Public Sub New(ByVal message As String)
            MyBase.New(message)
        End Sub

        Public Sub New(ByVal message As String, ByVal innerException As Exception)
            MyBase.New(message, innerException)
        End Sub

    End Class



    ' - location
    ' - capacity in size
    ' - capacity in elements
    ' - cleanup?

End Namespace


