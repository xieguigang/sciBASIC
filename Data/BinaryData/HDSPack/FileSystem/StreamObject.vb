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
        Public Property attributes As New LazyAttribute

        Sub New(path As FilePath)
            referencePath = path
        End Sub

        Sub New()
        End Sub

        Public Function hasAttributes() As Boolean
            Return attributes IsNot Nothing AndAlso Not attributes.attributes.IsNullOrEmpty
        End Function

        ''' <summary>
        ''' get attribute value by a given attribute name
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public Function GetAttribute(name As String) As Object
            If Not attributes.attributes.ContainsKey(name) Then
                Return Nothing
            Else
                Return attributes _
                    .attributes(name) _
                    .DoCall(AddressOf LazyAttribute.GetValue)
            End If
        End Function

        Public Sub AddAttributes(attrs As Dictionary(Of String, Object))
            For Each item As KeyValuePair(Of String, Object) In attrs.SafeQuery
                If item.Key = NameOf(description) Then
                    description = any.ToString(item.Value)
                Else
                    attributes.Add(item.Key, item.Value)
                End If
            Next
        End Sub

        Public Overrides Function ToString() As String
            Return referencePath.ToString
        End Function

    End Class
End Namespace