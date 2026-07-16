#Region "Microsoft.VisualBasic::1f7f70edce49dbaf0313b7a9183dccb3, gr\network-visualization\NetworkCanvas\InputDevice.vb"

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

    ' Class InputDevice
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: getNode, GetPointedNode
    ' 
    '     Sub: Canvas_MouseDown, Canvas_MouseMove, Canvas_MouseUp, Canvas_MouseWheel, (+2 Overloads) Dispose
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.SpringForce

''' <summary>
''' 使用鼠标左键进行拖拽
''' </summary>
Public Class InputDevice : Implements IDisposable

    Protected WithEvents Canvas As Canvas

    Sub New(canvas As Canvas)
        Me.Canvas = canvas
    End Sub

    Protected Overridable Sub Canvas_MouseMove(sender As Object, e As MouseEventArgs) Handles Canvas.MouseMove
        ' 拖拽节点
        If drag AndAlso dragNode IsNot Nothing Then
            If System.Math.Abs(e.Location.X - downPoint.X) > 3 OrElse
               System.Math.Abs(e.Location.Y - downPoint.Y) > 3 Then
                moved = True
            End If

            Dim vec As FDGVector2 = Canvas.fdgRenderer.ScreenToGraph(New Point(e.Location.X, e.Location.Y))
            dragNode.pinned = True
            Canvas.fdgPhysics.GetPoint(dragNode).position = vec
            Return
        End If

        ' 平移视图（空白处拖拽）
        If panning Then
            Dim dx = e.Location.X - lastPan.X
            Dim dy = e.Location.Y - lastPan.Y
            Canvas.view.Viewport.Pan(dx, dy)
            lastPan = e.Location
            Canvas.Invalidate()
            Return
        End If

        ' 悬停高亮 + tooltip
        Dim hit = Canvas.HitTest(e.Location)
        If hit IsNot lastHover Then
            lastHover = hit
            Canvas.view.Hovered = hit
            If hit Is Nothing Then
                Canvas.HideTooltip()
            Else
                Canvas.ShowNodeTooltip(hit, e.Location)
            End If
            Canvas.Invalidate()
        End If
    End Sub

    Protected dragNode As Node

    ''' <summary>
    ''' get target node which is pointed by the mouse
    ''' </summary>
    ''' <param name="p"></param>
    ''' <returns></returns>
    Public Function GetPointedNode(p As Point) As Node
        Return getNode(p)
    End Function

    ''' <summary>
    ''' get target node which is pointed by the mouse
    ''' </summary>
    Protected Overridable Function getNode(p As Point) As Node
        Return Canvas.HitTest(p)
    End Function

    Protected drag As Boolean
    Protected panning As Boolean
    Protected lastPan As Point
    Protected lastHover As Node
    Protected downPoint As Point
    Protected moved As Boolean

    Protected Overridable Sub Canvas_MouseDown(sender As Object, e As MouseEventArgs) Handles Canvas.MouseDown
        If e.Button = MouseButtons.Left Then
            Dim hit = Canvas.HitTest(e.Location)

            If hit IsNot Nothing Then
                drag = True
                dragNode = hit
                downPoint = e.Location
                moved = False
            Else
                panning = True
                lastPan = e.Location
            End If
        End If
    End Sub

    Protected Overridable Sub Canvas_MouseUp(sender As Object, e As MouseEventArgs) Handles Canvas.MouseUp
        If drag AndAlso dragNode IsNot Nothing Then
            ' 视为点击：选中节点（Ctrl 多选，否则单选）
            If Not moved Then
                If (Control.ModifierKeys And Keys.Control) = Keys.Control Then
                    If Canvas.view.Selected.Contains(dragNode) Then
                        Canvas.view.Selected.Remove(dragNode)
                    Else
                        Canvas.view.Selected.Add(dragNode)
                    End If
                Else
                    Canvas.view.Selected.Clear()
                    Canvas.view.Selected.Add(dragNode)
                End If
            End If

            dragNode.pinned = False
            dragNode = Nothing
        End If

        drag = False
        panning = False
        Canvas.Invalidate()
    End Sub

    Protected Overridable Sub Canvas_MouseWheel(sender As Object, e As MouseEventArgs) Handles Canvas.MouseWheel
        If Canvas.space3D Then
            ' 3D 视图：调整视距
            Canvas.ViewDistance += e.Delta / 10
        Else
            ' 2D 视图：以光标为锚点缩放
            Dim factor = 1.0F + e.Delta / 1000.0F
            Canvas.view.Viewport.ZoomAt(e.Location, factor, Canvas.fdgRenderer.ClientRegion)
            Canvas.Invalidate()
        End If
    End Sub

#Region "IDisposable Support"
    Private disposedValue As Boolean ' To detect redundant calls

    ' IDisposable
    Protected Overridable Sub Dispose(disposing As Boolean)
        If Not disposedValue Then
            If disposing Then
                ' TODO: dispose managed state (managed objects).
                Canvas = Nothing
            End If

            ' TODO: free unmanaged resources (unmanaged objects) and override Finalize() below.
            ' TODO: set large fields to null.
        End If
        disposedValue = True
    End Sub

    ' TODO: override Finalize() only if Dispose(disposing As Boolean) above has code to free unmanaged resources.
    'Protected Overrides Sub Finalize()
    '    ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
    '    Dispose(False)
    '    MyBase.Finalize()
    'End Sub

    ' This code added by Visual Basic to correctly implement the disposable pattern.
    Public Sub Dispose() Implements IDisposable.Dispose
        ' Do not change this code.  Put cleanup code in Dispose(disposing As Boolean) above.
        Dispose(True)
        ' TODO: uncomment the following line if Finalize() is overridden above.
        ' GC.SuppressFinalize(Me)
    End Sub
#End Region
End Class
