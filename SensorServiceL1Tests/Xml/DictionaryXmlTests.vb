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

    <TestClass()> Public Class DictionaryXmlTests
        Inherits XmlTests

        Private Const DictionaryCategory As String = "Dictionary"

        <TestMethod(), TestCategory(Xml), TestCategory(DictionaryCategory)>
        Public Sub WsbdDictionariesSerializeMessily()

            Dim dictionary As New Dictionary
            dictionary.Add("number_of_animals", 15)
            dictionary.Add("name_of_cats", "Bob")

            Dim expected =
                <Dictionary xmlns:i="http://www.w3.org/2001/XMLSchema-instance" xmlns="urn:oid:2.16.840.1.101.3.9.3.1">
                    <item>
                        <key>number_of_animals</key>
                        <value xmlns:d3p1="http://www.w3.org/2001/XMLSchema" i:type="d3p1:int">15</value>
                    </item>
                    <item>
                        <key>name_of_cats</key>
                        <value xmlns:d3p1="http://www.w3.org/2001/XMLSchema" i:type="d3p1:string">Bob</value>
                    </item>
                </Dictionary>

            Assert.IsTrue(XmlEqual(XmlUtil.ToXElement(dictionary), expected))

        End Sub


        <TestMethod(), TestCategory(Xml), TestCategory(DictionaryCategory)>
        Public Sub WsbdDictionariesCanBeTidied()

            Dim toTidy =
                  <Dictionary xmlns:i="http://www.w3.org/2001/XMLSchema-instance"
                      xmlns="urn:oid:2.16.840.1.101.3.9.3.1">
                      <item>
                          <key>number_of_animals</key>
                          <value xmlns:d3p1="http://www.w3.org/2001/XMLSchema" i:type="d3p1:int">15</value>
                      </item>
                      <item>
                          <key>name_of_cats</key>
                          <value xmlns:d3p1="http://www.w3.org/2001/XMLSchema" i:type="d3p1:string">Bob</value>
                      </item>
                  </Dictionary>

            Dim expected =
                <Dictionary xmlns:i="http://www.w3.org/2001/XMLSchema-instance"
                    xmlns:xs="http://www.w3.org/2001/XMLSchema"
                    xmlns="urn:oid:2.16.840.1.101.3.9.3.1">
                    <item>
                        <key>number_of_animals</key>
                        <value i:type="xs:int">15</value>
                    </item>
                    <item>
                        <key>name_of_cats</key>
                        <value i:type="xs:string">Bob</value>
                    </item>
                </Dictionary>


            toTidy.Tidy()

            Assert.IsTrue(XmlEqual(toTidy, expected))
        End Sub



        <TestMethod(), TestCategory(Xml), TestCategory(DictionaryCategory)>
        Sub WsbdDictionariesSurviveSerializationRoundtrip()

            Dim dictionary As New Dictionary
            dictionary.Add("number_of_animals", 15)
            dictionary.Add("name_of_cat", "Bob")
            dictionary.Add("name_of_dog", "Bob")

            Dim roundTrip = ToObject(Of Dictionary)(ToXElement(dictionary))
            CollectionAssert.AreEqual(dictionary, roundTrip)
        End Sub

        <TestMethod(), TestCategory(Xml), TestCategory(DictionaryCategory)>
        Sub WsbdDictionariesSurviveTidierRoundtrip()

            Dim dictionary As New Dictionary
            dictionary.Add("number_of_animals", 15)
            dictionary.Add("name_of_cat", "Bob")
            dictionary.Add("name_of_dog", "Bob")
            dictionary.Add("name_of_fish", "Bob")

            Dim roundTrip = ToObject(Of Dictionary)(ToXElement(dictionary).Tidy)
            CollectionAssert.AreEqual(dictionary, roundTrip)
        End Sub

        <TestMethod(), TestCategory(Xml), TestCategory(DictionaryCategory)>
        Sub UntidiedWsbdDictionariesPassValidation()

            Dim dictionary As New Dictionary
            dictionary.Add("number_of_animals", 15)
            dictionary.Add("name_of_cat", "Bob")
            dictionary.Add("name_of_dog", "Bob")

            Dim errors = XmlUtil.Validate(ToXElement(dictionary), Schema.Dictionary)
            Assert.AreEqual(0, errors.Count)
        End Sub

        <TestMethod(), TestCategory(Xml), TestCategory(DictionaryCategory)>
        Sub UntidiedConfigurationPassValidation()

            Dim configuration As New Configuration
            configuration.Add("number_of_animals", 15)
            configuration.Add("name_of_cat", "Bob")
            configuration.Add("name_of_dog", "Bob")

            Dim errors = XmlUtil.Validate(ToXElement(configuration), Schema.WSBD)
            Assert.AreEqual(0, errors.Count)
        End Sub

        <TestMethod(), TestCategory(Xml), TestCategory(DictionaryCategory)>
        Sub JunkXmlFailsWsbdDictionaryValidation()
            Dim errors = XmlUtil.Validate(<junk/>, Schema.WSBD)
            Assert.AreNotEqual(0, errors.Count)
        End Sub

        '<TestMethod(), TestCategory(Xml), TestCategory(DictionaryCategory)>
        'Sub WsbdDictionariesDoNotRequireAParticularRootElement()

        '    Dim example = <no_particular_name xmlns:i="http://www.w3.org/2001/XMLSchema-instance"
        '                      xmlns:xs="http://www.w3.org/2001/XMLSchema"
        '                      xmlns="urn:oid:2.16.840.1.101.3.9.3.1">
        '                      <item>
        '                          <key>number_of_animals</key>
        '                          <value i:type="xs:int">15</value>
        '                      </item>
        '                      <item>
        '                          <key>name_of_cat</key>
        '                          <value i:type="xs:string">Bob</value>
        '                      </item>
        '                  </no_particular_name>

        '    Dim expected As New WsbdDictionary
        '    expected.Add("number_of_animals", 15)
        '    expected.Add("name_of_cat", "Bob")

        '    CollectionAssert.AreEqual(expected, ToObject(Of WsbdDictionary)(example))

        'End Sub


    End Class

End Namespace