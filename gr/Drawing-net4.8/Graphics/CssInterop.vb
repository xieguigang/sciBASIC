#Region "Microsoft.VisualBasic::77bd46fafe45be6c2e89a3935aea554e, gr\Drawing-net4.8\Graphics\CssInterop.vb"

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

    '   Total Lines: 17
    '    Code Lines: 12 (70.59%)
    ' Comment Lines: 3 (17.65%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 2 (11.76%)
    '     File Size: 462 B


    ' Module CssInterop
    ' 
    '     Function: CreateCss
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MIME.Html.CSS

''' <summary>
''' .NET clr gdi+ component convert to css model
''' </summary>
Public Module CssInterop

    <Extension>
    Public Function CreateCss(font As System.Drawing.Font) As CSSFont
        Return New CSSFont() With {
            .style = font.Style,
            .family = font.Name,
            .size = font.Size
        }
    End Function
End Module

