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

Imports System.Runtime.Serialization

Namespace Nist.Bcl.Wsbd

    ' When generating XML schema from this class using DataContract annotations, the XML type
    ' generated from this class has the incorrect name and must be manually changed to
    ' Result.
    <DataContract(Name:="result", Namespace:=Constants.WsbdNamespace),
    KnownType(GetType(Dictionary))> _
    Public Class Result

        <DataMember(Name:="status", Order:=0, IsRequired:=True)> _
        Public Property Status As Status

        <DataMember(EmitDefaultValue:=False, Order:=1, Name:="badFields")> _
        Public Property BadFields As StringArray

        <DataMember(EmitDefaultValue:=False, Order:=2, Name:="captureIds")> _
        Public Property CaptureIds As GuidArray

        <DataMember(EmitDefaultValue:=False, Order:=3, Name:="metadata")> _
        Public Property Metadata As Dictionary

        <DataMember(EmitDefaultValue:=False, Order:=4, Name:="message")> _
        Public Property Message As String

        <DataMember(EmitDefaultValue:=False, Order:=5, Name:="sensorData")> _
        Public Property SensorData As Byte()

        <DataMember(EmitDefaultValue:=False, Order:=6, Name:="sessionId")> _
        Public Property SessionId As Guid?


        Public Sub New()
            Me.Status = Status.Failure
        End Sub

        Public Sub New(ByVal status As Status)
            Me.Status = status
        End Sub

    End Class

End Namespace