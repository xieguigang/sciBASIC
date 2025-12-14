#Region "Microsoft.VisualBasic::1c86359fb4135bfd4f83e2c6093ac26a, gr\Microsoft.VisualBasic.Imaging\d3js\labeler\labeler.vb"

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

    '   Total Lines: 339
    '    Code Lines: 197 (58.11%)
    ' Comment Lines: 86 (25.37%)
    '    - Xml Docs: 70.93%
    ' 
    '   Blank Lines: 56 (16.52%)
    '     File Size: 12.79 KB


    '     Delegate Function
    ' 
    ' 
    '     Class Labeler
    ' 
    '         Function: coolingSchedule, CoolingSchedule, defaultEnergyGet, energy, EnergyFunction
    '                   intersect, MaxMoveDistance, RotateChance, Start, Temperature
    ' 
    '         Sub: mclMove, mclRotate, MonteCarlo
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

Namespace d3js.Layout

    Public Delegate Function CoolingSchedule(currT#, initialT#, nsweeps#) As Double

    ''' <summary>
    ''' A D3 plug-in for automatic label placement using simulated annealing that 
    ''' easily incorporates into existing D3 code, with syntax mirroring other 
    ''' D3 layouts.
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/tinker10/D3-Labeler
    ''' </remarks>
    Public Class Labeler : Inherits DataLabeler

        Dim acc As Integer = 0
        Dim rej As Integer = 0

        ''' <summary>
        ''' the max move distance in each loop iteration
        ''' </summary>
        Friend maxMove As Double = 5
        Friend maxAngle As Double = 0.5
        ''' <summary>
        ''' the max total move distance of the label with 
        ''' the corresponding anchor point.
        ''' </summary>
        Friend maxDistance As Double = 30

#Region "weights"
        Friend w_len As Double = 0.2      ' leader line length 
        Friend w_inter As Double = 1.0    ' leader line intersection
        Friend w_lab2 As Double = 30.0    ' label-label overlap
        Friend w_lab_anc As Double = 30.0 ' label-anchor overlap
        Friend w_orient As Double = 3.0   ' orientation bias
#End Region

        Dim calcEnergy As Func(Of Integer, Label(), Anchor(), Double) = AddressOf defaultEnergyGet
        Dim definedCoolingSchedule As CoolingSchedule = AddressOf coolingSchedule

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function defaultEnergyGet(i%, labels As Label(), anchor As Anchor()) As Double
            Return energy(i)
        End Function

        ''' <summary>
        ''' energy function, tailored for label placement
        ''' </summary>
        ''' <param name="index%"></param>
        ''' <returns></returns>
        Public Function energy(index%) As Double
            Dim m = m_labels.Length,
                ener# = 0,
                dx = m_labels(index).X - m_anchors(index).x,
                dy = m_anchors(index).y - m_labels(index).Y

            ' 标签与anchor锚点之间的距离
            Dim dist = std.Sqrt(dx * dx + dy * dy),
                overlap = True,
                amount = 0

            ' penalty for length of leader line
            If (dist > 0) Then
                ener += dist * w_len
            End If

            ' label orientation bias
            dx /= dist
            dy /= dist

            If (dx > 0 AndAlso dy > 0) Then
                ener += 0 * w_orient
            ElseIf (dx < 0 AndAlso dy > 0) Then
                ener += 1 * w_orient
            ElseIf (dx < 0 AndAlso dy < 0) Then
                ener += 2 * w_orient
            Else
                ener += 3 * w_orient
            End If

            Dim x21 = m_labels(index).X,
                y21 = m_labels(index).Y - m_labels(index).height + 2.0,
                x22 = m_labels(index).X + m_labels(index).width,
                y22 = m_labels(index).Y + 2.0
            Dim x11, x12, y11, y12, x_overlap, y_overlap, overlap_area

            For i As Integer = 0 To m - 1
                If (i <> index) Then

                    ' penalty for intersection of leader lines
                    overlap = intersect(
                        m_anchors(index).x, m_labels(index).X, m_anchors(i).x, m_labels(i).X,
                        m_anchors(index).y, m_labels(index).Y, m_anchors(i).y, m_labels(i).Y
                    )

                    If overlap Then
                        ener += w_inter
                    End If

                    ' penalty for label-label overlap
                    x11 = m_labels(i).X
                    y11 = m_labels(i).Y - m_labels(i).height + 2.0
                    x12 = m_labels(i).X + m_labels(i).width
                    y12 = m_labels(i).Y + 2.0
                    x_overlap = std.Max(0, std.Min(x12, x22) - std.Max(x11, x21))
                    y_overlap = std.Max(0, std.Min(y12, y22) - std.Max(y11, y21))
                    overlap_area = x_overlap * y_overlap
                    ener += (overlap_area * w_lab2)
                End If

                ' penalty for label-anchor overlap
                x11 = m_anchors(i).x - m_anchors(i).r
                y11 = m_anchors(i).y - m_anchors(i).r
                x12 = m_anchors(i).x + m_anchors(i).r
                y12 = m_anchors(i).y + m_anchors(i).r

                x_overlap = std.Max(0, std.Min(x12, x22) - std.Max(x11, x21))
                y_overlap = std.Max(0, std.Min(y12, y22) - std.Max(y11, y21))

                overlap_area = x_overlap * y_overlap
                ener += (overlap_area * w_lab_anc)
            Next

            Return ener
        End Function

        ''' <summary>
        ''' returns true if two lines intersect, else false
        ''' from http:'paulbourke.net/geometry/lineline2d/
        ''' </summary>
        ''' <param name="x1"></param>
        ''' <param name="x2"></param>
        ''' <param name="x3"></param>
        ''' <param name="x4"></param>
        ''' <param name="y1"></param>
        ''' <param name="y2"></param>
        ''' <param name="y3"></param>
        ''' <param name="y4"></param>
        ''' <returns></returns>
        Private Shared Function intersect(x1#, x2#, x3#, x4#, y1#, y2#, y3#, y4#) As Boolean
            Dim mua, mub As Double
            Dim denom, numera, numerb As Double

            denom = (y4 - y3) * (x2 - x1) - (x4 - x3) * (y2 - y1)
            numera = (x4 - x3) * (y1 - y3) - (y4 - y3) * (x1 - x3)
            numerb = (x2 - x1) * (y1 - y3) - (y2 - y1) * (x1 - x3)

            ' Is the intersection along the the segments 
            mua = numera / denom
            mub = numerb / denom

            If (Not (mua < 0 OrElse mua > 1 OrElse mub < 0 OrElse mub > 1)) Then
                Return True
            End If

            Return False
        End Function

#Region "Monte Carlo"

        ''' <summary>
        ''' Monte Carlo translation move
        ''' </summary>
        Private Sub mclMove(i%)
            ' random translation
            m_labels(i).X += (randf.NextDouble - 0.5) * maxMove
            m_labels(i).Y += (randf.NextDouble - 0.5) * maxMove
        End Sub

        Private Sub MonteCarlo(currT#, action As Action(Of Integer))
            ' select a random label which is not pinned
            Dim i As Integer = unpinnedLabels(std.Floor(randf.NextDouble * unpinnedLabels.Length))
            Dim label As Label = m_labels(i)
            Dim anchor As Anchor = m_anchors(i)

            ' save old coordinates
            Dim x_old = label.X
            Dim y_old = label.Y
            Dim distance_old = label.distanceTo(anchor)

            ' old energy
            Dim old_energy# = calcEnergy(i, m_labels, m_anchors)

            Call action(i)

            ' hard wall boundaries
            If (label.X + label.width > CANVAS_WIDTH + offset.X) Then m_labels(i).X = x_old
            If (label.X < 0) Then m_labels(i).X = x_old
            If (label.Y + label.height > CANVAS_HEIGHT + offset.Y) Then m_labels(i).Y = y_old
            If (label.Y < 0) Then m_labels(i).Y = y_old

            ' New energy
            Dim new_energy# = calcEnergy(i, m_labels, m_anchors)
            Dim distance_new = label.distanceTo(anchor)
            ' delta E
            Dim delta_energy = (new_energy - old_energy) * If(distance_old = 0.0, 1, distance_new / distance_old)

            If distance_new > maxDistance Then
                delta_energy = 1.0E+64
            End If

            ' the lower of the delta energy
            ' the higher chance to accept current change
            If (randf.NextDouble < std.Exp(-delta_energy / currT)) Then
                acc += 1
            Else
                ' move back to old coordinates
                m_labels(i).X = x_old
                m_labels(i).Y = y_old
                rej += 1
            End If
        End Sub

        ''' <summary>
        ''' Monte Carlo rotation move
        ''' </summary>
        Private Sub mclRotate(i%)
            ' random angle
            Dim angle = (randf.NextDouble - 0.5) * maxAngle

            Dim s = std.Sin(angle)
            Dim c = std.Cos(angle)

            ' translate label (relative to anchor at origin):
            m_labels(i).X -= m_anchors(i).x
            m_labels(i).Y -= m_anchors(i).y

            ' rotate label
            Dim x_new = m_labels(i).X * c - m_labels(i).Y * s,
                y_new = m_labels(i).X * s + m_labels(i).Y * c

            ' translate label back
            m_labels(i).X = x_new + m_anchors(i).x
            m_labels(i).Y = y_new + m_anchors(i).y
        End Sub
#End Region

        ''' <summary>
        ''' Default is using linear cooling
        ''' </summary>
        ''' <param name="currT#"></param>
        ''' <param name="initialT#"></param>
        ''' <param name="nsweeps#"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function coolingSchedule(currT#, initialT#, nsweeps#) As Double
            Return (currT - (initialT / nsweeps))
        End Function

        Dim T# = 1
        Dim initialT# = 1
        Dim rotate# = 0.5

        Public Function Temperature(Optional T# = 1, Optional initialT# = 1) As Labeler
            Me.T = T
            Me.initialT = initialT
            Return Me
        End Function

        Public Function MaxMoveDistance(Optional max As Double = 50) As Labeler
            Me.maxDistance = max
            Return Me
        End Function

        Public Function RotateChance(Optional rotate# = 0.5) As Labeler
            Me.rotate = rotate
            Return Me
        End Function

        ''' <summary>
        ''' main simulated annealing function.(这个函数运行完成之后，可以直接使用<see cref="Label.X"/>和<see cref="Label.Y"/>位置数据进行作图)
        ''' </summary>
        ''' <param name="nsweeps"></param>
        ''' <returns></returns>
        Public Overrides Function Start(Optional nsweeps% = 2000, Optional showProgress As Boolean = True) As DataLabeler
            Dim moves As Action(Of Integer) = AddressOf mclMove
            Dim rotat As Action(Of Integer) = AddressOf mclRotate

            ' 在计算之前需要将label的坐标赋值为anchor的值，否则会无法正常的生成label的最终位置
            For i As Integer = 0 To m_labels.Length - 1
                If m_labels(i).X = 0.0 AndAlso m_labels(i).Y = 0.0 Then
                    m_labels(i).X = m_anchors(i).x
                    m_labels(i).Y = m_anchors(i).y
                End If
            Next

            If unpinnedLabels.Length = 0 Then
                Call "no unpinned label to be re-layout!".warning
                Return Me
            Else
                Call "labels layouting...".info
            End If

            Dim bar As Tqdm.ProgressBar = Nothing

            For Each i As Integer In Tqdm.Range(0, nsweeps, bar:=bar, wrap_console:=showProgress)
                For j As Integer = 0 To m_labels.Length
                    ' choose rotate or move action based on the 
                    ' random states
                    If (randf.seeds.NextDouble < rotate) Then
                        Call MonteCarlo(T, moves)
                    Else
                        Call MonteCarlo(T, rotat)
                    End If
                Next

                T = definedCoolingSchedule(T, initialT, nsweeps)
                bar?.SetLabel($"temperature: {T:F2}")
            Next

            Return Me
        End Function

        ''' <summary>
        ''' user defined energy
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Function EnergyFunction(x As Func(Of Integer, Label(), Anchor(), Double)) As Labeler
            calcEnergy = x
            Return Me
        End Function

        ''' <summary>
        ''' user defined cooling_schedule
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Function CoolingSchedule(x As CoolingSchedule) As Labeler
            definedCoolingSchedule = x
            Return Me
        End Function
    End Class
End Namespace
