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

Imports System.Text.RegularExpressions

Namespace Nist.Bcl.Wsbd.Experimental.Deprecated

    Public Class IntegerRange

        Public Const MinInclusiveToken = "["
        Public Const MinExclusiveToken = "("
        Public Const RangeDelimiterToken = ","
        Public Const MaxInclusiveToken = "]"
        Public Const MaxExclusiveToken = ")"



        Private Shared smRangeRegexPattern As String = Nothing
        Private Shared smRangeRegex As Regex
        Private Shared Function GenerateRangeRegex() As String
            Return String.Format("[{0}{1}]\s*(?<1>-?\d+)\s*{2}\s*(?<2>-?\d+)\s*[{3}{4}]",
                                 Regex.Escape(MinInclusiveToken),
                                 Regex.Escape(MinExclusiveToken),
                                 Regex.Escape(RangeDelimiterToken),
                                 Regex.Escape(MaxInclusiveToken),
                                 Regex.Escape(MaxExclusiveToken))
        End Function

        Public Shared ReadOnly Property RangeRegexPattern As String
            Get
                If smRangeRegexPattern Is Nothing Then
                    smRangeRegexPattern = GenerateRangeRegex()
                End If
                Return smRangeRegexPattern
            End Get
        End Property

        Public Property Minimum As Integer
        Public Property Maximum As Integer
        Public Property MinimumIsInclusive As Boolean
        Public Property MaximumIsInclusive As Boolean

        Public Sub New(ByVal minimum As Integer, ByVal maximum As Integer,
                       Optional ByVal minimumIsInclusive As Boolean = True,
                       Optional ByVal maximumIsInclusive As Boolean = False)
            Me.Minimum = minimum
            Me.Maximum = maximum
            Me.MinimumIsInclusive = minimumIsInclusive
            Me.MaximumIsInclusive = maximumIsInclusive
            Validate()
        End Sub

        Public Sub New(ByVal fixedValue As Integer)
            Me.Minimum = fixedValue
            Me.Maximum = fixedValue
            Me.MinimumIsInclusive = True
            Me.MaximumIsInclusive = True
            Validate()
        End Sub

        Public Overrides Function ToString() As String
            Dim minToken = MinExclusiveToken
            If MinimumIsInclusive Then minToken = MinInclusiveToken

            Dim maxToken = MaxExclusiveToken
            If MaximumIsInclusive Then maxToken = MaxInclusiveToken

            Return String.Format("{0}{1}{2} {3}{4}",
                                 minToken, Minimum,
                                 RangeDelimiterToken,
                                 Maximum, maxToken)

        End Function

        Public Shared Function Parse(ByVal value As String) As IntegerRange

            ' Remove leading and trailing whitespace
            Dim trimmed As String = value.Trim

            ' Create a regex matcher if we don't have one already
            If smRangeRegex Is Nothing Then
                smRangeRegex = New Regex(IntegerRange.RangeRegexPattern)
            End If
            Dim match = smRangeRegex.Match(trimmed)

            ' If we did not match, then there must have been a parse error
            If Not match.Success Then
                Throw New IntegerRangeParseErrorException
            End If

            ' Determine if min and max are inclusive or inclusive
            Dim minIsInclusive = trimmed.Contains(MinInclusiveToken)
            Dim maxIsInclusive = trimmed.Contains(MaxInclusiveToken)

            ' Finally, extract the min and max from the match result
            Dim min As Integer = CInt(match.Groups(1).Value)
            Dim max As Integer = CInt(match.Groups(2).Value)

            Return New IntegerRange(min, max, minIsInclusive, maxIsInclusive)

        End Function


        Private Sub Validate()

            If Maximum < Minimum Then
                Throw New InvalidIntegerRangeException
            End If

            If Minimum = Maximum AndAlso
                Not (MinimumIsInclusive And MaximumIsInclusive) Then
                Throw New InvalidIntegerRangeException
            End If

        End Sub

    End Class


    Public Class InvalidIntegerRangeException
        Inherits Exception
    End Class


    Public Class IntegerRangeParseErrorException
        Inherits Exception
    End Class




End Namespace
