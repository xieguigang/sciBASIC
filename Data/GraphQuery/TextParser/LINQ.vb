#Region "Microsoft.VisualBasic::ab0803602782baa3c6f04f6b4bb7aa16, Data\GraphQuery\TextParser\LINQ.vb"

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

    '   Total Lines: 56
    '    Code Lines: 38
    ' Comment Lines: 10
    '   Blank Lines: 8
    '     File Size: 2.07 KB


    '     Module LINQ
    ' 
    '         Function: eq, skip
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language.C
Imports Microsoft.VisualBasic.MIME.Html.Document
Imports Microsoft.VisualBasic.MIME.Html.Language.CSS
Imports Microsoft.VisualBasic.Net.Http
Imports Microsoft.VisualBasic.Text

Namespace TextParser

    ''' <summary>
    ''' LINQ functions for the graphquery parser
    ''' </summary>
    Module LINQ

        <ExportAPI("skip")>
        Public Function skip(document As InnerPlantText, parameters As String(), isArray As Boolean) As InnerPlantText
            If Not isArray Then
                Throw New InvalidExpressionException("data should be an array!")
            ElseIf Not TypeOf document Is HtmlElement Then
                Return document
            End If

            Dim array As New HtmlElement With {
                .TagName = "skip",
                .Attributes = {AutoContext.Attribute}
            }
            Dim n As Integer = Integer.Parse(parameters(Scan0))

            If TypeOf document Is HtmlElement Then
                For Each element In DirectCast(document, HtmlElement).HtmlElements.Skip(n)
                    Call array.Add(element)
                Next
            Else
                Throw New InvalidExpressionException
            End If

            Return array
        End Function

        ''' <summary>
        ''' Take the nth element in the current node collection
        ''' </summary>
        ''' <param name="document"></param>
        ''' <param name="parameters"></param>
        ''' <param name="isArray"></param>
        ''' <returns></returns>
        <ExportAPI("eq")>
        Public Function eq(document As InnerPlantText, parameters As String(), isArray As Boolean) As InnerPlantText
            Dim n As Integer = Integer.Parse(parameters(Scan0))
            Dim nItem As InnerPlantText = DirectCast(document, HtmlElement).HtmlElements(n)

            Return nItem
        End Function
    End Module
End Namespace
