#Region "Microsoft.VisualBasic::97f79b5060fb3cf7703069273bafa85d, ..\visualbasic_App\UXFramework\Molk+\Molk+\API\ControlMoverOrResizer.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
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

Imports System.Collections.Generic
Imports System.Drawing
Imports System.Globalization
Imports System.Linq
Imports System.Windows.Forms

Namespace API

    ''' <summary>
    ''' Move and Resize Controls on a Form at Runtime (With Mouse)
    ''' http://www.codeproject.com/Tips/709121/Move-and-Resize-Controls-on-a-Form-at-Runtime-With
    ''' </summary>
    Public Class ControlMoverOrResizer

        Dim _moving As Boolean
        Dim _cursorStartPoint As Point
        Dim _moveIsInterNal As Boolean
        Dim _resizing As Boolean
        Dim _currentControlStartSize As Size

        Dim MouseIsInLeftEdge As Boolean
        Dim MouseIsInRightEdge As Boolean
        Dim MouseIsInTopEdge As Boolean
        Dim MouseIsInBottomEdge As Boolean

        Public Enum MoveOrResize
            Move
            Resize
            MoveAndResize
        End Enum

        Public Property WorkType As MoveOrResize

        Public Property KeepAspectRatio As Boolean = False

        Sub New(control As Control)
            Call Me.New(control, control)
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="control">控件</param>
        ''' <param name="container">窗体</param>
        Sub New(control As Control, container As Control)
            _moving = False
            _resizing = False
            _moveIsInterNal = False
            _cursorStartPoint = Point.Empty
            MouseIsInLeftEdge = False
            MouseIsInLeftEdge = False
            MouseIsInRightEdge = False
            MouseIsInTopEdge = False
            MouseIsInBottomEdge = False
            WorkType = MoveOrResize.MoveAndResize

            AddHandler control.MouseDown, Sub(sender, e) StartMovingOrResizing(control, e)
            AddHandler control.MouseUp, Sub(sender, e) StopDragOrResizing(control)
            AddHandler control.MouseMove, Sub(sender, e) MoveControl(container, e)
        End Sub

        Private Sub UpdateMouseEdgeProperties(control As Control, mouseLocationInControl As Point)
            If WorkType = MoveOrResize.Move Then
                Return
            End If
            MouseIsInLeftEdge = Math.Abs(mouseLocationInControl.X) <= 2
            MouseIsInRightEdge = Math.Abs(mouseLocationInControl.X - control.Width) <= 2
            MouseIsInTopEdge = Math.Abs(mouseLocationInControl.Y) <= 2
            MouseIsInBottomEdge = Math.Abs(mouseLocationInControl.Y - control.Height) <= 2
        End Sub

        Private Sub UpdateMouseCursor(control As Control)
            If WorkType = MoveOrResize.Move Then
                Return
            End If
            If MouseIsInLeftEdge Then
                If MouseIsInTopEdge Then
                    control.Cursor = Cursors.SizeNWSE
                ElseIf MouseIsInBottomEdge Then
                    control.Cursor = Cursors.SizeNESW
                Else
                    control.Cursor = Cursors.SizeWE
                End If
            ElseIf MouseIsInRightEdge Then
                If MouseIsInTopEdge Then
                    control.Cursor = Cursors.SizeNESW
                ElseIf MouseIsInBottomEdge Then
                    control.Cursor = Cursors.SizeNWSE
                Else
                    control.Cursor = Cursors.SizeWE
                End If
            ElseIf MouseIsInTopEdge OrElse MouseIsInBottomEdge Then
                control.Cursor = Cursors.SizeNS
            Else
                control.Cursor = Cursors.[Default]
            End If
        End Sub

        Private Sub StartMovingOrResizing(control As Control, e As MouseEventArgs)
            If _moving OrElse _resizing Then
                Return
            End If
            If WorkType <> MoveOrResize.Move AndAlso (MouseIsInRightEdge OrElse MouseIsInLeftEdge OrElse MouseIsInTopEdge OrElse MouseIsInBottomEdge) Then
                _resizing = True
                _currentControlStartSize = control.Size
            ElseIf WorkType <> MoveOrResize.Resize Then
                _moving = True
                control.Cursor = Cursors.Hand
            End If
            _cursorStartPoint = New Point(e.X, e.Y)
            control.Capture = True
        End Sub

        Private Sub MoveControl(control As Control, e As MouseEventArgs)
            If Not _resizing AndAlso Not _moving Then
                UpdateMouseEdgeProperties(control, New Point(e.X, e.Y))
                UpdateMouseCursor(control)
            End If

            Dim ctrWidth, ctrLeft, ctrHeight, ctrTop As Integer

            ctrWidth = control.Width
            ctrHeight = control.Height
            ctrLeft = control.Left
            ctrTop = control.Top

            If _resizing Then
                If MouseIsInLeftEdge Then
                    If MouseIsInTopEdge Then
                        ctrWidth -= (e.X - _cursorStartPoint.X)
                        ctrLeft += (e.X - _cursorStartPoint.X)
                        ctrHeight -= (e.Y - _cursorStartPoint.Y)
                        ctrTop += (e.Y - _cursorStartPoint.Y)
                    ElseIf MouseIsInBottomEdge Then
                        ctrWidth -= (e.X - _cursorStartPoint.X)
                        ctrLeft += (e.X - _cursorStartPoint.X)
                        ctrHeight = (e.Y - _cursorStartPoint.Y) + _currentControlStartSize.Height
                    Else
                        ctrWidth -= (e.X - _cursorStartPoint.X)
                        ctrLeft += (e.X - _cursorStartPoint.X)
                    End If
                ElseIf MouseIsInRightEdge Then
                    If MouseIsInTopEdge Then
                        ctrWidth = (e.X - _cursorStartPoint.X) + _currentControlStartSize.Width
                        ctrHeight -= (e.Y - _cursorStartPoint.Y)

                        ctrTop += (e.Y - _cursorStartPoint.Y)
                    ElseIf MouseIsInBottomEdge Then
                        ctrWidth = (e.X - _cursorStartPoint.X) + _currentControlStartSize.Width
                        ctrHeight = (e.Y - _cursorStartPoint.Y) + _currentControlStartSize.Height
                    Else
                        ctrWidth = (e.X - _cursorStartPoint.X) + _currentControlStartSize.Width
                    End If
                ElseIf MouseIsInTopEdge Then
                    ctrHeight -= (e.Y - _cursorStartPoint.Y)
                    ctrTop += (e.Y - _cursorStartPoint.Y)
                ElseIf MouseIsInBottomEdge Then
                    ctrHeight = (e.Y - _cursorStartPoint.Y) + _currentControlStartSize.Height
                Else
                    StopDragOrResizing(control)
                End If

                If KeepAspectRatio Then
                    Dim Max = Math.Max(ctrWidth, ctrHeight)
                    control.Location = New Point(ctrLeft, ctrTop)
                    control.Size = New Size(Max, Max)
                Else
                    control.Location = New Point(ctrLeft, ctrTop)
                    control.Size = New Size(ctrWidth, ctrHeight)
                End If

            ElseIf _moving Then
                _moveIsInterNal = Not _moveIsInterNal
                If Not _moveIsInterNal Then
                    Dim x As Integer = (e.X - _cursorStartPoint.X) + control.Left
                    Dim y As Integer = (e.Y - _cursorStartPoint.Y) + control.Top
                    control.Location = New Point(x, y)
                End If
            End If
        End Sub

        Private Sub StopDragOrResizing(control As Control)
            _resizing = False
            _moving = False
            control.Capture = False
            UpdateMouseCursor(control)
        End Sub
    End Class
End Namespace
