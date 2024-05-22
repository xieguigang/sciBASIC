#Region "Microsoft.VisualBasic::58518d4883782fbc211928d0dd89ef0f, gr\Microsoft.VisualBasic.Imaging\Drawing2D\HeatMap\HeatMapRaster.vb"

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

    '   Total Lines: 200
    '    Code Lines: 112 (56.00%)
    ' Comment Lines: 58 (29.00%)
    '    - Xml Docs: 82.76%
    ' 
    '   Blank Lines: 30 (15.00%)
    '     File Size: 6.71 KB


    '     Class HeatMapRaster
    ' 
    '         Properties: HeatMatrix, Kernel
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: (+2 Overloads) GetRasterPixels, MultiplyKernel, SetDataInternal, SetDatas
    ' 
    '         Sub: gaussiankernel
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports std = System.Math

Namespace Drawing2D.HeatMap

    ''' <summary>
    ''' A helper class or produce heatmap raster matrix data
    ''' </summary>
    ''' <remarks>
    ''' https://github.com/RainkLH/HeatMapSharp
    ''' </remarks>
    Public Class HeatMapRaster(Of T As Pixel) : Implements IRasterGrayscaleHeatmap

        ''' <summary>
        ''' gaussian kernel size
        ''' </summary>
        Private gSize As Integer

        ''' <summary>
        ''' gaussian kernel sigma
        ''' </summary>
        Private gSigma As Double

        ''' <summary>
        ''' radius
        ''' </summary>
        Private r As Integer

        ''' <summary>
        ''' Two dimensional matrix corresponding to data list
        ''' 
        ''' the heatmap cells data, transform color scale from
        ''' this matrix data
        ''' </summary>
        Private m_heatMatrix As Double(,)

        ''' <summary>
        ''' gaussian kernel
        ''' </summary>
        Private m_kernelField As Double(,)

        ''' <summary>
        ''' gaussian kernel
        ''' </summary>
        Public ReadOnly Property Kernel As Double(,)
            Get
                Return m_kernelField
            End Get
        End Property

        ''' <summary>
        ''' Two dimensional matrix corresponding to data list
        ''' </summary>
        Public ReadOnly Property HeatMatrix As Double(,)
            Get
                Return m_heatMatrix
            End Get
        End Property

        ''' <summary>
        ''' construction
        ''' </summary>
        ''' <param name="gSize">gaussian kernel size</param>
        ''' <param name="gSigma">gaussian kernel sigma</param>
        Public Sub New(Optional gSize As Integer = 3, Optional gSigma As Double = 64)
            ' 对高斯核尺寸进行判断
            If gSize < 3 OrElse gSize > 400 Then
                Throw New Exception("Kernel size is invalid")
            End If

            Me.gSize = If(gSize Mod 2 = 0, gSize + 1, gSize)
            '高斯的sigma值，计算半径r
            Me.r = Me.gSize / 2
            Me.gSigma = gSigma
            '计算高斯核
            Me.m_kernelField = New Double(Me.gSize - 1, Me.gSize - 1) {}

            Call gaussiankernel()
        End Sub

        Private Sub gaussiankernel()
            Dim y = -r, i = 0
            Dim sigma_2 As Double = gSigma ^ 2

            While i < gSize
                Dim x = -r, j = 0

                While j < gSize
                    m_kernelField(i, j) = std.Exp((x * x + y * y) / (-2 * sigma_2)) / (2 * std.PI * sigma_2)
                    x += 1
                    j += 1
                End While

                y += 1
                i += 1
            End While
        End Sub

        Private Function MultiplyKernel(weight As Double) As Double(,)
            Dim wKernel As Double(,) = CType(m_kernelField.Clone(), Double(,))
            For i As Integer = 0 To gSize - 1
                For j As Integer = 0 To gSize - 1
                    wKernel(i, j) *= weight
                Next
            Next
            Return wKernel
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function SetDatas(datas As IEnumerable(Of T)) As HeatMapRaster(Of T)
            Return SetDataInternal(datas.ToArray)
        End Function

        ''' <summary>
        ''' set imaging raster pixels data to generate heatmap matrix
        ''' </summary>
        ''' <param name="datas"></param>
        Private Function SetDataInternal(datas As T()) As HeatMapRaster(Of T)
            Dim poly As New Polygon2D(datas.Select(Function(p) New PointF(p.X, p.Y)).ToArray)
            Dim hField As Integer = poly.ypoints.Max
            Dim wField As Integer = poly.xpoints.Max

            ' 初始化高斯累加图
            m_heatMatrix = New Double(hField - 1, wField - 1) {}

            For Each data As Pixel In RasterMatrix.PopulateDenseRasterMatrix(datas, wField, hField)
                Dim i, j, tx, ty, ir, jr As Integer
                Dim radius = gSize >> 1
                Dim x = data.X
                Dim y = data.Y
                Dim kernelMultiplied = MultiplyKernel(data.Scale)

                For i = 0 To gSize - 1
                    ir = i - radius
                    ty = y + ir

                    ' skip row
                    If ty < 0 Then
                        Continue For
                    End If

                    ' break Height
                    If ty >= hField Then
                        Exit For
                    End If

                    ' for each kernel column
                    For j = 0 To gSize - 1
                        jr = j - radius
                        tx = x + jr

                        ' skip column
                        If tx < 0 Then
                            Continue For
                        End If

                        If tx < wField Then
                            m_heatMatrix(ty, tx) += kernelMultiplied(i, j)
                        End If
                    Next
                Next
            Next

            Return Me
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="activator">
        ''' a delegate function for create the pixel spot data object
        ''' </param>
        ''' <returns></returns>
        Public Iterator Function GetRasterPixels(activator As Func(Of Integer, Integer, Double, T)) As IEnumerable(Of T)
            For i = 0 To m_heatMatrix.GetLength(0) - 1
                For j = 0 To m_heatMatrix.GetLength(1) - 1
                    Yield activator(j, i, m_heatMatrix(i, j))
                Next
            Next
        End Function

        ''' <summary>
        ''' populate out the default internal <see cref="PixelData"/> model
        ''' </summary>
        ''' <returns></returns>
        Public Iterator Function GetRasterPixels() As IEnumerable(Of Pixel) Implements IRasterGrayscaleHeatmap.GetRasterPixels
            For i = 0 To m_heatMatrix.GetLength(0) - 1
                For j = 0 To m_heatMatrix.GetLength(1) - 1
                    Yield New PixelData With {
                        .X = j,
                        .Y = i,
                        .Scale = m_heatMatrix(i, j)
                    }
                Next
            Next
        End Function
    End Class
End Namespace
