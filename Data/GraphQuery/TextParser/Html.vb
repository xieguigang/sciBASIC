#Region "Microsoft.VisualBasic::5bf4021cff87df53288bde7de9a11925, Data\GraphQuery\TextParser\Html.vb"

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

    '   Total Lines: 51
    '    Code Lines: 43 (84.31%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 8 (15.69%)
    '     File Size: 2.02 KB


    '     Module Html
    ' 
    '         Function: cssValue, html, urlQuery
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

    Module Html

        <ExportAPI("html")>
        Public Function html(document As InnerPlantText, parameters As String(), isArray As Boolean) As InnerPlantText
            Return ParserFunction.ParseDocument(document, Function(i) New InnerPlantText With {.InnerText = i.GetHtmlText}, isArray)
        End Function

        <ExportAPI("urlQuery")>
        Public Function urlQuery(document As InnerPlantText, parameters As String(), isArray As Boolean) As InnerPlantText
            Dim argName As String = parameters(Scan0)

            Return ParserFunction.ParseDocument(
                document:=document,
                pip:=Function(i)
                         Return New InnerPlantText With {
                             .InnerText = URL.Parse(i.GetHtmlText)(argName)
                         }
                     End Function,
                isArray:=isArray
            )
        End Function

        <ExportAPI("cssValue")>
        Public Function cssValue(document As InnerPlantText, parameters As String(), isArray As Boolean) As InnerPlantText
            Dim keyName As String = parameters(0)

            Return ParserFunction.ParseDocument(
                document:=document,
                pip:=Function(i)
                         Dim css = CssParser.ParseStyle(i.GetHtmlText)
                         Dim cssVal As String = css(keyName)

                         Return New InnerPlantText With {
                             .InnerText = cssVal
                         }
                     End Function,
                isArray:=isArray
            )
        End Function
    End Module
End Namespace
