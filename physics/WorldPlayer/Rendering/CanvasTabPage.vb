#Region "Microsoft.VisualBasic::fa698999095a12c10ac05ccd3f4cfb6f, physics\WorldPlayer\Rendering\CanvasTabPage.vb"

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

    '   Total Lines: 24
    '    Code Lines: 11 (45.83%)
    ' Comment Lines: 10 (41.67%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (12.50%)
    '     File Size: 958 B


    ' Class CanvasTabPage
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Windows.Forms

''' <summary>
''' 三维画布页：把 3D 画面<b>直接画在页面自身上</b>。
''' </summary>
''' <remarks>
''' 不使用 <c>PictureBox</c> 子控件，是因为实测 <see cref="TabPage"/> 的布局引擎
''' 不会可靠地把 <c>Dock=Fill</c> 子控件重排到新尺寸（页面已变成 1110x819，
''' 子控件仍停留在设计尺寸 884x497，且显式设尺寸会被同步驳回）。
''' 而 TabPage 自身尺寸始终是正确的，因此让页面自己当画布最稳。
''' 打开双缓冲以避免每帧重绘闪烁。
''' </remarks>
Public Class CanvasTabPage
    Inherits TabPage

    Public Sub New()
        Call SetStyle(ControlStyles.UserPaint Or
                      ControlStyles.AllPaintingInWmPaint Or
                      ControlStyles.OptimizedDoubleBuffer Or
                      ControlStyles.ResizeRedraw, True)

        Call UpdateStyles()
    End Sub
End Class

