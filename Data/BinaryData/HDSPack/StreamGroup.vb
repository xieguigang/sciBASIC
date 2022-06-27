Imports Microsoft.VisualBasic.FileIO.Path

Public Class StreamGroup : Inherits StreamObject

    Public ReadOnly Property tree As Dictionary(Of String, StreamObject)

    Public Function GetDataBlock(filepath As FilePath) As StreamBlock

    End Function

    Public Function AddDataBlock(filepath As FilePath) As StreamBlock
        Dim createNew As New StreamBlock(filepath)

        ' then add current file block to the tree

        Return createNew
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
        Dim tree As Dictionary(Of String, StreamObject) = Me.tree
        Dim file As StreamObject
        Dim names As String() = filepath.Components
        Dim name As String

        For i As Integer = 0 To names.Length - 1
            name = names(i)

            If Not tree.ContainsKey(name) Then
                Return False
            Else
                file = tree(name)

                If TypeOf file Is StreamGroup Then
                    tree = DirectCast(file, StreamGroup).tree
                ElseIf i <> names.Length - 1 Then
                    ' required a folder(file group)
                    ' but a file is exists?
                    Return False
                End If
            End If
        Next

        Return True
    End Function
End Class
