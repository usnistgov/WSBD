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

Namespace Nist.Bcl.Wsbd

    <DataContract(name:="Range", Namespace:=Constants.WsbdNamespace)>
    Public Class RangeOrValue
        'Implements IEquatable(Of WsbdRange)

        Public Const MinInclusiveToken = "["
        Public Const MinExclusiveToken = "("
        Public Const RangeDelimiterToken = ","
        Public Const MaxInclusiveToken = "]"
        Public Const MaxExclusiveToken = ")"


        Private mMinimum As Object
        <DataMember(EmitDefaultValue:=False, Name:="minimum")> _
        Public Property Minimum As Object
            Get
                'If Not ContainsValidValue() Then
                '    ValidateMinAndMax()
                'End If
                Return mMinimum
            End Get
            Set(ByVal value As Object)
                If mMinimum IsNot Nothing Then Throw New WsbdRangesAreImmutableException
                If ContainsValidValue() Then Throw New WsbdRangesAreImmutableException
                ValidateType(value.GetType())
                mMinimum = value
            End Set
        End Property

        Private mMaximum As Object
        <DataMember(EmitDefaultValue:=False, Name:="maximum")> _
        Public Property Maximum As Object
            Get
                'If Not ContainsValidValue() Then
                '    ValidateMinAndMax()
                'End If
                Return mMaximum
            End Get
            Set(ByVal value As Object)
                If mMaximum IsNot Nothing Then Throw New WsbdRangesAreImmutableException
                If ContainsValidValue() Then Throw New WsbdRangesAreImmutableException
                ValidateType(value.GetType())
                mMaximum = value
            End Set
        End Property

        Public ReadOnly Property MaximumToIComparable() As IComparable
            Get
                ValidateIComparable()
                Return DirectCast(mMaximum, IComparable)
            End Get
        End Property

        Public ReadOnly Property MinimumToIComparable() As IComparable
            Get
                ValidateIComparable()
                Return DirectCast(mMinimum, IComparable)
            End Get
        End Property



        <DataMember(EmitDefaultValue:=False, Name:="minimumIsExclusive")> _
        Public Property MinimumIsExclusive As Boolean?

        <DataMember(EmitDefaultValue:=False, Name:="maximumIsExclusive")> _
        Public Property MaximumIsExclusive As Boolean?

        Private mValue As Object
        <DataMember(EmitDefaultValue:=False, Name:="value")> _
        Public Property Value As Object
            Get
                'If Not ContainsValidMinAndMax() Then
                '    ValidateValue()
                'End If
                Return mValue
            End Get
            Set(ByVal value As Object)
                If mValue IsNot Nothing Then Throw New WsbdRangesAreImmutableException
                ValidateType(value.GetType())
                mValue = value
                ValidateValue()
            End Set
        End Property

        'Private smUpperAndLowerBounds As Dictionary(Of Type, Tuple(Of Object, Object)) = Nothing
        Public Sub New()
            'If smUpperAndLowerBounds Is Nothing Then
            '    smUpperAndLowerBounds = CreateUpperAndLowerBoundsTable()
            'End If
        End Sub


        Public Sub New(ByVal value As Object)
            ValueConstructor(value)
        End Sub

        Private Sub ValueConstructor(ByVal value As Object)
            '
            ' The value constructor is a method because it can be called implicitly from the range
            ' constructor, when the range is identical to a value.
            '
            If value Is Nothing Then Throw New ArgumentNullException
            ValidateType(value.GetType)
            mValue = value
            ValidateValue()
        End Sub

        Public Sub New(ByVal min As Object, ByVal max As Object, ByVal minIsExclusive As Boolean, ByVal maxIsExclusive As Boolean)

            If min Is Nothing Or max Is Nothing Then Throw New ArgumentNullException

            ValidateType(min.GetType())
            ValidateType(max.GetType())
            ValidateIComparable()

            ' If the endpoints are whole numbers, one apart, with one end exclusive
            ' and the other inclusive, then collapse the range to a value
            '
            If IsWholeNumber(mContentType) Then

                If minIsExclusive Then
                    min += 1
                    minIsExclusive = False
                End If

                If maxIsExclusive Then
                    max -= 1
                    maxIsExclusive = False
                End If

            End If

            If Object.Equals(min, max) Then
                ValueConstructor(min)
            Else
                mMinimum = min
                mMaximum = max

                MinimumIsExclusive = minIsExclusive
                MaximumIsExclusive = maxIsExclusive

                ValidateMinAndMax()
                ValidateMinVsMax()
            End If


        End Sub

        Private mContentType As Type
        Private mContentTypeImplementsIComparable As Boolean
        Private mContentIsWholeNumber As Boolean
        Public ReadOnly Property ContentType As Type
            Get
                Return mContentType
            End Get
        End Property


        Private Sub ValidateType(ByVal t As Type)

            ' Upon the first type validation, we cache the datatype and
            ' if the type is comparable or not. This cache is extremely
            ' effective in speeding up working with WsbdRanges

            If mContentType Is Nothing Then
                mContentType = t
                mContentTypeImplementsIComparable = t.GetInterfaces.Contains(GetType(IComparable))
                mContentIsWholeNumber = IsWholeNumber(t)
            Else
                If Not mContentType.Equals(t) Then
                    Throw New WsbdRangeTypeMismatchException
                End If
            End If
        End Sub

        Private Function IsWholeNumber(ByVal t As Type)
            Return _
                GetType(Byte).Equals(t) OrElse
                GetType(Short).Equals(t) OrElse
                GetType(Integer).Equals(t) OrElse
                GetType(Long).Equals(t) OrElse
                GetType(SByte).Equals(t) OrElse
                GetType(UShort).Equals(t) OrElse
                GetType(UInteger).Equals(t) OrElse
                GetType(ULong).Equals(t)
        End Function


        Private Sub ValidateIComparable()
            If Not mContentTypeImplementsIComparable Then
                Throw New WsbdRangeNotComparableException
            End If
        End Sub

        Private Function ImplementsIComparable() As Boolean
            Return mContentTypeImplementsIComparable
        End Function

        Private Function ContentTypeIsWholeNumber() As Boolean
            Return mContentIsWholeNumber
        End Function


        Private Sub ValidateValue()

            If Not ContainsValidValue() Then
                Throw New MalformedWsbdRangeException
            End If

        End Sub

        Public Overloads Overrides Function Equals(ByVal obj As Object) As Boolean
            If obj Is Nothing OrElse Not [GetType]().Equals(obj.GetType()) Then
                Return False
            End If

            Dim range = DirectCast(obj, RangeOrValue)


            Dim areValues = ContainsValidValue() AndAlso range.ContainsValidValue
            Dim areRanges = ContainsValidMinAndMax() AndAlso range.ContainsValidMinAndMax

            If areValues Then
                Return Value.Equals(range.Value) 'Object.Equals(Value, range.Value)
            ElseIf areRanges Then
                Return Object.Equals(Minimum, range.Minimum) AndAlso _
                    MinimumIsExclusive = range.MinimumIsExclusive AndAlso _
                    Object.Equals(Maximum, range.Maximum) AndAlso _
                    MaximumIsExclusive = range.MaximumIsExclusive
            Else
                Return False
            End If

            ' Not reached


        End Function


        Public Function ContainsValidValue()
            Dim hasValue = mValue IsNot Nothing
            Dim hasMax = mMaximum IsNot Nothing
            Dim hasMin = mMinimum IsNot Nothing

            Dim hasExcludeFlags = MaximumIsExclusive IsNot Nothing OrElse MinimumIsExclusive IsNot Nothing

            Return hasValue AndAlso Not hasMax AndAlso Not hasMin AndAlso Not hasExcludeFlags
        End Function

        Public Function ContainsValidMinAndMax()
            Dim hasValue = mValue IsNot Nothing
            Dim hasMax = mMaximum IsNot Nothing
            Dim hasMin = mMinimum IsNot Nothing

            Dim hasExcludeFlags = MaximumIsExclusive IsNot Nothing OrElse MinimumIsExclusive IsNot Nothing

            Return Not hasValue AndAlso
                hasMax AndAlso hasMin AndAlso
                hasExcludeFlags

        End Function

        Private Sub ValidateMinAndMax()

            If Not ContainsValidMinAndMax() Then
                Throw New MalformedWsbdRangeException
            End If

        End Sub

        Private Sub ValidateMinVsMax()
            Dim min As IComparable = DirectCast(mMinimum, IComparable)
            Dim max As IComparable = DirectCast(mMaximum, IComparable)

            If min.CompareTo(max) >= 0 Then Throw New MalformedWsbdRangeException
        End Sub

        Public Shared Function AreComparable(ByVal range1 As RangeOrValue, ByVal range2 As RangeOrValue)

            If range1 Is Nothing OrElse range2 Is Nothing Then Throw New ArgumentNullException

            Dim sameType = range1.ContentType.Equals(range2.ContentType)
            Dim isComparable = range1.ImplementsIComparable()

            Return sameType AndAlso isComparable
        End Function

        Public Shared Function Compare(ByVal source As RangeOrValue, ByVal target As RangeOrValue) As Integer

            ' This could be accomplished via a table, but explicitly unfolding each case allows
            ' us to use code coverage to ensure we've tested each case.

            ' GetRelativeOrder checks to see if the source and target are comparable, so there
            ' is no need to check again for that here.

            Dim outcome = GetRelativeOrder(source, target)
            Dim min = outcome.MinimumOrder
            Dim max = outcome.MaximumOrder

            Dim result As Integer?

            ' The code here is structured in such a manner  such that inverting the source
            ' and target will give consistent (i.e., opposite) results, and so that if
            ' the comparison results need to be changed/tweaked on a fine-grained level,
            ' then there is a 'single' place to do so.


            Dim lessThanMinLessThanMin = -1
            Dim greaterThanMaxGreatherThanMax As Integer = -lessThanMinLessThanMin

            Dim lessThanMinEqualToMin = -1
            Dim equalToMaxGreatherThanMax = -lessThanMinEqualToMin

            Dim lessThanMinBetweenMinAndMax = -1
            Dim betweenMinAndMaxGreaterThanMax = -lessThanMinBetweenMinAndMax

            Dim lessThanMinEqualToMax = -1
            Dim betweenMinAndMaxEqualToMax = -lessThanMinEqualToMax
            Dim equalToMaxEqualToMax = betweenMinAndMaxEqualToMax

            Dim lessThanMinGreaterThanMax = -1
            Dim betweenMinAndMaxBetweenMinAndMAx = -lessThanMinGreaterThanMax

            Dim equalToMinEqualToMin = -1
            Dim equalToMinBetweenMinAnMax = equalToMinEqualToMin
            Dim equalToMinGreatherThanMax = -equalToMinEqualToMin

            Dim equalToMinEqualToMax = 0


            If min = RelativeOrder.LessThanMin AndAlso max = RelativeOrder.LessThanMin Then
                result = lessThanMinLessThanMin
            ElseIf min = RelativeOrder.LessThanMin AndAlso max = RelativeOrder.EqualToMin Then
                result = lessThanMinEqualToMin
            ElseIf min = RelativeOrder.LessThanMin AndAlso max = RelativeOrder.BetweenMinAndMax Then
                result = lessThanMinBetweenMinAndMax
            ElseIf min = RelativeOrder.LessThanMin AndAlso max = RelativeOrder.EqualToMax Then
                result = lessThanMinEqualToMax
            ElseIf min = RelativeOrder.LessThanMin AndAlso max = RelativeOrder.GreaterThanMax Then
                result = lessThanMinGreaterThanMax
            ElseIf min = RelativeOrder.EqualToMin AndAlso max = RelativeOrder.EqualToMin Then
                result = equalToMinEqualToMin
            ElseIf min = RelativeOrder.EqualToMin AndAlso max = RelativeOrder.BetweenMinAndMax Then
                result = equalToMinBetweenMinAnMax
            ElseIf min = RelativeOrder.EqualToMin AndAlso max = RelativeOrder.EqualToMax Then
                result = equalToMinEqualToMax
            ElseIf min = RelativeOrder.EqualToMin AndAlso max = RelativeOrder.GreaterThanMax Then
                result = equalToMinGreatherThanMax
            ElseIf min = RelativeOrder.BetweenMinAndMax AndAlso max = RelativeOrder.BetweenMinAndMax Then
                result = betweenMinAndMaxBetweenMinAndMAx
            ElseIf min = RelativeOrder.BetweenMinAndMax AndAlso max = RelativeOrder.EqualToMax Then
                result = betweenMinAndMaxEqualToMax
            ElseIf min = RelativeOrder.BetweenMinAndMax AndAlso max = RelativeOrder.GreaterThanMax Then
                result = betweenMinAndMaxGreaterThanMax
            ElseIf min = RelativeOrder.EqualToMax AndAlso max = RelativeOrder.EqualToMax Then
                result = equalToMaxEqualToMax
            ElseIf min = RelativeOrder.EqualToMax AndAlso max = RelativeOrder.GreaterThanMax Then
                result = equalToMaxGreatherThanMax
            ElseIf min = RelativeOrder.GreaterThanMax AndAlso max = RelativeOrder.GreaterThanMax Then
                result = greaterThanMaxGreatherThanMax
            End If




            If result Is Nothing Then Throw New MalformedWsbdRangeException

            Return result.Value
        End Function

        Public Shared Function AreOverlapping(ByVal source As RangeOrValue, ByVal target As RangeOrValue) As Boolean

            Dim order = GetRelativeOrder(source, target)
            Dim min = order.MinimumOrder
            Dim max = order.MaximumOrder
            Dim result As Boolean


            If min = RelativeOrder.LessThanMin AndAlso max = RelativeOrder.LessThanMin Then
                result = False
            ElseIf min = RelativeOrder.GreaterThanMax AndAlso max = RelativeOrder.GreaterThanMax Then
                result = False
            Else
                result = True
            End If

            Return result
        End Function

        <EditorBrowsable(EditorBrowsableState.Never)> Enum RelativeOrder
            LessThanMin
            EqualToMin
            BetweenMinAndMax
            EqualToMax
            GreaterThanMax
        End Enum

        <EditorBrowsable(EditorBrowsableState.Never)>
        Public Class RelativeOrderResult

            Public Property MinimumOrder As RelativeOrder
            Public Property MaximumOrder As RelativeOrder
        End Class


        <EditorBrowsable(EditorBrowsableState.Never)>
        Shared Function GetRelativeOrder(ByVal range1 As RangeOrValue, ByVal range2 As RangeOrValue) As RelativeOrderResult

            ' Make sure that range1 and range2 are not null and are type compatible
            If range1 Is Nothing Then Throw New ArgumentNullException("range1")
            If range2 Is Nothing Then Throw New ArgumentNullException("range2")
            range1.ValidateType(range2.ContentType)

            ' For clarity, we avoid using 'range1' vs 'range2.' Insteald, we call range1 
            ' the'source' and range2 the 'target'.

            Dim targetMin As IComparable = Nothing
            Dim targetMax As IComparable = Nothing
            Dim targetMinIsExclusive, targetMaxIsExclusive As Boolean

            Dim sourceMin As IComparable = Nothing
            Dim sourceMax As IComparable = Nothing
            Dim sourceMinIsExclusive, sourceMaxIsExclusive As Boolean

            If Not AreComparable(range1, range2) Then
                Throw New WsbdRangeNotComparableException
            End If

            If range1.Value Is Nothing Then
                sourceMin = range1.MinimumToIComparable
                sourceMax = range1.MaximumToIComparable
                sourceMinIsExclusive = range1.MinimumIsExclusive
                sourceMaxIsExclusive = range1.MaximumIsExclusive
            Else
                sourceMin = DirectCast(range1.Value, IComparable)
                sourceMax = DirectCast(range1.Value, IComparable)
                sourceMinIsExclusive = False
                sourceMaxIsExclusive = False
            End If


            If range2.Value Is Nothing Then
                targetMin = range2.MinimumToIComparable
                targetMax = range2.MaximumToIComparable
                targetMinIsExclusive = range2.MinimumIsExclusive
                targetMaxIsExclusive = range2.MaximumIsExclusive
            Else
                ' If the target is just a value, we treat it like a range,
                ' but with inclusive endpoints.
                '
                targetMin = DirectCast(range2.Value, IComparable)
                targetMax = DirectCast(range2.Value, IComparable)
                targetMinIsExclusive = False
                targetMaxIsExclusive = False
            End If




            Dim sourceMinPosition As RelativeOrder

            Dim sourceMinVsTargetMax = sourceMin.CompareTo(targetMax)
            Dim sourceMinVsTargetMin = sourceMin.CompareTo(targetMin)



            If sourceMinVsTargetMin < 0 Then
                sourceMinPosition = RelativeOrder.LessThanMin
            ElseIf sourceMinVsTargetMin = 0 Then

                ' The the source mininum and target minimum are (a) both inclusive
                ' or (b) both exclusive, then they are equal. 
                '
                If sourceMinIsExclusive And Not targetMinIsExclusive Then

                    ' If the target is a point, then the source min is actually
                    ' also greater than the max. Otherwise, it's between the (target)
                    ' minimum and maximum.

                    If range2.ContainsValidValue Then
                        sourceMinPosition = RelativeOrder.GreaterThanMax
                    Else
                        sourceMinPosition = RelativeOrder.BetweenMinAndMax
                    End If

                ElseIf Not sourceMinIsExclusive And targetMinIsExclusive Then
                    sourceMinPosition = RelativeOrder.LessThanMin
                Else
                    sourceMinPosition = RelativeOrder.EqualToMin
                End If

            ElseIf sourceMinVsTargetMax < 0 AndAlso sourceMinVsTargetMin > 0 Then
                sourceMinPosition = RelativeOrder.BetweenMinAndMax
            ElseIf sourceMinVsTargetMax = 0 Then
                '
                ' The source minimum is always greater than the target maximum, 
                ' unless the source minimum and target maximum are both inclusive,
                ' in which case they are equal. We don't have to check if the target
                ' is a point again, since we've handlded that in the test to see 
                ' if sourceMinVsTargetMin = 0.
                '
                If Not sourceMinIsExclusive AndAlso Not targetMaxIsExclusive Then
                    sourceMinPosition = RelativeOrder.EqualToMax
                Else
                    sourceMinPosition = RelativeOrder.GreaterThanMax
                End If

            ElseIf sourceMinVsTargetMax > 0 Then
                sourceMinPosition = RelativeOrder.GreaterThanMax

            End If

            Dim sourceMaxPosition As RelativeOrder
            Dim sourceMaxVsTargetMin = sourceMax.CompareTo(targetMin)
            Dim sourceMaxVsTargetMax = sourceMax.CompareTo(targetMax)


            If sourceMaxVsTargetMax > 0 Then
                sourceMaxPosition = RelativeOrder.GreaterThanMax
            ElseIf sourceMaxVsTargetMax = 0 Then

                ' If the source maximum and target maximum are (a) both inclusive,
                ' or (b) both exclusive, then they are equal
                '
                If Not sourceMaxIsExclusive AndAlso targetMaxIsExclusive Then
                    sourceMaxPosition = RelativeOrder.GreaterThanMax
                ElseIf sourceMaxIsExclusive AndAlso Not targetMaxIsExclusive Then

                    ' If the target is a point, then the source max here
                    ' is actually less than the target min. Otherwise,
                    ' source max is between the target min and max.
                    '
                    If range2.ContainsValidValue Then
                        sourceMaxPosition = RelativeOrder.LessThanMin
                    Else
                        sourceMaxPosition = RelativeOrder.BetweenMinAndMax
                    End If

                Else
                    sourceMaxPosition = RelativeOrder.EqualToMax
                End If
            ElseIf sourceMaxVsTargetMin > 0 Then
                sourceMaxPosition = RelativeOrder.BetweenMinAndMax
            ElseIf sourceMaxVsTargetMin = 0 Then

                ' The source maximum is always less than the target minimum
                ' unless they are both inclusive, in which case they are equal.
                ' We don't have to check to see if the target is a point again,
                ' because that case is handled in the test to see if
                ' sourceMaxVsTargetMax = 0.
                '
                If Not sourceMaxIsExclusive AndAlso Not targetMinIsExclusive Then
                    sourceMaxPosition = RelativeOrder.EqualToMin
                Else
                    sourceMaxPosition = RelativeOrder.LessThanMin
                End If
            ElseIf sourceMaxVsTargetMin < 0 Then
                sourceMaxPosition = RelativeOrder.LessThanMin
            End If


            Dim result = New RelativeOrderResult With {
                .MinimumOrder = sourceMinPosition,
                .MaximumOrder = sourceMaxPosition
            }

            Return result

        End Function





        Shared Function AreAdjacent(ByVal range1 As RangeOrValue, ByVal range2 As RangeOrValue) As Boolean

            Dim adjacent As Boolean ' return value

            Dim order = GetRelativeOrder(range1, range2)

            If range1 Is Nothing OrElse range2 Is Nothing Then Return False

            If order.MinimumOrder = RelativeOrder.LessThanMin AndAlso order.MaximumOrder = RelativeOrder.LessThanMin Then

                Dim r1Max As Object = Nothing
                Dim r2Min As Object = Nothing
                Dim r1Exclusive, r2Exclusive As Boolean

                If range1.ContainsValidMinAndMax Then
                    r1Max = range1.Maximum
                    r1Exclusive = range1.MaximumIsExclusive
                ElseIf range1.ContainsValidValue Then
                    r1Max = range1.Value
                    r1Exclusive = False
                End If

                If range2.ContainsValidMinAndMax Then
                    r2Min = range2.Minimum
                    r2Exclusive = range2.MinimumIsExclusive
                ElseIf range2.ContainsValidValue Then
                    r2Min = range2.Value
                    r2Exclusive = False
                End If

                Dim sameValue = Object.Equals(r1Max, r2Min)
                Dim notBothExclusive = Not (r1Exclusive AndAlso r2Exclusive)


                adjacent = sameValue AndAlso notBothExclusive

                ' Two inclusive endpoints that differ by one are adjacent
                If Not adjacent AndAlso range1.ContentTypeIsWholeNumber Then
                    Dim difference = Convert.ToDecimal(r2Min) - Convert.ToDecimal(r1Max)
                    Dim bothInclusive = Not r1Exclusive AndAlso Not r2Exclusive
                    adjacent = difference = 1 AndAlso bothInclusive
                End If

            ElseIf order.MinimumOrder = RelativeOrder.GreaterThanMax AndAlso order.MaximumOrder = RelativeOrder.GreaterThanMax Then

                Dim r1Min As Object = Nothing
                Dim r2Max As Object = Nothing
                Dim r1Exclusive, r2Exclusive As Boolean

                If range1.ContainsValidMinAndMax Then
                    r1Min = range1.Minimum
                    r1Exclusive = range1.MinimumIsExclusive
                ElseIf range1.ContainsValidValue Then
                    r1Min = range1.Value
                    r1Exclusive = False
                End If

                If range2.ContainsValidMinAndMax Then
                    r2Max = range2.Maximum
                    r2Exclusive = range2.MaximumIsExclusive
                ElseIf range2.ContainsValidValue Then
                    r2Max = range2.Value
                    r2Exclusive = False
                End If

                Dim sameValue = Object.Equals(r1Min, r2Max)
                Dim notBothExclusive = Not (r1Exclusive AndAlso r2Exclusive)

                adjacent = sameValue AndAlso notBothExclusive

                ' Two inclusive endpoints that differ by one are adjacent
                If Not adjacent AndAlso range1.ContentTypeIsWholeNumber Then
                    Dim difference = Convert.ToDecimal(r1Min) - Convert.ToDecimal(r2Max)
                    Dim bothInclusive = Not r1Exclusive AndAlso Not r2Exclusive
                    adjacent = difference = 1 AndAlso bothInclusive
                End If

            Else
                adjacent = False
            End If

            Return adjacent
        End Function





        Public Shared Function Union(ByVal range1 As RangeOrValue, ByVal range2 As RangeOrValue) As RangeOrValue

            If range1 Is Nothing AndAlso range2 IsNot Nothing Then Return range2
            If range2 Is Nothing AndAlso range1 IsNot Nothing Then Return range1
            If range1 Is Nothing AndAlso range2 Is Nothing Then Return Nothing


            Dim order = GetRelativeOrder(range1, range2)
            Dim minOrder = order.MinimumOrder
            Dim maxOrder = order.MaximumOrder

            Dim result As RangeOrValue = Nothing

            '
            ' Denormalize the range/values into a common representation, so we don't have
            ' to keep checking for value vs. ranges within each test.
            '
            Dim r1Min, r1Max As Object
            Dim r1MinExclusive, r1MaxExclusive As Boolean

            If range1.ContainsValidValue Then
                r1Min = range1.Value
                r1Max = range1.Value
                r1MinExclusive = False
                r1MaxExclusive = False
            Else
                r1Min = range1.Minimum
                r1Max = range1.Maximum
                r1MinExclusive = range1.MinimumIsExclusive
                r1MaxExclusive = range1.MaximumIsExclusive
            End If

            Dim r2Min, r2Max As Object
            Dim r2MinExclusive, r2MaxExclusive As Boolean

            If range2.ContainsValidValue Then
                r2Min = range2.Value
                r2Max = range2.Value
                r2MinExclusive = False
                r2MaxExclusive = False
            Else
                r2Min = range2.Minimum
                r2Max = range2.Maximum
                r2MinExclusive = range2.MinimumIsExclusive
                r2MaxExclusive = range2.MaximumIsExclusive
            End If


            If minOrder = RelativeOrder.LessThanMin AndAlso maxOrder = RelativeOrder.LessThanMin Then
                If AreAdjacent(range1, range2) Then
                    result = New RangeOrValue(r1Min, r2Max, r1MinExclusive, r2MaxExclusive)
                Else
                    result = Nothing
                End If
            ElseIf minOrder = RelativeOrder.LessThanMin AndAlso maxOrder = RelativeOrder.EqualToMin Then
                result = New RangeOrValue(r1Min, r2Max, r1MinExclusive, r2MaxExclusive)
            ElseIf minOrder = RelativeOrder.LessThanMin AndAlso maxOrder = RelativeOrder.BetweenMinAndMax Then
                result = New RangeOrValue(r1Min, r2Max, r1MinExclusive, r2MaxExclusive)
            ElseIf minOrder = RelativeOrder.LessThanMin AndAlso maxOrder = RelativeOrder.EqualToMax Then
                result = New RangeOrValue(r1Min, r2Max, r1MinExclusive, r2MaxExclusive)
            ElseIf minOrder = RelativeOrder.LessThanMin AndAlso maxOrder = RelativeOrder.GreaterThanMax Then
                result = New RangeOrValue(r1Min, r1Max, r1MinExclusive, r1MaxExclusive)
            ElseIf minOrder = RelativeOrder.EqualToMin AndAlso maxOrder = RelativeOrder.EqualToMin Then
                result = New RangeOrValue(r2Min, r2Max, r2MinExclusive, r2MaxExclusive)
            ElseIf minOrder = RelativeOrder.EqualToMin AndAlso maxOrder = RelativeOrder.BetweenMinAndMax Then
                result = New RangeOrValue(r2Min, r2Max, r2MinExclusive, r2MaxExclusive)
            ElseIf minOrder = RelativeOrder.EqualToMin AndAlso maxOrder = RelativeOrder.EqualToMax Then
                result = New RangeOrValue(r1Min, r1Max, r1MinExclusive, r1MaxExclusive)
            ElseIf minOrder = RelativeOrder.EqualToMin AndAlso maxOrder = RelativeOrder.GreaterThanMax Then
                result = New RangeOrValue(r1Min, r1Max, r1MinExclusive, r1MaxExclusive)
            ElseIf minOrder = RelativeOrder.BetweenMinAndMax AndAlso maxOrder = RelativeOrder.BetweenMinAndMax Then
                result = New RangeOrValue(r2Min, r2Max, r2MinExclusive, r2MaxExclusive)
            ElseIf minOrder = RelativeOrder.BetweenMinAndMax AndAlso maxOrder = RelativeOrder.EqualToMax Then
                result = New RangeOrValue(r2Min, r1Max, r2MinExclusive, r1MaxExclusive)
            ElseIf minOrder = RelativeOrder.BetweenMinAndMax AndAlso maxOrder = RelativeOrder.GreaterThanMax Then
                result = New RangeOrValue(r2Min, r1Max, r2MinExclusive, r1MaxExclusive)
            ElseIf minOrder = RelativeOrder.EqualToMax AndAlso maxOrder = RelativeOrder.EqualToMax Then
                result = New RangeOrValue(r2Min, r1Max, r2MinExclusive, r1MaxExclusive)
            ElseIf minOrder = RelativeOrder.EqualToMax AndAlso maxOrder = RelativeOrder.GreaterThanMax Then
                result = New RangeOrValue(r2Min, r1Max, r2MinExclusive, r1MaxExclusive)
            ElseIf minOrder = RelativeOrder.GreaterThanMax AndAlso maxOrder = RelativeOrder.GreaterThanMax Then
                If AreAdjacent(range1, range2) Then
                    result = New RangeOrValue(r2Min, r1Max, r2MinExclusive, r1MaxExclusive)
                Else
                    result = Nothing
                End If

            End If



            Return result

        End Function



        Public Shared Function Intersection(ByVal range1 As RangeOrValue, ByVal range2 As RangeOrValue) As RangeOrValue

            If range1 Is Nothing OrElse range2 Is Nothing Then Return Nothing

            Dim order = GetRelativeOrder(range1, range2)
            Dim minOrder = order.MinimumOrder
            Dim maxOrder = order.MaximumOrder

            Dim result As RangeOrValue = Nothing

            '
            ' Denormalize the range/values into a common representation, so we don't have
            ' to keep checking for value vs. ranges within each test.
            '
            Dim r1Min, r1Max As Object
            Dim r1MinExclusive, r1MaxExclusive As Boolean

            If range1.ContainsValidValue Then
                r1Min = range1.Value
                r1Max = range1.Value
                r1MinExclusive = False
                r1MaxExclusive = False
            Else
                r1Min = range1.Minimum
                r1Max = range1.Maximum
                r1MinExclusive = range1.MinimumIsExclusive
                r1MaxExclusive = range1.MaximumIsExclusive
            End If

            Dim r2Min, r2Max As Object
            Dim r2MinExclusive, r2MaxExclusive As Boolean

            If range2.ContainsValidValue Then
                r2Min = range2.Value
                r2Max = range2.Value
                r2MinExclusive = False
                r2MaxExclusive = False
            Else
                r2Min = range2.Minimum
                r2Max = range2.Maximum
                r2MinExclusive = range2.MinimumIsExclusive
                r2MaxExclusive = range2.MaximumIsExclusive
            End If


            If minOrder = RelativeOrder.LessThanMin AndAlso maxOrder = RelativeOrder.LessThanMin Then '(L-, L-)
                result = Nothing
            ElseIf minOrder = RelativeOrder.LessThanMin AndAlso maxOrder = RelativeOrder.EqualToMin Then '(L-, L=)
                result = New RangeOrValue(r2Min, r1Max, r2MinExclusive, r1MaxExclusive)
            ElseIf minOrder = RelativeOrder.LessThanMin AndAlso maxOrder = RelativeOrder.BetweenMinAndMax Then '(L-, M)
                result = New RangeOrValue(r2Min, r1Max, r2MinExclusive, r1MaxExclusive)
            ElseIf minOrder = RelativeOrder.LessThanMin AndAlso maxOrder = RelativeOrder.EqualToMax Then '(L-, R=)
                result = New RangeOrValue(r2Min, r2Max, r2MinExclusive, r2MaxExclusive)
            ElseIf minOrder = RelativeOrder.LessThanMin AndAlso maxOrder = RelativeOrder.GreaterThanMax Then '(L-, R+)
                result = New RangeOrValue(r2Min, r2Max, r2MinExclusive, r2MaxExclusive)
            ElseIf minOrder = RelativeOrder.EqualToMin AndAlso maxOrder = RelativeOrder.EqualToMin Then '(L=, L=)
                result = New RangeOrValue(r1Min, r1Max, r1MinExclusive, r1MaxExclusive)
            ElseIf minOrder = RelativeOrder.EqualToMin AndAlso maxOrder = RelativeOrder.BetweenMinAndMax Then '(L=, M)
                result = New RangeOrValue(r1Min, r1Max, r1MinExclusive, r1MaxExclusive)
            ElseIf minOrder = RelativeOrder.EqualToMin AndAlso maxOrder = RelativeOrder.EqualToMax Then '(L=, R=)
                result = New RangeOrValue(r1Min, r1Max, r1MinExclusive, r1MaxExclusive)
            ElseIf minOrder = RelativeOrder.EqualToMin AndAlso maxOrder = RelativeOrder.GreaterThanMax Then '(L=, R+)
                result = New RangeOrValue(r2Min, r2Max, r2MinExclusive, r2MaxExclusive)
            ElseIf minOrder = RelativeOrder.BetweenMinAndMax AndAlso maxOrder = RelativeOrder.BetweenMinAndMax Then '(M, M)
                result = New RangeOrValue(r1Min, r1Max, r1MinExclusive, r1MaxExclusive)
            ElseIf minOrder = RelativeOrder.BetweenMinAndMax AndAlso maxOrder = RelativeOrder.EqualToMax Then '(M, R=)
                result = New RangeOrValue(r1Min, r1Max, r1MinExclusive, r1MaxExclusive)
            ElseIf minOrder = RelativeOrder.BetweenMinAndMax AndAlso maxOrder = RelativeOrder.GreaterThanMax Then '(M, R+)
                result = New RangeOrValue(r1Min, r2Max, r1MinExclusive, r2MaxExclusive)
            ElseIf minOrder = RelativeOrder.EqualToMax AndAlso maxOrder = RelativeOrder.EqualToMax Then '(R=, R=)
                result = New RangeOrValue(r1Min, r1Max, r1MinExclusive, r1MaxExclusive)
            ElseIf minOrder = RelativeOrder.EqualToMax AndAlso maxOrder = RelativeOrder.GreaterThanMax Then '(R=, R+)
                result = New RangeOrValue(r1Min, r2Max, r1MinExclusive, r2MaxExclusive)
            ElseIf minOrder = RelativeOrder.GreaterThanMax AndAlso maxOrder = RelativeOrder.GreaterThanMax Then '(R+, R+)
                result = Nothing
            End If

            Return result

        End Function


        Public Function Includes(ByVal range As RangeOrValue) As Boolean
            If range Is Nothing Then Return True

            Dim order = GetRelativeOrder(Me, range)
            Dim minOrder = order.MinimumOrder
            Dim maxOrder = order.MaximumOrder

            Dim result As Boolean

            If minOrder = RelativeOrder.LessThanMin AndAlso maxOrder = RelativeOrder.LessThanMin Then '(L-, L-)
                result = False
            ElseIf minOrder = RelativeOrder.LessThanMin AndAlso maxOrder = RelativeOrder.EqualToMin Then '(L-, L=)
                result = False ' Always false since (L-, L=) range vs point compares as (L-, R=)
            ElseIf minOrder = RelativeOrder.LessThanMin AndAlso maxOrder = RelativeOrder.BetweenMinAndMax Then '(L-, M)
                result = False
            ElseIf minOrder = RelativeOrder.LessThanMin AndAlso maxOrder = RelativeOrder.EqualToMax Then '(L-, R=)
                result = True
            ElseIf minOrder = RelativeOrder.LessThanMin AndAlso maxOrder = RelativeOrder.GreaterThanMax Then '(L-, R+)
                result = True
            ElseIf minOrder = RelativeOrder.EqualToMin AndAlso maxOrder = RelativeOrder.EqualToMin Then '(L=, L=)
                result = False
            ElseIf minOrder = RelativeOrder.EqualToMin AndAlso maxOrder = RelativeOrder.BetweenMinAndMax Then '(L=, M)
                result = False
            ElseIf minOrder = RelativeOrder.EqualToMin AndAlso maxOrder = RelativeOrder.EqualToMax Then '(L=, R=)
                result = True
            ElseIf minOrder = RelativeOrder.EqualToMin AndAlso maxOrder = RelativeOrder.GreaterThanMax Then '(L=, R+)
                result = True
            ElseIf minOrder = RelativeOrder.BetweenMinAndMax AndAlso maxOrder = RelativeOrder.BetweenMinAndMax Then '(M, M)
                result = False
            ElseIf minOrder = RelativeOrder.BetweenMinAndMax AndAlso maxOrder = RelativeOrder.EqualToMax Then '(M, R=)
                result = False
            ElseIf minOrder = RelativeOrder.BetweenMinAndMax AndAlso maxOrder = RelativeOrder.GreaterThanMax Then '(M, R+)
                result = False
            ElseIf minOrder = RelativeOrder.EqualToMax AndAlso maxOrder = RelativeOrder.EqualToMax Then '(R=, R=)
                result = False ' Always false because (R=, R=) point vs point compares as (L=, R=)
            ElseIf minOrder = RelativeOrder.EqualToMax AndAlso maxOrder = RelativeOrder.GreaterThanMax Then '(R=, R+)
                result = False ' Always false since (R=, R+) range vs point compares as (L=, R+)
            ElseIf minOrder = RelativeOrder.GreaterThanMax AndAlso maxOrder = RelativeOrder.GreaterThanMax Then '(R+, R+)
                result = False
            End If

            Return result

        End Function


        Public Overrides Function ToString() As String

            Dim result As String

            If ContainsValidValue() Then
                result = mValue.ToString
            ElseIf ContainsValidMinAndMax() Then

                Dim minToken As String
                If MinimumIsExclusive Then
                    minToken = MinExclusiveToken
                Else
                    minToken = MinInclusiveToken
                End If

                Dim maxToken As String
                If MaximumIsExclusive Then
                    maxToken = MaxExclusiveToken
                Else
                    maxToken = MaxInclusiveToken
                End If

                result = String.Format("{0}{1}{2} {3}{4}", minToken, Minimum, RangeDelimiterToken, Maximum, maxToken)
            Else
                ' Should not be called
                result = String.Empty
            End If

            Return result

        End Function

    End Class

    Public Class WsbdRangeComparer
        Implements IComparer(Of RangeOrValue)

        Public Function Compare(ByVal x As RangeOrValue, ByVal y As RangeOrValue) As Integer Implements IComparer(Of RangeOrValue).Compare
            Return RangeOrValue.Compare(x, y)
        End Function
    End Class

    '<Serializable()> Public MustInherit Class WsbdRangeException
    '    Inherits Exception
    '    Public Sub New()

    '    End Sub
    '    Public Sub New(ByVal message As String)
    '        MyBase.New(message)

    '    End Sub
    '    Public Sub New(ByVal message As String, ByVal innerException As Exception)
    '        MyBase.New(message, innerException)

    '    End Sub
    '    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)
    '        MyBase.New(info, context)

    '    End Sub
    'End Class

    <Serializable()> Public Class WsbdRangeTypeMismatchException
        Inherits WsbdRangeException
        Public Sub New()

        End Sub
        Public Sub New(ByVal message As String)
            MyBase.New(message)

        End Sub
        Public Sub New(ByVal message As String, ByVal innerException As Exception)
            MyBase.New(message, innerException)

        End Sub
        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)
            MyBase.New(info, context)

        End Sub
    End Class

    <Serializable()> Public Class WsbdRangeNotComparableException
        Inherits WsbdRangeException
        Public Sub New()

        End Sub
        Public Sub New(ByVal message As String)
            MyBase.New(message)

        End Sub
        Public Sub New(ByVal message As String, ByVal innerException As Exception)
            MyBase.New(message, innerException)

        End Sub
        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)
            MyBase.New(info, context)

        End Sub
    End Class

    <Serializable()> Public Class MalformedWsbdRangeException
        Inherits WsbdRangeException
        Public Sub New()

        End Sub
        Public Sub New(ByVal message As String)
            MyBase.New(message)

        End Sub
        Public Sub New(ByVal message As String, ByVal innerException As Exception)
            MyBase.New(message, innerException)

        End Sub
        Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)
            MyBase.New(info, context)

        End Sub
    End Class

    '<Serializable()> Public Class WsbdRangesAreImmutableException
    '    Inherits WsbdRangeException
    '    Public Sub New()

    '    End Sub
    '    Public Sub New(ByVal message As String)
    '        MyBase.New(message)

    '    End Sub
    '    Public Sub New(ByVal message As String, ByVal innerException As Exception)
    '        MyBase.New(message, innerException)

    '    End Sub
    '    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)
    '        MyBase.New(info, context)

    '    End Sub
    'End Class






End Namespace
