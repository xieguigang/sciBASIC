Imports Microsoft.VisualBasic.FileIO.Path

Public MustInherit Class StreamObject

    Public ReadOnly Property referencePath As FilePath

    Sub New(path As FilePath)
        referencePath = path
    End Sub

    Sub New()
    End Sub

    Public Overrides Function ToString() As String
        Return referencePath.ToString
    End Function

End Class