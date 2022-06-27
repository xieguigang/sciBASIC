Namespace FileIO.Path

    Public Class FilePath

        Public ReadOnly Property Components As String()
        Public ReadOnly Property IsDirectory As Boolean = False
        Public ReadOnly Property IsAbsolutePath As Boolean = False

        Public ReadOnly Property DirectoryPath As String
            Get
                Return If(IsAbsolutePath, "/", "") & combineDirectory()
            End Get
        End Property

        Public ReadOnly Property FileName As String
            Get
                Return Components.Last
            End Get
        End Property

        Public ReadOnly Property ParentDirectory As FilePath
            Get
                If Components.Length = 1 Then
                    Return New FilePath("/")
                Else
                    Return New FilePath(Components.Take(Components.Length - 1), True, IsAbsolutePath)
                End If
            End Get
        End Property

        Public Sub New(tokens As IEnumerable(Of String), isDir As Boolean, isAbs As Boolean)
            Components = (From name As String In tokens Where Not name.StringEmpty).ToArray
            IsDirectory = isDir
            IsAbsolutePath = isAbs
        End Sub

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

        Private Function combineDirectory() As String
            If IsDirectory Then
                Return Components.JoinBy("/")
            Else
                Return Components.Take(Components.Length - 1).JoinBy("/")
            End If
        End Function

        Public Overrides Function ToString() As String
            If IsDirectory Then
                Return DirectoryPath.StringReplace("[/]{2,}", "/")
            Else
                Return $"{DirectoryPath}/{FileName}".StringReplace("[/]{2,}", "/")
            End If
        End Function
    End Class
End Namespace