#Region "Microsoft.VisualBasic::3e7c1a9d5b2f4806c8e3a1d7b4f9c2e5, Data_science\Visualization\DataPlot\Statistics\ClusterTree.vb"

    ' 
    '       sciBASIC.NET Foundation, GPL3 Licensed
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.

    ' Module ClusterTree
    ' 
    '     Function: Draw, MeasureLeafLayout
    '     Enum TreeOrientation
    ' 
#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports std = System.Math

' ============================================================================
'  ClusterTree.vb - 层次聚类树的绘制
'
'  聚类算法本身在调用方完成，绘制只消费 <see cref="ClusterTreeNode"/>。
'  这里的实现被聚类热图、树+堆叠柱共用，后续 HCTreePlot 的树面板也可以接到同一份代码上。
' ============================================================================

''' <summary>层次聚类树绘制</summary>
Public Module ClusterTree

    ''' <summary>树的生长方向</summary>
    Public Enum TreeOrientation
        ''' <summary>根在左、叶子在右（行聚类树，常放在热图左侧）</summary>
        LeftToRight
        ''' <summary>根在上、叶子在下（列聚类树，常放在热图上方）</summary>
        TopToBottom
    End Enum

    ''' <summary>
    ''' 在给定矩形内画出聚类树，并返回每个叶子名称在「叶子轴」上的像素坐标。
    ''' 调用方拿到这份映射后，就能把热图的行（或列）与叶子的位置对齐。
    ''' </summary>
    ''' <param name="g">绘图设备</param>
    ''' <param name="root">树根</param>
    ''' <param name="rect">可绘制区域</param>
    ''' <param name="orientation">树的朝向</param>
    ''' <param name="theme">主题</param>
    ''' <param name="pen">连线画笔</param>
    ''' <returns>叶子名 -> 叶子轴坐标</returns>
    Public Function Draw(g As IGraphics, root As ClusterTreeNode, rect As RectangleF,
                         orientation As TreeOrientation, theme As PlotTheme, pen As Pen) As Dictionary(Of String, Single)
        If root Is Nothing Then Throw New ArgumentNullException(NameOf(root))

        Dim leaves = root.Leaves()
        If leaves Is Nothing OrElse leaves.Length = 0 Then Return New Dictionary(Of String, Single)()

        Dim maxHeight = MaxDistance(root)
        If maxHeight <= 0 Then maxHeight = 1

        Dim positions As New Dictionary(Of String, Single)()
        Dim cursor As Integer = 0
        Dim nLeaf = leaves.Length

        ' 叶子轴的步长：留出半个 NSNumber 的边距，让首尾叶子不贴边
        Dim spread = If(orientation = TreeOrientation.LeftToRight, rect.Height, rect.Width)
        Dim stepLen As Single = CSng(spread / nLeaf)
        Dim baseLeaf = If(orientation = TreeOrientation.LeftToRight, rect.Top, rect.Left)

        LayoutAndDraw(g, root, rect, orientation, maxHeight, baseLeaf, stepLen, cursor, positions, pen)

        Return positions
    End Function

    Private Function MaxDistance(node As ClusterTreeNode) As Double
        If node Is Nothing Then Return 0
        Return std.Max(node.Height, std.Max(MaxDistance(node.Left), MaxDistance(node.Right)))
    End Function

    ''' <summary>递归布局并绘制直角枝杈，同时登记每个叶子的坐标</summary>
    Private Function LayoutAndDraw(g As IGraphics, node As ClusterTreeNode, rect As RectangleF,
                                   orientation As TreeOrientation, maxHeight As Double,
                                   baseLeaf As Single, stepLen As Single, ByRef cursor As Integer,
                                   positions As Dictionary(Of String, Single), pen As Pen) As Single
        Dim horizontal = orientation = TreeOrientation.LeftToRight
        Dim depth As Single = CSng(node.Height / maxHeight)
        Dim leafCoord As Single

        If node.IsLeaf Then
            leafCoord = baseLeaf + (cursor + 0.5F) * stepLen
            cursor += 1

            If Not String.IsNullOrEmpty(node.Name) Then positions(node.Name) = leafCoord
        Else
            Dim a = LayoutAndDraw(g, node.Left, rect, orientation, maxHeight, baseLeaf, stepLen, cursor, positions, pen)
            Dim b = LayoutAndDraw(g, node.Right, rect, orientation, maxHeight, baseLeaf, stepLen, cursor, positions, pen)

            leafCoord = (a + b) / 2

            If horizontal Then
                ' 分支沿 X 轴推进，两子树的枝干在 leafCoord 上汇合
                Dim branchX = rect.Right - depth * rect.Width
                Dim childX = rect.Right - CSng(node.Left.Height / maxHeight) * rect.Width
                Dim childX2 = rect.Right - CSng(node.Right.Height / maxHeight) * rect.Width

                g.DrawLine(pen, branchX, leafCoord, branchX, a)
                g.DrawLine(pen, branchX, leafCoord, branchX, b)
                g.DrawLine(pen, branchX, a, childX, a)
                g.DrawLine(pen, branchX, b, childX2, b)
            Else
                Dim branchY = rect.Top + depth * rect.Height
                Dim childY = rect.Top + CSng(node.Left.Height / maxHeight) * rect.Height
                Dim childY2 = rect.Top + CSng(node.Right.Height / maxHeight) * rect.Height

                g.DrawLine(pen, leafCoord, branchY, a, branchY)
                g.DrawLine(pen, leafCoord, branchY, b, branchY)
                g.DrawLine(pen, a, branchY, a, childY)
                g.DrawLine(pen, b, branchY, b, childY2)
            End If
        End If

        Return leafCoord
    End Function
End Module
