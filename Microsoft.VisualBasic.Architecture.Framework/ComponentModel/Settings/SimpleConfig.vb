Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.DataFramework
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel.Settings

#If NET_40 = 0 Then

    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class SimpleConfig : Inherits Attribute
        Dim _ToLower As Boolean

        Public Shared ReadOnly Property TypeInfo As System.Type = GetType(SimpleConfig)
        Public ReadOnly Property Name As String

        Sub New(Optional Name As String = "", Optional toLower As Boolean = True)
            Me._Name = Name
            Me._ToLower = toLower
        End Sub

        Public Overrides Function ToString() As String
            Return Name
        End Function

        ''' <summary>
        '''
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="TConfig"></typeparam>
        ''' <param name="canRead">向文件之中写数据的时候，需要设置为真</param>
        ''' <param name="canWrite">从文件之中读取数据的时候，需要设置为真</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function TryParse(Of T As Class,
                                           TConfig As SimpleConfig)(
                                           canRead As Boolean,
                                           canWrite As Boolean) As BindProperty(Of TConfig)()

            Dim type As TypeInfo = GetType(T), configType As Type = GetType(TConfig)
            Dim Properties = type.GetProperties(BindingFlags.Instance Or BindingFlags.Public)
            Dim LQuery As BindProperty(Of TConfig)() =
                LinqAPI.Exec(Of BindProperty(Of TConfig)) <= From [property] As PropertyInfo
                                                             In Properties
                                                             Let attrs As Object() =
                                                                 [property].GetCustomAttributes(attributeType:=configType, inherit:=True)
                                                             Where Not attrs.IsNullOrEmpty AndAlso
                                                                 PrimitiveFromString.ContainsKey([property].PropertyType)
                                                             Select New BindProperty(Of TConfig)(DirectCast(attrs.First, TConfig), [property])
            If LQuery.IsNullOrEmpty Then Return Nothing

            Dim Schema As New List(Of BindProperty(Of TConfig))

            For Each line As BindProperty(Of TConfig) In LQuery
                If line.Property.CanRead AndAlso line.Property.CanWrite Then  '同时满足可读和可写的属性直接添加
                    GoTo INSERT
                End If

                '从这里开始的属性都是只读属性或者只写属性
                If canRead = True Then
                    If line.Property.CanRead = False Then
                        Continue For
                    End If
                End If
                If canWrite = True Then
                    If line.Property.CanWrite = False Then
                        Continue For
                    End If
                End If
INSERT:
                If String.IsNullOrEmpty(line.Column._Name) Then
                    line.Column._Name =
                        If(line.Column._ToLower,
                        line.Identity.ToLower,
                        line.Identity)
                End If

                ' 这里为什么会出现重复的键名？？？
                Schema += New BindProperty(Of TConfig)(line.Column, line.Property)
            Next

            Return Schema.ToArray
        End Function

        ''' <summary>
        ''' 从类型实体生成配置文件数据
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="target"></param>
        ''' <returns></returns>
        ''' <remarks>类型实体之中的简单属性，只要具备可读属性即可被解析出来</remarks>
        Public Shared Function GenerateConfigurations(Of T As Class)(target As T) As String()
            Dim type As Type = GetType(T)
            Dim Schema = TryParse(Of T, SimpleConfig)(canRead:=True, canWrite:=False)
            Dim mlen As Integer = (From cfg As SimpleConfig In Schema.Select(Function(x) x.Column) Select Len(cfg._Name)).Max
            Dim bufs As New List(Of String)

            For Each [property] As BindProperty(Of SimpleConfig) In Schema
                Dim blank As New String(" ", mlen - Len([property].Column._Name) + 2)
                Dim Name As String = [property].Column._Name & blank
                Dim value As String = Scripting.ToString([property].GetValue(target))

                bufs += $"{Name}= {value}"
            Next

            Return bufs.ToArray
        End Function
    End Class

#End If

End Namespace