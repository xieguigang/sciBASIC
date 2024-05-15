#Region "Microsoft.VisualBasic::84336cdfd4b2ad6df6f2ad4367255f1d, Data_science\Mathematica\Math\Math\Spline\B_Spline.vb"

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

    '   Total Lines: 143
    '    Code Lines: 85
    ' Comment Lines: 37
    '   Blank Lines: 21
    '     File Size: 5.16 KB


    '     Module B_Spline
    ' 
    '         Function: (+2 Overloads) BSpline, Compute, OutputPoint
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace Interpolation

    ''' <summary>
    ''' B-spline.
    ''' 
    ''' degree参数应该是大于1的，否则等于1的时候将不会进行插值处理
    ''' </summary>
    ''' <remarks>https://github.com/kerrot/B_Spline</remarks>
    Public Module B_Spline

        ''' <summary>
        ''' B-spline curve interpolation
        ''' </summary>
        ''' <param name="ctrlPts">Control points</param>
        ''' <param name="degree%"></param>
        ''' <param name="RESOLUTION%"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function BSpline(ctrlPts As IEnumerable(Of Point), Optional degree! = 5, Optional RESOLUTION% = 10) As IEnumerable(Of PointF)
            Return ctrlPts _
                .Select(Function(pt)
                            Return New PointF With {
                                .X = pt.X,
                                .Y = pt.Y
                            }
                        End Function) _
                .Compute(degree, RESOLUTION)
        End Function

        ''' <summary>
        ''' B-spline curve interpolation
        ''' </summary>
        ''' <param name="ctrlPts">Control points</param>
        ''' <param name="degree%"></param>
        ''' <param name="RESOLUTION%"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function BSpline(ctrlPts As IEnumerable(Of PointF), Optional degree! = 5, Optional RESOLUTION% = 10) As IEnumerable(Of PointF)
            Return ctrlPts.Compute(degree, RESOLUTION)
        End Function

        ''' <summary>
        ''' B-spline curve interpolation
        ''' </summary>
        ''' <param name="controlPoints">Control points</param>
        ''' <param name="degree%"></param>
        ''' <param name="RESOLUTION%"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Iterator Function Compute(controlPoints As IEnumerable(Of PointF), Optional degree! = 5, Optional RESOLUTION% = 10) As IEnumerable(Of PointF)
            Dim ctrlPts = controlPoints.ToArray
            Dim p As PointF?

            If ctrlPts.Length > 1 Then
                Dim ustep As Double = 1.0 / (RESOLUTION * (ctrlPts.Length - 1))

                Dim n As Integer = ctrlPts.Length - 1
                Dim k! = If((ctrlPts.Length > degree), degree + 1, ctrlPts.Length)
                Dim m As Integer = k + n

                Dim t As Double() = New Double(m) {}
                For i As Integer = 0 To k - 1
                    t(i) = 0
                    t(t.Length - 1 - i) = 1
                Next
                If m + 1 > 2 * k Then
                    Dim tstep As Double = 1.0 / (m - 2 * (k - 1))
                    For i As Integer = k To m - k
                        t(i) = t(i - 1) + tstep
                    Next
                End If

                Dim u As Double = 0

                While u < 1
                    p = ctrlPts.OutputPoint(t, k, u)
                    u += ustep

                    If Not p Is Nothing Then
                        Yield p.Value
                    End If
                End While

                Yield ctrlPts.Last()
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="ctrlPts"></param>
        ''' <param name="t"></param>
        ''' <param name="k%">
        ''' 因为在这个函数里面的k参数是用来计算数组元素的index的，所以在这里不使用实数来表示了，而是使用整形数
        ''' </param>
        ''' <param name="u"></param>
        <Extension>
        Private Function OutputPoint(ctrlPts As PointF(), t As Double(), k%, u As Double) As PointF?
            Dim i As Integer, j As Integer, r As Integer
            Dim d1 As Double, d2 As Double
            Dim l As Integer = 0

            While l < t.Length AndAlso t(l) <= u
                l += 1
            End While

            l -= 1

            Dim A As PointF() = New PointF(k - 1) {}

            For j = 0 To k - 1
                Dim index As Integer = l - k + 1 + j

                If index < 0 OrElse index > ctrlPts.Length - 1 Then
                    Return Nothing
                End If

                A(j) = New PointF() With {
                    .X = ctrlPts(index).X,
                    .Y = ctrlPts(index).Y
                }
            Next

            For r = 1 To k - 1
                For j = k - 1 To r Step -1
                    i = l - k + 1 + j
                    d1 = u - t(i)
                    d2 = t(i + k - r) - u
                    A(j).X = (d1 * A(j).X + d2 * A(j - 1).X) / (d1 + d2)
                    A(j).Y = (d1 * A(j).Y + d2 * A(j - 1).Y) / (d1 + d2)
                Next
            Next

            Return A(k - 1)
        End Function
    End Module
End Namespace
