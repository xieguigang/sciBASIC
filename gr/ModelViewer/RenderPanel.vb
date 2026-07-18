#Region "Microsoft.VisualBasic::7c3945ffd90074ebea96874589ffe41e, gr\ModelViewer\RenderPanel.vb"

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

    '   Total Lines: 15
    '    Code Lines: 9 (60.00%)
    ' Comment Lines: 3 (20.00%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (20.00%)
    '     File Size: 431 B


    ' Class RenderPanel
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: OnMouseWheel
    ' 
    ' /********************************************************************************/

#End Region

''' <summary>
''' 双缓冲画布面板，重写 OnMouseWheel 以消费滚轮事件（用于缩放），并对外抛出 Zoom 事件。
''' </summary>
Public Class RenderPanel : Inherits Panel

    Public Event Zoom(delta As Integer)

    Public Sub New()
        Me.DoubleBuffered = True
    End Sub

    Protected Overrides Sub OnMouseWheel(e As MouseEventArgs)
        RaiseEvent Zoom(e.Delta)
    End Sub
End Class
