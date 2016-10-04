Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class IObject

    Public ReadOnly Property Type As Type
    Public ReadOnly Property Schema As Dictionary(Of BindProperty(Of Field))
    Public Property x As Object

    Sub New(type As Type)
        Me.Type = type
    End Sub

    ''' <summary>
    ''' 返回: ``FiledName: value_string``
    ''' </summary>
    ''' <returns></returns>
    Public Iterator Function EnumerateFields() As IEnumerable(Of NamedValue(Of String))

    End Function
End Class
