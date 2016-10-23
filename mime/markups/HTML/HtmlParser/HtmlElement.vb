#Region "Microsoft.VisualBasic::4c83359a862f28cad7026278a4d5a38c, ..\visualbasic_App\mime\Markups\HTML\HtmlParser\HtmlElement.vb"

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

Imports System.Runtime.CompilerServices
Imports System.Net
Imports System.IO
Imports System.Text.RegularExpressions
Imports System.Text
Imports Microsoft.VisualBasic.Linq.Extensions
Imports Microsoft.VisualBasic.Scripting
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic

Namespace HTML

    Public Structure ValueAttribute : Implements sIdEnumerable

        Public Property Name As String Implements sIdEnumerable.Identifier
        Public Property Value As String

        Sub New(strText As String)
            Dim ep As Integer = InStr(strText, "=")
            Name = Mid(strText, 1, ep - 1)
            Value = Mid(strText, ep + 1)
            If Value.First = """"c AndAlso Value.Last = """"c Then
                Value = Mid(Value, 2, Len(Value) - 2)
            End If
        End Sub

        Sub New(name As String, value As String)
            Me.Name = name
            Me.Value = value
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Name}=""{Value}"""
        End Function
    End Structure

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
                Return __attrs.Values.ToArray
            End Get
            Set(value As ValueAttribute())
                If value.IsNullOrEmpty Then
                    __attrs = New Dictionary(Of ValueAttribute)
                Else
                    __attrs = value.ToDictionary
                End If
            End Set
        End Property

        Public Property HtmlElements As InnerPlantText()
            Get
                Return __elementNodes.ToArray
            End Get
            Set(value As InnerPlantText())
                If value.IsNullOrEmpty Then
                    __elementNodes = New List(Of InnerPlantText)
                Else
                    __elementNodes = value.ToList
                End If
            End Set
        End Property

        Default Public Property Attribute(name As String) As ValueAttribute
            Get
                Return __attrs.TryGetValue(name)
            End Get
            Set(value As ValueAttribute)
                If __attrs.ContainsKey(name) Then
                    __attrs(name) = value
                Else
                    Call __attrs.Add(name, value)
                End If
            End Set
        End Property

        Public Overrides ReadOnly Property IsPlantText As Boolean
            Get
                Return False
            End Get
        End Property

        Dim __attrs As Dictionary(Of ValueAttribute)
        Dim __elementNodes As New List(Of InnerPlantText)

        Public Overrides Function GetPlantText() As String
            Dim sbr As StringBuilder = New StringBuilder(Me.InnerText)

            If Not Me.HtmlElements Is Nothing Then
                For Each node In HtmlElements
                    Call sbr.Append(node.GetPlantText)
                Next
            End If

            Return sbr.ToString
        End Function

        Public Sub Add(attr As ValueAttribute)
            If __attrs Is Nothing Then
                __attrs = New Dictionary(Of ValueAttribute)
            End If
            Call __attrs.Add(attr.Name, attr)
        End Sub

        Public Sub Add(Node As InnerPlantText)
            Call __elementNodes.Add(Node)
        End Sub

        Public ReadOnly Property OnlyInnerText As Boolean
            Get
                Return __elementNodes.Count = 1 AndAlso
                __elementNodes(Scan0).IsPlantText
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

        Public Shared Function SingleNodeParser(value As String) As HtmlElement
            Return TextParse(value).As(Of HtmlElement)
        End Function

        Public Overrides Function ToString() As String
            Return Name
        End Function
    End Class

    ''' <summary>
    ''' Plant text inner the html.(HTML文档内的纯文本对象)
    ''' </summary>
    Public Class InnerPlantText

        Public Overridable Property InnerText As String

        Public Overrides Function ToString() As String
            Return InnerText
        End Function

        Public Overridable ReadOnly Property IsEmpty As Boolean
            Get
                Return String.IsNullOrEmpty(InnerText)
            End Get
        End Property

        Public Overridable ReadOnly Property IsPlantText As Boolean
            Get
                Return True
            End Get
        End Property

        Public Overridable Function GetPlantText() As String
            Return InnerText
        End Function
    End Class

    Public Module SpecialHtmlElements

        Public Const HTML_DOCTYPE As String = "<!DOCTYPE HTML>"

        Public Function IsBrChangeLine(str As String) As Boolean
            Return Regex.Match(str, "<br \s*/>").Success
        End Function

        Public ReadOnly Property Title As HtmlElement
            Get
                Return New HtmlElement With {.Name = "title"}
            End Get
        End Property

        Public ReadOnly Property DocumentType As HtmlElement
            Get
                Return New HtmlElement With {.Name = HTML_DOCTYPE}
            End Get
        End Property

        Public ReadOnly Property Html As HtmlElement
            Get
                Return New HtmlElement With {.Name = "html"}
            End Get
        End Property

        Public ReadOnly Property Br As HtmlElement
            Get
                Return New HtmlElement With {.Name = "br"}
            End Get
        End Property

        Public ReadOnly Property Head As HtmlElement
            Get
                Return New HtmlElement With {.Name = "head"}
            End Get
        End Property
    End Module
End Namespace
