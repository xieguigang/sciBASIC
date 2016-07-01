Friend Class UpdatesEditor : Implements ComponentModel.ITextFile.IDocumentEditor

    Private Sub UpdatesEditor_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

    Public Property DocumentPath As String Implements ComponentModel.ITextFile.IDocumentEditor.DocumentPath

    Public Function LoadDocument(Path As String) As Boolean Implements ComponentModel.ITextFile.IDocumentEditor.LoadDocument
        Throw New NotImplementedException
    End Function

    Public Function Save(Optional FilePath As String = "", Optional Encoding As Text.Encoding = Nothing) As Boolean Implements ComponentModel.ITextFile.IDocumentEditor.Save
        Throw New NotImplementedException
    End Function
End Class