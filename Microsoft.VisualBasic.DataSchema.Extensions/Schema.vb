Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization

Public Class Schema(Of T As Attribute)

    Public Property SchemaName As String
    Public Property Fields As BindProperty(Of T)()

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class
