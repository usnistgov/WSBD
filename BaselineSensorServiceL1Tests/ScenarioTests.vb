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

Imports System.Runtime.Serialization
Imports System.ServiceModel
Imports System.ServiceModel.Web

Imports Nist.Bcl.Wsbd
Imports System.Threading


<TestClass()>
Public Class BaselineSensorServiceScenarioTests
    Inherits BaselineSensorServiceTests

    <TestMethod()> _
    Sub UnregisteringWhileHoldingTheLockShouldReleaseTheLock()

        Dim client1 = CreateClient()
        Dim client2 = CreateClient()

        Dim session1 = client1.Register().WsbdResult.SessionId.Value.ToString
        Dim session2 = client2.Register().WsbdResult.SessionId.Value.ToString

        BaselineSensorService.InitializationTime = 0 'ms

        ' Before acquiring the lock, a locking call should yield 'LockNotHeld'
        Dim result1 = client1.Initialize(session1).WsbdResult
        Assert.AreEqual(Status.LockNotHeld, result1.Status)
        Dim result2 = client2.Initialize(session2).WsbdResult
        Assert.AreEqual(Status.LockNotHeld, result2.Status)

        ' After acquiring the lock, a second client should be denied the lock
        result1 = client1.Lock(session1).WsbdResult
        Assert.AreEqual(Status.Success, result1.Status)
        result2 = client2.Lock(session2).WsbdResult
        Assert.AreEqual(Status.LockHeldByAnother, result2.Status)

        ' Upon unregistering, session1 becomes an invalid ID, and client 2 is
        ' allowed to lock
        '
        client1.Unregister(session1)
        result1 = client1.Lock(session1).WsbdResult
        Assert.AreEqual(Status.InvalidId, result1.Status)
        result2 = client2.Lock(session2).WsbdResult
        Assert.AreEqual(Status.Success, result2.Status)

    End Sub

    '<TestMethod()> _
    'Sub UnregisteringWhilePerformingASensorOperationGivesASensorBusyStatus()

    '    Dim client = CreateClient()
    '    Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString
    '    client.Lock(sessionId)
    '    Dim init As New Thread(Sub()
    '                               client.Initialize(sessionId)
    '                           End Sub)
    '    init.Start()

    '    ' Wait just a hair for the init thread to start
    '    Threading.Thread.Sleep(500)

    '    Dim result = client.Unregister(sessionId).WsbdResult
    '    Assert.AreEqual(WsbdStatus.SensorBusy, result.Status)

    '    init.Join()

    'End Sub


End Class
