﻿#Region "Microsoft.VisualBasic::be27e0e360a6d1bf39235bae4103424b, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\MarchingSquares\ContourLayer.vb"

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

    '   Total Lines: 148
    '    Code Lines: 96 (64.86%)
    ' Comment Lines: 30 (20.27%)
    '    - Xml Docs: 90.00%
    ' 
    '   Blank Lines: 22 (14.86%)
    '     File Size: 5.64 KB


    '     Class ContourLayer
    ' 
    '         Properties: dimension, shapes, threshold
    ' 
    '         Function: FillDots, GetContours, (+3 Overloads) GetOutline
    ' 
    '     Class Polygon2D
    ' 
    '         Properties: x, y
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: GetDimension, ToArray
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Drawing2D.Math2D.MarchingSquares

    Public Class ContourLayer

        Public Property threshold As Double
        Public Property shapes As Polygon2D()
        Public Property dimension As Integer()

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="sample"></param>
        ''' <param name="epsilon"></param>
        ''' <param name="interpolateFill"></param>
        ''' <param name="levels">
        ''' [0,1]之间的等级值，空值则使用默认等级列表
        ''' </param>
        ''' <returns></returns>
        Public Shared Iterator Function GetContours(sample As IEnumerable(Of MeasureData),
                                                    Optional epsilon As Double = 0.00001,
                                                    Optional interpolateFill As Boolean = True,
                                                    Optional levels As Double() = Nothing) As IEnumerable(Of GeneralPath)

            Dim matrix As New MapMatrix(sample, interpolateFill:=interpolateFill)
            Dim level_cutoff As Double() = If(
                levels.IsNullOrEmpty,
                matrix.GetPercentages,
                matrix.GetPercentages(levels)
            )
            Dim data As Double()() = matrix _
                .GetMatrixInterpolation _
                .MatrixTranspose _
                .ToArray

            For Each polygon As GeneralPath In New MarchingSquares(matrix.dimension, epsilon).mkIsos(data, levels:=level_cutoff)
                Yield polygon
            Next
        End Function

        Private Shared Iterator Function FillDots(x As Integer(), y As Integer(), fillSize As Integer) As IEnumerable(Of MeasureData)
            If fillSize <= 1 Then
                For i As Integer = 0 To x.Length - 1
                    Yield New MeasureData(x(i), y(i), 1)
                Next
            Else
                Dim d As Integer = fillSize / 2

                For i As Integer = 0 To x.Length - 1
                    For z As Integer = -d To d
                        If x(i) + z >= 0 AndAlso y(i) + z >= 0 Then
                            Yield New MeasureData(x(i) + z, y(i) + z, 1)
                        End If
                    Next
                Next
            End If
        End Function

        ''' <summary>
        ''' Do contour tracing for measure outline
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        Public Shared Function GetOutline(x As Integer(), y As Integer(), Optional fillSize As Integer = 1) As GeneralPath
            Dim sample As MeasureData() = FillDots(x, y, fillSize).ToArray
            Dim topleft As New MeasureData(0, 0, 0)
            Dim topright As New MeasureData(x.Max, y.Max, 0)
            Dim bottomleft As New MeasureData(0, y.Max, 0)
            Dim bottomright As New MeasureData(x.Max, 0, 0)
            Dim allRegions = sample _
                .JoinIterates({topleft, topright, bottomleft, bottomright}) _
                .DoCall(Function(poly)
                            Dim matrix As New MapMatrix(poly, interpolateFill:=False)
                            Dim path As GeneralPath = ContourTracing.GetOutine(matrix)

                            Return path
                        End Function)

            Return allRegions
        End Function

        ''' <summary>
        ''' Do contour tracing for measure outline
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function GetOutline(x As Double(), y As Double(), Optional fillSize As Integer = 1) As GeneralPath
            Return GetOutline(x.AsInteger, y.AsInteger, fillSize)
        End Function

        ''' <summary>
        ''' Do contour tracing for measure outline
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function GetOutline(x As Single(), y As Single(), Optional fillSize As Integer = 1) As GeneralPath
            Return GetOutline(x.AsInteger, y.AsInteger, fillSize)
        End Function
    End Class

    Public Class Polygon2D

        Public Property x As Integer()
        Public Property y As Integer()

        Sub New()
        End Sub

        Sub New(pixels As IEnumerable(Of Pixel))
            Dim px As New List(Of Integer)
            Dim py As New List(Of Integer)

            For Each pi As Pixel In pixels
                Call px.Add(pi.X)
                Call py.Add(pi.Y)
            Next

            x = px.ToArray
            y = py.ToArray
        End Sub

        Public Function GetDimension() As Rectangle
            Dim xmax = x.Max
            Dim ymax = y.Max
            Dim xmin = x.Min
            Dim ymin = y.Min

            Return New Rectangle(New Point(xmin, ymin), New Size(xmax - xmin, ymax - ymin))
        End Function

        Public Function ToArray() As PointF()
            Return x _
                .Select(Function(xi, i) New PointF(xi, y(i))) _
                .ToArray
        End Function
    End Class
End Namespace
