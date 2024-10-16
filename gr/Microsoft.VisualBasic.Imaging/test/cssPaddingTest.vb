#Region "Microsoft.VisualBasic::4fa4d824b6e5919ecae711e5a98242fc, gr\Microsoft.VisualBasic.Imaging\test\cssPaddingTest.vb"

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
    '    Code Lines: 12 (75.00%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 4 (25.00%)
    '     File Size: 428 B


    ' Module cssPaddingTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.MIME.Html.CSS

Module cssPaddingTest

    Sub Main()
        Dim css As New CSSEnvirnment(New Size(1000, 1000), dpi:=300)
        Dim pad1 As Padding = "padding: 5% 5% 5% 5%;"
        Dim pad2 As Padding = "padding: 200px 300px 35% 45%;"

        Dim l1 = pad1.LayoutVector(css)
        Dim l2 = pad2.LayoutVector(css)

        Pause()
    End Sub
End Module

