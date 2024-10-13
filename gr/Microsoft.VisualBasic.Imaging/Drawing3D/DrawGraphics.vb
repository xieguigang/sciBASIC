#Region "Microsoft.VisualBasic::af8d1d673f61aad241b44bdcaa3f8c3e, gr\Microsoft.VisualBasic.Imaging\Drawing3D\DrawGraphics.vb"

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

    '   Total Lines: 12
    '    Code Lines: 4 (33.33%)
    ' Comment Lines: 5 (41.67%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (25.00%)
    '     File Size: 424 B


    '     Delegate Sub
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing

Namespace Drawing3D

    ''' <summary>
    ''' 3D plot for Gdidevice，由于是需要将图像显示到WinFom控件上面，所以在这里要求的是gdi+的图形驱动程序
    ''' </summary>
    ''' <param name="canvas">gdi+ handle</param>
    ''' <param name="camera">3d camera</param>
    Public Delegate Sub DrawGraphics(ByRef canvas As IGraphics, camera As Camera)

End Namespace
