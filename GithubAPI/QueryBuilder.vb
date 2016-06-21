Imports System.Collections.Specialized
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Web
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.InputHandler

Public Module QueryBuilder

    Public Const GithubAPI As String = "https://api.github.com"

    ''' <summary>
    ''' 假若有非法字符，则需要使用<see cref="Field"/>来标记出来
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="args"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Build(Of T As Structure)(args As T) As NameValueCollection
        Dim type As Type = GetType(T)
        Dim term As PropertyInfo = type.GetTerm
        Dim igs As String() = If(term Is Nothing, {}, {term.Name})
        Dim value As New NameValueCollection
        Dim s As String
        Dim o As Object

        If Not term Is Nothing Then
            s = Scripting.ToString(term.GetValue(args))
            Call value.Add(QueryBuilder.Term.Key, s)
        End If

        For Each prop In DataFrameColumnAttribute.LoadMapping(type, igs, True)
            o = prop.Value.GetValue(args)

            If Not o Is Nothing Then
                s = Scripting.ToString(o)
                Call value.Add(prop.Key, s)
            End If
        Next

        Return value
    End Function

    <Extension>
    Public Function GetTerm(type As Type) As PropertyInfo
        Dim props As IEnumerable(Of PropertyInfo) =
            type.GetProperties(BindingFlags.Public + BindingFlags.Instance)
        Dim term As Type = GetType(Term)
        Dim LQuery As PropertyInfo =
            LinqAPI.DefaultFirst(Of PropertyInfo) <= From p As PropertyInfo
                                                     In props
                                                     Let attrs As Attribute = p.GetCustomAttribute(term)
                                                     Where Not attrs Is Nothing
                                                     Select p
        Return LQuery
    End Function

    <Extension>
    Public Function BuildQueryArgs(x As NameValueCollection) As String
        Dim args As New StringBuilder(512)

        If Array.IndexOf(x.AllKeys, Term.Key) > -1 Then
            Call args.Append(x(Term.Key))
            Call x.Remove(Term.Key)
        End If

        Dim s As String() =
            LinqAPI.Exec(Of String) <= From key As String
                                       In x.Keys
                                       Select $"{key}:{HttpUtility.UrlEncode(x(key))}"
        If s.Length > 0 Then
            Call args.Append("+"c)
            Call args.Append(String.Join("+", s))
        End If

        Dim query As String = args.ToString
        Return query
    End Function

    ''' <summary>
    ''' 当使用<see cref="Term"/>标记的时候，表明这个属性为必须的参数并且没有名称
    ''' </summary>
    <AttributeUsage(AttributeTargets.Field Or AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class Term : Inherits Attribute

        Public Const Key As String = "<" & NameOf(Term) & ">"

        Public Overrides Function ToString() As String
            Return Key
        End Function
    End Class
End Module
