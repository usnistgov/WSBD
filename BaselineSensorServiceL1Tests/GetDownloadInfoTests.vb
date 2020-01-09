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

Option Strict On
Option Infer On

Imports System.Runtime.Serialization
Imports System.ServiceModel
Imports System.ServiceModel.Web

Imports Nist.Bcl.Wsbd


<TestClass()>
Public Class BaselineSensorServiceDownloadInfoTests
    Inherits BaselineSensorServiceTests

    <TestMethod(), TestCategory(GetDownloadInfo), TestCategory(BadValue)> _
    Sub GetDownloadInfoForAnUnparseableIdGivesABadValue()
        Dim client = CreateClient()
        Dim result = client.GetDownloadInfo("this_is_not_a_guid").WsbdResult
        Assert.AreEqual(Status.BadValue, result.Status)
    End Sub

    <TestMethod(), TestCategory(GetDownloadInfo), TestCategory(BadValue)> _
    Sub GetDownloadInfoForAnUnknownIdGivesInvalidId()
        Dim client = CreateClient()
        Dim result = client.GetDownloadInfo(Guid.NewGuid.ToString).WsbdResult
        Assert.AreEqual(Status.InvalidId, result.Status)
    End Sub

    <TestMethod(), TestCategory(GetDownloadInfo), TestCategory(Success)> _
    Sub GetDownloadInfoCanBeSuccessful()
        BaselineSensorService.CaptureTime = 100 'ms
        BaselineSensorService.PendingDownloadTime = 1000 'ms

        Dim client = CreateClient()
        Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString

        client.Lock(sessionId)

        Dim captureId = client.Capture(sessionId).WsbdResult.CaptureIds(0).Value.ToString

        Threading.Thread.Sleep(BaselineSensorService.CaptureTime + BaselineSensorService.PendingDownloadTime + 0)

        Dim result As Result

        Do
            Threading.Thread.Sleep(1000)
            result = client.GetDownloadInfo(captureId).WsbdResult
            Assert.IsNotNull(result)
        Loop While result.Status = Status.PreparingDownload

        Assert.AreEqual(Status.Success, result.Status)
        Assert.IsNotNull(result.Metadata)
        Assert.IsTrue(result.Metadata.Count > 0)
        Assert.AreEqual("image/png", result.Metadata(Constants.ContentTypeKey))
    End Sub



    <TestMethod(), TestCategory(GetDownloadInfo), TestCategory(PreparingDownload)> _
    Sub GettingDownloadInfoImmediatelyAfterCaptureCanReturnPreparingDownload()

        BaselineSensorService.PendingDownloadTime = 1000 'ms

        BaselineSensorService.CaptureTime = 100 'ms
        Dim client = CreateClient()
        Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString

        client.Lock(sessionId)

        ' Immediately after capture, a download may not be ready.
        Dim captureId = client.Capture(sessionId).WsbdResult.CaptureIds(0).Value.ToString
        Dim result = client.GetDownloadInfo(captureId).WsbdResult
        Assert.AreEqual(Status.PreparingDownload, result.Status)

        ' For testing purposes, wait until the download is no longer pending
        Threading.Thread.Sleep(BaselineSensorService.PendingDownloadTime + 1000)

        ' After the service has completed preparing the download, the correct
        ' mime type data should be available.
        '
        result = client.GetDownloadInfo(captureId).WsbdResult
        Assert.AreEqual(Status.Success, result.Status)
        Assert.AreEqual("image/png", result.Metadata(Constants.ContentTypeKey))

    End Sub



    <TestMethod(), TestCategory(GetDownloadInfo), TestCategory(Failure)> _
    Sub GetDownloadInfoCanFail()

        BaselineSensorService.CaptureTime = 100 'ms
        BaselineSensorService.PendingDownloadTime = 1 'ms

        Dim client = CreateClient()
        Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString

        client.Lock(sessionId)

        Dim captureId = client.Capture(sessionId).WsbdResult.CaptureIds(0).Value.ToString

        ' To ensure that the test succeeds, we wait additional time to make sure that the server has
        ' had time to prepare the download. 
        '
        Threading.Thread.Sleep(BaselineSensorService.PendingDownloadTime + 1500)

        ' For the purposes of testing, we corrupt the data store by deleting the underlying data.
        Dim provider = DirectCast(BaselineSensorService.StorageProvider, FileStorageProvider)
        IO.File.Delete(provider.MetadataFileName(New Guid(captureId)))

        Dim result = client.GetDownloadInfo(captureId).WsbdResult
        Assert.AreEqual(Status.Failure, result.Status)

    End Sub


End Class


