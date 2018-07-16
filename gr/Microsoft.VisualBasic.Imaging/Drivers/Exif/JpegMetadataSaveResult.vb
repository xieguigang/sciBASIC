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