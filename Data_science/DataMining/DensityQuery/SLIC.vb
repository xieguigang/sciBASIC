#Region "Microsoft.VisualBasic::e4e658f066eb6328a95646a96d5975ce, Data_science\DataMining\DensityQuery\SLIC.vb"

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
    '    Code Lines: 131 (75.29%)
    ' Comment Lines: 19 (10.92%)
    '    - Xml Docs: 73.68%
    ' 
    '   Blank Lines: 24 (13.79%)
    '     File Size: 7.04 KB


    ' Class SLIC
    ' 
    '     Constructor: (+2 Overloads) Sub New
    ' 
    '     Function: InitializeCenters, Mean, MeasureSegments, ReadImagePixels
    ' 
    '     Sub: IterateClustering
    ' 
    ' Class SLICPixel
    ' 
    '     Properties: cluster, color, x, y
    ' 
    '     Function: DistanceTo
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar.Tqdm
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Correlations
Imports std = System.Math

''' <summary>
''' SLIC (Simple Linear Iterative Clustering) clusters pixels using pixel channels and image plane space
''' to efficiently generate compact, nearly uniform superpixels. The simplicity of approach makes it
''' extremely easy To use a lone parameter specifies the number Of superpixels And the efficiency Of
''' the algorithm makes it very practical.
''' </summary>
Public Class SLIC

    ReadOnly bitmap As SLICPixel()
    ReadOnly width As Integer

    ''' <summary>
    ''' argb
    ''' </summary>
    ''' <param name="bitmap"></param>
    Sub New(bitmap As BitmapBuffer)
        Me.width = 4
        Me.bitmap = ReadImagePixels(bitmap).ToArray
    End Sub

    Sub New(spatial As IEnumerable(Of SLICPixel))
        Me.bitmap = spatial.ToArray
        Me.width = bitmap(0).color.Length
    End Sub

    Public Shared Iterator Function ReadImagePixels(bitmap As BitmapBuffer) As IEnumerable(Of SLICPixel)
        Dim width As Integer = bitmap.Width
        Dim height As Integer = bitmap.Height

        For y As Integer = 0 To height - 1
            For x As Integer = 0 To width - 1
                Dim color As Color = bitmap.GetPixel(x, y)

                Yield New SLICPixel With {
                    .x = x,
                    .y = y,
                    .color = {color.A, color.R, color.G, color.B},
                    .cluster = -1
                }
            Next
        Next
    End Function

    Public Iterator Function InitializeCenters(pixels As SLICPixel(), regionSize As Integer) As IEnumerable(Of SLICPixel)
        Dim width As Integer = pixels.Max(Function(p) p.x) + 1
        Dim height As Integer = pixels.Max(Function(p) p.y) + 1
        Dim numClustersX As Integer = width \ regionSize
        Dim numClustersY As Integer = height \ regionSize

        For iy As Integer = 0 To numClustersY - 1
            For ix As Integer = 0 To numClustersX - 1
                Dim x As Integer = ix * regionSize + regionSize \ 2
                Dim y As Integer = iy * regionSize + regionSize \ 2
                Dim pixel As SLICPixel = pixels.FirstOrDefault(Function(p) p.x = x AndAlso p.y = y)
                If pixel IsNot Nothing Then
                    Yield pixel
                Else
                    ' 如果像素点不存在，选择附近的像素点
                    Dim found As Boolean = False
                    For dy As Integer = -1 To 1
                        For dx As Integer = -1 To 1
                            Dim px As Integer = x + dx
                            Dim py As Integer = y + dy
                            If px >= 0 AndAlso px < width AndAlso py >= 0 AndAlso py < height Then
                                pixel = pixels.FirstOrDefault(Function(p) p.x = px AndAlso p.y = py)
                                If pixel IsNot Nothing Then
                                    Yield pixel
                                    found = True
                                    Exit For
                                End If
                            End If
                        Next
                        If found Then Exit For
                    Next
                    If Not found Then
                        ' 如果仍然找不到，选择第一个像素点
                        Yield pixels.First()
                    End If
                End If
            Next
        Next
    End Function

    Public Function MeasureSegments(regionSize As Integer, numIterations As Integer) As SLICPixel()
        Dim centers As List(Of SLICPixel) = InitializeCenters(bitmap, regionSize).ToList

        For Each i As Integer In TqdmWrapper.Range(0, numIterations, wrap_console:=App.EnableTqdm)
            Call IterateClustering(bitmap, centers, regionSize)
        Next

        Return bitmap
    End Function

    Public Sub IterateClustering(pixels As SLICPixel(), centers As List(Of SLICPixel), regionSize As Integer)
        ' 为每个像素点找到最近的聚类中心
        For Each pixel As SLICPixel In pixels
            Dim closest = centers.SeqIterator _
                .AsParallel _
                .Select(Function(a)
                            Return (pixel.DistanceTo(a, regionSize), a.i)
                        End Function) _
                .OrderBy(Function(d) d.Item1) _
                .First
            Dim closestCenter As Integer = closest.i

            pixel.cluster = closestCenter
        Next

        ' 更新聚类中心的位置
        Dim newCenters As New List(Of SLICPixel)

        For i As Integer = 0 To centers.Count - 1
            Dim offset As Integer = i
            Dim clusterPixels As List(Of SLICPixel) = pixels.AsParallel.Where(Function(p) p.cluster = offset).ToList()
            If clusterPixels.Count > 0 Then
                Dim centerX As Single = clusterPixels.Average(Function(p) p.x)
                Dim centerY As Single = clusterPixels.Average(Function(p) p.y)
                Dim centerColor As Double() = Mean(clusterPixels, width)
                Dim centerPixel As New SLICPixel With {
                    .x = CInt(std.Round(centerX)),
                    .y = CInt(std.Round(centerY)),
                    .color = centerColor,
                    .cluster = i
                }
                newCenters.Add(centerPixel)
            Else
                ' 如果聚类中没有像素点，保留原来的中心
                newCenters.Add(centers(i))
            End If
        Next

        centers.Clear()
        centers.AddRange(newCenters)
    End Sub

    Private Shared Function Mean(clusterPixels As IEnumerable(Of SLICPixel), width As Integer) As Double()
        Dim v As Double() = New Double(width - 1) {}
        Dim n As Integer = 0

        For Each pixel As SLICPixel In clusterPixels
            v = SIMD.Add.f64_op_add_f64(v, pixel.color)
            n += 1
        Next

        Return SIMD.Divide.f64_op_divide_f64_scalar(v, n)
    End Function

End Class

Public Class SLICPixel

    Public Property x As Integer
    Public Property y As Integer
    ''' <summary>
    ''' a general vector for store the pixel color data in multiple data mode
    ''' </summary>
    ''' <returns></returns>
    Public Property color As Double()
    Public Property cluster As Integer

    Public Function DistanceTo(p2 As SLICPixel, regionSize As Integer) As Single
        Dim spatialDistance As Single = std.Sqrt((x - p2.x) ^ 2 + (y - p2.y) ^ 2)
        Dim colorDistance As Single = color.EuclideanDistance(p2.color)
        Return spatialDistance / regionSize + colorDistance
    End Function
End Class
