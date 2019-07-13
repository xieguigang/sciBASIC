#Region "Microsoft.VisualBasic::e18d848709af6737a7bbf3b6de22c74f, gr\Microsoft.VisualBasic.Imaging\Drivers\Exif\JpegMetadataSaveResult.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class JpegMetadataSaveResult
    ' 
    '         Properties: FilePath, IsSuccess, Metadata
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Driver

    Friend Class JpegMetadataSaveResult

        Public Property IsSuccess() As Boolean
        Public Property FilePath() As String
        Public Property Metadata() As JpegMetadata

        Public Sub New(filePath$, metadata As JpegMetadata)
            Me.FilePath = filePath
            Me.Metadata = metadata
        End Sub
    End Class
End Namespace
