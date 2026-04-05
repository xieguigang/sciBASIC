Namespace ComponentModel.DataSourceModel.SchemaMaps

    ''' <summary>
    ''' field data mapping
    ''' </summary>
    <AttributeUsage(AttributeTargets.[Property], Inherited:=True, AllowMultiple:=False)>
    Public Class ColumnAttribute : Inherits Attribute

        Public Property Name As String
        Public ReadOnly Property [alias] As String()

        Sub New(name As String, ParamArray [alias] As String())
            Me.Name = name
            Me.alias = [alias]
        End Sub

        Sub New()
        End Sub

        Public Overrides Function ToString() As String
            Return Name
        End Function
    End Class
End Namespace