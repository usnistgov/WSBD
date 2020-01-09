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

Imports System.ServiceModel
Imports System.ServiceModel.Web
Imports System.Threading
Imports System.Threading.Tasks

Imports BaselineSensorServiceTestsExtentionMethods

Imports Nist.Bcl.Wsbd


<TestClass()>
Public Class BaselineSensorDownloadTests
    Inherits BaselineSensorServiceTests

    <TestMethod(), TestCategory(Download), TestCategory(BadValue)> _
    Sub DownloadingAnUnparseableIdGivesABadValue()
        Dim client = CreateClient()
        Dim result = client.Download("this_is_not_a_guid").WsbdResult
        Assert.AreEqual(Status.BadValue, result.Status)
    End Sub

    <TestMethod(), TestCategory(Download), TestCategory(BadValue)> _
    Sub DownloadingAnUnknownIdGivesInvalidId()
        Dim client = CreateClient()
        Dim result = client.Download(Guid.NewGuid.ToString).WsbdResult
        Assert.AreEqual(Status.InvalidId, result.Status)
    End Sub

    <TestMethod(), TestCategory(Download), TestCategory(Success)> _
    Sub DownloadCanBeSuccessful()

        BaselineSensorService.CaptureTime = 1000 'ms
        BaselineSensorService.PendingDownloadTime = 1000 'ms

        Dim client = CreateClient()
        Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString

        client.Lock(sessionId)

        Dim captureId = client.Capture(sessionId).WsbdResult.CaptureIds(0).Value.ToString

        ' To ensure that the test succeeds, we wait an additional second to make sure that the server has
        ' had time to prepare the download. 
        '
        Threading.Thread.Sleep(BaselineSensorService.CaptureTime + BaselineSensorService.PendingDownloadTime + 1000)

        Dim result = client.Download(captureId).WsbdResult

        Dim expected = BaselineSensorService.GenerateCapturedImage(New Guid(captureId))
        Assert.IsTrue(result.SensorData.SequenceEqual(expected))

        ' We know a priori that the BaselineSensorService should return this particular MIME type
        Assert.AreEqual("image/png", result.Metadata(Constants.ContentTypeKey))

    End Sub

    <TestMethod(), TestCategory(Download), TestCategory(PreparingDownload)> _
    Sub DownloadingImmediatelyAfterCaptureCanReturnPreparingDownload()

        BaselineSensorService.PendingDownloadTime = 1000 'ms
        BaselineSensorService.CaptureTime = 100 'ms
        Dim client = CreateClient()
        Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString

        client.Lock(sessionId)

        ' Immediately after capture, a download may not be ready.
        Dim captureId = client.Capture(sessionId).WsbdResult.CaptureIds(0).Value.ToString
        Dim result = client.Download(captureId).WsbdResult
        Assert.AreEqual(Status.PreparingDownload, result.Status)

        ' For testing purposes, wait until the download is no longer pending. We add a some time
        ' just to make sure.
        '
        Console.WriteLine("Sleeping to make sure download is no longer pending...")
        Threading.Thread.Sleep(BaselineSensorService.PendingDownloadTime + 2000)
        Console.WriteLine("Done sleeping")

        ' After the service has completed preparing the download, the correct
        ' sensor data should be available.
        '
        Dim downloadResult = client.Download(captureId).WsbdResult
        Dim expected = BaselineSensorService.GenerateCapturedImage(New Guid(captureId))
        Assert.IsTrue(downloadResult.SensorData.SequenceEqual(expected))

    End Sub

    <TestMethod(), TestCategory(Download), TestCategory(Failure)> _
    Sub DownloadCanFail()

        BaselineSensorService.CaptureTime = 100 'ms
        BaselineSensorService.PendingDownloadTime = 1 'ms

        Dim client = CreateClient()
        Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString

        client.Lock(sessionId)

        Dim captureId = client.Capture(sessionId).WsbdResult.CaptureIds(0).Value.ToString

        ' To ensure that the test succeeds, we wait an additional second to make sure that the server has
        ' had time to prepare the download. 
        '
        Threading.Thread.Sleep(BaselineSensorService.PendingDownloadTime + 1000)

        ' For the purposes of testing, we corrupt the data store by deleting the underlying data.
        Dim provider = DirectCast(BaselineSensorService.StorageProvider, FileStorageProvider)
        IO.File.Delete(provider.MetadataFileName(New Guid(captureId)))

        Dim result = client.Download(captureId).WsbdResult
        Assert.AreEqual(Status.Failure, result.Status)

    End Sub



    <TestMethod(), TestCategory(ThriftyDownload), TestCategory(BadValue)> _
    Sub ThriftyDownloadingAnUnparseableIdGivesABadValue()
        Dim client = CreateClient()
        Dim result = client.ThriftyDownload("this_is_not_a_guid", "0").WsbdResult
        Assert.AreEqual(Status.BadValue, result.Status)
        Assert.IsTrue(result.BadFields.Contains(Constants.CaptureIdParameterName))
    End Sub

    <TestMethod(), TestCategory(ThriftyDownload), TestCategory(BadValue)> _
    Sub ThriftyDownloadingAnUnparseableMaxValueGivesABadValue()
        Dim client = CreateClient()
        Dim result = client.ThriftyDownload(Guid.NewGuid().ToString, "this_is_not_an_integer").WsbdResult
        Assert.AreEqual(Status.BadValue, result.Status)
        Assert.IsTrue(result.BadFields.Contains(Constants.MaxSizeParameterName))
    End Sub

    <TestMethod(), TestCategory(ThriftyDownload), TestCategory(BadValue)> _
    Sub ThriftyDownloadingAnUnparseableParametersGivesMultipleBadFields()
        Dim client = CreateClient()
        Dim result = client.ThriftyDownload("not_a_guid", "this_is_not_an_integer").WsbdResult
        Assert.AreEqual(Status.BadValue, result.Status)
        Assert.IsTrue(result.BadFields.Contains(Constants.MaxSizeParameterName))
        Assert.IsTrue(result.BadFields.Contains(Constants.CaptureIdParameterName))
    End Sub

    <TestMethod(), TestCategory(ThriftyDownload), TestCategory(Unsupported)> _
    Sub DownloadingImagesLargerThanTheOriginalIsUnsupported()

        BaselineSensorService.CaptureTime = 1 ' ms
        BaselineSensorService.PendingDownloadTime = 1 ' ms

        Dim client = CreateClient()
        Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString

        client.Lock(sessionId)
        Dim captureId = client.Capture(sessionId).WsbdResult.CaptureIds(0).Value.ToString
        Threading.Thread.Sleep(BaselineSensorService.PendingDownloadTime + 1000)

        Dim result = client.ThriftyDownload(captureId, "50000").WsbdResult

        Dim expected = BaselineSensorService.GenerateCapturedImage(New Guid(captureId))
        Assert.AreEqual(Status.Unsupported, result.Status)

    End Sub

    <TestMethod(), TestCategory(ThriftyDownload), TestCategory(BadValue)> _
    Sub ThriftyDownloadingAnUnknownIdGivesInvalidId()
        Dim client = CreateClient()
        Dim result = client.ThriftyDownload(Guid.NewGuid.ToString, "100").WsbdResult
        Assert.AreEqual(Status.InvalidId, result.Status)
    End Sub

    <TestMethod(), TestCategory(ThriftyDownload), TestCategory(Success)> _
    Sub ThriftyDownloadCanBeSuccessful()

        Dim maxSize As Integer = 100

        ' Begin test

        BaselineSensorService.CaptureTime = 1 'ms
        BaselineSensorService.PendingDownloadTime = 1 'ms

        Dim client = CreateClient()
        Dim sessionId = client.Register().WsbdResult.SessionId.Value.ToString

        client.Lock(sessionId)

        Dim captureId = client.Capture(sessionId).WsbdResult.CaptureIds(0).Value.ToString

        ' Make sure the service has time to save the capture
        Threading.Thread.Sleep(BaselineSensorService.PendingDownloadTime + 1000)


        Dim result = client.ThriftyDownload(captureId, maxSize.ToString).WsbdResult
        Dim expected = BaselineSensorService.GenerateCapturedImage(New Guid(captureId), maxSize)

        Assert.IsTrue(result.SensorData.SequenceEqual(expected))

        ' We know a priori that the BaselineSensorService should return this particular MIME type
        Assert.AreEqual("image/png", result.Metadata(Constants.ContentTypeKey))

    End Sub

End Class