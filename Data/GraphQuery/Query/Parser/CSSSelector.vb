#Region "Microsoft.VisualBasic::996193447a4bfb34039752d2adcb6440, Data\GraphQuery\Query\Parser\CSSSelector.vb"

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

    ' Class CSSSelector
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: ParseImpl, selectByClass, selectByList, selectByTagName
    ' 
    ' Structure Selector
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.Html.Document

Public Class CSSSelector : Inherits Parser

    Sub New(func As String, parameters As String())
        Call MyBase.New(func, parameters)
    End Sub

    Protected Overrides Function ParseImpl(document As InnerPlantText, isArray As Boolean, env As Engine) As InnerPlantText
        Dim query As String = parameters(Scan0)
        Dim n As String = parameters.ElementAtOrDefault(1)

        If TypeOf document Is HtmlDocument Then
            document = DirectCast(document, HtmlDocument).HtmlElements(Scan0)
        End If
        If document.GetType Is GetType(InnerPlantText) Then
            Return New InnerPlantText
        End If

        If query.IndexOf(","c) > -1 Then
            Return selectByList(document, New Selector(query, n, isArray))
        ElseIf query.First = "#"c Then
            ' get element by id
            Return DirectCast(document, HtmlElement).getElementById(query.Substring(1))
        ElseIf query.First = "."c Then
            Return selectByClass(document, New Selector(query, n, isArray))
        Else
            Return selectByTagName(document, New Selector(query, n, isArray))
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
                    .ToArray
            }
        ElseIf selector.isArray AndAlso selector.n = "*" Then
            Return New HtmlElement With {
                .TagName = selector.query,
                .HtmlElements = list
            }
        Else
            Return GetElementByIndex(list, CInt(Val(selector.n)))
        End If
    End Function

    Private Function selectByTagName(document As InnerPlantText, selector As Selector) As InnerPlantText
        Dim list As HtmlElement() = DirectCast(document, HtmlElement).getElementsByTagName(selector.query)

        If selector.isArray AndAlso parameters.Length = 1 Then
            ' get elements by tag name
            Return New HtmlElement With {
                .TagName = selector.query,
                .HtmlElements = DirectCast(document, HtmlElement) _
                    .GetDirectChilds(list) _
                    .ToArray
            }
        ElseIf selector.isArray AndAlso selector.n = "*" Then
            Return New HtmlElement With {
                .TagName = selector.query,
                .HtmlElements = list
            }
        Else
            Return GetElementByIndex(list, CInt(Val(selector.n)))
        End If
    End Function

    Private Function selectByClass(document As InnerPlantText, selector As Selector) As InnerPlantText
        Dim list As HtmlElement() = DirectCast(document, HtmlElement).getElementsByClassName(selector.query.Substring(1))

        If selector.isArray AndAlso parameters.Length = 1 Then
            ' get elements by class name
            Return New HtmlElement With {
                .TagName = selector.query,
                .HtmlElements = DirectCast(document, HtmlElement) _
                    .GetDirectChilds(list) _
                    .ToArray
            }
        ElseIf selector.isArray AndAlso selector.n = "*" Then
            Return New HtmlElement With {
                .TagName = selector.query,
                .HtmlElements = list
            }
        Else
            Return GetElementByIndex(list, CInt(Val(selector.n)))
        End If
    End Function
End Class

Public Structure Selector

    Dim query As String
    Dim isArray As Boolean
    Dim n As String

    Sub New(query As String, n As String, isArray As Boolean)
        Me.query = query
        Me.n = n
        Me.isArray = isArray
    End Sub

End Structure
