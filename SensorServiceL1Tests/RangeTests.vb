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

Imports System.Runtime.Serialization
Imports System.Text
Imports System.Xml

Namespace Nist.Bcl.Wsbd.Experimental

    <TestClass()>
    Public Class RangeTests

        ' Documentation of the tests may use the following shorthand to describe
        ' the relative order of two ranges.
        '
        ' L- : Less than min
        ' L= : Equal to min
        ' M  : Between min and max
        ' R= : Equal to max
        ' R+ : Greater than max
        '
        ' For example, (L=, M) means a relative order where :
        '   - the min of the first argument is less than the min of the second argument
        '   - the max of the first argument is beteween the min and max of the second argument

        ' The 'best written' test is probably the union


        <TestMethod()>
        Sub WsbdRangeCanBeConstructedFromSingleValue()
            Dim integerValue = New RangeOrValue(0)
            Dim floatValue = New RangeOrValue(0.0!)
            Dim longValue = New RangeOrValue(0L)
            Dim stringValue = New RangeOrValue("Hello!")
            Dim objectValue = New RangeOrValue(New Object)
        End Sub

        <TestMethod(), ExpectedException(GetType(ArgumentNullException))>
        Sub WsbdRangeCannotBeConstructedFromNullValue()
            Dim r = New RangeOrValue(Nothing)
        End Sub

        <TestMethod(), ExpectedException(GetType(ArgumentNullException))>
        Sub WsbdRangeCannotBeConstructedFromNullValues()
            Dim r = New RangeOrValue(Nothing, Nothing, True, True)
        End Sub

        <TestMethod(), ExpectedException(GetType(WsbdRangesAreImmutableException))>
        Sub WsbdRangeValuePropertyEnforcesImmutability()
            Dim r = New RangeOrValue
            r.Value = 1
            r.Value = 1
        End Sub

        <TestMethod(), ExpectedException(GetType(WsbdRangeTypeMismatchException))>
        Sub WsbdRangeCannotBeMixedTypes()
            Dim mixedRange = New RangeOrValue(0, 1L, True, True)
        End Sub

        <TestMethod()>
        Sub WsbdRangeCanBeConstructedFromPairs()
            Dim integerRange = New RangeOrValue(0, 10, True, True)
            Dim floatRange = New RangeOrValue(0.0!, 1.0!, False, False)
            Dim longRange = New RangeOrValue(0L, 1L, False, True)
            Dim stringRange = New RangeOrValue("a", "z", False, False)
        End Sub

        <TestMethod(), ExpectedException(GetType(WsbdRangesAreImmutableException))>
        Sub WsbdRangeMinimumEnforcesImmutability()
            Dim r = New RangeOrValue
            r.Minimum = 0
            r.Minimum = 0
        End Sub

        <TestMethod(), ExpectedException(GetType(WsbdRangesAreImmutableException))>
        Sub WsbdRangeMaximumEnforcesImmutability()
            Dim r = New RangeOrValue
            r.Maximum = 0
            r.Maximum = 0
        End Sub

        <TestMethod(), ExpectedException(GetType(MalformedWsbdRangeException))>
        Sub WsbdRangeMinAndMaxMusftBeInCorrectOrder()
            Dim stringRange = New RangeOrValue("z", "a", False, False)
        End Sub

        <TestMethod()>
        Sub WsbdRangeConstructorCanCastFromRangeToValue()
            Dim r = New RangeOrValue(0, 0, False, False)
            Assert.AreEqual(New RangeOrValue(0), r)
        End Sub

        <TestMethod(), ExpectedException(GetType(MalformedWsbdRangeException))>
        Sub WsbdRangeConstructorCannotCastInvalidRangeToValue()
            Dim r = New RangeOrValue(0, 0, False, True)
        End Sub

        <TestMethod(), ExpectedException(GetType(ArgumentNullException))>
        Sub WsbdRangeValueConstructorCannotAcceptNull()
            Dim r = New RangeOrValue(Nothing)
        End Sub

        <TestMethod(), ExpectedException(GetType(WsbdRangeNotComparableException))>
        Sub RangedWsbdRangesMustUseIComparables()
            Dim objectRange = New RangeOrValue(New Object, New Object, False, False)
        End Sub

        <TestMethod()>
        Sub WsbdRangesCanReturnMinAndMaxValues()
            Dim r = New RangeOrValue
            r.Minimum = 0
            r.Maximum = 1
            r.MinimumIsExclusive = False
            r.MaximumIsExclusive = False
            Assert.AreEqual(0, r.Minimum)
            Assert.AreEqual(1, r.Maximum)
        End Sub

        <TestMethod(), ExpectedException(GetType(MalformedWsbdRangeException))>
        Sub WsbdRangeThrowExceptionsWhenValueIsOverpopulated()
            Dim r = New RangeOrValue(0, 1, False, False)
            r.Value = 2
        End Sub


        <TestMethod(), ExpectedException(GetType(WsbdRangesAreImmutableException))>
        Sub WsbdRangeCannotBeChangedFromValueToRangeBySettingTheMinimum()
            Dim r = New RangeOrValue(0)
            r.Minimum = 2
        End Sub

        <TestMethod(), ExpectedException(GetType(WsbdRangesAreImmutableException))>
        Sub WsbdRangeCannotBeChangedFromValueToRangeBySettingTheMaximum()
            Dim r = New RangeOrValue(0)
            r.Maximum = 2
        End Sub

        <TestMethod()>
        Sub WsbdRangeCanReturnValue()
            Dim r = New RangeOrValue(0)
            Assert.AreEqual(0, r.Value)
        End Sub

        <TestMethod(), ExpectedException(GetType(WsbdRangeTypeMismatchException))>
        Sub WsbdRangeCannotAcceptMixedTypes()
            Dim r As New RangeOrValue(0, 0.0!, True, True)
        End Sub

        <TestMethod()>
        Sub WsbdRangeTracksContentTypeCorrectly()
            Dim r As New RangeOrValue(0)
            Assert.AreEqual(GetType(Integer), r.ContentType)
        End Sub

        <TestMethod(), ExpectedException(GetType(ArgumentNullException))>
        Sub Argument1OfWsbdRangeAreComparableCannotBeNull()
            RangeOrValue.AreComparable(Nothing, New RangeOrValue(0))
        End Sub

        <TestMethod(), ExpectedException(GetType(ArgumentNullException))>
        Sub Argument2OfWsbdRangeAreComparableCannotBeNull()
            RangeOrValue.AreComparable(New RangeOrValue(0), Nothing)
        End Sub

        <TestMethod()>
        Sub RelativeOrderOfSourceMinimumIsCorrect()

            Dim r1, r2 As RangeOrValue


            ' [a] vs [b] where a > b
            r1 = New RangeOrValue(6.0)
            r2 = New RangeOrValue(5.0)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, RangeOrValue.GetRelativeOrder(r1, r2).MinimumOrder)

            ' (a vs a)
            r1 = New RangeOrValue(5.0, 6.0, True, True) ' (5, 6)
            r2 = New RangeOrValue(0.0, 5.0, True, True) ' (0, 5)

            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, RangeOrValue.GetRelativeOrder(r1, r2).MinimumOrder)

            ' [a vs a]
            r2 = New RangeOrValue(0.0, 5.0, True, False) ' (0, 5]
            r1 = New RangeOrValue(5.0, 6.0, False, True) ' [5, 6)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, RangeOrValue.GetRelativeOrder(r1, r2).MinimumOrder)

            ' (a vs a]
            r1 = New RangeOrValue(5.0, 6.0, True, True) ' (5, 6)
            r2 = New RangeOrValue(0.0, 5.0, True, False) ' (0, 5]
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, RangeOrValue.GetRelativeOrder(r1, r2).MinimumOrder)

            ' [a vs a)
            r1 = New RangeOrValue(5.0, 6.0, False, True) ' [5, 6)
            r2 = New RangeOrValue(0.0, 5.0, True, True) ' (0, 5)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, RangeOrValue.GetRelativeOrder(r1, r2).MinimumOrder)

            ' {a, b} vs {*, c} where a < c < b
            r1 = New RangeOrValue(3.0, 8.0, False, False) ' [3, 8]
            r2 = New RangeOrValue(0.0, 5.0, True, True) ' (0, 5)
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, RangeOrValue.GetRelativeOrder(r1, r2).MinimumOrder)

            ' (a vs (a
            r1 = New RangeOrValue(5.0, 8.0, True, True) ' (5, 8)
            r2 = New RangeOrValue(5.0, 10.0, True, True) ' (5, 10)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, RangeOrValue.GetRelativeOrder(r1, r2).MinimumOrder)

            ' (a vs [a
            r1 = New RangeOrValue(5.0, 8.0, True, True) ' (5, 8)
            r2 = New RangeOrValue(5.0, 10.0, False, True) ' [5, 10)
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, RangeOrValue.GetRelativeOrder(r1, r2).MinimumOrder)

            ' (a vs [a]
            r1 = New RangeOrValue(5.0, 8.0, True, True) ' (5, 8)
            r2 = New RangeOrValue(5.0) ' [5]
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, RangeOrValue.GetRelativeOrder(r1, r2).MinimumOrder)

            ' [a vs (a
            r1 = New RangeOrValue(5.0, 8.0, False, True) ' [5, 8)
            r2 = New RangeOrValue(5.0, 10.0, True, True) ' (5, 10)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, RangeOrValue.GetRelativeOrder(r1, r2).MinimumOrder)

            ' [a vs [a
            r1 = New RangeOrValue(5.0, 8.0, False, True) ' [5, 8)
            r2 = New RangeOrValue(5.0, 10.0, False, True) ' [5, 10)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, RangeOrValue.GetRelativeOrder(r1, r2).MinimumOrder)

            ' [a] vs [b] where a < b
            r1 = New RangeOrValue(5.0)
            r2 = New RangeOrValue(6.0)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, RangeOrValue.GetRelativeOrder(r1, r2).MinimumOrder)

        End Sub


        <TestMethod()>
        Sub RelativeOrderOfSourceMaximumIsCorrect()

            '
            ' This test should excersize all of the comparisons done with the source max.
            '

            Dim r1, r2 As RangeOrValue

            ' [a] vs [b] where a < b
            r1 = New RangeOrValue(4.0) ' [4]
            r2 = New RangeOrValue(5.0) ' [5]
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, RangeOrValue.GetRelativeOrder(r1, r2).MaximumOrder)

            ' a) vs (a
            r1 = New RangeOrValue(0.0, 5.0, True, True) ' (0, 5)
            r2 = New RangeOrValue(5.0, 10.0, True, True) ' (5, 10)

            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, RangeOrValue.GetRelativeOrder(r1, r2).MaximumOrder)

            ' a) vs [a
            r1 = New RangeOrValue(0.0, 5.0, True, True) ' (0, 5)
            r2 = New RangeOrValue(5.0, 10.0, False, True) ' [5, 10)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, RangeOrValue.GetRelativeOrder(r1, r2).MaximumOrder)

            ' a] vs (a
            r1 = New RangeOrValue(0.0, 5.0, True, False) ' (0, 5]
            r2 = New RangeOrValue(5.0, 10.0, True, True) ' (5, 10)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, RangeOrValue.GetRelativeOrder(r1, r2).MaximumOrder)

            ' a] vs [a
            r1 = New RangeOrValue(0.0, 5.0, True, False) ' (0, 5]
            r2 = New RangeOrValue(5.0, 10.0, False, True) ' [5, 10)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, RangeOrValue.GetRelativeOrder(r1, r2).MaximumOrder)

            ' [a, b] vs c} where c > b
            r1 = New RangeOrValue(1.0, 3.0, False, False) ' [1, 3]
            r2 = New RangeOrValue(0.0, 5.0, True, True) ' (0, 5)
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, RangeOrValue.GetRelativeOrder(r1, r2).MaximumOrder)

            ' a) vs a)
            r1 = New RangeOrValue(8.0, 10.0, True, True) ' (8, 10)
            r2 = New RangeOrValue(5.0, 10.0, True, True) ' (5, 10)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, RangeOrValue.GetRelativeOrder(r1, r2).MaximumOrder)

            ' a) vs a]
            r1 = New RangeOrValue(8.0, 10.0, True, True) ' (8, 10)
            r2 = New RangeOrValue(5.0, 10.0, True, False) ' (5, 10]
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, RangeOrValue.GetRelativeOrder(r1, r2).MaximumOrder)

            ' a) vs [a]
            r1 = New RangeOrValue(5.0, 8.0, True, True) ' (5, 8)
            r2 = New RangeOrValue(8.0) ' [8]
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, RangeOrValue.GetRelativeOrder(r1, r2).MaximumOrder)

            ' a] vs [a]
            r1 = New RangeOrValue(5.0, 8.0, True, False) ' (5, 8]
            r2 = New RangeOrValue(8.0) ' [8]
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, RangeOrValue.GetRelativeOrder(r1, r2).MaximumOrder)

            ' a] vs a)
            r1 = New RangeOrValue(8.0, 10.0, True, False) ' (8, 10]
            r2 = New RangeOrValue(5.0, 10.0, True, True) ' (5, 10)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, RangeOrValue.GetRelativeOrder(r1, r2).MaximumOrder)

            ' a] vs a]
            r2 = New RangeOrValue(5.0, 10.0, True, False) ' (5, 10]
            r1 = New RangeOrValue(8.0, 10.0, True, False) ' (8, 10]
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, RangeOrValue.GetRelativeOrder(r1, r2).MaximumOrder)

            ' [a] vs [b] where a > b
            r1 = New RangeOrValue(5.0) '[5]
            r2 = New RangeOrValue(4.0) '[4]
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, RangeOrValue.GetRelativeOrder(r1, r2).MaximumOrder)

        End Sub

        <TestMethod()>
        Sub WsbdRangesCompareCorrectly()

            Dim r1, r2 As RangeOrValue

            ' [6, 10] vs. [0, 5]
            r1 = New RangeOrValue(6.0, 10.0, False, False)
            r2 = New RangeOrValue(0.0, 5.0, False, False)
            Assert.AreEqual(-1, RangeOrValue.Compare(r2, r1))
            Assert.AreEqual(1, RangeOrValue.Compare(r1, r2))

            ' [6, 10] vs. [0, 6]
            r1 = New RangeOrValue(6.0, 10.0, False, False)
            r2 = New RangeOrValue(0.0, 6.0, False, False)
            Assert.AreEqual(-1, RangeOrValue.Compare(r2, r1))
            Assert.AreEqual(1, RangeOrValue.Compare(r1, r2))

            ' (6, 10] vs. [0, 6]
            r1 = New RangeOrValue(6.0, 10.0, True, False)
            r2 = New RangeOrValue(0.0, 6.0, False, False)
            Assert.AreEqual(-1, RangeOrValue.Compare(r2, r1))
            Assert.AreEqual(1, RangeOrValue.Compare(r1, r2))

            ' [6, 10] vs. [0, 6)
            r1 = New RangeOrValue(6.0, 10.0, False, False)
            r2 = New RangeOrValue(0.0, 6.0, False, True)
            Assert.AreEqual(-1, RangeOrValue.Compare(r2, r1))
            Assert.AreEqual(1, RangeOrValue.Compare(r1, r2))

            ' (6, 10] vs. [0, 6)
            r1 = New RangeOrValue(6.0, 10.0, True, False)
            r2 = New RangeOrValue(0.0, 6.0, False, True)
            Assert.AreEqual(-1, RangeOrValue.Compare(r2, r1))
            Assert.AreEqual(1, RangeOrValue.Compare(r1, r2))

            ' [6, 10] vs [0, 8]
            r1 = New RangeOrValue(6.0, 10.0, False, False)
            r2 = New RangeOrValue(0.0, 8.0, False, False)
            Assert.AreEqual(-1, RangeOrValue.Compare(r2, r1))
            Assert.AreEqual(1, RangeOrValue.Compare(r1, r2))

            ' [6, 10] vs [6, 8]
            r1 = New RangeOrValue(6.0, 10.0, False, False)
            r2 = New RangeOrValue(6.0, 8.0, False, False)
            Assert.AreEqual(-1, RangeOrValue.Compare(r2, r1))
            Assert.AreEqual(1, RangeOrValue.Compare(r1, r2))

            ' (6, 10] vs [6, 8]
            r1 = New RangeOrValue(6, 10, True, False)
            r2 = New RangeOrValue(6, 8, False, False)
            Assert.AreEqual(-1, RangeOrValue.Compare(r2, r1))
            Assert.AreEqual(1, RangeOrValue.Compare(r1, r2))

            ' (6, 10] vs (6, 8]
            r1 = New RangeOrValue(6, 10, True, False)
            r2 = New RangeOrValue(6, 8, True, False)
            Assert.AreEqual(-1, RangeOrValue.Compare(r2, r1))
            Assert.AreEqual(1, RangeOrValue.Compare(r1, r2))

            ' [6, 10] vs [6]
            r1 = New RangeOrValue(6, 10, False, False)
            r2 = New RangeOrValue(6)
            Assert.AreEqual(-1, RangeOrValue.Compare(r2, r1))
            Assert.AreEqual(1, RangeOrValue.Compare(r1, r2))
            Assert.AreEqual(0, RangeOrValue.Compare(r2, r2))
            Assert.AreEqual(0, RangeOrValue.Compare(r1, r1))

            ' [6, 10] vs [10]
            r1 = New RangeOrValue(6, 10, False, False)
            r2 = New RangeOrValue(10)
            Assert.AreEqual(1, RangeOrValue.Compare(r2, r1))
            Assert.AreEqual(-1, RangeOrValue.Compare(r1, r2))

            ' (6, 10] vs [10]
            r1 = New RangeOrValue(6, 10, True, False)
            r2 = New RangeOrValue(10)
            Assert.AreEqual(1, RangeOrValue.Compare(r2, r1))
            Assert.AreEqual(-1, RangeOrValue.Compare(r1, r2))

            ' [6, 10) vs [10]
            r1 = New RangeOrValue(6, 10, False, True)
            r2 = New RangeOrValue(10)
            Assert.AreEqual(1, RangeOrValue.Compare(r2, r1))
            Assert.AreEqual(-1, RangeOrValue.Compare(r1, r2))

            ' [6, 10] vs [0, 10]
            r1 = New RangeOrValue(6, 10, False, False)
            r2 = New RangeOrValue(0, 10, False, False)
            Assert.AreEqual(-1, RangeOrValue.Compare(r2, r1))
            Assert.AreEqual(1, RangeOrValue.Compare(r1, r2))

            ' [6, 10] vs [0, 10)
            r1 = New RangeOrValue(6, 10, False, False)
            r2 = New RangeOrValue(0, 10, False, True)
            Assert.AreEqual(-1, RangeOrValue.Compare(r2, r1))
            Assert.AreEqual(1, RangeOrValue.Compare(r1, r2))

            ' [6, 10) vs [0, 10]
            r1 = New RangeOrValue(6, 10, False, True)
            r2 = New RangeOrValue(0, 10, False, False)
            Assert.AreEqual(-1, RangeOrValue.Compare(r2, r1))
            Assert.AreEqual(1, RangeOrValue.Compare(r1, r2))

            ' [6, 10) vs [0, 10)
            r1 = New RangeOrValue(6, 10, False, True)
            r2 = New RangeOrValue(0, 10, False, True)
            Assert.AreEqual(-1, RangeOrValue.Compare(r2, r1))
            Assert.AreEqual(1, RangeOrValue.Compare(r1, r2))

            ' [6, 10] vs [0, 12]
            r1 = New RangeOrValue(6, 10, False, False)
            r2 = New RangeOrValue(0, 12, False, False)
            Assert.AreEqual(-1, RangeOrValue.Compare(r2, r1))
            Assert.AreEqual(1, RangeOrValue.Compare(r1, r2))

        End Sub



        Private Shared Function ToDataContractXml(ByVal source As RangeOrValue) As String

            Dim builder As New StringBuilder
            Dim serializer As New DataContractSerializer(GetType(RangeOrValue))
            Using writer = XmlWriter.Create(builder)
                serializer.WriteObject(writer, source)
                writer.Flush()
            End Using
            Return builder.ToString

        End Function

        <TestMethod()>
        Sub WsbdRangeIsDataContractSerializable()

            Dim integerValue = ToDataContractXml(New RangeOrValue(0))
            Dim floatValue = ToDataContractXml(New RangeOrValue(0.0!))
            Dim longValue = ToDataContractXml(New RangeOrValue(0L))
            Dim stringValue = ToDataContractXml(New RangeOrValue("Hello!"))

            Dim integerRange = ToDataContractXml(New RangeOrValue(0, 1, False, False))
            Dim floatRange = ToDataContractXml(New RangeOrValue(0.0!, 1.0!, False, True))
            Dim longRange = ToDataContractXml(New RangeOrValue(0L, 1L, True, False))
            Dim stringRange = ToDataContractXml(New RangeOrValue("Hello!", "world.", True, True))


        End Sub

        <TestMethod()>
        Sub AbiguousWsbdRangesResolvePredictably()

            Dim r1, r2 As RangeOrValue
            Dim order As RangeOrValue.RelativeOrderResult

            ' For point vs point, (L=, L=), (R=, R=) and (L=, R=) all resolve to (L=, R=)
            r1 = New RangeOrValue(0.0)
            r2 = New RangeOrValue(0.0)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MaximumOrder)

            ' For range (arg1) vs point (arg2), (R=, R+) resolves to (L=, R+)
            r1 = New RangeOrValue(5.0, 10.0, False, False)
            r2 = New RangeOrValue(5.0)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)

            ' For range (arg1) vs point (arg2), (L-, L=) resolves to (L-, R=)
            r1 = New RangeOrValue(5.0, 10.0, False, False)
            r2 = New RangeOrValue(10.0)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MaximumOrder)




        End Sub


        <TestMethod()>
        Sub WsbdRangeEqualsIsCorrect()

            Dim r1, r2 As RangeOrValue

            r1 = New RangeOrValue(0, 5, True, True)
            r2 = New RangeOrValue(0, 5, True, True)
            Assert.AreEqual(r1, r2)

            r1 = New RangeOrValue(0, 5, True, True)
            r2 = New RangeOrValue(1, 5, True, True)
            Assert.AreNotEqual(r1, r2)

            r1 = New RangeOrValue(0, 5, True, True)
            r2 = New RangeOrValue(0, 4, True, True)
            Assert.AreNotEqual(r1, r2)


            r1 = New RangeOrValue(0, 5, True, True)
            r2 = New RangeOrValue(0, 5, False, True)
            Assert.AreNotEqual(r1, r2)

            r1 = New RangeOrValue(0, 5, True, True)
            r2 = New RangeOrValue(0, 5, True, False)
            Assert.AreNotEqual(r1, r2)

            r1 = New RangeOrValue("a")
            r2 = New RangeOrValue("a")
            Assert.AreEqual(r1, r2)

            r1 = New RangeOrValue("a")
            r2 = New RangeOrValue("b")
            Assert.AreNotEqual(r1, r2)

            r1 = New RangeOrValue("fa")
            r2 = New RangeOrValue(0)
            Assert.AreNotEqual(r1, r2)

            r1 = New RangeOrValue(0)
            r2 = Nothing
            Assert.AreNotEqual(r1, r2)

            r1 = New RangeOrValue(0)
            r2 = New RangeOrValue(0, 1, False, False)
            Assert.AreNotEqual(r1, r2)

            Assert.AreNotEqual(New RangeOrValue(0), "foo")

        End Sub


        <TestMethod(), ExpectedException(GetType(ArgumentNullException))>
        Sub Argument1OfWsbdRangeAreAdjacentCannotBeNull()
            RangeOrValue.AreAdjacent(Nothing, New RangeOrValue(0))
        End Sub

        <TestMethod(), ExpectedException(GetType(ArgumentNullException))>
        Sub Argument2OfWsbdRangeAreAdjacentCannotBeNull()
            RangeOrValue.AreAdjacent(New RangeOrValue(0), Nothing)
        End Sub


        <TestMethod()>
        Sub WsbdRangesDeterminesAdjacencyCorrectly()

            Dim r1, r2, u As RangeOrValue
            Dim order As RangeOrValue.RelativeOrderResult

            ' 01. (L=, R=) with two ranges
            r1 = New RangeOrValue(0, 5, True, True)
            order = RangeOrValue.GetRelativeOrder(r1, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreAdjacent(r1, r1))

            ' 02. (L=, R=) with two points
            r1 = New RangeOrValue(0)
            order = RangeOrValue.GetRelativeOrder(r1, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreAdjacent(r1, r1))

            ' 03. (L=, M) with two ranges
            r1 = New RangeOrValue(0.0, 5.0, True, False)
            r2 = New RangeOrValue(0.0, 10.0, True, True)
            u = New RangeOrValue(0.0, 10.0, True, True)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreAdjacent(r1, r2))

            ' 04. (L=, M) with two ranges inverts to (L=, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreAdjacent(r2, r1))

            ' 05. (L=, R+) with range (arg1) and point (arg2)
            r1 = New RangeOrValue(0.0, 10.0, False, True) ' [0, 10)
            r2 = New RangeOrValue(0.0) ' [0]
            u = New RangeOrValue(0.0, 10.0, False, True)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreAdjacent(r1, r2))

            ' 06. (L=, R+) with range (arg1) and point (arg2) inverts to (L=, L=)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreAdjacent(r2, r1))

            ' 07. (R=, R+) with two ranges
            r1 = New RangeOrValue(5.0, 10.0, False, True) ' [5, 10)
            r2 = New RangeOrValue(0.0, 5.0, True, False) ' (0, 5]
            u = New RangeOrValue(0.0, 10.0, True, True)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreAdjacent(r1, r2))

            ' 08. (R=, R+) with two ranges inverts to (L-, L=)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreAdjacent(r2, r1))


            ' 09. (R=, R=) with point (arg1) and range (arg2)
            r1 = New RangeOrValue(10.0) ' [10]
            r2 = New RangeOrValue(0.0, 10.0, True, False) ' (0, 10]
            u = New RangeOrValue(0.0, 10.0, True, False) ' (0, 10]
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreAdjacent(r1, r2))

            ' 10. (R=, R=) with point (arg1) and range (arg2) inverts to (L-, R=)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreAdjacent(r2, r1))

            ' 11. (L-, R=) with two ranges
            r1 = New RangeOrValue(0, 10, False, True) ' [0, 10)
            r2 = New RangeOrValue(6, 10, False, True) ' [6, 10)
            u = New RangeOrValue(0, 10, False, True) ' [0, 10)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreAdjacent(r1, r2))

            ' 12. (L-, R=) with two ranges inverts to (M, R=)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreAdjacent(r2, r1))

            ' 13a. (L-, L-) with two points 
            r1 = New RangeOrValue(0) ' [6]
            r2 = New RangeOrValue(6) ' [8]
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreAdjacent(r1, r2))

            ' 14a. 13a. inverts to (R+, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreAdjacent(r2, r1))

            ' 13b. (L-, L-) with two non-adjacent (floating point) ranges 
            r1 = New RangeOrValue(0.0, 5.0, False, False) ' [0, 5]
            r2 = New RangeOrValue(6.0, 10.0, False, False) ' [6, 10]
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreAdjacent(r1, r2))

            ' 14b. 13b. inverts to (R+, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreAdjacent(r2, r1))


            ' 13c. (L-, L-) with two floating-point ranges, one apart
            r1 = New RangeOrValue(0.0, 5.0, False, False) ' [0, 5]
            r2 = New RangeOrValue(6.0, 10.0, False, False) ' [6, 10]
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreAdjacent(r1, r2))

            ' 14c. 13c inverts to (R+, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreAdjacent(r2, r1))

            ' 13d. (L-, L-) with two ranges adjacent with a closed endpoint less than an open endpoint
            r1 = New RangeOrValue(0.0, 6.0, True, False) ' (0, 6]
            r2 = New RangeOrValue(6.0, 10.0, True, True) ' (6, 10)
            u = New RangeOrValue(0.0, 10.0, True, True)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MaximumOrder)
            Assert.IsTrue(RangeOrValue.AreAdjacent(r1, r2))

            ' 14d. 13d inverts to (R+, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.IsTrue(RangeOrValue.AreAdjacent(r2, r1))

            ' 13e. (L-, L-) with two ranges adjacent with open endpoint less than closed endpoint
            r1 = New RangeOrValue(0.0, 6.0, True, True) ' (0, 6)
            r2 = New RangeOrValue(6.0, 10.0, False, True) ' [6, 10)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MaximumOrder)
            Assert.IsTrue(RangeOrValue.AreAdjacent(r1, r2))

            ' 14e. 13e inverts to (R+, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.IsTrue(RangeOrValue.AreAdjacent(r2, r1))

            ' 13f. (L-, L-) with two ranges adjacent with open endpoint adjacent to point
            r1 = New RangeOrValue(0.0, 5.0, True, True) ' (0, 5)
            r2 = New RangeOrValue(5.0) ' [5]
            u = New RangeOrValue(0.0, 5.0, True, False)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MaximumOrder)
            Assert.IsTrue(RangeOrValue.AreAdjacent(r1, r2))

            ' 14f. 13f inverts to (R+, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.IsTrue(RangeOrValue.AreAdjacent(r2, r1))

            ' 13g. (L-, L-) with ranges with open endpoint adjacent to point at lower endpoint
            r1 = New RangeOrValue(0.0) ' [5]
            r2 = New RangeOrValue(0.0, 5.0, True, True) ' (0, 5)
            u = New RangeOrValue(0.0, 5.0, False, True)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MaximumOrder)
            Assert.IsTrue(RangeOrValue.AreAdjacent(r1, r2))

            ' 14g. 13g inverts to (R+, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.IsTrue(RangeOrValue.AreAdjacent(r2, r1))


            ' 15. (L-, M) with two ranges
            r1 = New RangeOrValue(0.0, 6.0, False, True) ' [0, 6)
            r2 = New RangeOrValue(5.0, 10.0, False, True) ' [5, 10)
            u = New RangeOrValue(0.0, 10.0, False, True) ' [0, 10)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreAdjacent(r1, r2))

            ' 16. (L-, M) with two ranges inverts to (M, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreAdjacent(r2, r1))

            ' 17. (M, M) with two ranges
            r1 = New RangeOrValue(6, 10, True, True) ' (6, 10)
            r2 = New RangeOrValue(6, 10, False, False) ' [6, 10]
            u = New RangeOrValue(6, 10, False, False)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreAdjacent(r1, r2))

            ' 18. (M, M) with two ranges inverts to (L-, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreAdjacent(r2, r1))




        End Sub


        <TestMethod()>
        Sub WsbdRangeUnionWithNullAsFirstArgumentYieldsFirstArgument()
            Assert.AreEqual(New RangeOrValue(0), RangeOrValue.Union(Nothing, New RangeOrValue(0)))
        End Sub

        <TestMethod()>
        Sub WsbdRangeUnionWithNullAsSecondArgumentYieldsSecondArgument()
            Assert.AreEqual(New RangeOrValue(0), RangeOrValue.Union(New RangeOrValue(0), Nothing))
        End Sub

        <TestMethod()>
        Sub WsbdRangeUnionWithTwoNullArgumentsYieldsNull()
            Assert.AreEqual(Nothing, RangeOrValue.Union(Nothing, Nothing))
        End Sub

        <TestMethod()>
        Sub WsbdRangesUnionCorrectly()

            Dim r1, r2, u As RangeOrValue
            Dim order As RangeOrValue.RelativeOrderResult

            ' 01. (L=, R=) with two ranges
            r1 = New RangeOrValue(0, 5, True, True)
            order = RangeOrValue.GetRelativeOrder(r1, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MaximumOrder)
            Assert.AreEqual(r1, RangeOrValue.Union(r1, r1))

            ' 02. (L=, R=) with two points
            r1 = New RangeOrValue(0)
            order = RangeOrValue.GetRelativeOrder(r1, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MaximumOrder)
            Assert.AreEqual(r1, RangeOrValue.Union(r1, r1))

            ' 03. (L=, M) with two ranges
            r1 = New RangeOrValue(0.0, 5.0, True, False)
            r2 = New RangeOrValue(0.0, 10.0, True, True)
            u = New RangeOrValue(0.0, 10.0, True, True)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, order.MaximumOrder)
            Assert.AreEqual(u, RangeOrValue.Union(r1, r2))

            ' 04. (L=, M) with two ranges inverts to (L=, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.AreEqual(u, RangeOrValue.Union(r2, r1))

            ' 05. (L=, R+) with range (arg1) and point (arg2)
            r1 = New RangeOrValue(0.0, 10.0, False, True) ' [0, 10)
            r2 = New RangeOrValue(0.0) ' [0]
            u = New RangeOrValue(0.0, 10.0, False, True)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.AreEqual(u, RangeOrValue.Union(r1, r2))

            ' 06. (L=, R+) with range (arg1) and point (arg2) inverts to (L=, L=)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MaximumOrder)
            Assert.AreEqual(u, RangeOrValue.Union(r2, r1))

            ' 07. (R=, R+) with two ranges
            r1 = New RangeOrValue(5.0, 10.0, False, True) ' [5, 10)
            r2 = New RangeOrValue(0.0, 5.0, True, False) ' (0, 5]
            u = New RangeOrValue(0.0, 10.0, True, True)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.AreEqual(u, RangeOrValue.Union(r1, r2))

            ' 08. (R=, R+) with two ranges inverts to (L-, L=)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MaximumOrder)
            Assert.AreEqual(u, RangeOrValue.Union(r2, r1))


            ' 09. (R=, R=) with point (arg1) and range (arg2)
            r1 = New RangeOrValue(10.0) ' [10]
            r2 = New RangeOrValue(0.0, 10.0, True, False) ' (0, 10]
            u = New RangeOrValue(0.0, 10.0, True, False) ' (0, 10]
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MaximumOrder)
            Assert.AreEqual(u, RangeOrValue.Union(r1, r2))

            ' 10. (R=, R=) with point (arg1) and range (arg2) inverts to (L-, R=)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MaximumOrder)
            Assert.AreEqual(u, RangeOrValue.Union(r2, r1))

            ' 11. (L-, R=) with two ranges
            r1 = New RangeOrValue(0.0, 10.0, False, True) ' [0, 10)
            r2 = New RangeOrValue(6.0, 10.0, False, True) ' [6, 10)
            u = New RangeOrValue(0.0, 10.0, False, True) ' [0, 10)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MaximumOrder)
            Assert.AreEqual(u, RangeOrValue.Union(r1, r2))

            ' 12. (L-, R=) with two ranges inverts to (M, R=)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MaximumOrder)
            Assert.AreEqual(u, RangeOrValue.Union(r2, r1))

            ' 13a. (L-, L-) with two points 
            r1 = New RangeOrValue(0.0) ' [6]
            r2 = New RangeOrValue(6.0) ' [8]
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MaximumOrder)
            Assert.AreEqual(Nothing, RangeOrValue.Union(r1, r2))

            ' 14a. 13a. inverts to (R+, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.AreEqual(Nothing, RangeOrValue.Union(r2, r1))

            ' 13b. (L-, L-) with two non-adjacent (floating point) ranges 
            r1 = New RangeOrValue(0.0, 5.0, False, False) ' [0, 5]
            r2 = New RangeOrValue(6.0, 10.0, False, False) ' [6, 10]
            u = Nothing
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MaximumOrder)
            Assert.AreEqual(Nothing, RangeOrValue.Union(r1, r2))

            ' 14b. 13b. inverts to (R+, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.AreEqual(Nothing, RangeOrValue.Union(r2, r1))

            ' 13c. (L-, L-) with two ranges adjacent with a closed endpoint less than an open endpoint
            r1 = New RangeOrValue(0.0, 6.0, True, False) ' (0, 6]
            r2 = New RangeOrValue(6.0, 10.0, True, True) ' (6, 10)
            u = New RangeOrValue(0.0, 10.0, True, True)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MaximumOrder)
            Assert.AreEqual(u, RangeOrValue.Union(r1, r2))

            ' 14c. 13c inverts to (R+, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.AreEqual(u, RangeOrValue.Union(r2, r1))

            ' 13d. (L-, L-) with two ranges adjacent with open endpoint less than closed endpoint
            r1 = New RangeOrValue(0.0, 6.0, True, True) ' (0, 6)
            r2 = New RangeOrValue(6.0, 10.0, False, True) ' [6, 10)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MaximumOrder)
            Assert.AreEqual(u, RangeOrValue.Union(r1, r2))

            ' 14d. 13d inverts to (R+, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.AreEqual(u, RangeOrValue.Union(r2, r1))

            ' 13e. (L-, L-) with ranges with open endpoint adjacent to point at upper endpoint
            r1 = New RangeOrValue(0.0, 5.0, True, True) ' (0, 5)
            r2 = New RangeOrValue(5.0) ' [5]
            u = New RangeOrValue(0.0, 5.0, True, False)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MaximumOrder)
            Assert.AreEqual(u, RangeOrValue.Union(r1, r2))

            ' 14e. 13e inverts to (R+, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.AreEqual(u, RangeOrValue.Union(r2, r1))

            ' 13f. (L-, L-) with ranges with open endpoint adjacent to point at lower endpoint
            r1 = New RangeOrValue(0.0) ' [5]
            r2 = New RangeOrValue(0.0, 5.0, True, True) ' (0, 5)
            u = New RangeOrValue(0.0, 5.0, False, True)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MaximumOrder)
            Assert.AreEqual(u, RangeOrValue.Union(r1, r2))

            ' 14f. 13e inverts to (R+, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.AreEqual(u, RangeOrValue.Union(r2, r1))


            ' 13g. (L-, L-) with two one-apart integer ranges 
            r1 = New RangeOrValue(0, 5, False, False) ' [0, 5]
            r2 = New RangeOrValue(6, 10, False, False) ' [6, 10]
            u = New RangeOrValue(0, 10, False, False) ' [0, 10]
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MaximumOrder)
            Assert.AreEqual(u, RangeOrValue.Union(r1, r2))

            ' 14g. (L-, L-) with two one-apart integer ranges inverts to (R+, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.AreEqual(u, RangeOrValue.Union(r2, r1))

            ' 15. (L-, M) with two ranges
            r1 = New RangeOrValue(0.0, 6.0, False, True) ' [0, 6)
            r2 = New RangeOrValue(5.0, 10.0, False, True) ' [5, 10)
            u = New RangeOrValue(0.0, 10.0, False, True) ' [0, 10)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, order.MaximumOrder)
            Assert.AreEqual(u, RangeOrValue.Union(r1, r2))

            ' 16. (L-, M) with two ranges inverts to (M, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.AreEqual(u, RangeOrValue.Union(r2, r1))

            ' 17. (M, M) with two ranges
            r1 = New RangeOrValue(6.0, 10.0, True, True) ' (6, 10)
            r2 = New RangeOrValue(6.0, 10.0, False, False) ' [6, 10]
            u = New RangeOrValue(6.0, 10.0, False, False)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, order.MaximumOrder)
            Assert.AreEqual(u, RangeOrValue.Union(r1, r2))

            ' 18. (M, M) with two ranges inverts to (L-, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.AreEqual(u, RangeOrValue.Union(r2, r1))

        End Sub


        <TestMethod(), ExpectedException(GetType(ArgumentNullException))>
        Sub Argument1OfWsbdRangesAreOverlappingUnionCannotBeNull()
            RangeOrValue.AreOverlapping(Nothing, New RangeOrValue(0))
        End Sub

        <TestMethod(), ExpectedException(GetType(ArgumentNullException))>
        Sub Argument2OfWsbdRangesAreOverlappingCannotBeNull()
            RangeOrValue.AreOverlapping(New RangeOrValue(0), Nothing)
        End Sub

        <TestMethod()>
        Sub WsbdRangesDetermineOverlapCorrectly()

            Dim r1, r2, u As RangeOrValue
            Dim order As RangeOrValue.RelativeOrderResult

            ' 01. (L=, R=) with two ranges
            r1 = New RangeOrValue(0, 5, True, True)
            order = RangeOrValue.GetRelativeOrder(r1, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MaximumOrder)
            Assert.IsTrue(RangeOrValue.AreOverlapping(r1, r1))

            ' 02. (L=, R=) with two points
            r1 = New RangeOrValue(0)
            order = RangeOrValue.GetRelativeOrder(r1, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MaximumOrder)
            Assert.IsTrue(RangeOrValue.AreOverlapping(r1, r1))

            ' 03. (L=, M) with two ranges
            r1 = New RangeOrValue(0.0, 5.0, True, False)
            r2 = New RangeOrValue(0.0, 10.0, True, True)
            u = New RangeOrValue(0.0, 10.0, True, True)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, order.MaximumOrder)
            Assert.IsTrue(RangeOrValue.AreOverlapping(r1, r2))

            ' 04. (L=, M) with two ranges inverts to (L=, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.IsTrue(RangeOrValue.AreOverlapping(r2, r1))

            ' 05. (L=, R+) with range (arg1) and point (arg2)
            r1 = New RangeOrValue(0, 10, False, True) ' [0, 10)
            r2 = New RangeOrValue(0) ' [0]
            u = New RangeOrValue(0, 10, False, True)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.IsTrue(RangeOrValue.AreOverlapping(r1, r2))

            ' 06. (L=, R+) with range (arg1) and point (arg2) inverts to (L=, L=)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MaximumOrder)
            Assert.IsTrue(RangeOrValue.AreOverlapping(r2, r1))

            ' 07. (R=, R+) with two ranges
            r1 = New RangeOrValue(5, 10, False, True) ' [5, 10)
            r2 = New RangeOrValue(0, 5, True, False) ' (0, 5]
            u = New RangeOrValue(0, 10, True, True)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.IsTrue(RangeOrValue.AreOverlapping(r1, r2))

            ' 08. (R=, R+) with two ranges inverts to (L-, L=)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MaximumOrder)
            Assert.IsTrue(RangeOrValue.AreOverlapping(r2, r1))


            ' 09. (R=, R=) with point (arg1) and range (arg2)
            r1 = New RangeOrValue(10) ' [10]
            r2 = New RangeOrValue(0, 10, True, False) ' (0, 10]
            u = New RangeOrValue(0, 10, True, False) ' (0, 10]
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MaximumOrder)
            Assert.IsTrue(RangeOrValue.AreOverlapping(r1, r2))

            ' 10. (R=, R=) with point (arg1) and range (arg2) inverts to (L-, R=)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MaximumOrder)
            Assert.IsTrue(RangeOrValue.AreOverlapping(r2, r1))

            ' 11. (L-, R=) with two ranges
            r1 = New RangeOrValue(0, 10, False, True) ' [0, 10)
            r2 = New RangeOrValue(6, 10, False, True) ' [6, 10)
            u = New RangeOrValue(0, 10, False, True) ' [0, 10)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MaximumOrder)
            Assert.IsTrue(RangeOrValue.AreOverlapping(r1, r2))

            ' 12. (L-, R=) with two ranges inverts to (M, R=)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MaximumOrder)
            Assert.IsTrue(RangeOrValue.AreOverlapping(r2, r1))

            ' 13a. (L-, L-) with two points 
            r1 = New RangeOrValue(0) ' [6]
            r2 = New RangeOrValue(6) ' [8]
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreOverlapping(r1, r2))

            ' 14a. (L-, L-) with two points inverts to (R+, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreOverlapping(r2, r1))

            ' 13b. (L-, L-) with two non-adjacent ranges 
            r1 = New RangeOrValue(0, 5, False, False) ' [0, 5]
            r2 = New RangeOrValue(6, 10, False, False) ' [6, 10]
            u = Nothing
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreOverlapping(r1, r2))

            ' 14b. (L-, L-) with two non-adjacent ranges inverts to (R+, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreOverlapping(r2, r1))

            ' 13c. (L-, L-) with two ranges adjacent with a closed endpoint less than an open endpoint
            r1 = New RangeOrValue(0, 6, True, False) ' (0, 6]
            r2 = New RangeOrValue(6, 10, True, True) ' (6, 10)
            u = New RangeOrValue(0, 10, True, True)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreOverlapping(r1, r2))

            ' 14c. 13c inverts to (R+, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreOverlapping(r2, r1))

            ' 13d. (L-, L-) with two ranges adjacent with open endpoint less than closed endpoint
            r1 = New RangeOrValue(0, 6, True, True) ' (0, 6)
            r2 = New RangeOrValue(6, 10, False, True) ' [6, 10)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreOverlapping(r1, r2))

            ' 14d. 13d inverts to (R+, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreOverlapping(r2, r1))

            ' 13e. (L-, L-) with two ranges adjacent with open endpoint adjacent to point
            r1 = New RangeOrValue(0, 5, True, True) ' (0, 5)
            r2 = New RangeOrValue(5) ' [5]
            u = New RangeOrValue(0, 5, True, False)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreOverlapping(r1, r2))

            ' 14e. 13e inverts to (R+, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreOverlapping(r2, r1))

            ' 13f. (L-, L-) with ranges with open endpoint adjacent to point at lower endpoint
            r1 = New RangeOrValue(0) ' [5]
            r2 = New RangeOrValue(0, 5, True, True) ' (0, 5)
            u = New RangeOrValue(0, 5, False, True)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreOverlapping(r1, r2))

            ' 14f. 13e inverts to (R+, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.IsFalse(RangeOrValue.AreOverlapping(r2, r1))


            ' 15. (L-, M) with two ranges
            r1 = New RangeOrValue(0.0, 6.0, False, True) ' [0, 6)
            r2 = New RangeOrValue(5.0, 10.0, False, True) ' [5, 10)
            u = New RangeOrValue(0.0, 10.0, False, True) ' [0, 10)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, order.MaximumOrder)
            Assert.IsTrue(RangeOrValue.AreOverlapping(r1, r2))

            ' 16. (L-, M) with two ranges inverts to (M, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.IsTrue(RangeOrValue.AreOverlapping(r2, r1))

            ' 17. (M, M) with two ranges
            r1 = New RangeOrValue(6.0, 10.0, True, True) ' (6, 10)
            r2 = New RangeOrValue(6.0, 10.0, False, False) ' [6, 10]
            u = New RangeOrValue(6.0, 10.0, False, False)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, order.MaximumOrder)
            Assert.IsTrue(RangeOrValue.AreOverlapping(r1, r2))

            ' 18. (M, M) with two ranges inverts to (L-, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.IsTrue(RangeOrValue.AreOverlapping(r2, r1))




        End Sub


        <TestMethod()>
        Sub WsbdRangeIntersectionWithNullAsFirstArgumentYieldsNull()
            Assert.AreEqual(Nothing, RangeOrValue.Intersection(Nothing, New RangeOrValue(0)))
        End Sub

        <TestMethod()>
        Sub WsbdRangeIntersectionWithNullAsSecondArgumentYieldsNull()
            Assert.AreEqual(Nothing, RangeOrValue.Intersection(New RangeOrValue(0), Nothing))
        End Sub


        <TestMethod()>
        Sub WsbdRangesIntersectCorrectly()

            Dim r1, r2, i As RangeOrValue
            Dim order As RangeOrValue.RelativeOrderResult

            ' 01. (L=, R=) with two ranges
            r1 = New RangeOrValue(0, 5, True, True) ' (0, 5)
            order = RangeOrValue.GetRelativeOrder(r1, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MaximumOrder)
            Assert.AreEqual(r1, RangeOrValue.Intersection(r1, r1))

            ' 02. (L=, R=) with two points
            r1 = New RangeOrValue(0) ' [0]
            order = RangeOrValue.GetRelativeOrder(r1, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MaximumOrder)
            Assert.AreEqual(r1, RangeOrValue.Intersection(r1, r1))

            ' 03. (L=, M) with two ranges
            r1 = New RangeOrValue(0.0, 5.0, True, False) ' (0, 5]
            r2 = New RangeOrValue(0.0, 10.0, True, True) ' (0, 10)
            i = New RangeOrValue(0.0, 5.0, True, False)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, order.MaximumOrder)
            Assert.AreEqual(i, RangeOrValue.Intersection(r1, r2))

            ' 04. (L=, M) with two ranges inverts to (L=, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.AreEqual(i, RangeOrValue.Intersection(r2, r1))

            ' 05. (L=, R+) with range (arg1) and point (arg2)
            r1 = New RangeOrValue(0.0, 10.0, False, True) ' [0, 10)
            r2 = New RangeOrValue(0.0) ' [0]
            i = New RangeOrValue(0.0)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.AreEqual(i, RangeOrValue.Intersection(r1, r2))

            ' 06. (L=, R+) with range (arg1) and point (arg2) inverts to (L=, L=)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MaximumOrder)
            Assert.AreEqual(i, RangeOrValue.Intersection(r2, r1))

            ' 07. (R=, R+) with two ranges
            r1 = New RangeOrValue(5.0, 10.0, False, True) ' [5, 10)
            r2 = New RangeOrValue(0.0, 5.0, True, False) ' (0, 5]
            i = New RangeOrValue(5.0)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.AreEqual(i, RangeOrValue.Intersection(r1, r2))

            ' 08. (R=, R+) with two ranges inverts to (L-, L=)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MaximumOrder)
            Assert.AreEqual(i, RangeOrValue.Intersection(r2, r1))


            ' 09. (R=, R=) with point (arg1) and range (arg2)
            r1 = New RangeOrValue(10.0) ' [10]
            r2 = New RangeOrValue(0.0, 10.0, True, False) ' (0, 10]
            i = New RangeOrValue(10.0) ' [10]
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MaximumOrder)
            Assert.AreEqual(i, RangeOrValue.Intersection(r1, r2))

            ' 10. (R=, R=) with point (arg1) and range (arg2) inverts to (L-, R=)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MaximumOrder)
            Assert.AreEqual(i, RangeOrValue.Intersection(r2, r1))

            ' 11. (L-, R=) with two ranges
            r1 = New RangeOrValue(0.0, 10.0, False, True) ' [0, 10)
            r2 = New RangeOrValue(6.0, 10.0, False, True) ' [6, 10)
            i = New RangeOrValue(6.0, 10.0, False, True) ' [6, 10)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MaximumOrder)
            Assert.AreEqual(i, RangeOrValue.Intersection(r1, r2))

            ' 12. (L-, R=) with two ranges inverts to (M, R=)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MaximumOrder)
            Assert.AreEqual(i, RangeOrValue.Intersection(r2, r1))

            ' 13a. (L-, L-) with two points 
            r1 = New RangeOrValue(0.0) ' [6]
            r2 = New RangeOrValue(6.0) ' [8]
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MaximumOrder)
            Assert.AreEqual(Nothing, RangeOrValue.Intersection(r1, r2))

            ' 14a. (L-, L-) with two points inverts to (R+, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.AreEqual(Nothing, RangeOrValue.Intersection(r2, r1))

            ' 13b. (L-, L-) with two non-adjacent ranges 
            r1 = New RangeOrValue(0.0, 5.0, False, False) ' [0, 5]
            r2 = New RangeOrValue(6.0, 10.0, False, False) ' [6, 10]
            i = Nothing
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MaximumOrder)
            Assert.AreEqual(Nothing, RangeOrValue.Intersection(r1, r2))

            ' 14b. (L-, L-) with two non-adjacent ranges inverts to (R+, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.AreEqual(Nothing, RangeOrValue.Intersection(r2, r1))

            ' 13c. (L-, L-) with two ranges adjacent with a closed endpoint less than an open endpoint
            r1 = New RangeOrValue(0.0, 6.0, True, False) ' (0, 6]
            r2 = New RangeOrValue(6.0, 10.0, True, True) ' (6, 10)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MaximumOrder)
            Assert.AreEqual(Nothing, RangeOrValue.Intersection(r1, r2))

            ' 14c. 13c inverts to (R+, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.AreEqual(Nothing, RangeOrValue.Intersection(r2, r1))

            ' 13d. (L-, L-) with two ranges adjacent with open endpoint less than closed endpoint
            r1 = New RangeOrValue(0.0, 6.0, True, True) ' (0, 6)
            r2 = New RangeOrValue(6.0, 10.0, False, True) ' [6, 10)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MaximumOrder)
            Assert.AreEqual(Nothing, RangeOrValue.Intersection(r1, r2))

            ' 14d. 13d inverts to (R+, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.AreEqual(Nothing, RangeOrValue.Intersection(r2, r1))

            ' 13e. (L-, L-) with ranges with open endpoint adjacent to point at upper endpoint
            r1 = New RangeOrValue(0.0, 5.0, True, True) ' (0, 5)
            r2 = New RangeOrValue(5.0) ' [5]
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MaximumOrder)
            Assert.AreEqual(Nothing, RangeOrValue.Intersection(r1, r2))

            ' 14e. 13e inverts to (R+, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.AreEqual(Nothing, RangeOrValue.Intersection(r2, r1))

            ' 13f. (L-, L-) with ranges with open endpoint adjacent to point at lower endpoint
            r1 = New RangeOrValue(0.0) ' [5]
            r2 = New RangeOrValue(0.0, 5.0, True, True) ' (0, 5)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MaximumOrder)
            Assert.AreEqual(Nothing, RangeOrValue.Intersection(r1, r2))

            ' 14f. 13e inverts to (R+, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.AreEqual(Nothing, RangeOrValue.Intersection(r2, r1))

            ' 15. (L-, M) with two ranges
            r1 = New RangeOrValue(0.0, 6.0, False, True) ' [0, 6)
            r2 = New RangeOrValue(5.0, 10.0, False, True) ' [5, 10)
            i = New RangeOrValue(5.0, 6.0, False, True) ' [5, 6)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, order.MaximumOrder)
            Assert.AreEqual(i, RangeOrValue.Intersection(r1, r2))

            ' 16. (L-, M) with two ranges inverts to (M, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.AreEqual(i, RangeOrValue.Intersection(r2, r1))

            ' 17. (M, M) with two ranges
            r1 = New RangeOrValue(6.0, 10.0, True, True) ' (6, 10)
            r2 = New RangeOrValue(6.0, 10.0, False, False) ' [6, 10]
            i = New RangeOrValue(6.0, 10.0, True, True)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, order.MaximumOrder)
            Assert.AreEqual(i, RangeOrValue.Intersection(r1, r2))

            ' 18. (M, M) with two ranges inverts to (L-, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.AreEqual(i, RangeOrValue.Intersection(r2, r1))

        End Sub

        <TestMethod()>
        Sub WsbdRangeIncludesNull()
            Assert.IsTrue(New RangeOrValue(0).Includes(Nothing))
        End Sub

        <TestMethod()>
        Sub WsbdRangesIncludeCorrectly()

            Dim r1, r2 As RangeOrValue
            Dim order As RangeOrValue.RelativeOrderResult

            ' (L=, R=) with two ranges
            r1 = New RangeOrValue(0.0, 5.0, True, True) ' (0, 5)
            order = RangeOrValue.GetRelativeOrder(r1, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MaximumOrder)
            Assert.IsTrue(r1.Includes(r1))

            ' 02. (L=, R=) with two points
            r1 = New RangeOrValue(0.0) ' [0]
            order = RangeOrValue.GetRelativeOrder(r1, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MaximumOrder)
            Assert.IsTrue(r1.Includes(r1))

            ' 03. (L=, M) with two ranges
            r1 = New RangeOrValue(0.0, 5.0, True, False) ' (0, 5]
            r2 = New RangeOrValue(0.0, 10.0, True, True) ' (0, 10)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, order.MaximumOrder)
            Assert.IsFalse(r1.Includes(r2))

            ' 04. (L=, M) with two ranges inverts to (L=, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.IsTrue(r2.Includes(r1))

            ' 05. (L=, R+) with range (arg1) and point (arg2)
            r1 = New RangeOrValue(0.0, 10.0, False, True) ' [0, 10)
            r2 = New RangeOrValue(0.0) ' [0]
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.IsTrue(r1.Includes(r2))

            ' 06. (L=, R+) with range (arg1) and point (arg2) inverts to (L=, L=)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MaximumOrder)
            Assert.IsFalse(r2.Includes(r1))

            ' 07. (R=, R+) with two ranges
            r1 = New RangeOrValue(5.0, 10.0, False, True) ' [5, 10)
            r2 = New RangeOrValue(0.0, 5.0, True, False) ' (0, 5]
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.IsFalse(r1.Includes(r2))

            ' 08. (R=, R+) with two ranges inverts to (L-, L=)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMin, order.MaximumOrder)
            Assert.IsFalse(r2.Includes(r1))

            ' 09. (R=, R=) with point (arg1) and range (arg2)
            r1 = New RangeOrValue(10.0) ' [10]
            r2 = New RangeOrValue(0.0, 10.0, True, False) ' (0, 10]
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MaximumOrder)
            Assert.IsFalse(r1.Includes(r2))

            ' 10. (R=, R=) with point (arg1) and range (arg2) inverts to (L-, R=)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MaximumOrder)
            Assert.IsFalse(r1.Includes(r2))

            ' 11. (L-, R=) with two ranges
            r1 = New RangeOrValue(0.0, 10.0, False, True) ' [0, 10)
            r2 = New RangeOrValue(6.0, 10.0, False, True) ' [6, 10)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MaximumOrder)
            Assert.IsTrue(r1.Includes(r2))

            ' 12. (L-, R=) with two ranges inverts to (M, R=)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.EqualToMax, order.MaximumOrder)
            Assert.IsFalse(r2.Includes(r1))

            ' 13a. (L-, L-) with two points 
            r1 = New RangeOrValue(0.0) ' [6]
            r2 = New RangeOrValue(6.0) ' [8]
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MaximumOrder)
            Assert.IsFalse(r1.Includes(r2))

            ' 14a. (L-, L-) with two points inverts to (R+, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.IsFalse(r2.Includes(r1))

            ' 13b. (L-, L-) with two non-adjacent ranges 
            r1 = New RangeOrValue(0.0, 5.0, False, False) ' [0, 5]
            r2 = New RangeOrValue(6.0, 10.0, False, False) ' [6, 10]
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MaximumOrder)
            Assert.IsFalse(r1.Includes(r2))

            ' 14b. (L-, L-) with two non-adjacent ranges inverts to (R+, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.IsFalse(r2.Includes(r1))

            ' 15. (L-, M) with two ranges
            r1 = New RangeOrValue(0.0, 6.0, False, True) ' [0, 6)
            r2 = New RangeOrValue(5.0, 10.0, False, True) ' [5, 10)
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, order.MaximumOrder)
            Assert.IsFalse(r1.Includes(r2))

            ' 16. (L-, M) with two ranges inverts to (M, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.IsFalse(r2.Includes(r1))

            ' 17. (M, M) with two ranges
            r1 = New RangeOrValue(6.0, 10.0, True, True) ' (6, 10)
            r2 = New RangeOrValue(6.0, 10.0, False, False) ' [6, 10]
            order = RangeOrValue.GetRelativeOrder(r1, r2)
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.BetweenMinAndMax, order.MaximumOrder)
            Assert.IsFalse(r1.Includes(r2))

            ' 18. (M, M) with two ranges inverts to (L-, R+)
            order = RangeOrValue.GetRelativeOrder(r2, r1)
            Assert.AreEqual(RangeOrValue.RelativeOrder.LessThanMin, order.MinimumOrder)
            Assert.AreEqual(RangeOrValue.RelativeOrder.GreaterThanMax, order.MaximumOrder)
            Assert.IsTrue(r2.Includes(r1))

        End Sub


        <TestMethod()>
        Public Sub WsbdRangeToStringIsCorrect()
            Assert.AreEqual("(0, 10)", New RangeOrValue(0.0, 10.0, True, True).ToString)
            Assert.AreEqual("[alpha, omega]", New RangeOrValue("alpha", "omega", False, False).ToString)
            Assert.AreEqual("5", New RangeOrValue(5).ToString)
        End Sub


        <TestMethod()>
        Public Sub WsbdRangeCollapsesWholeNumberRangesEquivalentToValues()
            Assert.AreEqual(New RangeOrValue(1), New RangeOrValue(1, 2, False, True))
            Assert.AreEqual(New RangeOrValue(1), New RangeOrValue(0, 1, True, False))
        End Sub

        <TestMethod()>
        Public Sub WsbdRangeCollapsesWholeNumberRanges()
            Assert.AreEqual(New RangeOrValue(1, 2, False, False), New RangeOrValue(0, 3, True, True))
        End Sub

        <TestMethod(), ExpectedException(GetType(MalformedWsbdRangeException))>
        Public Sub WsbdRangesOfWholeNumbersCannotBeOneApartAndOpen()
            Dim x = New RangeOrValue(0, 1, True, True)
        End Sub


    End Class

End Namespace