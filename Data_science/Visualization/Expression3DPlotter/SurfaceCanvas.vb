#Region "Microsoft.VisualBasic::b3f5b1754e9a349d7a3d469996a4add8, Data_science\Visualization\Expression3DPlotter\SurfaceCanvas.vb"

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

    '   Total Lines: 76
    '    Code Lines: 58 (76.32%)
    ' Comment Lines: 7 (9.21%)
    '    - Xml Docs: 71.43%
    ' 
    '   Blank Lines: 11 (14.47%)
    '     File Size: 2.45 KB


    ' Class SurfaceCanvas
    ' 
    '     Properties: Scene
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Sub: OnMouseDown, OnMouseMove, OnMouseUp, OnMouseWheel, OnPaint
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Windows.Forms
Imports System.Drawing

''' <summary>
''' 交互式三维显示画布控件。重写 OnMouseWheel 抛出 Zoom 事件，
''' 并通过鼠标左键拖拽旋转视角、右键拖拽平移、滚轮调整视距。
''' 绘制委托给所持有的 PlotScene。
''' </summary>
Public Class SurfaceCanvas : Inherits Panel

    Public Event Zoom(delta As Integer)

    Public Property Scene As PlotScene

    Private dragging As Boolean = False
    Private panning As Boolean = False
    Private lastX As Integer = 0
    Private lastY As Integer = 0

    Public Sub New()
        Me.DoubleBuffered = True
        Me.BackColor = Color.White
        Me.TabStop = True
    End Sub

    Protected Overrides Sub OnMouseWheel(e As MouseEventArgs)
        RaiseEvent Zoom(e.Delta)
    End Sub

    Protected Overrides Sub OnMouseDown(e As MouseEventArgs)
        MyBase.OnMouseDown(e)
        Me.Focus()
        If e.Button = MouseButtons.Left Then
            dragging = True
            lastX = e.X : lastY = e.Y
        ElseIf e.Button = MouseButtons.Right Then
            panning = True
            lastX = e.X : lastY = e.Y
        End If
    End Sub

    Protected Overrides Sub OnMouseMove(e As MouseEventArgs)
        MyBase.OnMouseMove(e)
        If Scene Is Nothing Then Return

        If dragging Then
            Dim dx = e.X - lastX
            Dim dy = e.Y - lastY
            ' 左键拖拽：旋转模型（修改摄像机 Euler 角）
            Scene.Camera.AngleY = Scene.Camera.AngleY + dx * 0.4F
            Scene.Camera.AngleX = Scene.Camera.AngleX + dy * 0.4F
            lastX = e.X : lastY = e.Y
            Invalidate()
        ElseIf panning Then
            Dim dx = e.X - lastX
            Dim dy = e.Y - lastY
            ' 右键拖拽：平移画布（修改摄像机偏移）
            Scene.Camera.Offset = New PointF(Scene.Camera.Offset.X + dx, Scene.Camera.Offset.Y + dy)
            lastX = e.X : lastY = e.Y
            Invalidate()
        End If
    End Sub

    Protected Overrides Sub OnMouseUp(e As MouseEventArgs)
        MyBase.OnMouseUp(e)
        dragging = False
        panning = False
    End Sub

    Protected Overrides Sub OnPaint(e As PaintEventArgs)
        MyBase.OnPaint(e)
        If Scene IsNot Nothing Then
            Scene.Draw(e.Graphics, Me.ClientSize)
        End If
    End Sub
End Class

