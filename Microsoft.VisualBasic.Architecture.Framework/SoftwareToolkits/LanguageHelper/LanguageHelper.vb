#Region "Microsoft.VisualBasic::1b06906f7d2b3533e1d55d2647f856c3, ..\VisualBasic_AppFramework\Microsoft.VisualBasic.Architecture.Framework\SoftwareToolkits\LanguageHelper\LanguageHelper.vb"

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

Namespace Logging.LanguageHelper
    Public Class LanguageHelper

        ''' <summary>
        ''' Enum the mainly used language.
        ''' </summary>
        ''' <remarks></remarks>
        Public Enum Languages
            ''' <summary>
            ''' Following the system default language.
            ''' </summary>
            ''' <remarks></remarks>
            <Description("System")> System

            ''' <summary>
            ''' Language in Chinese simplify.
            ''' (简体中文) 
            ''' </summary>
            ''' <remarks></remarks>
            <Description("zh-CN")> ZhCN
            ''' <summary>
            ''' Language in English.
            ''' (英语语言)
            ''' </summary>
            ''' <remarks></remarks>
            <Description("en-US")> EnUS
            ''' <summary>
            ''' Language in french.
            ''' (法语语言)
            ''' </summary>
            ''' <remarks></remarks>
            <Description("fr-FR")> FrFR
        End Enum

        Dim LanguageDatas As Dictionary(Of Languages, String) = New Dictionary(Of Languages, String)

        ''' <summary>
        ''' Get the string that store as specific language.
        ''' (获取指定语言的字符串)
        ''' </summary>
        ''' <param name="Language">字符串的语言</param>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Default Public ReadOnly Property Text(Language As Languages) As String
            Get
                Return LanguageDatas(Language)
            End Get
        End Property

        Shared Widening Operator CType(e As PropertyInfo) As LanguageHelper
            Return LanguageHelper.Create(GetAttribute(Of LanguageAttribute)(e))
        End Operator

        Shared Widening Operator CType(e As MemberInfo) As LanguageHelper
            Return LanguageHelper.Create(GetAttribute(Of LanguageAttribute)(e))
        End Operator

        Shared Widening Operator CType(e As MethodInfo) As LanguageHelper
            Return LanguageHelper.Create(GetAttribute(Of LanguageAttribute)(e))
        End Operator

        Private Shared Function Create(e As LanguageAttribute()) As LanguageHelper
            Dim NewObj As New LanguageHelper

            For Each Language In e
                NewObj.LanguageDatas.Add(Language, Language)
            Next
            Return NewObj
        End Function

#Region ""
        ''' <summary>
        ''' Get the specific type of custom attribute from a property.
        ''' (从一个属性对象中获取特定的自定义属性对象)
        ''' </summary>
        ''' <typeparam name="T">The type of the custom attribute.(自定义属性的类型)</typeparam>
        ''' <param name="Property">Target property object.(目标属性对象)</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Friend Shared Function GetAttribute(Of T As Attribute)([Property] As PropertyInfo) As T()
            Return GetAttribute(Of T)([Property].GetCustomAttributes(GetType(T), True))
        End Function

        ''' <summary>
        ''' Get the specific type of custom attribute from a property.
        ''' (从一个属性对象中获取特定的自定义属性对象)
        ''' </summary>
        ''' <typeparam name="T">The type of the custom attribute.(自定义属性的类型)</typeparam>
        ''' <param name="Property">Target property object.(目标属性对象)</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Friend Shared Function GetAttribute(Of T As Attribute)([Property] As MemberInfo) As T()
            Return GetAttribute(Of T)([Property].GetCustomAttributes(GetType(T), True))
        End Function

        ''' <summary>
        ''' Get the specific type of custom attribute from a property.
        ''' (从一个属性对象中获取特定的自定义属性对象)
        ''' </summary>
        ''' <typeparam name="T">The type of the custom attribute.(自定义属性的类型)</typeparam>
        ''' <param name="Property">Target property object.(目标属性对象)</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Friend Shared Function GetAttribute(Of T As Attribute)([Property] As MethodInfo) As T()
            Return GetAttribute(Of T)([Property].GetCustomAttributes(GetType(T), True))
        End Function

        Friend Shared Function GetAttribute(Of T As Attribute)(Attributes As Object()) As T()
            Dim AttrList As List(Of T) = New List(Of T)

            If Attributes Is Nothing Then Return Nothing

            For Each Attr In Attributes
                Dim CustomAttr As T = CType(Attr, T)

                If Not CustomAttr Is Nothing Then
                    AttrList.Add(CustomAttr)
                End If
            Next

            Return AttrList.ToArray
        End Function

        Public Shared Function GetDescription(e As [Enum]) As String
            Dim Type As Type = e.GetType()
            Dim MemInfos As MemberInfo() = Type.GetMembers(e.ToString)

            Const Scan0 As Integer = 0

            If Not MemInfos Is Nothing AndAlso MemInfos.Length > 0 Then
                Dim CustomAttrs As Object() =
                    MemInfos(Scan0).GetCustomAttributes(GetType(DescriptionAttribute), inherit:=False)

                If CustomAttrs Is Nothing AndAlso CustomAttrs.Length > 0 Then
                    Return CType(CustomAttrs.First, DescriptionAttribute).Description
                End If
            End If

            Return e.ToString
        End Function
#End Region
    End Class
End Namespace
