Imports Microsoft.VisualBasic.FileIO.Path

Namespace FileSystem

    Public Class StreamBlock : Inherits StreamObject

        Public Property offset As Long
        Public Property size As Long

        Sub New()
        End Sub

        Sub New(filepath As FilePath)
            Call MyBase.New(filepath)
        End Sub

        Public Overrides Function ToString() As String
            Return $"{MyBase.ToString} [offset={offset}, size={StringFormats.Lanudry(size)}]"
        End Function

    End Class
End Namespace