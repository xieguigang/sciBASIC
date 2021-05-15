#Region "Microsoft.VisualBasic::b55c6e14690ff006ade8c2ae5bc0bcd1, mime\text%html\HTML\HtmlParser\HtmlDocument.vb"

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

'     Class HtmlDocument
' 
'         Properties: Tags
' 
'         Function: Load, LoadDocument
' 
' 
' /********************************************************************************/

#End Region

Namespace HTML

    Public Class HtmlDocument : Inherits HtmlElement

        ''' <summary>
        ''' 假设所加载的html文档是完好的格式的，即没有不匹配的标签的
        ''' </summary>
        ''' <param name="handle">document text or url or file path</param>
        ''' <returns></returns>
        Public Shared Function LoadDocument(handle As String) As HtmlDocument
            Dim text As String = handle.SolveStream
            Dim document As HtmlDocument = HtmlParser.ParseTree(document:=text)

            Return document
        End Function
    End Class
End Namespace
