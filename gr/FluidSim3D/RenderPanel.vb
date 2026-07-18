#Region "Microsoft.VisualBasic::cab2e587e0c6457e9e7177174e231b0f, gr\FluidSim3D\RenderPanel.vb"

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

    '   Total Lines: 30
    '    Code Lines: 12 (40.00%)
    ' Comment Lines: 10 (33.33%)
    '    - Xml Docs: 30.00%
    ' 
    '   Blank Lines: 8 (26.67%)
    '     File Size: 942 B


    '     Class RenderPanel
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: OnMouseWheel
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' /********************************************************************************/
'
'     RenderPanel - a double buffered canvas used by the 3D water simulator.
'     It swallows the mouse wheel event (so the form can use it for zoom)
'     and raises a Zoom event with the wheel delta.
'
' /********************************************************************************/

Imports System.Windows.Forms

Namespace FluidSim3D

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

End Namespace

