#Region "Microsoft.VisualBasic::6090615ec98e193210749f6d98b4e511, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\MarchingSquares\MapMatrix.vb"

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

'     Class MapMatrix
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: GetMatrixInterpolation, InitData
' 
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.Quantile
Imports stdNum = System.Math

Namespace Drawing2D.Math2D.MarchingSquares

    ''' <summary>
    ''' 将稀疏矩阵数据转换为稠密矩阵数据
    ''' </summary>
    Public Class MapMatrix

        ''' <summary>
        ''' 插值后得到的稠密矩阵数据
        ''' </summary>
        Friend data As Double(,)
        Friend x_num% = 100
        Friend y_num% = 100

#Region "the input parameters"
        Friend grid_w#
        Friend grid_h#
        Friend w#, h#
#End Region

        ''' <summary>
        ''' 实际的测量结果数据，一般为一个稀疏矩阵
        ''' </summary>
        ReadOnly dots() As MeasureData

        Friend min#
        Friend max#

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="raw">现实世界中的原始测量结果数据</param>
        ''' <param name="size">画布的大小</param>
        ''' <param name="gridSize">网格的大小</param>
        Sub New(raw As IEnumerable(Of MeasureData), size As SizeF, gridSize As SizeF)
            dots = raw.ToArray
            w = size.Width
            h = size.Height
            grid_w = gridSize.Width
            grid_h = gridSize.Height
        End Sub

        Public Function GetLevelQuantile() As QuantileEstimationGK
            Dim data As Double() = dots.Select(Function(t) t.Z).ToArray
            Dim q As QuantileEstimationGK = data.GKQuantile

            Return q
        End Function

        ''' <summary>
        ''' 返回一个稠密状态的结果矩阵
        ''' </summary>
        ''' <returns>
        ''' 以行扫描的方式返回结果
        ''' </returns>
        Public Iterator Function GetMatrixInterpolation() As IEnumerable(Of IntMeasureData())
            Dim x, y As Double
            Dim dx As Double = grid_w
            Dim dy As Double = grid_h
            Dim scan As New List(Of IntMeasureData)

            For i As Integer = 0 To x_num - 1
                For j As Integer = 0 To y_num - 1
                    scan += New IntMeasureData With {
                        .X = x,
                        .Y = y,
                        .Z = data(i, j)
                    }
                    y += dy
                Next

                y = 0
                x = x + dx

                Yield scan.PopAll
            Next
        End Function

        ''' <summary>
        ''' 数据插值
        ''' </summary>
        Friend Function InitData() As MapMatrix
            Dim measure_data = New IntMeasureData(dots.Length - 1) {}
            Dim d As Double

            x_num = CInt(w / grid_w)
            y_num = CInt(h / grid_h)
            data = New Double(x_num - 1, y_num - 1) {}

            For i = dots.Length - 1 To 0 Step -1
                measure_data(i) = New IntMeasureData(dots(i), x_num, y_num)
            Next

            min = Single.MaxValue
            max = Single.MinValue

            For i As Integer = 0 To x_num - 1
                For j As Integer = 0 To y_num - 1
                    Dim value As Single = 0
                    Dim find = False

                    For Each imd As IntMeasureData In measure_data
                        If i = imd.X AndAlso j = imd.Y Then
                            value = imd.Z
                            find = True
                            Exit For
                        End If
                    Next

                    If Not find Then
                        Dim lD As Double = 0
                        Dim DV As Double = 0

                        For Each imd As IntMeasureData In measure_data
                            d = 1.0 / ((imd.X - i) * (imd.X - i) + (imd.Y - j) * (imd.Y - j))
                            lD += d
                            DV += imd.Z * d
                        Next

                        value = CSng(DV / lD)
                    End If

                    data(i, j) = value
                    min = stdNum.Min(min, value)
                    max = stdNum.Max(max, value)
                Next
            Next

            Return Me
        End Function
    End Class
End Namespace
