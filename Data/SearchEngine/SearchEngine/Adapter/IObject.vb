Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Scripting.TokenIcer
Imports Microsoft.VisualBasic.Serialization.JSON

Public Structure IObject

    Public ReadOnly Property Type As Type
    Public ReadOnly Property Schema As Dictionary(Of String, IProperty)

    Sub New(type As Type)
        Me.Type = type
        Me.Schema = New Dictionary(Of String, IProperty)

        For Each p In DataFrameColumnAttribute _
            .LoadMapping(type, , mapsAll:=True)
            Call Schema.Add(p.Key, p.Value)
        Next
    End Sub

    ''' <summary>
    ''' 这个接口是针对字典类型的对象而准备的
    ''' </summary>
    ''' <param name="keys"></param>
    Sub New(keys As IEnumerable(Of String))
        Type = GetType(IDictionary)
        Schema = New Dictionary(Of String, IProperty)

        For Each key$ In keys
            Schema.Add(key$, New DictionaryKey(key$))
        Next
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

    Public Overrides Function ToString() As String
        Return New NamedValue(Of String()) With {
            .Name = Type.FullName,
            .x = Schema.Keys.ToArray
        }.GetJson
    End Function
End Structure