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

Option Strict On
Option Infer On

Imports System.Text.RegularExpressions

Imports Nist.Bcl.Wsbd.Experimental.Deprecated

<TestClass()>
Public Class IntegerRangeTests

    <TestMethod()>
    Public Sub RangeRegexCanBeRetrieved()
        Assert.IsNotNull(IntegerRange.RangeRegexPattern)
    End Sub

    <TestMethod()>
    Public Sub RangeRegexRejectsBadRanges()

        Dim r As New Regex(IntegerRange.RangeRegexPattern)
        Assert.IsFalse(r.IsMatch(""))
        Assert.IsFalse(r.IsMatch("a"))
        Assert.IsFalse(r.IsMatch("abc"))
        Assert.IsFalse(r.IsMatch("(a,b)"))
        Assert.IsFalse(r.IsMatch("(a,b]"))
        Assert.IsFalse(r.IsMatch("[a,b)"))
        Assert.IsFalse(r.IsMatch("[a,b]"))
        Assert.IsFalse(r.IsMatch("1"))
        Assert.IsFalse(r.IsMatch("1,2"))
        Assert.IsFalse(r.IsMatch("(1,2"))
        Assert.IsFalse(r.IsMatch("[1,2"))
        Assert.IsFalse(r.IsMatch("1,2)"))
        Assert.IsFalse(r.IsMatch("1,2]"))
    End Sub


    <TestMethod()>
    Public Sub RangeRegexAcceptsValidRanges()
        Dim r As New Regex(IntegerRange.RangeRegexPattern)
        Dim m As Match
        m = r.Match("(0,10)") : Assert.AreEqual("0", m.Groups(1).Value) : Assert.AreEqual("10", m.Groups(2).Value)
        m = r.Match("[0,10)") : Assert.AreEqual("0", m.Groups(1).Value) : Assert.AreEqual("10", m.Groups(2).Value)
        m = r.Match("(0,10]") : Assert.AreEqual("0", m.Groups(1).Value) : Assert.AreEqual("10", m.Groups(2).Value)
        m = r.Match("[0,10]") : Assert.AreEqual("0", m.Groups(1).Value) : Assert.AreEqual("10", m.Groups(2).Value)

        m = r.Match("(-10,10)") : Assert.AreEqual("-10", m.Groups(1).Value) : Assert.AreEqual("10", m.Groups(2).Value)
        m = r.Match("[-10,10)") : Assert.AreEqual("-10", m.Groups(1).Value) : Assert.AreEqual("10", m.Groups(2).Value)
        m = r.Match("(-10,10]") : Assert.AreEqual("-10", m.Groups(1).Value) : Assert.AreEqual("10", m.Groups(2).Value)
        m = r.Match("[-10,10]") : Assert.AreEqual("-10", m.Groups(1).Value) : Assert.AreEqual("10", m.Groups(2).Value)

        m = r.Match("(-20,-10)") : Assert.AreEqual("-20", m.Groups(1).Value) : Assert.AreEqual("-10", m.Groups(2).Value)
        m = r.Match("[-20,-10)") : Assert.AreEqual("-20", m.Groups(1).Value) : Assert.AreEqual("-10", m.Groups(2).Value)
        m = r.Match("(-20,-10]") : Assert.AreEqual("-20", m.Groups(1).Value) : Assert.AreEqual("-10", m.Groups(2).Value)
        m = r.Match("[-20,-10]") : Assert.AreEqual("-20", m.Groups(1).Value) : Assert.AreEqual("-10", m.Groups(2).Value)


    End Sub

    <TestMethod()>
    Public Sub RangeRegexHandlesInternalWhitespace()
        Dim r As New Regex(IntegerRange.RangeRegexPattern)
        Dim m As Match

        m = r.Match("(  0,10)") : Assert.AreEqual("0", m.Groups(1).Value) : Assert.AreEqual("10", m.Groups(2).Value)
        m = r.Match("(0  ,10)") : Assert.AreEqual("0", m.Groups(1).Value) : Assert.AreEqual("10", m.Groups(2).Value)
        m = r.Match("(0,  10)") : Assert.AreEqual("0", m.Groups(1).Value) : Assert.AreEqual("10", m.Groups(2).Value)
        m = r.Match("(0,10  )") : Assert.AreEqual("0", m.Groups(1).Value) : Assert.AreEqual("10", m.Groups(2).Value)
        m = r.Match("( 0 , 10 )") : Assert.AreEqual("0", m.Groups(1).Value) : Assert.AreEqual("10", m.Groups(2).Value)
    End Sub

    <TestMethod()>
    Public Sub RangeDescriptorConstructsValidStrings()
        Assert.AreEqual(New IntegerRange(0, 10, False, False).ToString, "(0, 10)")
        Assert.AreEqual(New IntegerRange(0, 10).ToString, "[0, 10)")
        Assert.AreEqual(New IntegerRange(0, 10, False, True).ToString, "(0, 10]")
        Assert.AreEqual(New IntegerRange(0, 10, True, True).ToString, "[0, 10]")
    End Sub

    <TestMethod(), ExpectedException(GetType(InvalidIntegerRangeException))>
    Public Sub MinMaxInversionThrowsException()
        Dim r As New IntegerRange(10, 9)
    End Sub

    <TestMethod()>
    Public Sub InclusiveMinEqualsInclusiveMaxIsOkay()
        Dim good As New IntegerRange(10, 10, True, True)
    End Sub


    <TestMethod(), ExpectedException(GetType(InvalidIntegerRangeException))>
    Public Sub MinEqualsMaxWithExclusiveMinThrowsException()
        Dim r As New IntegerRange(10, 10, False)
    End Sub

    <TestMethod(), ExpectedException(GetType(InvalidIntegerRangeException))>
    Public Sub MinEqualsMaxWithExclusiveMaxThrowsException()
        Dim r As New IntegerRange(10, 10, True, False)
    End Sub


    <TestMethod()>
    Public Sub ValidRangesCanBeParsedCorrectly()

        Dim r = IntegerRange.Parse("(0, 10)")
        Assert.AreEqual(0, r.Minimum) : Assert.IsFalse(r.MinimumIsInclusive)
        Assert.AreEqual(10, r.Maximum) : Assert.IsFalse(r.MaximumIsInclusive)

        r = IntegerRange.Parse("[-10, 10)")
        Assert.AreEqual(-10, r.Minimum) : Assert.IsTrue(r.MinimumIsInclusive)
        Assert.AreEqual(10, r.Maximum) : Assert.IsFalse(r.MaximumIsInclusive)

    End Sub


    <TestMethod(), ExpectedException(GetType(InvalidIntegerRangeException))>
    Public Sub ParsingAndInvalidRangeThrowsException()
        Dim r = IntegerRange.Parse("(100, -100)")
    End Sub

End Class