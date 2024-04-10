Namespace ApplicationServices

    ''' <summary>
    ''' A virtual filesystem tree
    ''' </summary>
    Public Class FileSystemTree

        Public Property Name As String
        Public Property Files As Dictionary(Of String, FileSystemTree)
        Public Property Parent As FileSystemTree

        Public ReadOnly Property FullName As String
            Get
                If Parent Is Nothing OrElse Parent Is Me Then
                    Return $"/{Name}"
                Else
                    Return $"{Parent}/{Name}"
                End If
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns>
        ''' returns nothing if the file with given <paramref name="name"/> was could not be found
        ''' </returns>
        Public Function GetFile(name As String) As FileSystemTree
            Return Files.TryGetValue(name)
        End Function

        ''' <summary>
        ''' add a new filesystem node and then returns the new node object.
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public Function AddFile(name As String) As FileSystemTree
            Dim node As New FileSystemTree With {
                .Name = name,
                .Files = New Dictionary(Of String, FileSystemTree),
                .Parent = Me
            }
            Files.Add(name, node)
            Return node
        End Function

        Public Overrides Function ToString() As String
            Return FullName
        End Function

        Public Shared Function BuildTree(files As IEnumerable(Of String)) As FileSystemTree
            Dim root As New FileSystemTree With {
                .Files = New Dictionary(Of String, FileSystemTree),
                .Parent = Nothing,
                .Name = Nothing
            }
            Dim tokens As String()
            Dim dir As FileSystemTree
            Dim node As FileSystemTree

            For Each path As String In files
                tokens = path.Replace("\", "/").Trim("/"c).StringSplit("[/]+")
                dir = root

                For Each name As String In tokens
                    node = dir.GetFile(name)

                    If node Is Nothing Then
                        dir = dir.AddFile(name)
                    Else
                        dir = node
                    End If
                Next
            Next

            Return root
        End Function

    End Class
End Namespace