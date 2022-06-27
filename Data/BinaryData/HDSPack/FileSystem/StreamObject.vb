Imports Microsoft.VisualBasic.FileIO.Path
Imports Microsoft.VisualBasic.Linq
Imports any = Microsoft.VisualBasic.Scripting

Namespace FileSystem

    ''' <summary>
    ''' the abstract type of the file or directory object
    ''' </summary>
    Public MustInherit Class StreamObject

        Public ReadOnly Property referencePath As FilePath
        Public ReadOnly Property fileName As String
            Get
                Return referencePath.FileName
            End Get
        End Property

        ''' <summary>
        ''' comments about this file object
        ''' </summary>
        ''' <returns></returns>
        Public Property description As String
        Public Property attributes As New Dictionary(Of String, Object)

        Sub New(path As FilePath)
            referencePath = path
        End Sub

        Sub New()
        End Sub

        Public Sub AddAttributes(attrs As Dictionary(Of String, Object))
            For Each item As KeyValuePair(Of String, Object) In attrs.SafeQuery
                If item.Key = NameOf(description) Then
                    description = any.ToString(item.Value)
                Else
                    _attributes(item.Key) = item.Value
                End If
            Next
        End Sub

        Public Overrides Function ToString() As String
            Return referencePath.ToString
        End Function

    End Class
End Namespace