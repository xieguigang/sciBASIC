Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Linq
Imports System.Reflection

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

    ''' <summary>
    ''' <see cref="Members"/> As <see cref="Dictionary"/>
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property Tables As IReadOnlyDictionary(Of String, String)

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function

    Public Shared Function GetSchema(Of T As Class)() As Schema
        Return GetSchema(GetType(T))
    End Function

    Public Shared Function GetSchema(type As Type) As Schema
        Dim props = type.GetProperties(BindingFlags.Public + BindingFlags.Instance)
        Dim members As New Dictionary(Of NamedValue(Of String))

        For Each prop As PropertyInfo In props
            If prop.PropertyType.IsPrimitive Then

            End If
        Next
    End Function
End Class
