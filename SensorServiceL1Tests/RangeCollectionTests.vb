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

Imports System.ComponentModel
Imports System.Runtime.Serialization
Imports System.Security.Cryptography

Namespace Nist.Bcl.Wsbd.Experimental

    Public Class NonComparable
        Public Property Value As Integer

        Public Overrides Function Equals(ByVal other As Object) As Boolean
            If other Is Nothing Then Return False
            If Not TypeOf other Is NonComparable Then Return False
            Return other.Value = Value
        End Function

        Public Sub New(ByVal value As Integer)
            Me.Value = value
        End Sub

        Public Overrides Function ToString() As String
            Return Value.ToString
        End Function
    End Class

    <TestClass()>
    Public Class RangeCollectionTests

        <TestInitialize()>
        Public Sub TestInitialize()
            Randomize()
        End Sub

        Private Function GenerateRanges(ByVal seed As Integer, ByVal min As Integer,
                                        ByVal span As Integer, ByVal count As Integer) As List(Of RangeOrValue)

            Dim ranges As New List(Of RangeOrValue)

            Dim r As New Random(seed)
            For i As Integer = 0 To count - 1
                Dim isPoint = r.NextDouble < 0.5
                If isPoint Then
                    ranges.Add(New RangeOrValue(r.Next(min, min + span) * 1.0))
                Else
                    Dim minExclusive = r.NextDouble < 0.5
                    Dim maxExclusive = r.NextDouble < 0.5

                    Dim left = r.Next(min, min + span) * 1.0
                    Dim right = left + r.Next(1, span) * 1.0
                    ranges.Add(New RangeOrValue(left, right, minExclusive, maxExclusive))
                End If


            Next

            'Dim result(ranges.Count - 1) As WsbdRange
            'ranges.CopyTo(result)
            'Return result
            Return ranges
        End Function



        Private Function PermuteRanges(ByVal seed As Integer, ByVal source As List(Of RangeOrValue)) As RangeCollection
            Dim r As New Random(seed)
            Dim temp(source.Count - 1) As RangeOrValue
            source.CopyTo(temp)
            Dim result As New RangeCollection
            For Each range In New List(Of RangeOrValue)(temp).OrderBy(Function() r.NextDouble)
                result.Add(range)
            Next
            Return result
        End Function

        Private Function ToWsbdRangeCollection(ByVal source As List(Of RangeOrValue)) As RangeCollection
            Dim result As New RangeCollection
            For Each range In source
                result.Add(range)
            Next
            Return result
        End Function

        <TestMethod()>
        Public Sub WsbdRangeCollectionSortIsStable()
            Dim r As New Random

            Dim maximumNumberOfIntervals As Integer = 30 '250
            Dim numberOfSets = 100
            Dim numberOfPermutations = 50

            Dim intervalSize As Integer = 100

            For i As Integer = 0 To numberOfSets - 1

                Dim numberOfIntervals = r.Next(1, maximumNumberOfIntervals)
                Dim rangeSeed = r.Next

                ' Create a random set of ranges
                Dim baseline = GenerateRanges(rangeSeed, 0, intervalSize, numberOfIntervals)

                ' Sort the random set of ranges to make a baseline
                Dim sortedBaseline = ToWsbdRangeCollection(baseline)
                sortedBaseline.Sort()


                For j As Integer = 0 To numberOfPermutations - 1

                    ' Make a copy of the baseline and permute it randomly. 
                    Dim permutationSeed = r.Next
                    Dim permutation = PermuteRanges(permutationSeed, baseline)
                    permutation.Sort()

                    Dim errorMessage = String.Format("# intervals = {0}, range seed = {1}, permutation seed = {2}",
                                                     numberOfIntervals, rangeSeed, permutationSeed)
                    Assert.AreEqual(sortedBaseline.ToString, permutation.ToString, errorMessage)
                    permutation.Consolidate()

                Next

                Console.WriteLine(sortedBaseline)
                sortedBaseline.Consolidate()
                Console.WriteLine(sortedBaseline)
                Console.WriteLine()


            Next


        End Sub


        <TestMethod()>
        Public Sub WsbdRangeCollectionConsolidatesNonComparables()
            Dim ranges As New RangeCollection
            ranges.Add(New RangeOrValue(New NonComparable(10)))
            ranges.Add(New RangeOrValue(New NonComparable(10)))
            ranges.Add(New RangeOrValue(New NonComparable(10)))
            ranges.Add(New RangeOrValue(New NonComparable(8)))
            ranges.Add(New RangeOrValue(New NonComparable(7)))
            ranges.Add(New RangeOrValue(New NonComparable(10)))
            ranges.Consolidate()
            Assert.AreEqual("10 8 7".Trim, ranges.ToString.Trim)
        End Sub

        <TestMethod()>
        Public Sub WsbdRangeCollectionConsolidatesDoubleRanges()

            Dim expected, ranges As New RangeCollection

            ' Create a collection of ranges that once consolidated, look like the following.
            ' Redundant additions should be eliminated during the consolidation.
            '
            ' 1  2  3  4  5  6  7  8  9
            ' o--o--o  o--o--o  o--o--o
            '
            Dim r12 = New RangeOrValue(1.0, 2.0, True, True)
            Dim r23 = New RangeOrValue(2.0, 3.0, True, True)
            Dim r45 = New RangeOrValue(4.0, 5.0, True, True)
            Dim r56 = New RangeOrValue(5.0, 6.0, True, True)
            Dim r78 = New RangeOrValue(7.0, 8.0, True, True)
            Dim r89 = New RangeOrValue(8.0, 9.0, True, True)

            expected = New RangeCollection(New RangeOrValue() {r12, r23, r45, r56, r78, r89})

            ' Build the redundant collection of ranges; in an arbitrary order
            ranges = New RangeCollection
            For i As Integer = 0 To 10
                ranges.Add(New RangeOrValue(7.0, 8.0, True, True))
                ranges.Add(New RangeOrValue(8.0, 9.0, True, True))
                ranges.Add(New RangeOrValue(2.0, 3.0, True, True))
                ranges.Add(New RangeOrValue(1.0, 2.0, True, True))
                ranges.Add(New RangeOrValue(4.0, 5.0, True, True))
                ranges.Add(New RangeOrValue(5.0, 6.0, True, True))
            Next

            CollectionAssert.AreNotEqual(expected, ranges)
            ranges.Consolidate()
            CollectionAssert.AreEqual(expected, ranges)


            ' Recreate the collection of ranges, but add values at the open
            ' points 2, 5, and 8.
            '
            ' 1  2  3  4  5  6  7  8  9
            ' o-----o  o-----o  o-----o
            '
            Dim r13 = New RangeOrValue(1.0, 3.0, True, True)
            Dim r46 = New RangeOrValue(4.0, 6.0, True, True)
            Dim r79 = New RangeOrValue(7.0, 9.0, True, True)


            expected = New RangeCollection(New RangeOrValue() {r13, r46, r79})

            ' Build the redundant collection of ranges; in an arbitrary order
            ranges = New RangeCollection
            For i As Integer = 0 To 10
                ranges.Add(New RangeOrValue(7.0, 8.0, True, True))
                ranges.Add(New RangeOrValue(8.0, 9.0, True, True))
                ranges.Add(New RangeOrValue(2.0, 3.0, True, True))
                ranges.Add(New RangeOrValue(1.0, 2.0, True, True))
                ranges.Add(New RangeOrValue(4.0, 5.0, True, True))
                ranges.Add(New RangeOrValue(5.0, 6.0, True, True))
                ranges.Add(New RangeOrValue(2.0))
                ranges.Add(New RangeOrValue(8.0))
                ranges.Add(New RangeOrValue(5.0))
            Next

            CollectionAssert.AreNotEqual(expected, ranges)
            ranges.Consolidate()
            CollectionAssert.AreEqual(expected, ranges)


            ' Recreate the original collection of ranges, but add values at the open
            ' points 2, 5, and 8, and add closed intervals [3.0, 4.0] and [6.0, 7.0].
            '
            ' 1  2  3  4  5  6  7  8  9
            ' o-----------------------o
            '
            expected = New RangeCollection(New RangeOrValue() {New RangeOrValue(1.0, 9.0, True, True)})

            ' Build the redundant collection of ranges; in an arbitrary order
            ranges = New RangeCollection
            For i As Integer = 0 To 10
                ranges.Add(New RangeOrValue(7.0, 8.0, True, True))
                ranges.Add(New RangeOrValue(8.0, 9.0, True, True))
                ranges.Add(New RangeOrValue(3.0, 4.0, False, False))
                ranges.Add(New RangeOrValue(2.0, 3.0, True, True))
                ranges.Add(New RangeOrValue(1.0, 2.0, True, True))
                ranges.Add(New RangeOrValue(6.0, 7.0, False, False))
                ranges.Add(New RangeOrValue(4.0, 5.0, True, True))
                ranges.Add(New RangeOrValue(5.0, 6.0, True, True))
                ranges.Add(New RangeOrValue(2.0))
                ranges.Add(New RangeOrValue(8.0))
                ranges.Add(New RangeOrValue(5.0))
            Next

            CollectionAssert.AreNotEqual(expected, ranges)
            ranges.Consolidate()
            CollectionAssert.AreEqual(expected, ranges)


        End Sub

        <TestMethod()>
        Public Sub WsbdRangeCollectionConsolidatesIntegerRanges()
            Dim expected, ranges As RangeCollection

            ' This is a simple test to make sure that the consolidation
            ' of [i, i+1) (for i = 1 to 10) consolidates to [1, 10]. 

            expected = New RangeCollection()
            expected.Add(New RangeOrValue(1, 10, False, False))

            ranges = New RangeCollection
            For i As Integer = 1 To 100 ' test that redundant intervals are removed
                For j As Integer = 1 To 10
                    ranges.Add(New RangeOrValue(j, j + 1, False, True))
                Next
            Next

            CollectionAssert.AreNotEqual(expected, ranges)
            ranges.Consolidate()
            CollectionAssert.AreEqual(expected, ranges)


            ' Make sure that the consolidation of [i, i+1) (for i = 1 to 10)
            ' except for i=6 consolidates to [1, 5] [7, 10] 

            expected = New RangeCollection()
            expected.Add(New RangeOrValue(1, 5, False, False))
            expected.Add(New RangeOrValue(7, 10, False, False))

            ranges = New RangeCollection
            For i As Integer = 1 To 100 ' test that redundant intervals are removed
                For j As Integer = 1 To 10
                    If j = 6 Then Continue For
                    ranges.Add(New RangeOrValue(j, j + 1, False, True))
                Next
            Next

            CollectionAssert.AreNotEqual(expected, ranges)
            ranges.Consolidate()
            CollectionAssert.AreEqual(expected, ranges)



        End Sub


        <TestMethod()>
        Public Sub WsbdRangeIncludesWsbdRangeCorrectly()
            Dim range1 = New RangeOrValue(0, 5, False, False)
            Dim range2 = New RangeOrValue(7, 10, False, False)
            Dim ranges = New RangeCollection({range1, range2})

            For i As Integer = 0 To 10
                If i = 6 Then
                    Assert.IsFalse(ranges.Includes(New RangeOrValue(i)))
                Else
                    Assert.IsTrue(ranges.Includes(New RangeOrValue(i)))
                End If

            Next
        End Sub

        <TestMethod()>
        Public Sub WsbdRangeIncludesValueCorrectly()
            Dim range1 = New RangeOrValue(0, 5, False, False)
            Dim range2 = New RangeOrValue(7, 10, False, False)
            Dim ranges = New RangeCollection({range1, range2})

            For i As Integer = 0 To 10
                If i = 6 Then
                    Assert.IsFalse(ranges.Includes(i))
                Else
                    Assert.IsTrue(ranges.Includes(i))
                End If
            Next

        End Sub

        <TestMethod(), ExpectedException(GetType(WsbdRangeTypeMismatchException))>
        Public Sub WsbdRangeIncludesCanThrowTypeMismatch()
            Dim range1 = New RangeOrValue(0, 5, False, False)
            Dim range2 = New RangeOrValue(7, 10, False, False)
            Dim ranges = New RangeCollection({range1, range2})
            ranges.Includes(New RangeOrValue(1.0))
        End Sub


    End Class

End Namespace
