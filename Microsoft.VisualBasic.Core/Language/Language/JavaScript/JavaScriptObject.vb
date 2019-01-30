Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps

Namespace Language.JavaScript

    Public Class JavaScriptObject

        Private members As Dictionary(Of String, BindProperty(Of DataFrameColumnAttribute))

        Public Property Accessor(memberName As String) As Object
            Get
                If members.ContainsKey(memberName) Then
                    Return members(memberName).GetValue(Me)
                Else
                    ' Returns undefined in javascript
                    Return Nothing
                End If
            End Get
            Set(value As Object)
                If members.ContainsKey(memberName) Then
                    members(memberName).SetValue(Me, value)
                Else
                    ' 添加一个新的member？
                    Throw New NotImplementedException
                End If
            End Set
        End Property

    End Class
End Namespace