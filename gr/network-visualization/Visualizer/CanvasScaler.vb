#Region "Microsoft.VisualBasic::e795e0327d2f449f9e908c37ba300aee, gr\network-visualization\Visualizer\CanvasScaler.vb"

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

    ' Module CanvasScaler
    ' 
    '     Function: CalculateNodePositions, CentralOffsets
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

''' <summary>
''' 进行网络模型之中的节点的位置缩放以及中心化平移操作的帮助模块
''' </summary>
''' <remarks>
''' 计算节点在画布上面的正确的位置操作：
''' 
''' 1. 计算整个网络图形的边界
''' 2. 计算出缩放因子
''' 3. 执行位置的缩放
''' 4. 计算出中心点平移的偏移值
''' 5. 执行中心点平移
''' 6. 完成节点的位置计算操作
''' </remarks>
Public Module CanvasScaler

    '<Extension>
    'Public Function CalculateEdgeBends(net As NetworkGraph, frameSize As SizeF, padding As Padding) As Dictionary(Of Edge, PointF())
    '    Dim edgeBundling As New Dictionary(Of Edge, PointF())
    '    Dim scaleFactor As SizeF = Nothing
    '    Dim centraOffset As PointF = Nothing

    '    Call net.CalculateNodePositions(frameSize, padding, scaleFactor, centraOffset)
    '    Call $"Scale factor of polygon shape: [{scaleFactor.Width}, {scaleFactor.Height}]".__DEBUG_ECHO
    '    Call $"centraOffset of polygon shape: [{centraOffset.X}, {centraOffset.Y}]".__DEBUG_ECHO

    '    ' 1. 先做缩放
    '    Dim edges As Edge() = net.graphEdges _
    '        .Where(Function(e)
    '                   ' 空集合会在下面的分割for循环中产生移位bug
    '                   ' 跳过
    '                   Return Not e.data.bends.IsNullOrEmpty
    '               End Function) _
    '        .ToArray
    '    Dim edgeBundlingShape As PointF() = edges _
    '        .Select(Function(e) e.data.bends) _
    '        .IteratesALL _
    '        .Select(Function(v) New PointF With {.X = v.x, .Y = v.y}) _
    '        .ToArray
    '    Dim scale = (CDbl(scaleFactor.Width), CDbl(scaleFactor.Height))

    '    If edgeBundlingShape.Length > 0 Then
    '        Dim pointList As New List(Of PointF)
    '        Dim i As Integer

    '        edgeBundlingShape = edgeBundlingShape.Enlarge(scale)

    '        For Each edge As Edge In edges
    '            For Each null In edge.data.bends
    '                ' 20191103
    '                ' 在这里因为每一个edge的边连接点的数量是不一样的
    '                ' 所以在这里使用for loop加上递增序列来
    '                ' 正确的获取得到每一条边所对应的边连接节点
    '                pointList += edgeBundlingShape(i).OffSet2D(centraOffset)
    '                i += 1
    '            Next

    '            edgeBundling(edge) = pointList
    '            pointList *= 0
    '        Next
    '    End If

    '    Return edgeBundling
    'End Function

    ''' <summary>
    ''' <see cref="Node.label"/>
    ''' </summary>
    ''' <param name="net"><see cref="Node.label"/></param>
    ''' <param name="frameSize"></param>
    ''' <param name="padding"></param>
    ''' <returns></returns>
    <Extension>
    Public Function CalculateNodePositions(net As NetworkGraph, frameSize As SizeF, padding As Padding,
                                           Optional ByRef scaleFactor As SizeF = Nothing,
                                           Optional ByRef centraOffset As PointF = Nothing) As Dictionary(Of String, PointF)

        Dim points As Dictionary(Of String, PointF) = net.vertex _
            .ToDictionary(Function(n) n.label,
                          Function(n)
                              Return New PointF With {
                                  .X = n.data.initialPostion.x,
                                  .Y = n.data.initialPostion.y
                              }
                          End Function)
        ' 3. 执行缩放
        Dim keys As String() = points.Keys.ToArray
        Dim polygon As PointF() = keys _
            .Select(Function(i) points(i)) _
            .ToArray _
            .ScalePoints(
                frameSize:=frameSize,
                padding:=padding,
                scaleFactor:=scaleFactor,
                centraOffset:=centraOffset
            )

        For i As Integer = 0 To keys.Length - 1
            points(keys(i)) = polygon(i)
        Next

        ' 6. 完成节点的位置计算操作
        '    返回节点位置结果
        Return points
    End Function

    ''' <summary>
    ''' 这里是计算出网络几点偏移到图像的中心所需要的偏移量
    ''' </summary>
    ''' <param name="nodes"></param>
    ''' <param name="size"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function CentralOffsets(nodes As Dictionary(Of Node, PointF), size As SizeF) As PointF
        Return nodes.Values.CentralOffset(size)
    End Function
End Module
