Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class IObject

    Public ReadOnly Property Type As Type
    Public ReadOnly Property Schema As Dictionary(Of BindProperty(Of DataFrameColumnAttribute))
    Public Overridable Property x As Object

    Sub New(type As Type)
        Me.Type = type
        Me.Schema = DataFrameColumnAttribute.LoadMapping(type, , mapsAll:=True)
    End Sub

    ''' <summary>
    ''' 返回: ``FiledName: value_string``
    ''' </summary>
    ''' <returns></returns>
    Public Overridable Iterator Function EnumerateFields() As IEnumerable(Of NamedValue(Of String))
        For Each field In Schema.Values
            Yield New NamedValue(Of String) With {
                .Name = field.Identity,
                .x = Scripting.ToString(field.GetValue(x))
            }
        Next
    End Function

    Public Shared Function FromString(s As String) As IObject
        Return New IObject(GetType(Text)) With {
            .x = New Text With {
                .Text = s
            }
        }
    End Function
End Class

Public Structure Text
    Public Property Text As String

    Public Overrides Function ToString() As String
        Return Text
    End Function
End Structure