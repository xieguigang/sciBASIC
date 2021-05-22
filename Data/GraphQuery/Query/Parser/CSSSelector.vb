#Region "Microsoft.VisualBasic::48875161f7f771ed88ba75a382d578ec, Data\GraphQuery\Query\Parser\CSSSelector.vb"

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
    '     Function: ParseImpl
    ' 
    ' /********************************************************************************/

#End Region

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

        If query.First = "#"c Then
            ' get element by id
            Return DirectCast(document, HtmlElement).getElementById(query.Substring(1))
        ElseIf query.First = "."c Then
            Dim list As HtmlElement() = DirectCast(document, HtmlElement).getElementsByClassName(query.Substring(1))

            If isArray AndAlso parameters.Length = 1 Then
                ' get elements by class name
                Return New HtmlElement With {
                    .TagName = query,
                    .HtmlElements = DirectCast(document, HtmlElement) _
                        .GetDirectChilds(list) _
                        .ToArray
                }
            Else
                Return GetElementByIndex(list, CInt(Val(n)))
            End If
        Else
            Dim list As HtmlElement() = DirectCast(document, HtmlElement).getElementsByTagName(query)

            If isArray AndAlso parameters.Length = 1 Then
                ' get elements by tag name
                Return New HtmlElement With {
                    .TagName = query,
                    .HtmlElements = DirectCast(document, HtmlElement) _
                        .GetDirectChilds(list) _
                        .ToArray
                }
            Else
                Return GetElementByIndex(list, CInt(Val(n)))
            End If
        End If
    End Function
End Class

