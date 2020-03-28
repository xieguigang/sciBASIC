#Region "Microsoft.VisualBasic::d62131f55650835a87e59a1a3e52cab8, gr\Microsoft.VisualBasic.Imaging\d3js\labeler\labeler.vb"

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

    '     Delegate Function
    ' 
    ' 
    '     Class Labeler
    ' 
    '         Function: Anchors, coolingSchedule, CoolingSchedule, defaultEnergyGet, energy
    '                   EnergyFunction, GetEnumerator, Height, IEnumerable_GetEnumerator, intersect
    '                   Labels, Size, Start, Width
    ' 
    '         Sub: mclMove, mclRotate, MonteCarlo
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports Microsoft.VisualBasic.Linq
Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports stdNum = System.Math

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
    Public Class Labeler : Implements IEnumerable(Of Label)

        Dim lab As Label()
        Dim anc As Anchor()
        Dim unpinnedLabels As Integer()

        ''' <summary>
        ''' box width/height
        ''' </summary>
        Dim w As Double = 1, h As Double = 1

        Dim acc As Integer = 0
        Dim rej As Integer = 0

        Friend maxMove As Double = 5
        Friend maxAngle As Double = 0.5

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
            Dim m = lab.Length,
                ener# = 0,
                dx = lab(index).X - anc(index).x,
                dy = anc(index).y - lab(index).Y

            ' 标签与anchor锚点之间的距离
            Dim dist = Math.Sqrt(dx * dx + dy * dy),
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

            Dim x21 = lab(index).X,
                y21 = lab(index).Y - lab(index).height + 2.0,
                x22 = lab(index).X + lab(index).width,
                y22 = lab(index).Y + 2.0
            Dim x11, x12, y11, y12, x_overlap, y_overlap, overlap_area

            For i As Integer = 0 To m - 1
                If (i <> index) Then

                    ' penalty for intersection of leader lines
                    overlap = intersect(
                        anc(index).x, lab(index).X, anc(i).x, lab(i).X,
                        anc(index).y, lab(index).Y, anc(i).y, lab(i).Y)

                    If (overlap) Then
                        ener += w_inter
                    End If

                    ' penalty for label-label overlap
                    x11 = lab(i).X
                    y11 = lab(i).Y - lab(i).height + 2.0
                    x12 = lab(i).X + lab(i).width
                    y12 = lab(i).Y + 2.0
                    x_overlap = Math.Max(0, stdNum.Min(x12, x22) - Math.Max(x11, x21))
                    y_overlap = Math.Max(0, stdNum.Min(y12, y22) - Math.Max(y11, y21))
                    overlap_area = x_overlap * y_overlap
                    ener += (overlap_area * w_lab2)
                End If

                ' penalty for label-anchor overlap
                x11 = anc(i).x - anc(i).r
                y11 = anc(i).y - anc(i).r
                x12 = anc(i).x + anc(i).r
                y12 = anc(i).y + anc(i).r

                x_overlap = Math.Max(0, stdNum.Min(x12, x22) - Math.Max(x11, x21))
                y_overlap = Math.Max(0, stdNum.Min(y12, y22) - Math.Max(y11, y21))

                overlap_area = x_overlap * y_overlap
                ener += (overlap_area * w_lab_anc)
            Next

            Return ener
        End Function

        ''' <summary>
        ''' returns true if two lines intersect, else false
        ''' from http:'paulbourke.net/geometry/lineline2d/
        ''' </summary>
        ''' <param name="x1#"></param>
        ''' <param name="x2#"></param>
        ''' <param name="x3#"></param>
        ''' <param name="x4#"></param>
        ''' <param name="y1#"></param>
        ''' <param name="y2#"></param>
        ''' <param name="y3#"></param>
        ''' <param name="y4#"></param>
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
            lab(i).X += (Rnd() - 0.5) * maxMove
            lab(i).Y += (Rnd() - 0.5) * maxMove
        End Sub

        Private Sub MonteCarlo(currT#, action As Action(Of Integer))
            ' select a random label which is not pinned
            Dim i As Integer = unpinnedLabels(Math.Floor(Rnd() * unpinnedLabels.Length))

            ' save old coordinates
            Dim x_old = lab(i).X
            Dim y_old = lab(i).Y

            ' old energy
            Dim old_energy# = calcEnergy(i, lab, anc)

            Call action(i)

            ' hard wall boundaries
            If (lab(i).X > w) Then lab(i).X = x_old
            If (lab(i).X < 0) Then lab(i).X = x_old
            If (lab(i).Y > h) Then lab(i).Y = y_old
            If (lab(i).Y < 0) Then lab(i).Y = y_old

            ' New energy
            Dim new_energy# = calcEnergy(i, lab, anc)
            ' delta E
            Dim delta_energy = new_energy - old_energy

            If (Rnd() < Math.Exp(-delta_energy / currT)) Then
                acc += 1
            Else
                ' move back to old coordinates
                lab(i).X = x_old
                lab(i).Y = y_old
                rej += 1
            End If
        End Sub

        ''' <summary>
        ''' Monte Carlo rotation move
        ''' </summary>
        Private Sub mclRotate(i%)
            ' random angle
            Dim angle = (Rnd() - 0.5) * maxAngle

            Dim s = Math.Sin(angle)
            Dim c = Math.Cos(angle)

            ' translate label (relative to anchor at origin):
            lab(i).X -= anc(i).x
            lab(i).Y -= anc(i).y

            ' rotate label
            Dim x_new = lab(i).X * c - lab(i).Y * s,
                y_new = lab(i).X * s + lab(i).Y * c

            ' translate label back
            lab(i).X = x_new + anc(i).x
            lab(i).Y = y_new + anc(i).y
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

        ''' <summary>
        ''' main simulated annealing function.(这个函数运行完成之后，可以直接使用<see cref="Label.X"/>和<see cref="Label.Y"/>位置数据进行作图)
        ''' </summary>
        ''' <param name="nsweeps"></param>
        ''' <returns></returns>
        Public Function Start(Optional nsweeps% = 2000,
                              Optional T# = 1,
                              Optional initialT# = 1,
                              Optional rotate# = 0.5,
                              Optional showProgress As Boolean = True) As Labeler

            Dim moves As Action(Of Integer) = AddressOf mclMove
            Dim rotat As Action(Of Integer) = AddressOf mclRotate
            Dim progress As ProgressBar = Nothing
            Dim tick As Action(Of Double)

            ' 在计算之前需要将label的坐标赋值为anchor的值，否则会无法正常的生成label的最终位置
            For i As Integer = 0 To lab.Length - 1
                If lab(i).X = 0.0 AndAlso lab(i).Y = 0.0 Then
                    lab(i).X = anc(i).x
                    lab(i).Y = anc(i).y
                End If
            Next

            unpinnedLabels = lab.SeqIterator _
                .Where(Function(l) Not l.value.pinned) _
                .Select(Function(lb) lb.i) _
                .ToArray

            If unpinnedLabels.Length = 0 Then
                Call "No unpinned label to be re-layout!".Warning
                Return Me
            End If

            If showProgress Then
                Dim tickProvider As ProgressProvider
                Dim p#

                progress = New ProgressBar("Labels layouting...")
                tickProvider = New ProgressProvider(progress, nsweeps)
                tick = Sub(currT#)
                           p = tickProvider.StepProgress
                           progress.SetProgress(p, "Current temperature: " & currT.ToString("F2"))
                       End Sub
            Else
                tick = Sub()
                       End Sub
            End If

            For i As Integer = 0 To nsweeps
                For j As Integer = 0 To lab.Length
                    If (randf.seeds.NextDouble < rotate) Then
                        Call MonteCarlo(T, moves)
                    Else
                        Call MonteCarlo(T, rotat)
                    End If
                Next

                T = definedCoolingSchedule(T, initialT, nsweeps)
                tick(T)
            Next

            Call progress?.Dispose()

            Return Me
        End Function

        ''' <summary>
        ''' users insert graph width
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Function Width(x#) As Labeler
            w = x
            Return Me
        End Function

        ''' <summary>
        ''' users insert graph height
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Function Height(x#) As Labeler
            h = x
            Return Me
        End Function

        Public Function Size(x As SizeF) As Labeler
            With x
                w = .Width
                h = .Height
            End With

            Return Me
        End Function

        ''' <summary>
        ''' users insert label positions
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Function Labels(x As IEnumerable(Of Label)) As Labeler
            lab = x.ToArray
            Return Me
        End Function

        ''' <summary>
        ''' users insert anchor positions
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Function Anchors(x As IEnumerable(Of Anchor)) As Labeler
            anc = x.ToArray
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

        Public Iterator Function GetEnumerator() As IEnumerator(Of Label) Implements IEnumerable(Of Label).GetEnumerator
            For Each label As Label In lab
                Yield label
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
