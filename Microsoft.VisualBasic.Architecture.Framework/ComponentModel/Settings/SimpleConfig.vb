#Region "Microsoft.VisualBasic::12b1342bd6c52d96e702bba16ac4a44f, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ComponentModel\Settings\SimpleConfig.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.DataFramework
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel.Settings

#If NET_40 = 0 Then

    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class SimpleConfig : Inherits Attribute
        Dim _ToLower As Boolean

        Public Shared ReadOnly Property TypeInfo As Type = GetType(SimpleConfig)
        Public ReadOnly Property Name As String

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(Optional Name As String = "", Optional toLower As Boolean = True)
            Me._Name = Name
            Me._ToLower = toLower
        End Sub

        ''' <summary>
        ''' Display <see cref="Name"/>
        ''' </summary>
        ''' <returns></returns>
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
                Dim [property] As PropertyInfo = DirectCast(line.member, PropertyInfo)

                If [property].CanRead AndAlso [property].CanWrite Then  '同时满足可读和可写的属性直接添加
                    GoTo INSERT
                End If

                '从这里开始的属性都是只读属性或者只写属性
                If canRead = True Then
                    If [property].CanRead = False Then
                        Continue For
                    End If
                End If
                If canWrite = True Then
                    If [property].CanWrite = False Then
                        Continue For
                    End If
                End If
INSERT:
                If String.IsNullOrEmpty(line.Field._Name) Then
                    line.Field._Name =
                        If(line.Field._ToLower,
                        line.Identity.ToLower,
                        line.Identity)
                End If

                ' 这里为什么会出现重复的键名？？？
                Schema += New BindProperty(Of TConfig)(line.Field, [property])
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
            Dim mlen As Integer = (From cfg As SimpleConfig In Schema.Select(Function(x) x.Field) Select Len(cfg._Name)).Max
            Dim bufs As New List(Of String)

            For Each [property] As BindProperty(Of SimpleConfig) In Schema
                Dim blank As New String(" ", mlen - Len([property].Field._Name) + 2)
                Dim Name As String = [property].Field._Name & blank
                Dim value As String = Scripting.ToString([property].GetValue(target))

                bufs += $"{Name}= {value}"
            Next

            Return bufs.ToArray
        End Function
    End Class

#End If

End Namespace
