Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.d3js.Layout
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Math2D.ConvexHull
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.MIME.Markup.HTML
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization.JSON
Imports stdNum = System.Math

''' <summary>
''' 进行网络模型之中的节点的位置缩放以及中心化平移操作的帮助模块
''' </summary>
Public Module CanvasScaler

    ''' <summary>
    ''' 这里是计算出网络几点偏移到图像的中心所需要的偏移量
    ''' </summary>
    ''' <param name="nodes"></param>
    ''' <param name="size"></param>
    ''' <returns></returns>
    <Extension>
    Public Function CentralOffsets(nodes As Dictionary(Of Node, PointF), size As SizeF) As PointF
        Return nodes.Values.CentralOffset(size)
    End Function

    <Extension>
    Private Function scales(nodes As IEnumerable(Of Node), scale As SizeF) As Dictionary(Of Node, Point)
        Dim table As New Dictionary(Of Node, Point)

        For Each n As Node In nodes
            With n.data.initialPostion.Point2D
                Call table.Add(n, New Point(.X * scale.Width, .Y * scale.Height))
            End With
        Next

        Return table
    End Function

    <Extension>
    Public Function GetBounds(graph As NetworkGraph) As RectangleF
        Dim points As Point() = graph _
            .vertex _
            .scales(scale:=New SizeF(1, 1)) _
            .Values _
            .ToArray
        Dim rect = points.GetBounds
        Return rect
    End Function

    <Extension>
    Public Function AutoScaler(graph As NetworkGraph, frameSize As Size, padding As Padding) As SizeF
        With graph.GetBounds
            Return New SizeF(
                frameSize.Width / (.Width + padding.Horizontal),
                frameSize.Height / (.Height + padding.Vertical)
            )
        End With
    End Function

    <Extension>
    Public Function AutoScaler(shape As IEnumerable(Of PointF), frameSize As SizeF, padding As Padding) As SizeF
        With shape.GetBounds
            Dim width = frameSize.Width - padding.Horizontal
            Dim height = frameSize.Height - padding.Vertical

            Return New SizeF(
                width:=width / .Width,
                height:=height / .Height
            )
        End With
    End Function
End Module
