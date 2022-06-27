Imports Microsoft.VisualBasic.FileIO.Path

Public Class StreamGroup : Inherits StreamObject

    Public ReadOnly Property tree As Dictionary(Of String, StreamObject)

    Sub New()
    End Sub

    ''' <summary>
    ''' create a new file tree
    ''' </summary>
    ''' <param name="directory">
    ''' the folder directory path
    ''' </param>
    Sub New(directory As String)
        Call Me.New(directory.Split("/"c))
    End Sub

    ''' <summary>
    ''' create a new file tree
    ''' </summary>
    ''' <param name="dirs"></param>
    Sub New(dirs As IEnumerable(Of String))
        Call MyBase.New(New FilePath(dirs, isDir:=True, isAbs:=True))
        ' create a new empty folder tree
        tree = New Dictionary(Of String, StreamObject)
    End Sub

    Public Function GetDataBlock(filepath As FilePath) As StreamBlock
        Return VisitBlock(filepath)
    End Function

    Public Function AddDataBlock(filepath As FilePath) As StreamBlock
        Dim createNew As New StreamBlock(filepath)
        ' then add current file block to the tree
        Dim dir As FilePath = filepath.ParentDirectory
        Dim dirBlock As StreamGroup = VisitBlock(dir)

        If dirBlock Is Nothing Then
            ' no dir tree
            dirBlock = AddDataGroup(filepath)
        End If

        Call dirBlock.tree.Add(filepath.FileName, createNew)

        Return createNew
    End Function

    Public Function AddDataGroup(filepath As FilePath) As StreamGroup
        Dim tree As Dictionary(Of String, StreamObject) = Me.tree
        Dim file As StreamObject
        Dim names As String() = filepath.Components
        Dim name As String

        For i As Integer = 0 To names.Length - 1
            name = names(i)

            If Not tree.ContainsKey(name) Then
                tree.Add(name, New StreamGroup(filepath.Components.Take(i)))
                tree = DirectCast(tree(name), StreamGroup).tree
            Else
                file = tree(name)

                If TypeOf file Is StreamGroup Then
                    tree = DirectCast(file, StreamGroup).tree
                Else
                    ' required a folder(file group)
                    ' but a file is exists?
                    Throw New InvalidProgramException($"a folder can not be created at location '{names.Take(i).JoinBy("/")}' due to the reason of there is a file whith the same name on the target location!")
                End If
            End If
        Next

        Return tree(filepath.Components.Last)
    End Function

    Private Function VisitBlock(filepath As FilePath) As StreamObject
        Dim tree As Dictionary(Of String, StreamObject) = Me.tree
        Dim file As StreamObject
        Dim names As String() = filepath.Components
        Dim name As String

        For i As Integer = 0 To names.Length - 1
            name = names(i)

            If Not tree.ContainsKey(name) Then
                Return Nothing
            Else
                file = tree(name)

                If TypeOf file Is StreamGroup Then
                    tree = DirectCast(file, StreamGroup).tree
                ElseIf i <> names.Length - 1 Then
                    ' required a folder(file group)
                    ' but a file is exists?
                    Return Nothing
                End If
            End If
        Next

        Return tree(filepath.Components.Last)
    End Function

    ''' <summary>
    ''' check file exists in current tree?
    ''' </summary>
    ''' <param name="filepath">
    ''' should be a path data which is relative
    ''' to current file tree node.
    ''' </param>
    ''' <returns></returns>
    Public Function BlockExists(filepath As FilePath) As Boolean
        Return Not VisitBlock(filepath) Is Nothing
    End Function

    Public Shared Function CreateRootTree() As StreamGroup
        Return New StreamGroup("/")
    End Function
End Class
