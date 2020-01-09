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
Imports System.IO
Imports System.Runtime.CompilerServices

Namespace Nist.Bcl.Wsbd

    <TestClass()>
    Public Class ArrayXmlTests
        Inherits XmlTests

        Private Const WsbdArray As String = "WsbdArray"

        <TestMethod(), TestCategory(Xml), TestCategory(WsbdArray)>
        Public Sub WsbdArraysSerializeMessily()
            Dim a As New Array
            a.Add(1)
            a.Add("foo")

            Dim expected =
                <Array xmlns:i="http://www.w3.org/2001/XMLSchema-instance" xmlns="urn:oid:2.16.840.1.101.3.9.3.1">
                    <element xmlns:d2p1="http://www.w3.org/2001/XMLSchema" i:type="d2p1:int">1</element>
                    <element xmlns:d2p1="http://www.w3.org/2001/XMLSchema" i:type="d2p1:string">foo</element>
                </Array>

            Assert.IsTrue(XmlEqual(ToXElement(a), expected))
        End Sub

        <TestMethod(), TestCategory(Xml), TestCategory(WsbdArray)>
        Public Sub WsbdArraysCanBeTidied()

            Dim toTidy =
                <Array xmlns:i="http://www.w3.org/2001/XMLSchema-instance" xmlns="urn:oid:2.16.840.1.101.3.9.3.1">
                    <element xmlns:d2p1="http://www.w3.org/2001/XMLSchema" i:type="d2p1:int">1</element>
                    <element xmlns:d2p1="http://www.w3.org/2001/XMLSchema" i:type="d2p1:string">foo</element>
                </Array>

            Dim expected =
                <Array
                    xmlns:i="http://www.w3.org/2001/XMLSchema-instance"
                    xmlns:xs="http://www.w3.org/2001/XMLSchema"
                    xmlns="urn:oid:2.16.840.1.101.3.9.3.1">
                    <element i:type="xs:int">1</element>
                    <element i:type="xs:string">foo</element>
                </Array>

            Assert.IsTrue(XmlEqual(toTidy.Tidy, expected))

        End Sub

        <TestMethod(), TestCategory(Xml), TestCategory(WsbdArray)>
        Public Sub WsbdArraysSurviveTidierRoundtrip()

            Dim a As New Array
            a.Add(1)
            a.Add("foo")

            Dim roundTrip = ToObject(Of Array)(ToXElement(a).Tidy)
            CollectionAssert.AreEqual(a, roundTrip)

        End Sub

        <TestMethod(), TestCategory(Xml), TestCategory(WsbdArray)>
        Sub JunkXmlFailsWsbdDictionaryValidation()
            Dim errors = XmlUtil.Validate(<junk/>, Schema.WsbdArray)
            Assert.AreNotEqual(0, errors.Count)
        End Sub


    End Class

End Namespace
