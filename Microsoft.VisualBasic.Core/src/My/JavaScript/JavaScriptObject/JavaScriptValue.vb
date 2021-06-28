Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Language
Imports any = Microsoft.VisualBasic.Scripting

Namespace My.JavaScript

    Public Class JavaScriptValue

        Public Property Accessor As BindProperty(Of DataFrameColumnAttribute)
        Public Property Literal As Object

        Dim target As JavaScriptObject

        Public ReadOnly Property IsConstant As Boolean
            Get
                Return Accessor.member Is Nothing
            End Get
        End Property

        Sub New(bind As BindProperty(Of DataFrameColumnAttribute), target As JavaScriptObject)
            Me.Accessor = bind
            Me.target = target
        End Sub

        Sub New()
        End Sub

        Public Function GetValue() As Object
            If IsConstant Then
                Return Literal
            Else
                Return Accessor.GetValue(target)
            End If
        End Function

        Public Sub SetValue(value As Object)
            If IsConstant Then
                Literal = value
            Else
                Accessor.SetValue(target, value)
            End If
        End Sub

        Public Overrides Function ToString() As String
            Return any.ToString(GetValue)
        End Function
    End Class

End Namespace