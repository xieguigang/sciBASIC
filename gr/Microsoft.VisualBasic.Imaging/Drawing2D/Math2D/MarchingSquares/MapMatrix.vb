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
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
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
        ''' <summary>
        ''' 实际的测量结果数据，一般为一个稀疏矩阵
        ''' </summary>
        ReadOnly dots() As MeasureData

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
        Sub New(raw As IEnumerable(Of MeasureData))
            dots = raw.ToArray
            InitData()
        End Sub

        Public Function GetLevelQuantile() As QuantileEstimationGK
            Dim data As Double() = Me.data.RowIterator.IteratesALL.ToArray
            Dim q As QuantileEstimationGK = data.GKQuantile

            Return q
        End Function

        Public Function GetPercentages() As Double()
            Dim data As Double() = Me.data _
                .RowIterator _
                .IteratesALL _
                .ToArray
            Dim range As DoubleRange = data
            Dim percentage As DoubleRange = {0, 1}

            Return {0.05, 0.1, 0.2, 0.3, 0.4, 0.5, 0.6, 0.7, 0.8, 0.9, 0.95, 0.975} _
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
        Friend Function InitData() As MapMatrix
            Dim dims = dimension
            Dim x_num = dims.Width
            Dim y_num = dims.Height

            data = New Double(x_num - 1, y_num - 1) {}

            For i As Integer = 0 To x_num - 1
                For Each j In getYScan(i, y_num)
                    data(i, j) = j.value
                Next
            Next

            Return Me
        End Function

        Private Function getYScan(i As Integer, y_num As Integer) As IEnumerable(Of SeqValue(Of Double))
            Return y_num.Sequence _
                .AsParallel _
                .Select(Function(j)
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

                            If Not find Then
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
                        End Function) _
                .OrderBy(Function(j) j.i)
        End Function
    End Class
End Namespace
