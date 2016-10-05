Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Serialization.JSON

Public Structure IObject

    Public ReadOnly Property Type As Type
    Public ReadOnly Property Schema As Dictionary(Of BindProperty(Of DataFrameColumnAttribute))

    Sub New(type As Type)
        Me.Type = type
        Me.Schema = DataFrameColumnAttribute.LoadMapping(type, , mapsAll:=True)
    End Sub

    ''' <summary>
    ''' 返回: ``FiledName: value_string``
    ''' </summary>
    ''' <returns></returns>
    Public Iterator Function EnumerateFields(x As Object) As IEnumerable(Of NamedValue(Of String))
        For Each field In Schema.Values
            Yield New NamedValue(Of String) With {
                .Name = field.Identity,
                .x = Scripting.ToString(field.GetValue(x))
            }
        Next
    End Function
End Structure

Public Structure Text
    Public Property Text As String

    Public Overrides Function ToString() As String
        Return Text
    End Function
End Structure