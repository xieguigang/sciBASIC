#Region "Microsoft.VisualBasic::c5405833bab8d1314ab3022bfc834a46, Microsoft.VisualBasic.Core\src\ComponentModel\Settings\SimpleConfig.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 128
    '    Code Lines: 83
    ' Comment Lines: 26
    '   Blank Lines: 19
    '     File Size: 5.76 KB


    '     Class SimpleConfig
    ' 
    '         Properties: Name, TypeInfo
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: bindProperties, GenerateConfigurations, ToString, TryParse
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.DataFramework
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel.SchemaMaps
Imports Microsoft.VisualBasic.Language.Default
Imports TypeSchema = System.Reflection.TypeInfo

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
        Public Shared Iterator Function TryParse(Of T As Class, TConfig As SimpleConfig)(canRead As Boolean, canWrite As Boolean) As IEnumerable(Of BindProperty(Of TConfig))
            Dim type As TypeSchema = GetType(T)
            Dim configType As Type = GetType(TConfig)
            Dim properties = type.GetProperties(BindingFlags.Instance Or BindingFlags.Public)
            Dim LQuery = bindProperties(Of TConfig)(properties, configType).ToArray

            If LQuery.IsNullOrEmpty Then
                Return
            End If

            For Each line As BindProperty(Of TConfig) In LQuery
                Dim [property] As PropertyInfo = DirectCast(line.member, PropertyInfo)

                If [property].CanRead AndAlso [property].CanWrite Then
                    ' 同时满足可读和可写的属性直接添加
                    GoTo INSERT
                End If

                ' 从这里开始的属性都是只读属性或者只写属性
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
                If String.IsNullOrEmpty(line.field._Name) Then
                    If line.field._ToLower Then
                        line.field._Name = line.Identity.ToLower
                    Else
                        line.field._Name = line.Identity
                    End If
                End If

                ' 这里为什么会出现重复的键名？？？
                Yield New BindProperty(Of TConfig)(line.field, [property])
            Next
        End Function

        Private Shared Function bindProperties(Of Tconfig As SimpleConfig)(properties As PropertyInfo(), configType As Type) As IEnumerable(Of BindProperty(Of Tconfig))
            Return From [property] As PropertyInfo
                   In properties
                   Let attrs As Object() = [property].GetCustomAttributes(attributeType:=configType, inherit:=True)
                   Let info As Type = [property].PropertyType
                   Where Not attrs.IsNullOrEmpty AndAlso StringParsers.ContainsKey(info)
                   Let attr = DirectCast(attrs.First, Tconfig)
                   Select New BindProperty(Of Tconfig)(attr, [property])
        End Function

        Shared ReadOnly defaultFormat As New [Default](Of Func(Of String, String, String))(Function(name, value) $"{name}= {value}")

        ''' <summary>
        ''' 从类型实体生成配置文件数据
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="target"></param>
        ''' <param name="formats">
        ''' ``[name, value] => format_output``
        ''' </param>
        ''' <returns></returns>
        ''' <remarks>类型实体之中的简单属性，只要具备可读属性即可被解析出来</remarks>
        Public Shared Iterator Function GenerateConfigurations(Of T As Class)(target As T, Optional formats As Func(Of String, String, String) = Nothing) As IEnumerable(Of String)
            Dim type As Type = GetType(T)
            Dim schema = TryParse(Of T, SimpleConfig)(canRead:=True, canWrite:=False).ToArray
            Dim mlen As Integer = Aggregate cfg As SimpleConfig
                                  In schema.Select(Function(x) x.field)
                                  Let l = Len(cfg._Name)
                                  Into Max(l)
            Dim formatter = formats Or defaultFormat

            For Each [property] As BindProperty(Of SimpleConfig) In schema
                Dim blank As New String(" ", mlen - Len([property].field._Name) + 2)
                Dim name As String = [property].field._Name & blank
                Dim value As String = Scripting.ToString([property].GetValue(target))

                Yield formatter(name, value)
            Next
        End Function
    End Class

#End If

End Namespace
