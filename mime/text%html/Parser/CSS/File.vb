#Region "Microsoft.VisualBasic::ae0102c311bd2a91b517518b33cbe3f5, mime\text%html\Parser\CSS\File.vb"

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

    '   Total Lines: 105
    '    Code Lines: 60 (57.14%)
    ' Comment Lines: 34 (32.38%)
    '    - Xml Docs: 88.24%
    ' 
    '   Blank Lines: 11 (10.48%)
    '     File Size: 4.00 KB


    '     Class CSSFile
    ' 
    '         Properties: ByClass, ByExpression, ByID, ByTag, Selectors
    ' 
    '         Function: FindStyle, GetAllStylesByType, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Language.CSS

    ''' <summary>
    ''' CSS文件的对象模型，一个CSS文件是由若干个selector节点选择器所构成的，以及每一个选择器都是由若干样式属性定义所构成
    ''' </summary>
    Public Class CSSFile

        ''' <summary>
        ''' a collection of the html element node selector expression
        ''' </summary>
        ''' <returns></returns>
        Public Property Selectors As Dictionary(Of Selector)

        ''' <summary>
        ''' GetElementByID
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ByID As Selector()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return GetAllStylesByType(CSSSelectorTypes.id)
            End Get
        End Property

        ''' <summary>
        ''' GetElementsByClass
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ByClass As Selector()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return GetAllStylesByType(CSSSelectorTypes.class)
            End Get
        End Property

        ''' <summary>
        ''' By html tags
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ByTag As Selector()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return GetAllStylesByType(CSSSelectorTypes.tag)
            End Get
        End Property

        Public ReadOnly Property ByExpression As Selector()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return GetAllStylesByType(CSSSelectorTypes.expression)
            End Get
        End Property

        Default Public ReadOnly Property GetSelector(name$) As Selector
            Get
                If Selectors.ContainsKey(name) Then
                    Return Selectors(name)
                Else
                    ' 因为CSS是手工编写的，所以可能会出现大小写错误的问题
                    ' 如果字典查找失败的话，则尝试使用字符串匹配来查找
                    Return Selectors _
                        .Values _
                        .Where(Function(style)
                                   Return style.Selector.TextEquals(name)
                               End Function) _
                        .FirstOrDefault
                End If
            End Get
        End Property

        ''' <summary>
        ''' 获取某一种类型之下的所有的该类型的CSS的样式定义
        ''' </summary>
        ''' <param name="type"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetAllStylesByType(type As CSSSelectorTypes) As Selector()
            Return Selectors.Values _
                .Where(Function(style) style.Type = type) _
                .ToArray
        End Function

        ''' <summary>
        ''' 根据类型来获取得到相应的选择器的样式
        ''' </summary>
        ''' <param name="name$">没有class或者ID的符号前缀的名称</param>
        ''' <param name="type">class还是ID或者还是html的标签名称？</param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function FindStyle(name$, type As CSSSelectorTypes) As Selector
            Return GetSelector(name.BuildSelector(type))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return Selectors.Keys.ToArray.GetJson
        End Function
    End Class
End Namespace
