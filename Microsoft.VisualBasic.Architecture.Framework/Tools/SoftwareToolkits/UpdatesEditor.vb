Imports Microsoft.VisualBasic.ComponentModel

Friend Class UpdatesEditor : Implements ComponentModel.IDocumentEditor

    Private Sub UpdatesEditor_Load(sender As Object, e As EventArgs) Handles MyBase.Load

    End Sub

    Public Property DocumentPath As String Implements ComponentModel.IDocumentEditor.DocumentPath

    Public Function LoadDocument(Path As String) As Boolean Implements ComponentModel.IDocumentEditor.LoadDocument
        Throw New NotImplementedException
    End Function

    Public Function Save(Optional FilePath As String = "", Optional Encoding As System.Text.Encoding = Nothing) As Boolean Implements ComponentModel.IDocumentEditor.Save
        Throw New NotImplementedException
    End Function

    Public Function Save(Optional Path As String = "", Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
        Return Save(Path, encoding.GetEncodings)
    End Function
End Class