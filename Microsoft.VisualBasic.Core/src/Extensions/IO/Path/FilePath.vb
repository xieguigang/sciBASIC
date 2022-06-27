Namespace FileIO.Path

    Public Class FilePath

        Public ReadOnly Property Components As String()
        Public ReadOnly Property IsDirectory As Boolean = False
        Public ReadOnly Property IsAbsolutePath As Boolean = False

        Public ReadOnly Property DirectoryPath As String
            Get
                If IsDirectory Then
                    Return If(IsAbsolutePath, "/", "") & Components.JoinBy("/")
                Else
                    Return If(IsAbsolutePath, "/", "") & Components.Take(Components.Length - 1).JoinBy("/")
                End If
            End Get
        End Property

        Public ReadOnly Property FileName As String
            Get
                Return Components.Last
            End Get
        End Property

        Sub New(filepath As String)
            If filepath.EndsWith("/"c) OrElse filepath.EndsWith("\"c) Then
                IsDirectory = True
            End If
            If filepath.StartsWith("/"c) OrElse filepath.StartsWith("\"c) Then
                IsAbsolutePath = True
            End If

            Components = filepath _
                .Replace("\", "/") _
                .Split("/"c) _
                .Where(Function(t) Not t.StringEmpty) _
                .ToArray
        End Sub


    End Class
End Namespace