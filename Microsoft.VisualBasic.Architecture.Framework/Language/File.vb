Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel

Namespace Language

    Public MustInherit Class File : Inherits ClassObject
        Implements ISaveHandle

        Public Function Save(Optional Path As String = "", Optional encoding As Encodings = Encodings.UTF8) As Boolean Implements ISaveHandle.Save
            Return Save(Path, encoding.GetEncodings)
        End Function

        Public MustOverride Function Save(Optional Path As String = "", Optional encoding As Encoding = Nothing) As Boolean Implements ISaveHandle.Save

        Public Shared Operator >(file As File, path As String) As Boolean
            Return file.Save(path, Encodings.UTF8)
        End Operator

        Public Shared Operator <(file As File, path As String) As Boolean
            Throw New NotImplementedException
        End Operator
    End Class
End Namespace