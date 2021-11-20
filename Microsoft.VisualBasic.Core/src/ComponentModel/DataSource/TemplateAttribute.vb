Namespace ComponentModel.DataSourceModel

    Public Class TemplateAttribute : Inherits Attribute

        ReadOnly schema As Type
        ReadOnly schemaName As String

        Sub New(schema As Type)
            Me.schema = schema
        End Sub

        Sub New(name As String)
            Me.schemaName = name
        End Sub

    End Class
End Namespace