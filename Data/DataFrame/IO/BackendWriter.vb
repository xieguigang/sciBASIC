Imports Microsoft.VisualBasic.Text

Namespace IO

    ''' <summary>
    ''' 常用于WebApp进行后端数据保存
    ''' </summary>
    Public Class BackendWriter

        Sub New(path$, Optional append As Boolean = True, Optional encoding As Encodings = Encodings.UTF8)

        End Sub

        Public Sub Queue(row As RowObject)

        End Sub
    End Class
End Namespace