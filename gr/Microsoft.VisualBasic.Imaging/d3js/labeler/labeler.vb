Imports System.Drawing
Imports sys = System.Math

Namespace d3js

    ''' <summary>
    ''' A D3 plug-in for automatic label placement using simulated annealing that 
    ''' easily incorporates into existing D3 code, with syntax mirroring other 
    ''' D3 layouts.
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/tinker10/D3-Labeler
    ''' </remarks>
    Public Class Labeler
        Dim lab As Label()
        Dim anc As Anchor()
        Dim w = 1, h = 1 ' box width/height
        Dim labeler

        Dim max_move As Double = 5
        Dim max_angle As Double = 0.5
        Dim acc As Double = 0
        Dim rej As Double = 0

        ' weights
        Dim w_len As Double = 0.2 ' leader line length 
        Dim w_inter As Double = 1.0 ' leader line intersection
        Dim w_lab2 As Double = 30.0 ' label-label overlap
        Dim w_lab_anc As Double = 30.0 ' label-anchor overlap
        Dim w_orient As Double = 3.0 ' orientation bias

        ' booleans for user defined functions
        Dim user_energy As Boolean = False
        Dim user_schedule As Boolean = False

        Dim user_defined_energy, user_defined_schedule

        ''' <summary>
        ''' energy function, tailored for label placement
        ''' </summary>
        ''' <param name="index%"></param>
        ''' <returns></returns>
        Public Function energy(index%) As Double
            Dim m = lab.Length,
                ener# = 0,
                dx = lab(index).X - anc(index).x,
                dy = anc(index).y - lab(index).Y,
                dist = Math.Sqrt(dx * dx + dy * dy),
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
                    overlap = intersect(anc(index).x, lab(index).X, anc(i).x, lab(i).X,
            anc(index).y, lab(index).Y, anc(i).y, lab(i).Y)
                    If (overlap) Then
                        ener += w_inter
                    End If

                    ' penalty for label-label overlap
                    x11 = lab(i).X
                    y11 = lab(i).Y - lab(i).height + 2.0
                    x12 = lab(i).X + lab(i).width
                    y12 = lab(i).Y + 2.0
                    x_overlap = Math.Max(0, sys.Min(x12, x22) - Math.Max(x11, x21))
                    y_overlap = Math.Max(0, sys.Min(y12, y22) - Math.Max(y11, y21))
                    overlap_area = x_overlap * y_overlap
                    ener += (overlap_area * w_lab2)
                End If

                ' penalty for label-anchor overlap
                x11 = anc(i).x - anc(i).r
                y11 = anc(i).y - anc(i).r
                x12 = anc(i).x + anc(i).r
                y12 = anc(i).y + anc(i).r
                x_overlap = Math.Max(0, sys.Min(x12, x22) - Math.Max(x11, x21))
                y_overlap = Math.Max(0, sys.Min(y12, y22) - Math.Max(y11, y21))
                overlap_area = x_overlap * y_overlap
                ener += (overlap_area * w_lab_anc)
            Next

            Return ener
        End Function

        Private Shared Function intersect(x1#, x2#, x3#, x4#, y1#, y2#, y3#, y4#) As Boolean

            ' returns true if two lines intersect, else false
            ' from http://paulbourke.net/geometry/lineline2d/

            Dim mua, mub
            Dim denom, numera, numerb

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
    End Class
End Namespace
