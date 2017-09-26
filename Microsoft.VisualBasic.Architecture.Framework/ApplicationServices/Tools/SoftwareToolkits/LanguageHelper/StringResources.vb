#Region "Microsoft.VisualBasic::b3a7db6832238e428a4ce4c22f99a164, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\ApplicationServices\Tools\SoftwareToolkits\LanguageHelper\StringResources.vb"

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
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace SoftwareToolkits.Globalization

    Public Class StringResources(Of TLanguage)

        Public ReadOnly Property Resources As Dictionary(Of TLanguage, LanguageAttribute)
        Public ReadOnly Property Name As String
        ''' <summary>
        ''' 从属性或者域上面解析出来的默认的语言值
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property [Default] As String

        Sub New(Name As String, resources As LanguageAttribute())
            Me.Name = Name
            Me.Resources = resources.ToDictionary(Of TLanguage, LanguageAttribute)(
                Function(lang) CTypeDynamic(Of TLanguage)(lang.Language),
                Function(lang) lang)
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Name}({Resources.Count}) = {[Default]}"
        End Function

        Public Shared Function SafelyGenerates(member As MemberInfo) As StringResources(Of TLanguage)
            Dim langResources = __getLanguageResources(member)
            If langResources.IsNullOrEmpty Then
                Return Nothing
            Else
                Return New StringResources(Of TLanguage)(member.Name, langResources) With {
                    ._Default = __getValue(member)
                }
            End If
        End Function

        Private Shared Function __getValue(member As MemberInfo) As String
            Dim value As Object = If(member.MemberType = MemberTypes.Property,
                DirectCast(member, PropertyInfo).GetValue(Nothing, Nothing),
                DirectCast(member, FieldInfo).GetValue(Nothing))
            Return Scripting.ToString(value)
        End Function

        Private Shared Function __getLanguageResources(member As MemberInfo) As LanguageAttribute()
            Dim attrs As Object() = member.GetCustomAttributes(attributeType:=GetType(LanguageAttribute), inherit:=True)
            If attrs.IsNullOrEmpty Then
                Return Nothing
            Else
                Return attrs.ToArray(Function(attr) DirectCast(attr, LanguageAttribute))
            End If
        End Function
    End Class
End Namespace
