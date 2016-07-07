#Region "Microsoft.VisualBasic::1e04e39afecfcf1c6acb938e90fa302f, ..\VisualBasic_AppFramework\DocumentFormats\VB_HTML\VB_HTML\StreamWriter\Markdown.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Namespace StreamWriter

    Public Module Markdown

        <Extension>
        Public Function TextMarkdown(link As Hyperlink) As String
            If String.IsNullOrEmpty(link.Title) Then
                Return $"[{link.Text}]({link.Links})"
            Else
                Return $"[{link.Text}]({link.Links} ""{link.Title}"")"
            End If
        End Function
    End Module
End Namespace
