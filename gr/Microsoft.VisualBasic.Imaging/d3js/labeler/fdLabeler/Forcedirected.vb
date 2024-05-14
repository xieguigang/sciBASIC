#Region "Microsoft.VisualBasic::27ad0f8913cd8872988932d954b935d4, gr\Microsoft.VisualBasic.Imaging\d3js\labeler\fdLabeler\Forcedirected.vb"

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

    '   Total Lines: 225
    '    Code Lines: 169
    ' Comment Lines: 12
    '   Blank Lines: 44
    '     File Size: 7.80 KB


    '     Class Forcedirected
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Height, Start, Width
    ' 
    '         Sub: Collide, RejectRegions, reset, runAttraction, runRepulsive
    '              setPosition
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Imaging.LayoutModel
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports stdNum = System.Math

Namespace d3js.Layout

    Public Class Forcedirected ： Inherits DataLabeler

        Protected ReadOnly mDxMap As New Dictionary(Of String, Double)
        Protected ReadOnly mDyMap As New Dictionary(Of String, Double)

        Protected ReadOnly ejectFactor As Integer = 6
        Protected ReadOnly condenseFactor As Integer = 3
        Protected ReadOnly dist_thresh As DoubleRange

        Dim k As Double
        Dim maxtx As Integer = 4
        Dim maxty As Integer = 3

        ''' <summary>
        ''' 会尽量避免在这个区域内存在网络的节点，这个区域一般为legend的绘制区域
        ''' </summary>
        Protected ReadOnly avoidRegions As (rect As Rectangle2D, center As PointF)()

        Sub New(Optional ejectFactor As Integer = 6,
                Optional condenseFactor As Integer = 3,
                Optional dist$ = "30,250",
                Optional avoidRegions As RectangleF() = Nothing)

            Me.dist_thresh = dist.NumericRangeParser
            Me.condenseFactor = condenseFactor
            Me.ejectFactor = ejectFactor
            Me.avoidRegions = avoidRegions _
                .SafeQuery _
                .Select(Function(rect) (New Rectangle2D(rect), rect.Centre)) _
                .ToArray
        End Sub

        Public Overrides Function Width(x As Double) As DataLabeler
            CANVAS_WIDTH = x
            maxtx = CANVAS_WIDTH / 3

            Return Me
        End Function

        Public Overrides Function Height(x As Double) As DataLabeler
            CANVAS_HEIGHT = x
            maxty = CANVAS_HEIGHT / 3

            Return Me
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Collide()
            Call reset()
            Call runRepulsive()
            Call runAttraction()
            Call RejectRegions()
            Call setPosition()
        End Sub

        Protected Sub reset()
            For Each v As Label In m_labels
                mDxMap(v.text) = 0.0
                mDyMap(v.text) = 0.0
            Next
        End Sub

        Protected Sub RejectRegions()
            Dim dist, distX, distY As Double
            Dim id As String
            Dim dx, dy As Double

            For Each rect In avoidRegions
                For Each v As Label In m_labels

                    If v.pinned Then
                        Continue For
                    End If

                    distX = rect.center.X - v.X
                    distY = rect.center.Y - v.Y
                    dist = stdNum.Sqrt(distX * distX + distY * distY)
                    id = v.text

                    If dist > 0 Then
                        dx = (distX / dist) * (k * k / dist) * ejectFactor * 10
                        dy = (distY / dist) * (k * k / dist) * ejectFactor * 10

                        mDxMap(id) = mDxMap(id) + dx
                        mDyMap(id) = mDyMap(id) + dy
                    End If
                Next
            Next
        End Sub

        ''' <summary>
        ''' 标签节点之间存在的排斥力
        ''' </summary>
        Protected Overridable Sub runRepulsive()
            Dim distX, distY, dist As Double
            Dim id As String
            Dim dx, dy As Double
            Dim ejectFactor = Me.ejectFactor

            For Each v As Label In m_labels
                If v.pinned Then
                    Continue For
                Else
                    id = v.text
                End If

                mDxMap(id) = 0.0
                mDyMap(id) = 0.0

                For Each u As Label In m_labels.Where(Function(ui) Not ui Is v)
                    distX = v.X - u.X
                    distY = v.Y - u.Y
                    dist = stdNum.Sqrt(distX * distX + distY * distY)

                    'If (dist < dist_thresh.Min) Then
                    '    ejectFactor = 5
                    'End If

                    If dist > 0 AndAlso dist < dist_thresh.Max Then
                        dx = (distX / dist) * (k * k / dist) * ejectFactor
                        dy = (distY / dist) * (k * k / dist) * ejectFactor

                        mDxMap(id) = mDxMap(id) + dx
                        mDyMap(id) = mDyMap(id) + dy
                    End If
                Next
            Next
        End Sub

        ''' <summary>
        ''' 标签节点与anchor之间得吸引力
        ''' </summary>
        Protected Overridable Sub runAttraction()
            Dim u As Label
            Dim v As Anchor
            Dim distX, distY, dist As Double
            Dim dx, dy As Double

            For i As Integer = 0 To m_labels.Length - 1
                u = m_labels(i)
                v = m_anchors(i)

                If u.pinned Then
                    Continue For
                End If

                distX = u.X - v.x
                distY = u.Y - v.y
                dist = stdNum.Sqrt(distX * distX + distY * distY)
                dx = distX * dist / k * condenseFactor
                dy = distY * dist / k * condenseFactor

                mDxMap(u.text) = mDxMap(u.text) - dx / u.text.Length
                mDyMap(u.text) = mDyMap(u.text) - dy / u.text.Length
            Next
        End Sub

        Private Sub setPosition()
            Dim dx, dy As Double
            Dim x, y As Double

            For Each node As Label In m_labels.Where(Function(v) Not v.pinned)
                dx = mDxMap(node.text)
                dy = mDyMap(node.text)

                If dx <> 0 Then
                    dx = stdNum.Sign(dx) * stdNum.Log(stdNum.Abs(dx))
                End If
                If dy <> 0 Then
                    dy = stdNum.Sign(dy) * stdNum.Log(stdNum.Abs(dy))
                End If

                If (dx < -maxtx) Then dx = -maxtx
                If (dx > maxtx) Then dx = maxtx
                If (dy < -maxty) Then dy = -maxty
                If (dy > maxty) Then dy = maxty

                x = node.X
                y = node.Y
                x = x + dx ' If((x + dx) >= CANVAS_WIDTH OrElse (x + dx) <= 0, x - dx, x + dx)
                y = y + dy ' If((y + dy) >= CANVAS_HEIGHT OrElse (y + dy <= 0), y - dy, y + dy)

                If x + node.width >= CANVAS_WIDTH Then
                    x = CANVAS_WIDTH - node.width
                ElseIf x < offset.X Then
                    x = offset.X
                End If

                If y + node.height >= CANVAS_HEIGHT Then
                    y = CANVAS_HEIGHT - node.height
                ElseIf y < offset.Y Then
                    y = offset.Y
                End If

                node.X = x
                node.Y = y
            Next
        End Sub

        Public Overrides Function Start(Optional nsweeps As Integer = 2000, Optional showProgress As Boolean = True) As DataLabeler
            Me.k = stdNum.Sqrt(CANVAS_WIDTH * CANVAS_HEIGHT / m_labels.Length)

            For i As Integer = 0 To nsweeps
                Call Collide()

                If showProgress AndAlso (100 * i / nsweeps) Mod 5 = 0 Then
                    Console.WriteLine($"- Completed {i + 1} of {nsweeps} [{CInt(100 * i / nsweeps)}%]")
                End If
            Next

            Return Me
        End Function
    End Class
End Namespace
