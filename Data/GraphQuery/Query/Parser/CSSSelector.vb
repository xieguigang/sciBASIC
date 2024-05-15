#Region "Microsoft.VisualBasic::4b1b372f53fb06a946c5cd5bf836a9b0, Data\GraphQuery\Query\Parser\CSSSelector.vb"

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

    '   Total Lines: 195
    '    Code Lines: 133
    ' Comment Lines: 37
    '   Blank Lines: 25
    '     File Size: 7.52 KB


    ' Class CSSSelector
    ' 
    '     Constructor: (+2 Overloads) Sub New
    '     Function: getElementQueryOutput, ParseImpl, selectByClass, selectByList, selectByTagName
    ' 
    ' Structure Selector
    ' 
    '     Properties: isComposeCssQuery
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: ParseIndex, RunComposeCssQuery
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.Html.Document

Public Class CSSSelector : Inherits Parser

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="func">always css</param>
    ''' <param name="parameters"></param>
    Sub New(func As String, parameters As String())
        Call MyBase.New(func, parameters)
    End Sub

    Sub New(selector As String)
        Call MyBase.New("css", {selector, "*"})
    End Sub

    Protected Overrides Function ParseImpl(document As InnerPlantText, isArray As Boolean, env As Engine) As InnerPlantText
        Dim query As String = parameters(Scan0)
        Dim n As String = parameters.ElementAtOrDefault(1)

        If TypeOf document Is HtmlDocument Then
            If DirectCast(document, HtmlDocument).HtmlElements.IsNullOrEmpty Then
                document = New InnerPlantText
            Else
                document = DirectCast(document, HtmlDocument).HtmlElements(Scan0)
            End If
        End If

        If document Is Nothing OrElse document.GetType Is GetType(InnerPlantText) Then
            Return New InnerPlantText
        End If

        Dim css As New Selector(query, n, isArray)

        If css.isComposeCssQuery Then
            Return css.RunComposeCssQuery(document, env)
        End If

        If query.IndexOf(","c) > -1 Then
            Return selectByList(document, css)
        ElseIf query.First = "#"c Then
            ' get element by id
            Return DirectCast(document, HtmlElement).getElementById(query.Substring(1))
        ElseIf query.First = "."c Then
            Return selectByClass(document, css)
        Else
            Return selectByTagName(document, css)
        End If
    End Function

    ''' <summary>
    ''' ### 4.1. Selector Lists
    ''' 
    ''' https://www.w3.org/TR/selectors/#grouping
    ''' 
    ''' A comma-separated list of selectors represents the union of all elements 
    ''' selected by each of the individual selectors in the selector list. (A 
    ''' comma is U+002C.) For example, in CSS when several selectors share the 
    ''' same declarations, they may be grouped into a comma-separated list. White 
    ''' space may appear before and/or after the comma.
    ''' 
    ''' CSS example: In this example, we condense three rules with identical 
    ''' declarations into one. Thus,
    ''' 
    ''' ```css
    ''' h1 { font-family sans-serif }
    ''' h2 { font-family: sans-serif }
    ''' h3 { font-family: sans-serif }
    ''' ```
    ''' 
    ''' Is equivalent to:
    '''
    ''' ```css
    ''' h1, h2, h3 { font-family sans-serif }
    ''' ```
    ''' </summary>
    ''' <param name="document"></param>
    ''' <param name="selector"></param>
    ''' <returns></returns>
    Private Function selectByList(document As InnerPlantText, selector As Selector) As InnerPlantText
        Dim any As String() = selector.query.StringSplit("\s*,\s*")
        Dim list As New List(Of InnerPlantText)

        For Each query As String In any
            Select Case query.First
                Case "."c
                    list += DirectCast(document, HtmlElement).getElementsByClassName(query.Substring(1))
                Case "#"c
                    list += DirectCast(document, HtmlElement).getElementById(query.Substring(1))
                Case Else
                    list += DirectCast(document, HtmlElement).getElementsByTagName(query)
            End Select
        Next

        If selector.isArray AndAlso parameters.Length = 1 Then
            Return New HtmlElement With {
                .TagName = selector.query,
                .HtmlElements = DirectCast(document, HtmlElement) _
                    .GetDirectChilds(list) _
                    .ToArray,
                .Attributes = {AutoContext.Attribute}
            }
        ElseIf selector.isArray AndAlso selector.n = "*" Then
            Return New HtmlElement With {
                .TagName = selector.query,
                .HtmlElements = list,
                .Attributes = {AutoContext.Attribute}
            }
        Else
            Return GetElementByIndex(list, selector.ParseIndex)
        End If
    End Function

    Private Function selectByTagName(document As InnerPlantText, selector As Selector) As InnerPlantText
        Dim list As HtmlElement() = DirectCast(document, HtmlElement).getElementsByTagName(selector.query)
        Dim query As InnerPlantText = getElementQueryOutput(document, list, selector)

        Return query
    End Function

    Friend Shared Function getElementQueryOutput(document As HtmlElement, list As HtmlElement(), selector As Selector) As InnerPlantText
        If selector.isArray AndAlso selector.n.StringEmpty Then
            ' get elements by tag name/class
            Return New HtmlElement With {
                .TagName = selector.query,
                .HtmlElements = DirectCast(document, HtmlElement) _
                    .GetDirectChilds(list) _
                    .ToArray,
                .Attributes = {AutoContext.Attribute}
            }
        ElseIf selector.isArray AndAlso selector.n = "*" Then
            Return New HtmlElement With {
                .TagName = selector.query,
                .HtmlElements = list,
                .Attributes = {AutoContext.Attribute}
            }
        Else
            Return GetElementByIndex(list, selector.ParseIndex)
        End If
    End Function

    Private Function selectByClass(document As InnerPlantText, selector As Selector) As InnerPlantText
        Dim list As HtmlElement() = DirectCast(document, HtmlElement) _
            .getElementsByClassName(selector.query.Substring(1)) _
            .ToArray
        Dim query As InnerPlantText = getElementQueryOutput(document, list, selector)

        Return query
    End Function
End Class

Public Structure Selector

    Dim query As String
    Dim isArray As Boolean
    Dim n As String

    Public ReadOnly Property isComposeCssQuery As Boolean
        Get
            Return query.Contains("[") AndAlso query.Contains("]")
        End Get
    End Property

    Sub New(query As String, n As String, isArray As Boolean)
        Me.query = query
        Me.n = n
        Me.isArray = isArray
    End Sub

    Public Function ParseIndex() As Integer?
        If n.StringEmpty OrElse n = "*" Then
            Return Nothing
        Else
            Return Integer.Parse(n)
        End If
    End Function

    Public Function RunComposeCssQuery(document As InnerPlantText, env As Engine) As InnerPlantText
        ' there is a whitespace that needs to be trimed
        Dim query = Me.query.GetTagValue("[", trim:="] ")
        Dim attributeQuery As NamedValue(Of String) = query.Value.GetTagValue("=", trim:="'""")
        Dim list As HtmlElement() = DirectCast(document, HtmlElement).getElementsByTagName(query.Name)
        Dim queryByAttr = (From tag As HtmlElement
                           In list
                           Where tag.hasAttribute(attributeQuery.Name)
                           Where tag(attributeQuery.Name).Equals(attributeQuery.Value)).ToArray
        Dim queryOut As InnerPlantText = CSSSelector.getElementQueryOutput(document, queryByAttr, Me)

        Return queryOut
    End Function

End Structure
