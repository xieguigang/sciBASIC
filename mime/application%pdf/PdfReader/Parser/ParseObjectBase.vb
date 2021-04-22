
Namespace PdfReader
    Public MustInherit Class ParseObjectBase
        Public Shared ReadOnly [True] As ParseBoolean = New ParseBoolean(True)
        Public Shared ReadOnly [False] As ParseBoolean = New ParseBoolean(False)
        Public Shared ReadOnly Null As ParseNull = New ParseNull()
    End Class
End Namespace
