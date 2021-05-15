#Region "Microsoft.VisualBasic::ddee2d7a2f2014576eb07d0a6774f086, mime\text%html\HTML\HtmlParser\HtmlElement.vb"

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

    '     Structure ValueAttribute
    ' 
    '         Properties: IsEmpty, Name, Value
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class HtmlElement
    ' 
    '         Properties: Attributes, HtmlElements, IsEmpty, IsPlantText, Name
    '                     OnlyInnerText
    ' 
    '         Function: GetPlantText, SingleNodeParser, ToString
    ' 
    '         Sub: (+2 Overloads) Add
    ' 
    '     Class InnerPlantText
    ' 
    '         Properties: InnerText, IsEmpty, IsPlantText
    ' 
    '         Function: GetPlantText, ToString
    ' 
    '     Module SpecialHtmlElements
    ' 
    '         Properties: Br, DocumentType, Head, Html, Title
    ' 
    '         Function: IsBrChangeLine
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace HTML

    ''' <summary>
    ''' 一个标签所标记的元素以及内部文本
    ''' </summary>
    Public Class HtmlElement : Inherits InnerPlantText

        ''' <summary>
        ''' 标签名
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Name As String

        ''' <summary>
        ''' 标签的属性列表
        ''' </summary>
        ''' <returns></returns>
        Public Property Attributes As ValueAttribute()
            Get
                Return attrs.Values.ToArray
            End Get
            Set(value As ValueAttribute())
                If value.IsNullOrEmpty Then
                    attrs = New Dictionary(Of ValueAttribute)
                Else
                    attrs = value.ToDictionary
                End If
            End Set
        End Property

        Public Property HtmlElements As InnerPlantText()
            Get
                Return elementNodes.ToArray
            End Get
            Set(value As InnerPlantText())
                If value.IsNullOrEmpty Then
                    elementNodes = New List(Of InnerPlantText)
                Else
                    elementNodes = value.AsList
                End If
            End Set
        End Property

        Default Public Property Attribute(name As String) As ValueAttribute
            Get
                Return attrs.TryGetValue(name)
            End Get
            Set(value As ValueAttribute)
                If attrs.ContainsKey(name) Then
                    attrs(name) = value
                Else
                    Call attrs.Add(name, value)
                End If
            End Set
        End Property

        Public Overrides ReadOnly Property IsPlantText As Boolean
            Get
                Return False
            End Get
        End Property

        Dim attrs As New Dictionary(Of ValueAttribute)
        ''' <summary>
        ''' 当前的这个节点下面所拥有的子节点
        ''' </summary>
        Dim elementNodes As New List(Of InnerPlantText)

        Public Overrides Function GetPlantText() As String
            Dim sb As New StringBuilder(Me.InnerText)

            If Not Me.HtmlElements Is Nothing Then
                For Each node In HtmlElements
                    Call sb.Append(node.GetPlantText)
                Next
            End If

            Return sb.ToString
        End Function

        Public Sub Add(attr As ValueAttribute)
            If attrs.ContainsKey(attr.Name) Then
                Call attrs(attr.Name).Values.AddRange(attr.Values)
            Else
                Call attrs.Add(attr.Name, attr)
            End If
        End Sub

        Public Sub Add(name As String, value As String)
            If attrs.ContainsKey(name) Then
                Call attrs(name).Values.Add(value)
            Else
                Call attrs.Add(name, New ValueAttribute With {.Name = name, .Values = New List(Of String) From {value}})
            End If
        End Sub

        Public Sub Add(Node As InnerPlantText)
            Call elementNodes.Add(Node)
        End Sub

        Public ReadOnly Property OnlyInnerText As Boolean
            Get
                Return elementNodes.Count = 1 AndAlso elementNodes(Scan0).IsPlantText
            End Get
        End Property

        Public Overrides ReadOnly Property IsEmpty As Boolean
            Get
                Return MyBase.IsEmpty AndAlso
                    String.IsNullOrEmpty(Name) AndAlso
                    Attributes.IsNullOrEmpty AndAlso
                    HtmlElements.IsNullOrEmpty
            End Get
        End Property

        Public Overrides Function ToString() As String
            If attrs.Count = 0 Then
                Return $"<{Name}>...</{Name}>"
            Else
                Return $"<{Name} {attrs.Values.JoinBy(" ")}>...</{Name}>"
            End If
        End Function
    End Class
End Namespace
