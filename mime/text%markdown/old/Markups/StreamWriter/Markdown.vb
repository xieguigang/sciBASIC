﻿#Region "Microsoft.VisualBasic::1e04e39afecfcf1c6acb938e90fa302f, sciBASIC#\mime\text%markdown\Markups\StreamWriter\Markdown.vb"

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

    '   Total Lines: 16
    '    Code Lines: 13
    ' Comment Lines: 0
    '   Blank Lines: 3
    '     File Size: 457 B


    '     Module Markdown
    ' 
    '         Function: TextMarkdown
    ' 
    ' 
    ' /********************************************************************************/

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
