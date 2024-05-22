#Region "Microsoft.VisualBasic::4a640e3c3c6edac4efa438368a486f88, Data\GraphQuery\Query\Parser\XPathSelector.vb"

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

    '   Total Lines: 32
    '    Code Lines: 27 (84.38%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 5 (15.62%)
    '     File Size: 1.15 KB


    ' Class XPathSelector
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: ParseImpl
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.MIME.application.xml
Imports Microsoft.VisualBasic.MIME.Html.Document

Public Class XPathSelector : Inherits Parser

    Sub New(func As String, parameters As String())
        Call MyBase.New(func, parameters)
    End Sub

    Protected Overrides Function ParseImpl(document As InnerPlantText, isArray As Boolean, env As Engine) As InnerPlantText
        Dim xpath As XPath = XPathParser.Parse(parameters(Scan0))
        Dim engine As New XPathQuery(xpath)
        Dim query As InnerPlantText

        If isArray Then
            query = New HtmlElement With {
                .TagName = parameters(Scan0),
                .HtmlElements = engine _
                    .QueryAll(document) _
                    .Select(Function(n)
                                Return DirectCast(DirectCast(n, HtmlElement), InnerPlantText)
                            End Function) _
                    .ToArray,
                .Attributes = {AutoContext.Attribute}
            }
        Else
            query = engine.QuerySingle(document)
        End If

        Return query
    End Function
End Class
