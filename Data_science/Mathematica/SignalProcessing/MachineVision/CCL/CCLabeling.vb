#Region "Microsoft.VisualBasic::159dc3c420bce1628754b9de95801d69, Data_science\Mathematica\SignalProcessing\MachineVision\CCL\CCLabeling.vb"

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

    '   Total Lines: 288
    '    Code Lines: 191 (66.32%)
    ' Comment Lines: 51 (17.71%)
    '    - Xml Docs: 41.18%
    ' 
    '   Blank Lines: 46 (15.97%)
    '     File Size: 11.60 KB


    '     Class CCLabeling
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: AggregatePatterns, Find, FindRoot, GetNeighboringLabels, Process
    '                   TwoPassProcess
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Math2D

Namespace CCL

    ''' <summary>
    ''' Connected Component Labeling
    ''' </summary>
    Public Class CCLabeling

        Private _board As Integer(,)
        Private _input As BitmapBuffer
        Private _width As Integer
        Private _height As Integer

        Private Sub New()
        End Sub

        ''' <summary>
        ''' 使用Two-Pass算法处理图像
        ''' </summary>
        ''' <param name="input">输入图像缓冲区</param>
        ''' <param name="background">背景颜色（通常为白色）</param>
        ''' <param name="tolerance">颜色容差</param>
        ''' <returns>标记后的多边形列表</returns>
        Public Shared Function TwoPassProcess(input As BitmapBuffer,
                                         Optional background As Color = Nothing,
                                         Optional tolerance As Integer = 30) As List(Of Polygon2D)
            ' 初始化背景颜色
            If background = Nothing Then background = Color.White

            ' 获取图像尺寸
            Dim width As Integer = input.Width
            Dim height As Integer = input.Height

            ' 创建标签矩阵
            Dim labels(width - 1, height - 1) As Integer
            Dim currentLabel As Integer = 1
            Dim labelEquivalences As New Dictionary(Of Integer, List(Of Integer))

            ' 第一遍扫描：分配临时标签并记录等价关系
            For y As Integer = 0 To height - 1
                For x As Integer = 0 To width - 1
                    ' 检查是否为背景
                    If input.GetPixel(x, y).Equals(background, tolerance:=tolerance) Then
                        labels(x, y) = 0
                        Continue For
                    End If

                    ' 获取邻域标签
                    Dim neighbors As New List(Of Integer)
                    ' 检查上方像素
                    If y > 0 Then neighbors.Add(labels(x, y - 1))
                    ' 检查左方像素
                    If x > 0 Then neighbors.Add(labels(x - 1, y))
                    ' 检查左上方像素
                    If x > 0 AndAlso y > 0 Then neighbors.Add(labels(x - 1, y - 1))
                    ' 检查右上方像素
                    If x < width - 1 AndAlso y > 0 Then neighbors.Add(labels(x + 1, y - 1))

                    ' 移除背景标签(0)和重复标签
                    neighbors = neighbors.Where(Function(l) l > 0).Distinct().ToList()

                    If neighbors.Count = 0 Then
                        ' 没有邻域标签，分配新标签
                        labels(x, y) = currentLabel
                        currentLabel += 1
                    Else
                        ' 使用最小邻域标签
                        Dim minLabel As Integer = neighbors.Min()
                        labels(x, y) = minLabel

                        ' 记录等价关系
                        For Each label In neighbors
                            If label <> minLabel Then
                                If Not labelEquivalences.ContainsKey(minLabel) Then
                                    labelEquivalences(minLabel) = New List(Of Integer)
                                End If
                                If Not labelEquivalences(minLabel).Contains(label) Then
                                    labelEquivalences(minLabel).Add(label)
                                End If
                            End If
                        Next
                    End If
                Next
            Next

            ' 解析等价关系，建立并查集
            Dim parent As New Dictionary(Of Integer, Integer)
            For i As Integer = 1 To currentLabel - 1
                parent(i) = i
            Next

            ' 处理等价关系
            For Each kvp In labelEquivalences
                Dim root As Integer = FindRoot(kvp.Key, parent)
                For Each label In kvp.Value
                    Dim otherRoot As Integer = FindRoot(label, parent)
                    If root <> otherRoot Then
                        ' 合并集合
                        If root < otherRoot Then
                            parent(otherRoot) = root
                        Else
                            parent(root) = otherRoot
                        End If
                    End If
                Next
            Next

            ' 第二遍扫描：替换为最终标签
            Dim labelPoints As New Dictionary(Of Integer, List(Of Point))
            For y As Integer = 0 To height - 1
                For x As Integer = 0 To width - 1
                    If labels(x, y) > 0 Then
                        Dim root As Integer = FindRoot(labels(x, y), parent)
                        labels(x, y) = root

                        ' 收集点
                        If Not labelPoints.ContainsKey(root) Then
                            labelPoints(root) = New List(Of Point)
                        End If
                        labelPoints(root).Add(New Point(x, y))
                    End If
                Next
            Next

            ' 创建多边形
            Dim polygons As New List(Of Polygon2D)
            For Each kvp In labelPoints
                If kvp.Value.Count > 10 Then ' 过滤小区域
                    Dim points As List(Of Point) = kvp.Value
                    Dim polygon As New Polygon2D(points.Select(Function(p) p.X).ToArray(),
                                             points.Select(Function(p) p.Y).ToArray())
                    polygons.Add(polygon)
                End If
            Next

            Return polygons
        End Function

        ''' <summary>
        ''' 查找根节点（带路径压缩）
        ''' </summary>
        Private Shared Function FindRoot(label As Integer, parent As Dictionary(Of Integer, Integer)) As Integer
            If parent(label) <> label Then
                parent(label) = FindRoot(parent(label), parent)
            End If
            Return parent(label)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="input"></param>
        ''' <param name="background">
        ''' the background color usually be white color
        ''' </param>
        ''' <param name="tolerance"></param>
        ''' <returns></returns>
        Public Shared Iterator Function Process(input As BitmapBuffer,
                                       Optional background As Color? = Nothing,
                                       Optional tolerance As Integer = 3) As IEnumerable(Of Polygon2D)

            Dim ccl As New CCLabeling With {
                ._input = input,
                ._width = input.Width,
                ._height = input.Height,
                ._board = New Integer(._width - 1, ._height - 1) {}
            }

            If background Is Nothing Then
                background = Color.White
            End If

            Dim patterns As Dictionary(Of Integer, List(Of Point)) = ccl.Find(background, tolerance)

            For Each pattern In patterns
                With pattern.Value
                    ' get x and y axis data from the point collection
                    ' for contruct a 2d polygon shape
                    ' as the label target
                    Yield New Polygon2D(.X, .Y)
                End With
            Next
        End Function

        Private Function Find(background As Color, tolerance As Integer) As Dictionary(Of Integer, List(Of Point))
            Dim labelCount As Integer = 1
            Dim allLabels As New Dictionary(Of Integer, Label)()

            For i = 0 To _height - 1
                For j = 0 To _width - 1
                    Dim color = _input.GetPixel(j, i)

                    If color.Equals(background, tolerance:=tolerance) Then
                        Continue For
                    End If

                    Dim currentPixel As New Point(j, i)
                    Dim neighboringLabels = GetNeighboringLabels(currentPixel)
                    Dim currentLabel As Integer

                    If Not neighboringLabels.Any() Then
                        currentLabel = labelCount
                        allLabels.Add(currentLabel, New Label(currentLabel))
                        labelCount += 1
                    Else
                        Dim rootLabels = neighboringLabels.Select(Function(l) allLabels(l).GetRoot()).ToList()
                        currentLabel = rootLabels.Min(Function(r) r.Name)
                        Dim currentRoot As Label = allLabels(currentLabel).GetRoot()

                        ' 合并所有其他根标签到当前根标签
                        For Each root In rootLabels.Distinct
                            If root.Name <> currentRoot.Name Then
                                root.Join(currentRoot)
                            End If
                        Next
                    End If

                    _board(j, i) = currentLabel
                Next
            Next


            Dim patterns = AggregatePatterns(allLabels)

            Return patterns
        End Function

        Private Function GetNeighboringLabels(pix As Point) As IEnumerable(Of Integer)
            Dim neighboringLabels = New List(Of Integer)()
            Dim x = pix.X
            Dim y = pix.Y

            ' 检查左上 (x-1, y-1)
            If x > 0 AndAlso y > 0 Then
                Dim label = _board(x - 1, y - 1)
                If label <> 0 Then neighboringLabels.Add(label)
            End If

            ' 检查上 (x, y-1)
            If y > 0 Then
                Dim label = _board(x, y - 1)
                If label <> 0 Then neighboringLabels.Add(label)
            End If

            ' 检查右上 (x+1, y-1)
            If x < _width - 1 AndAlso y > 0 Then
                Dim label = _board(x + 1, y - 1)
                If label <> 0 Then neighboringLabels.Add(label)
            End If

            ' 检查左 (x-1, y)
            If x > 0 Then
                Dim label = _board(x - 1, y)
                If label <> 0 Then neighboringLabels.Add(label)
            End If

            Return neighboringLabels
        End Function

        Private Function AggregatePatterns(allLabels As Dictionary(Of Integer, Label)) As Dictionary(Of Integer, List(Of Point))
            Dim patterns = New Dictionary(Of Integer, List(Of Point))()

            For i = 0 To _height - 1
                For j = 0 To _width - 1
                    Dim patternNumber = _board(j, i)

                    If patternNumber <> 0 Then
                        ' 获取根标签
                        Dim rootLabel = allLabels(patternNumber).GetRoot()
                        Dim rootName = rootLabel.Name

                        If Not patterns.ContainsKey(rootName) Then
                            patterns(rootName) = New List(Of Point)()
                        End If

                        patterns(rootName).Add(New Point(j, i))
                    End If
                Next
            Next

            Return patterns
        End Function
    End Class
End Namespace

