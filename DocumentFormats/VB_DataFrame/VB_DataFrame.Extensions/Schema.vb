Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Linq

''' <summary>
''' The schema project json file.
''' </summary>
Public Class Schema : Inherits ClassObject

    ''' <summary>
    ''' 默认的主文件的名称
    ''' </summary>
    Public Const DefaultName As String = NameOf(Schema) & ".json"

    ''' <summary>
    ''' {member.Name, fileName}
    ''' </summary>
    ''' <returns></returns>
    Public Property Members As NamedValue(Of String)()
        Get
            Return Tables.ToArray(
                Function(x) New NamedValue(Of String) With {
                    .Name = x.Key,
                    .x = x.Value
                })
        End Get
        Set(value As NamedValue(Of String)())
            _Tables = value.ToDictionary(Function(x) x.Name, Function(x) x.x)
        End Set
    End Property

    Public ReadOnly Property Tables As IReadOnlyDictionary(Of String, String)

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class
