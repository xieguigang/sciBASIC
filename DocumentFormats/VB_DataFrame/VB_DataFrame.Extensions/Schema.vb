Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.DocumentFormat.Csv.StorageProvider.Reflection.TypeSchemaProvider
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' The schema project json file.
''' </summary>
Public Class Schema : Inherits ClassObject

    ''' <summary>
    ''' 默认的主文件的名称
    ''' </summary>
    Public Const DefaultName As String = NameOf(Schema) & ".json"

    ''' <summary>
    ''' ``{member.Name, fileName}``, ``#`` value means this filed its type is the primitive type. 
    ''' If not, then the value goes a external file name.
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

    Public Property Type As String

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
        Dim members As New Dictionary(Of NamedValue(Of String))

        Call __memberStack(members, type, "$", "#")

        Return New Schema With {
            .Type = type.FullName,
            .Members = members.Values.ToArray
        }
    End Function

    Private Shared Sub __memberStack(members As Dictionary(Of NamedValue(Of String)), type As Type, parent As String, path As String)
        Dim props = type.GetProperties(BindingFlags.Public + BindingFlags.Instance)
        Dim pType As Type
        Dim elType As Type

        For Each prop As PropertyInfo In props
            pType = prop.PropertyType

            ' 假若是基本类型或者字符串，则会直接添加
            If pType.IsPrimitive OrElse pType.Equals(GetType(String)) Then
                members += New NamedValue(Of String) With {
                    .Name = $"{parent}::{prop.Name}",
                    .x = path
                }
            Else

                elType = pType.GetThisElement(False)

                If elType Is Nothing Then
                    ' 不是集合类型
                    Call __memberStack(members, prop.PropertyType, $"{parent}::{prop.Name}", parent.Replace("::", "/") & $"/{prop.Name}.Csv")
                ElseIf elType.IsPrimitive OrElse elType.Equals(GetType(String)) Then
                    ' 基本类型
                    members += New NamedValue(Of String) With {
                        .Name = $"{parent}::{prop.Name}",
                        .x = "#"
                    }
                Else     ' 复杂类型
                    Call __memberStack(members, elType, $"{parent}::{prop.Name}", parent.Replace("::", "/") & $"/{prop.Name}.Csv")
                End If
            End If
        Next
    End Sub
End Class
