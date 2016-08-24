#Region "Microsoft.VisualBasic::ab23c14cb9f893e66995d09da1938651, ..\visualbasic_App\Datavisualization\Datavisualization.Network\Datavisualization.Network\Visualize\Lines.vb"

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

Imports System.Drawing
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Visualize

    Public Module Lines

        ''' <summary>
        ''' Calculates interpolated point between two points using Catmull-Rom Spline/// </summary>
        ''' <remarks>
        ''' Points calculated exist on the spline between points two and three./// </remarks>
        ''' <param name="p0">First Point</param>
        ''' <param name="p1">Second Point</param>
        ''' <param name="p2">Third Point</param>
        ''' <param name="p3">Fourth Point</param>
        ''' <param name="t">
        ''' Normalised distance between second and third point /// where the spline point will be calculated/// </param>
        ''' <returns>Calculated Spline Point/// </returns>
        ''' 
        <ExportAPI("CatmullRom.Spline",
                   Info:="Calculates interpolated point between two points using Catmull-Rom Spline")>
        Private Function PointOnCurve(p0 As Point, p1 As Point, p2 As Point, p3 As Point, t As Double) As Point
            Dim ret As New Point()

            Dim t2 As Single = t * t
            Dim t3 As Single = t2 * t

            ret.X = 0.5F * ((2.0F * p1.X) + (-p0.X + p2.X) * t + (2.0F * p0.X - 5.0F * p1.X + 4 * p2.X - p3.X) * t2 + (-p0.X + 3.0F * p1.X - 3.0F * p2.X + p3.X) * t3)
            ret.Y = 0.5F * ((2.0F * p1.Y) + (-p0.Y + p2.Y) * t + (2.0F * p0.Y - 5.0F * p1.Y + 4 * p2.Y - p3.Y) * t2 + (-p0.Y + 3.0F * p1.Y - 3.0F * p2.Y + p3.Y) * t3)

            Return ret
        End Function

        <ExportAPI("CatmullRom.Spline")>
        Public Function CatmullRomSpline(Points As List(Of Point),
                                     <Parameter("Interpolation.Steps")> Optional InterpolationStep As Double = 0.1,
                                     <Parameter("Is.Polygon")> Optional IsPolygon As Boolean = False) As List(Of Point)
            If Points.Count <= 2 Then
                Return Points
            End If

            If IsPolygon Then Return __catmullRomSplinePolygon(Points, InterpolationStep)

            Dim result As New List(Of Point)
            Dim yarray, xarray As New List(Of Double)
            xarray.Add(Points(0).X - (Points(1).X - Points(0).X) / 2)
            yarray.Add(Points(0).Y - (Points(1).Y - Points(0).Y) / 2)

            For Each ps As Point In Points
                xarray.Add(ps.X)
                yarray.Add(ps.Y)
            Next

            xarray.Add((Points(Points.Count - 1).X - (Points(Points.Count - 2).X) / 2 + Points(Points.Count - 1).X))
            yarray.Add((Points(Points.Count - 1).Y - (Points(Points.Count - 2).Y) / 2 + Points(Points.Count - 1).Y))

            Dim r As New List(Of Point)
            For i As Integer = 0 To yarray.Count - 1
                r.Add(New Point(xarray(i), yarray(i)))
            Next

            For i As Integer = 3 To r.Count - 1
                For k As Double = 0 To (1 - InterpolationStep) Step InterpolationStep
                    result.Add(PointOnCurve(r(i - 3), r(i - 2), r(i - 1), r(i), k))
                Next
            Next
            result.Add(Points(Points.Count - 1))

            Return result
        End Function

        Private Function __catmullRomSplinePolygon(points As List(Of Point), InterpolationStep As Double) As List(Of Point)
            Dim result As New List(Of Point)

            For i As Integer = 0 To points.Count - 1
                If i = 0 Then
                    For k As Double = 0 To (1 - InterpolationStep) Step InterpolationStep
                        result.Add(PointOnCurve(points(points.Count - 1), points(i), points(i + 1), points(i + 2), k))
                    Next
                ElseIf i = points.Count - 1 Then
                    For k As Double = 0 To (1 - InterpolationStep) Step InterpolationStep
                        result.Add(PointOnCurve(points(i - 1), points(i), points(0), points(1), k))
                    Next
                ElseIf i = points.Count - 2 Then
                    For k As Double = 0 To (1 - InterpolationStep) Step InterpolationStep
                        result.Add(PointOnCurve(points(i - 1), points(i), points(i + 1), points(0), k))
                    Next
                Else
                    For k As Double = 0 To (1 - InterpolationStep) Step InterpolationStep
                        result.Add(PointOnCurve(points(i - 1), points(i), points(i + 1), points(i + 2), k))
                    Next
                End If
            Next

            Call result.Add(points(0))

            Return result
        End Function
    End Module
End Namespace
