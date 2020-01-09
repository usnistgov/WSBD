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

Option Infer On

Imports System.Runtime.Serialization
Imports System.Text

Namespace Nist.Bcl.Wsbd

    <CollectionDataContract(Name:="RangeCollection", ItemName:="range",
        Namespace:=Constants.WsbdNamespace)>
    Public Class RangeCollection
        Inherits List(Of RangeOrValue)

        Private mContentType As Type
        Private mContentTypeImplementsIComparable As Boolean

        Public Sub New()
            MyBase.New()
            ' Do nothing
        End Sub

        Public Sub New(ByVal ranges As RangeOrValue())
            For Each range In ranges
                Add(range)
            Next
        End Sub

        <DataMember(EmitDefaultValue:=False, Name:="name")> _
        Public Property Name As String


        Public ReadOnly Property ContentType As Type
            Get
                Return mContentType
            End Get
        End Property

        Private ReadOnly Property ImplementsIComparable() As Boolean
            Get
                Return mContentTypeImplementsIComparable
            End Get
        End Property

        Public Overloads Sub Add(ByVal range As RangeOrValue)
            ValidateType(range.ContentType())
            MyBase.Add(range)
        End Sub


        Private Sub ValidateType(ByVal t As Type)
            If mContentType Is Nothing Then
                mContentType = t
                mContentTypeImplementsIComparable = t.GetInterfaces().Contains(GetType(IComparable))
            Else
                If Not mContentType.Equals(t) Then
                    Throw New WsbdRangeTypeMismatchException
                End If
            End If

        End Sub

        Public Overloads Sub Sort()
            If mContentType Is Nothing Then Return

            If mContentType.GetInterfaces.Contains(GetType(IComparable)) Then
                MyBase.Sort(New WsbdRangeComparer)
            Else
                Throw New WsbdRangeNotComparableException
            End If

        End Sub

        Public Overrides Function ToString() As String
            Dim builder As New StringBuilder
            For i As Integer = 0 To Count - 1
                builder.Append(Item(i).ToString)
                builder.Append(Space(1))
            Next
            Return builder.ToString.Trim
        End Function


        Public Sub Consolidate()
            Dim tmp As New RangeCollection
            If mContentTypeImplementsIComparable Then
                For i As Integer = 0 To Count - 1
                    tmp.ConsolidatedAdd(Item(i))
                Next
            Else
                For i As Integer = 0 To Count - 1
                    tmp.ConsolidatedAddValue(Item(i))
                Next
            End If
            Clear()
            AddRange(tmp)
            If mContentTypeImplementsIComparable Then Sort()
        End Sub

        Private Sub ConsolidatedAddValue(ByVal value As RangeOrValue)
            If Not MyBase.Contains(value) Then Add(value)
        End Sub

        Public Sub ConsolidatedAdd(ByVal range As RangeOrValue)

            ' Gather a list of all of the ranges that overlap with the input
            Dim toMerge As New List(Of RangeOrValue)
            For i As Integer = 0 To Count - 1
                If RangeOrValue.AreOverlapping(Item(i), range) Or RangeOrValue.AreAdjacent(Item(i), range) Then
                    toMerge.Add(Item(i))
                End If

            Next

            ' Merge all of the overlapping intervals 
            For i As Integer = 0 To toMerge.Count - 1
                range = RangeOrValue.Union(range, toMerge(i))
                Remove(toMerge(i))
            Next

            Add(range)

        End Sub

        Public Function Includes(ByVal range As RangeOrValue) As Boolean
            ValidateType(range.ContentType)

            Dim result As Boolean = False
            For i As Integer = 0 To Count - 1
                If Item(i).Includes(range) Then
                    result = True
                    Exit For
                End If
            Next
            Return result
        End Function

        Public Function Includes(ByVal value As Object) As Boolean
            If value Is Nothing Then Throw New ArgumentNullException("value")
            Return Includes(New RangeOrValue(value))
        End Function



    End Class

End Namespace