#Region "Microsoft.VisualBasic::d74776ac9c522d8067ba749e08e25a74, ..\Microsoft.VisualBasic.Architecture.Framework\Tools\SoftwareToolkits\LanguageHelper\LanguageHelper.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Imports System.ComponentModel
Imports System.Reflection

Namespace SoftwareToolkits.Globalization

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <typeparam name="TLanguage">必须是枚举类型</typeparam>
    Public Class LanguageHelper(Of TLanguage)

        Public ReadOnly Property LanguageResources As SortedDictionary(Of String, StringResources(Of TLanguage))
        Public ReadOnly Property DeclaringType As Type

        Public Property CurrentLanguage As TLanguage

        ''' <summary>
        ''' Get the string that store as specific language.
        ''' (获取指定语言的字符串)
        ''' </summary>
        ''' <param name="Language">字符串的语言</param>
        ''' <value></value>
        ''' <param name="res">请使用 NameOf 操作符来获取</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Default Public ReadOnly Property Text(Language As TLanguage, res As String) As String
            Get
                If Not _languageResources.ContainsKey(res) Then
                    Return ""
                End If
                Dim resource = _languageResources(res)
                If resource.Resources.ContainsKey(Language) Then
                    Return resource.Resources(Language).Text
                Else
                    Return resource.Default
                End If
            End Get
        End Property

        Default Public ReadOnly Property Text(res As String) As String
            Get
                Return Text(CurrentLanguage, res)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{DeclaringType.FullName}@{DirectCast(DirectCast(CurrentLanguage, Object), System.Enum).Description}"
        End Function

        ''' <summary>
        ''' 解析出所有属性，域上面的语言定义
        ''' </summary>
        ''' <param name="type">必须是模块类型或者属性或者域是共享类型的</param>
        Sub New(type As Type)
            Dim Propertys = type.GetProperties(BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Static)
            Dim Fields = type.GetFields(BindingFlags.Public Or BindingFlags.NonPublic Or BindingFlags.Static)
            Dim Members As List(Of System.Reflection.MemberInfo) = New List(Of MemberInfo)
            Call Members.AddRange(Propertys)
            Call Members.AddRange(Fields)
            Call __init(Members)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="members"></param>
        Private Sub __init(members As Generic.IEnumerable(Of MemberInfo))
            Dim LQuery = (From member As System.Reflection.MemberInfo
                          In members
                          Let res As StringResources(Of TLanguage) = StringResources(Of TLanguage).SafelyGenerates(member)
                          Where Not res Is Nothing
                          Select res).ToArray
            _LanguageResources = New SortedDictionary(Of String, StringResources(Of TLanguage))(LQuery.ToDictionary(Function(res) res.Name))
            _DeclaringType = members(Scan0).DeclaringType
        End Sub
    End Class
End Namespace
