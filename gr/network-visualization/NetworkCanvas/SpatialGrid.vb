#Region "Microsoft.VisualBasic::5bd00c9fcce5c9fec9efb2490a18b247, gr\network-visualization\NetworkCanvas\SpatialGrid.vb"

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

    '   Total Lines: 89
    '    Code Lines: 52 (58.43%)
    ' Comment Lines: 22 (24.72%)
    '    - Xml Docs: 59.09%
    ' 
    '   Blank Lines: 15 (16.85%)
    '     File Size: 3.38 KB


    ' Class SpatialGrid
    ' 
    '     Function: Query
    ' 
    '     Sub: Build
    ' 
    ' /********************************************************************************/

#End Region

' /********************************************************************************/
'
' 均匀网格空间索引
'
' 用于悬停/选中的命中测试：将节点按屏幕坐标分桶，使查询从 O(N) 线性扫描
' 降为近似 O(1)。由于物理布局在持续运动，网格在每次命中测试时基于当前
' 屏幕位置重建（O(N)），但查询本身只检查邻近格子，对“数千节点”规模高效。
'
' /********************************************************************************/

Imports System.Collections.Generic
Imports System.Drawing
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

''' <summary>
''' 屏幕空间均匀网格，加速鼠标命中测试。
''' </summary>
Public Class SpatialGrid

    Private cells As New Dictionary(Of Point, List(Of Node))
    Private cellSize As Single

    ''' <summary>
    ''' 以当前屏幕位置构建网格。
    ''' </summary>
    ''' <param name="nodes">所有节点</param>
    ''' <param name="toScreen">节点 -> 屏幕坐标</param>
    ''' <param name="radiusOf">节点 -> 屏幕半径（已含缩放）</param>
    ''' <param name="cellSize">格子边长（像素），过小会自动放大</param>
    Public Sub Build(nodes As IEnumerable(Of Node),
                     toScreen As Func(Of Node, PointF),
                     radiusOf As Func(Of Node, Single),
                     cellSize As Single)
        cells.Clear()
        Me.cellSize = System.Math.Max(20.0F, cellSize)

        For Each n In nodes
            Dim p As PointF = toScreen(n)
            Dim key As New Point(CInt(System.Math.Floor(p.X / Me.cellSize)),
                                 CInt(System.Math.Floor(p.Y / Me.cellSize)))

            If Not cells.ContainsKey(key) Then
                cells(key) = New List(Of Node)
            End If

            cells(key).Add(n)
        Next
    End Sub

    ''' <summary>
    ''' 返回包含屏幕点 pt 的最近命中节点；未命中返回 Nothing。
    ''' </summary>
    Public Function Query(pt As PointF,
                          toScreen As Func(Of Node, PointF),
                          radiusOf As Func(Of Node, Single)) As Node
        Dim cx As Integer = CInt(System.Math.Floor(pt.X / cellSize))
        Dim cy As Integer = CInt(System.Math.Floor(pt.Y / cellSize))

        Dim best As Node = Nothing
        Dim bestD As Single = Single.MaxValue

        For dx As Integer = -1 To 1
            For dy As Integer = -1 To 1
                Dim key As New Point(cx + dx, cy + dy)

                If Not cells.ContainsKey(key) Then
                    Continue For
                End If

                For Each n In cells(key)
                    Dim p As PointF = toScreen(n)
                    Dim r As Single = System.Math.Max(2.0F, radiusOf(n))
                    Dim rect As New RectangleF(p.X - r, p.Y - r, r * 2.0F, r * 2.0F)

                    If rect.Contains(pt) Then
                        Dim d As Single = CSng((p.X - pt.X) ^ 2 + (p.Y - pt.Y) ^ 2)

                        If d < bestD Then
                            bestD = d
                            best = n
                        End If
                    End If
                Next
            Next
        Next

        Return best
    End Function
End Class

