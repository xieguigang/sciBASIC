#Region "Microsoft.VisualBasic::00f6b414013df4cdb6385201f6cf46a8, sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\MarchingSquares\ContourLayer.vb"

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

    '   Total Lines: 97
    '    Code Lines: 67
    ' Comment Lines: 16
    '   Blank Lines: 14
    '     File Size: 3.78 KB


    '     Class ContourLayer
    ' 
    '         Properties: dimension, shapes, threshold
    ' 
    '         Function: FillDots, GetContours, GetOutline
    ' 
    '     Class Polygon2D
    ' 
    '         Properties: x, y
    ' 
    '         Function: ToArray
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Linq

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

        Private Shared Iterator Function FillDots(x As Double(), y As Double(), fillSize As Integer) As IEnumerable(Of MeasureData)
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
        Public Shared Function GetOutline(x As Double(), y As Double(), Optional fillSize As Integer = 1) As GeneralPath
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
    End Class

    Public Class Polygon2D

        Public Property x As Integer()
        Public Property y As Integer()

        Public Function ToArray() As PointF()
            Return x _
                .Select(Function(xi, i) New PointF(xi, y(i))) _
                .ToArray
        End Function
    End Class
End Namespace
