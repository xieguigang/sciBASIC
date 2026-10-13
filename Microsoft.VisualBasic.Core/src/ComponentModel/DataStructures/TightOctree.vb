#Region "Microsoft.VisualBasic::2be385373fdf9f3afc63fdc5b3a43919, Microsoft.VisualBasic.Core\src\ComponentModel\DataStructures\TightOctree.vb"

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

    '   Total Lines: 262
    '    Code Lines: 183 (69.85%)
    ' Comment Lines: 40 (15.27%)
    '    - Xml Docs: 97.50%
    ' 
    '   Blank Lines: 39 (14.89%)
    '     File Size: 10.26 KB


    '     Structure TightOctreeItem
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class TightOctreeCell
    ' 
    '         Properties: Bounds, Depth, IsBoundaryCell, Items
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class TightOctree
    ' 
    '         Properties: Bounds, Depth, IsLeaf
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: IntersectingCells, Leaves
    ' 
    '         Sub: collectLeaves, Insert, insertNode, intersectNode, Refine
    '              RefineBoundaryCells, refineBoundaryNode, refineNode, RefineToUniform, subdivide
    '              Traverse
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports std = System.Math

Namespace ComponentModel.DataStructures

    ''' <summary>
    ''' 紧八叉树（TightOctree）的负载项：一个空间包围盒及其关联的对象（如三角面片索引）。
    ''' </summary>
    Public Structure TightOctreeItem(Of T)

        Public Bounds As BoundingBox3D
        Public Value As T

        Sub New(bounds As BoundingBox3D, value As T)
            Me.Bounds = bounds
            Me.Value = value
        End Sub

        Public Overrides Function ToString() As String
            Return $"{Bounds.ToString}"
        End Function
    End Structure

    ''' <summary>
    ''' 紧八叉树的叶子单元：携带一个空间盒与全部与其相交的负载项。
    ''' 在笛卡尔网格生成场景中，<see cref="Items"/> 非空即为与几何边界相交的切割单元候选。
    ''' </summary>
    Public Class TightOctreeCell(Of T)

        ReadOnly _bounds As BoundingBox3D
        ReadOnly _depth As Integer
        ReadOnly _items As List(Of TightOctreeItem(Of T))

        Friend Sub New(bounds As BoundingBox3D, depth As Integer, items As List(Of TightOctreeItem(Of T)))
            _bounds = bounds
            _depth = depth
            _items = items
        End Sub

        Public ReadOnly Property Bounds As BoundingBox3D
            Get
                Return _bounds
            End Get
        End Property

        ''' <summary>单元所处深度（根为 0）。</summary>
        Public ReadOnly Property Depth As Integer
            Get
                Return _depth
            End Get
        End Property

        ''' <summary>与该单元相交的全部负载项。</summary>
        Public ReadOnly Property Items As List(Of TightOctreeItem(Of T))
            Get
                Return _items
            End Get
        End Property

        ''' <summary>是否与任何负载项（几何边界）相交。</summary>
        Public ReadOnly Property IsBoundaryCell As Boolean
            Get
                Return _items.Count > 0
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"[{_depth}] {_bounds.ToString} ({_items.Count} items)"
        End Function
    End Class

    ''' <summary>
    ''' 紧八叉树（TightOctree）：笛卡尔网格生成专用的空间划分结构。
    ''' 每个负载项（如三角面片的包围盒）会登记到与其相交的每一个叶子单元上，
    ''' 叶子单元因此可以区分为内部单元（无负载项）与边界切割单元（有负载项）。
    ''' 支持按谓词递归细化（如"与边界相交且尺度大于目标网格尺度则继续八分"）。
    ''' </summary>
    Public Class TightOctree(Of T)

        ReadOnly _bounds As BoundingBox3D
        ReadOnly _maxDepth As Integer
        Dim _depth As Integer
        Dim _children As TightOctree(Of T)()
        Dim _items As List(Of TightOctreeItem(Of T))

        ''' <summary>
        ''' 构建以 <paramref name="bounds"/> 为根空间的紧八叉树。
        ''' </summary>
        ''' <param name="bounds">根空间包围盒（笛卡尔网格的外层大立方体）。</param>
        ''' <param name="maxDepth">最大细分深度，限制最细单元尺度为 bounds 最长边 / 2^maxDepth。</param>
        Sub New(bounds As BoundingBox3D, Optional maxDepth As Integer = 10)
            _bounds = bounds
            _maxDepth = maxDepth
            _depth = 0
            _items = New List(Of TightOctreeItem(Of T))
        End Sub

        Public ReadOnly Property Bounds As BoundingBox3D
            Get
                Return _bounds
            End Get
        End Property

        Public ReadOnly Property IsLeaf As Boolean
            Get
                Return _children Is Nothing
            End Get
        End Property

        Public ReadOnly Property Depth As Integer
            Get
                Return _depth
            End Get
        End Property

        ''' <summary>插入一个负载项：登记到当前节点路径上所有与其包围盒相交的叶子单元。</summary>
        Public Sub Insert(bounds As BoundingBox3D, value As T)
            If Not _bounds.Overlaps(bounds) Then
                Throw New ArgumentOutOfRangeException(NameOf(bounds),
                    $"item bounds {bounds.ToString} does not overlap the octree root {_bounds.ToString}")
            End If

            Call insertNode(New TightOctreeItem(Of T)(bounds, value))
        End Sub

        Private Sub insertNode(item As TightOctreeItem(Of T))
            If IsLeaf Then
                _items.Add(item)
            Else
                For Each child As TightOctree(Of T) In _children
                    If child._bounds.Overlaps(item.Bounds) Then
                        Call child.insertNode(item)
                    End If
                Next
            End If
        End Sub

        ''' <summary>
        ''' 对所有满足 <paramref name="refine"/> 谓词的叶子单元递归八分（并重分布负载项）。
        ''' 谓词入参为叶子单元的包围盒与深度，返回 True 表示需要继续细分。
        ''' 细分受构造时的 <c>maxDepth</c> 硬上限保护。
        ''' </summary>
        Public Sub Refine(refine As Func(Of BoundingBox3D, Integer, Boolean))
            Call refineNode(refine)
        End Sub

        Private Sub refineNode(refine As Func(Of BoundingBox3D, Integer, Boolean))
            If IsLeaf Then
                If _depth < _maxDepth AndAlso refine(_bounds, _depth) Then
                    Call subdivide()
                Else
                    Return
                End If
            End If

            For Each child As TightOctree(Of T) In _children
                Call child.refineNode(refine)
            Next
        End Sub

        ''' <summary>
        ''' 将所有叶子单元细分至指定层数（即所有深度小于 <paramref name="targetDepth"/> 的单元继续八分），
        ''' 生成尺寸均匀的满八叉树。
        ''' </summary>
        Public Sub RefineToUniform(targetDepth As Integer)
            Call Refine(Function(b, depth) depth < targetDepth)
        End Sub

        ''' <summary>
        ''' 边界自适应细分：递归细分所有与负载项（几何边界）相交、
        ''' 且尺寸大于 <paramref name="minCellSize"/> 的叶子单元（受 maxDepth 硬上限保护）。
        ''' 这是笛卡尔八叉树网格的标准生成方式：远场保持粗单元，壁面附近细化至目标尺度。
        ''' </summary>
        Public Sub RefineBoundaryCells(minCellSize As Double)
            Call refineBoundaryNode(minCellSize)
        End Sub

        Private Sub refineBoundaryNode(minCellSize As Double)
            If IsLeaf Then
                If _depth < _maxDepth AndAlso _items.Count > 0 AndAlso
                    std.Max(std.Max(_bounds.SizeX, _bounds.SizeY), _bounds.SizeZ) > minCellSize Then

                    Call subdivide()
                Else
                    Return
                End If
            End If

            If Not IsLeaf Then
                For Each child As TightOctree(Of T) In _children
                    Call child.refineBoundaryNode(minCellSize)
                Next
            End If
        End Sub

        Private Sub subdivide()
            Dim items As List(Of TightOctreeItem(Of T)) = _items
            _items = New List(Of TightOctreeItem(Of T))
            _children = New TightOctree(Of T)(7) {}

            For i As Integer = 0 To 7
                _children(i) = New TightOctree(Of T)(_bounds.Octant(i), _maxDepth)
                _children(i)._depth = _depth + 1
            Next

            ' 负载项登记到每一个与其相交的子单元
            For Each item As TightOctreeItem(Of T) In items
                For Each child As TightOctree(Of T) In _children
                    If child._bounds.Overlaps(item.Bounds) Then
                        Call child._items.Add(item)
                    End If
                Next
            Next
        End Sub

        ''' <summary>枚举全部叶子单元（内部单元与边界单元均包含）。</summary>
        Public Function Leaves() As List(Of TightOctreeCell(Of T))
            Dim result As New List(Of TightOctreeCell(Of T))
            Call collectLeaves(result)
            Return result
        End Function

        Private Sub collectLeaves(result As List(Of TightOctreeCell(Of T)))
            If IsLeaf Then
                result.Add(New TightOctreeCell(Of T)(_bounds, _depth, _items))
            Else
                For Each child As TightOctree(Of T) In _children
                    Call child.collectLeaves(result)
                Next
            End If
        End Sub

        ''' <summary>查询与给定包围盒相交的全部叶子单元。</summary>
        Public Function IntersectingCells(bounds As BoundingBox3D) As List(Of TightOctreeCell(Of T))
            Dim result As New List(Of TightOctreeCell(Of T))
            Call intersectNode(bounds, result)
            Return result
        End Function

        Private Sub intersectNode(bounds As BoundingBox3D, result As List(Of TightOctreeCell(Of T)))
            If Not _bounds.Overlaps(bounds) Then
                Return
            ElseIf IsLeaf Then
                result.Add(New TightOctreeCell(Of T)(_bounds, _depth, _items))
            Else
                For Each child As TightOctree(Of T) In _children
                    Call child.intersectNode(bounds, result)
                Next
            End If
        End Sub

        ''' <summary>遍历全部节点（含内部节点）。</summary>
        Public Sub Traverse(action As Action(Of TightOctree(Of T)))
            Call action(Me)

            If Not IsLeaf Then
                For Each child As TightOctree(Of T) In _children
                    Call child.Traverse(action)
                Next
            End If
        End Sub
    End Class
End Namespace

