#Region "Microsoft.VisualBasic::97c416c546eed13394017f6e64c61c89, mime\application%xml\XPath\XPathParser.vb"

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

    ' Class XPathParser
    ' 
    '     Function: Parse
    ' 
    ' /********************************************************************************/

#End Region

Public Class XPathParser

    Public Shared Function Parse(expression As String) As XPath
        Dim path As XPath

        If expression.StringEmpty Then
            Return Nothing
        ElseIf expression.StartsWith("//") Then
            path = New CurrentNodes
            expression = expression.Substring(2)
        ElseIf expression.StartsWith("/") Then
            path = New RootPathSelector
            expression = expression.Substring(1)
        ElseIf expression.StartsWith("..") Then
            path = New ParentNode
            expression = expression.Substring(2)
        ElseIf expression.StartsWith(".") Then
            path = New CurrentNode
            expression = expression.Substring(1)
        ElseIf expression.StartsWith("@") Then
            path = New SelectAttributes
            expression = expression.Substring(1)
        Else
            path = New SelectByNodeName
        End If

        Dim i As Integer = InStrAny(expression, "/", "@")

        If i > 0 Then
            path.expression = expression.Substring(0, i)
            path.selectNext = Parse(expression)
        Else
            path.expression = expression
        End If

        Return path
    End Function
End Class

