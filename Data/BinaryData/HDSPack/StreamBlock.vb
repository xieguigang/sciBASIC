Imports Microsoft.VisualBasic.FileIO.Path

Public Class StreamBlock : Inherits StreamObject

    Public Property offset As Long
    Public Property size As Long

    Sub New()
    End Sub

    Sub New(filepath As FilePath)
        Call MyBase.New(filepath)
    End Sub

End Class
