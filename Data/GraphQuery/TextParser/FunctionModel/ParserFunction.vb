#Region "Microsoft.VisualBasic::265ef4cec7a031996ab307e235479dc6, Data\GraphQuery\TextParser\FunctionModel\ParserFunction.vb"

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

    '   Total Lines: 38
    '    Code Lines: 29 (76.32%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (23.68%)
    '     File Size: 1.40 KB


    '     Delegate Function
    ' 
    ' 
    '     Class ParserFunction
    ' 
    '         Function: ParseDocument
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MIME.Html.Document

Namespace TextParser

    Public Delegate Function IParserPipeline(document As InnerPlantText) As InnerPlantText

    Public MustInherit Class ParserFunction

        Public MustOverride Function GetToken(document As InnerPlantText, parameters As String(), isArray As Boolean) As InnerPlantText

        Public Shared Function ParseDocument(document As InnerPlantText,
                                             pip As IParserPipeline,
                                             isArray As Boolean,
                                             <CallerMemberName>
                                             Optional callFunc As String = Nothing) As InnerPlantText
            If Not isArray Then
                Return pip(document)
            End If

            Dim array As New HtmlElement With {
                .TagName = callFunc,
                .Attributes = {AutoContext.Attribute}
            }

            If TypeOf document Is HtmlElement Then
                For Each element In DirectCast(document, HtmlElement).HtmlElements
                    Call array.Add(pip(element))
                Next
            Else
                Call array.Add(pip(document))
            End If

            Return array
        End Function

    End Class
End Namespace
