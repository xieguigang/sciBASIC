#Region "Microsoft.VisualBasic::0f1c2b2debd2f2741ae244538ea696d1, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\MarchingSquares\MapMatrix.vb"

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

    '   Total Lines: 174
    '    Code Lines: 108
    ' Comment Lines: 37
    '   Blank Lines: 29
    '     File Size: 5.84 KB


    '     Class MapMatrix
    ' 
    '         Properties: dimension, size
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetLevelQuantile, GetMatrixInterpolation, (+2 Overloads) GetPercentages, getYScan, interpolate
    '                   interpolateData
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Quantile

Namespace Drawing2D.Math2D.MarchingSquares

    ''' <summary>
    ''' 将稀疏矩阵数据转换为稠密矩阵数据
    ''' </summary>
    Public Class MapMatrix

        ''' <summary>
        ''' 插值后得到的稠密矩阵数据
        ''' </summary>
        Friend data As Double(,)
        ''' <summary>
        ''' 实际的测量结果数据，一般为一个稀疏矩阵
        ''' </summary>
        ReadOnly dots() As MeasureData

        Public ReadOnly Property size As Integer
            Get
                Dim dims = dimension
                Dim l = dims.Width * dims.Height

                Return l
            End Get
        End Property

        Public ReadOnly Property dimension As Size
            Get
                Dim w As Integer = Aggregate p In dots Into Max(p.X)
                Dim h As Integer = Aggregate p In dots Into Max(p.Y)

                Return New Size(w, h)
            End Get
        End Property

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="raw">现实世界中的原始测量结果数据</param>
        Sub New(raw As IEnumerable(Of MeasureData), Optional interpolateFill As Boolean = True)
            dots = raw.ToArray

            ' raw sparse point interpolate
            ' into dense matrix
            Call interpolateData(interpolateFill)
        End Sub

        Public Function GetLevelQuantile() As QuantileEstimationGK
            Dim data As Double() = Me.data.RowIterator.IteratesALL.ToArray
            Dim q As QuantileEstimationGK = data.GKQuantile

            Return q
        End Function

        Public Function GetPercentages() As Double()
            Return GetPercentages(0.05, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 0.95, 0.975)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="levels">``[0,1]``</param>
        ''' <returns></returns>
        Public Function GetPercentages(ParamArray levels As Double()) As Double()
            Dim data As Double() = Me.data _
                .RowIterator _
                .IteratesALL _
                .ToArray
            Dim range As DoubleRange = data
            Dim percentage As New DoubleRange(0, 1)

            Return levels _
                .Select(Function(p)
                            Return percentage.ScaleMapping(p, range)
                        End Function) _
                .ToArray
        End Function

        ''' <summary>
        ''' 返回一个稠密状态的结果矩阵
        ''' </summary>
        ''' <returns>
        ''' 以行扫描的方式返回结果
        ''' </returns>
        Public Iterator Function GetMatrixInterpolation() As IEnumerable(Of Double())
            Dim x, y As Double
            Dim dims As Size = dimension
            Dim scan As New List(Of Double)

            For i As Integer = 0 To dims.Width - 1
                For j As Integer = 0 To dims.Height - 1
                    scan += data(i, j)
                    y += 1
                Next

                y = 0
                x = x + 1

                Yield scan.PopAll
            Next
        End Function

        ''' <summary>
        ''' 数据插值
        ''' </summary>
        Friend Function interpolateData(interpolateFill As Boolean) As MapMatrix
            Dim dims As Size = dimension
            Dim x_num = dims.Width
            Dim y_num = dims.Height

            data = New Double(x_num - 1, y_num - 1) {}

            For i As Integer = 0 To x_num - 1
                For Each j In getYScan(i, y_num, interpolateFill)
                    data(i, j) = j.value
                Next
            Next

            Return Me
        End Function

        Private Function getYScan(i As Integer, y_num As Integer, interpolateFill As Boolean) As IEnumerable(Of SeqValue(Of Double))
            Return y_num.Sequence _
                .AsParallel _
                .Select(Function(j)
                            Return interpolate(i, j, interpolateFill)
                        End Function) _
                .OrderBy(Function(j) j.i)
        End Function

        ''' <summary>
        ''' 进行数据插值
        ''' </summary>
        ''' <param name="i"></param>
        ''' <param name="j"></param>
        ''' <param name="interpolateFill"></param>
        ''' <returns></returns>
        Private Function interpolate(i As Integer, j As Integer, interpolateFill As Boolean) As SeqValue(Of Double)
            Dim value As Single = 0
            Dim find As Boolean = False
            Dim d As Double

            For Each imd As MeasureData In dots
                If i = imd.X AndAlso j = imd.Y Then
                    value = imd.Z
                    find = True
                    Exit For
                End If
            Next

            ' 20220210 进行矩阵插值似乎会导致绘制的图形非常失真？
            If interpolateFill AndAlso Not find Then
                Dim lD As Double = 0
                Dim DV As Double = 0

                For Each imd As MeasureData In dots
                    d = 1.0 / ((imd.X - i) * (imd.X - i) + (imd.Y - j) * (imd.Y - j))
                    lD += d
                    DV += imd.Z * d
                Next

                value = CSng(DV / lD)
            End If

            Return New SeqValue(Of Double)(j, value)
        End Function
    End Class
End Namespace
