#Region "Microsoft.VisualBasic::e5edaf5369d53c5468fdb9e5cbdd22a4, Data_science\Mathematica\Math\Math\Spline\CatmullRomSpline.vb"

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

    '   Total Lines: 155
    '    Code Lines: 96 (61.94%)
    ' Comment Lines: 35 (22.58%)
    '    - Xml Docs: 97.14%
    ' 
    '   Blank Lines: 24 (15.48%)
    '     File Size: 6.89 KB


    '     Module CatmullRomSpline
    ' 
    '         Function: __catmullRomSplinePolygon, (+2 Overloads) CatmullRomSpline, PointOnCurve
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Interpolation

    ''' <summary>
    ''' Calculates interpolated point between two points using Catmull-Rom Spline
    ''' </summary>
    ''' <remarks>https://en.wikipedia.org/wiki/Centripetal_Catmull%E2%80%93Rom_spline</remarks>
    Public Module CatmullRomSpline

        ''' <summary>
        ''' Calculates interpolated point between two points using Catmull-Rom Spline</summary>
        ''' <remarks>
        ''' Points calculated exist on the spline between points two and three.</remarks>
        ''' <param name="p0">First Point</param>
        ''' <param name="p1">Second Point</param>
        ''' <param name="p2">Third Point</param>
        ''' <param name="p3">Fourth Point</param>
        ''' <param name="t">
        ''' Normalised distance between second and third point where the spline point will be calculated</param>
        ''' <returns>Calculated Spline Point</returns>
        ''' 
        Public Function PointOnCurve(p0 As PointF, p1 As PointF, p2 As PointF, p3 As PointF, t#) As PointF
            Dim t2! = t * t
            Dim t3! = t2 * t
            Dim ret As New PointF With {
                .X = 0.5F * (
                    (2.0F * p1.X) + (-p0.X + p2.X) * t +
                    (2.0F * p0.X - 5.0F * p1.X + 4 * p2.X - p3.X) * t2 +
                    (-p0.X + 3.0F * p1.X - 3.0F * p2.X + p3.X) * t3),
                .Y = 0.5F * (
                    (2.0F * p1.Y) + (-p0.Y + p2.Y) * t +
                    (2.0F * p0.Y - 5.0F * p1.Y + 4 * p2.Y - p3.Y) * t2 +
                    (-p0.Y + 3.0F * p1.Y - 3.0F * p2.Y + p3.Y) * t3)
            }

            Return ret
        End Function

        ''' <summary>
        ''' Catmull-Rom splines are a family of cubic interpolating splines formulated such 
        ''' that the tangent at each point **Pi** Is calculated using the previous And next 
        ''' point on the spline
        ''' </summary>
        ''' <param name="raw"></param>
        ''' <param name="interpolationStep#"></param>
        ''' <param name="isPolygon"></param>
        ''' <returns></returns>
        <ExportAPI("CatmullRom.Spline")>
        <Extension>
        Public Function CatmullRomSpline(raw As IEnumerable(Of Point),
                                         <Parameter("Interpolation.Steps")>
                                         Optional interpolationStep# = 0.1,
                                         <Parameter("Is.Polygon")>
                                         Optional isPolygon As Boolean = False) As List(Of Point)

            Dim data As IEnumerable(Of PointF) = raw.Select(Function(pt) New PointF(pt.X, pt.Y))
            Dim spline = data.CatmullRomSpline(interpolationStep, isPolygon)
            Dim result As New List(Of Point)(spline.Select(Function(pt) New Point(pt.X, pt.Y)))

            Return result
        End Function

        ''' <summary>
        ''' Catmull-Rom splines are a family of cubic interpolating splines formulated such 
        ''' that the tangent at each point **Pi** Is calculated using the previous And next 
        ''' point on the spline
        ''' </summary>
        ''' <param name="points"></param>
        ''' <param name="interpolationStep#"></param>
        ''' <param name="isPolygon"></param>
        ''' <returns></returns>
        ''' <remarks>http://www.codeproject.com/Articles/747928/Spline-Interpolation-history-theory-and-implementa</remarks>
        <ExportAPI("CatmullRom.Spline")>
        <Extension>
        Public Function CatmullRomSpline(points As IEnumerable(Of PointF),
                                         <Parameter("Interpolation.Steps")>
                                         Optional interpolationStep# = 0.1,
                                         <Parameter("Is.Polygon")>
                                         Optional isPolygon As Boolean = False) As List(Of PointF)

            Dim raw As New List(Of PointF)(points)

            If raw.Count <= 2 Then
                Return raw
            End If

            If isPolygon Then
                Return __catmullRomSplinePolygon(raw, interpolationStep)
            End If

            Dim yarray, xarray As New List(Of Double)
            Dim result As New List(Of PointF)

            Call xarray.Add(raw(0).X - (raw(1).X - raw(0).X) / 2)
            Call yarray.Add(raw(0).Y - (raw(1).Y - raw(0).Y) / 2)

            For Each ps As PointF In raw
                Call xarray.Add(ps.X)
                Call yarray.Add(ps.Y)
            Next

            Call xarray.Add((raw(raw.Count - 1).X - (raw(raw.Count - 2).X) / 2 + raw(raw.Count - 1).X))
            Call yarray.Add((raw(raw.Count - 1).Y - (raw(raw.Count - 2).Y) / 2 + raw(raw.Count - 1).Y))

            Dim r As New List(Of PointF)

            For i As Integer = 0 To yarray.Count - 1
                Call r.Add(New PointF(xarray(i), yarray(i)))
            Next

            For i As Integer = 3 To r.Count - 1
                For k As Double = 0 To (1 - interpolationStep) Step interpolationStep
                    Call result.Add(PointOnCurve(r(i - 3), r(i - 2), r(i - 1), r(i), k))
                Next
            Next

            Call result.Add(raw(raw.Count - 1))

            Return result
        End Function

        Private Function __catmullRomSplinePolygon(points As List(Of PointF), steps#) As List(Of PointF)
            Dim result As New List(Of PointF)

            For i As Integer = 0 To points.Count - 1
                If i = 0 Then
                    For k As Double = 0 To (1 - steps) Step steps
                        result.Add(PointOnCurve(points(points.Count - 1), points(i), points(i + 1), points(i + 2), k))
                    Next
                ElseIf i = points.Count - 1 Then
                    For k As Double = 0 To (1 - steps) Step steps
                        result.Add(PointOnCurve(points(i - 1), points(i), points(0), points(1), k))
                    Next
                ElseIf i = points.Count - 2 Then
                    For k As Double = 0 To (1 - steps) Step steps
                        result.Add(PointOnCurve(points(i - 1), points(i), points(i + 1), points(0), k))
                    Next
                Else
                    For k As Double = 0 To (1 - steps) Step steps
                        result.Add(PointOnCurve(points(i - 1), points(i), points(i + 1), points(i + 2), k))
                    Next
                End If
            Next

            Call result.Add(points(0))

            Return result
        End Function
    End Module
End Namespace
