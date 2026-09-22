#Region "Microsoft.VisualBasic::18efb640df84f6d2df6f5ff3d7a00bd0, Data_science\DataMining\hierarchical-clustering\hierarchical-clustering\HierarchyBuilder\DistanceMap.vb"

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

    '   Total Lines: 158
    '    Code Lines: 108 (68.35%)
    ' Comment Lines: 20 (12.66%)
    '    - Xml Docs: 95.00%
    ' 
    '   Blank Lines: 30 (18.99%)
    '     File Size: 5.06 KB


    '     Class DistanceMap
    ' 
    '         Properties: MinimalDistance
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: Add, Dequeue, FindByCodePair, (+2 Overloads) Remove, RemoveFirst
    '                   ToList, ToString
    ' 
    '         Sub: Enqueue, Sort
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language

Namespace Hierarchy

    ''' <summary>
    ''' Container for linkages
    ''' with the minimal methods needed in the package
    ''' Created by Alexandre Masselot on 7/18/14.
    ''' </summary>
    Public Class DistanceMap

        ''' <summary>
        ''' O(1) 按键查找：<see cref="HashCodePair"/> 组合键 -> 链接
        ''' </summary>
        Dim linkTable As New Dictionary(Of ULong, HierarchyLink)(New LinkKeyComparer)

        ''' <summary>
        ''' 数组式二叉最小堆，按 <see cref="HierarchyTreeNode.LinkageDistance"/> 维护堆序。
        ''' 
        ''' <para>
        ''' 这里采用「惰性删除」策略：<see cref="Remove(HierarchyTreeNode)"/> 仅将链接标记为
        ''' <c>removed</c> 并从 <see cref="linkTable"/> 中移除，堆中的陈旧条目会在
        ''' <see cref="MinimalDistance"/> / <see cref="RemoveFirst"/> 出堆时被跳过。
        ''' </para>
        ''' <para>
        ''' 这样单条链接的删除是 O(1) 的，避免了此前基于 <see cref="List(Of HierarchyLink)"/>
        ''' 的 <c>List.Remove</c> 线性查找（单次合并累计约 O(c³)，整体 O(n⁴) 的主要瓶颈），
        ''' 同时也不再需要每次插入/每轮合并都对整个链接表做全量排序。
        ''' </para>
        ''' </summary>
        Dim heap As New List(Of HierarchyLink)

        ''' <summary>
        ''' 堆中已经失效、但仍未被弹出的陈旧条目数量。
        ''' 当其超过存活链接数时触发一次堆压缩，以维持 <see cref="heap"/> 的规模与缓存友好性
        ''' （这是惰性删除策略的代价）。
        ''' </summary>
        Dim staleCount As Integer = 0

#Region "Link hash key comparer"

        ''' <summary>
        ''' 链接键（<see cref="HashCodePair"/>）的字典比较器。
        ''' 
        ''' <para>
        ''' 链接键由两个簇 ID 位拼接而成（<c>(min &lt;&lt; 32) | max</c>）。由于簇 ID 的取值较小，
        ''' 拼接结果的高/低 32 位都集中在很窄的数值范围内；若直接使用 <see cref="UInt64.GetHashCode"/>
        ''' （其实现为高 32 位与低 32 位异或），得到的哈希码会几乎全部落在 <c>[0, 2^k)</c> 的小区间内，
        ''' 导致哈希桶严重冲突、单次查找退化为近似线性扫描。
        ''' 这里改用 64 位 avalanche mix 打散哈希码，保持位拼接键无冲突的同时恢复 O(1) 查找。
        ''' </para>
        ''' </summary>
        Private Class LinkKeyComparer : Implements IEqualityComparer(Of ULong)

            Public Overloads Function Equals(a As ULong, b As ULong) As Boolean Implements IEqualityComparer(Of ULong).Equals
                Return a = b
            End Function

            Public Overloads Function GetHashCode(key As ULong) As Integer Implements IEqualityComparer(Of ULong).GetHashCode
                ' 拆出高/低 32 位（两个簇 ID），分别做乘法扰动后再混合。
                ' 乘法因子与操作数范围经过约束，保证不会触发 VB 的整数溢出检查（两个乘积均 < 2^63）。
                Dim hi As ULong = key >> 32
                Dim lo As ULong = key And &HFFFFFFFFUL
                Dim h As ULong = (hi * 2654435761UL) Xor (lo * 2246822519UL)

                h = h Xor (h >> 27)
                h = h Xor (h << 31)

                Return CInt(h And &H7FFFFFFFUL)
            End Function
        End Class

#End Region

        ''' <summary>
        ''' Peak into the minimum distance
        ''' @return
        ''' </summary>
        Public ReadOnly Property MinimalDistance() As Double
            Get
                ' 惰性清理堆顶已经失效的链接
                Call CleanStale()

                If heap.Count > 0 Then
                    Return heap(Scan0).Tree.LinkageDistance
                Else
                    Return Nothing
                End If
            End Get
        End Property

        Sub New()
        End Sub

        Sub New(links As IEnumerable(Of HierarchyTreeNode))
            For Each x As HierarchyTreeNode In links
                Dim link As New HierarchyLink(x)

                If Not linkTable.ContainsKey(link.HashKey) Then
                    Call linkTable.Add(link.HashKey, link)
                    Call heap.Add(link)
                End If
            Next

            ' 一次性建堆（O(m)），替代此前对整个链接表的一次性 Sort()，
            ' 保证 heap(0) 即当前最小距离的链接（MinimalDistance / RemoveFirst / flatAgg 均依赖该语义）
            Call Heapify()
        End Sub

#Region "Binary min-heap"

        ''' <summary>
        ''' 最小堆比较器：仅比较链接距离（与 <see cref="HierarchyLink.compareTo"/> 的语义一致）
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Shared Function compareLink(a As HierarchyLink, b As HierarchyLink) As Integer
            Return a.Tree.LinkageDistance.CompareTo(b.Tree.LinkageDistance)
        End Function

        ''' <summary>
        ''' 自底向上的建堆操作，O(m)
        ''' </summary>
        Private Sub Heapify()
            Dim n As Integer = heap.Count

            For i As Integer = (n \ 2) - 1 To 0 Step -1
                Call SiftDown(i, n)
            Next
        End Sub

        ''' <summary>
        ''' 入堆，O(log m)
        ''' </summary>
        Private Sub Push(item As HierarchyLink)
            Call heap.Add(item)

            Dim i As Integer = heap.Count - 1

            While i > 0
                Dim parent As Integer = (i - 1) \ 2

                If compareLink(heap(i), heap(parent)) >= 0 Then
                    Exit While
                End If

                Call swap(i, parent)
                i = parent
            End While
        End Sub

        ''' <summary>
        ''' 弹出堆顶，O(log m)。调用方需要自行保证堆非空。
        ''' </summary>
        Private Sub PopRoot()
            Dim last As Integer = heap.Count - 1
            Dim tail As HierarchyLink = heap(last)
            Call heap.RemoveAt(last)

            If heap.Count > 0 Then
                heap(Scan0) = tail
                Call SiftDown(0, heap.Count)
            End If
        End Sub

        ''' <summary>
        ''' 丢弃堆顶已经失效（被删除）的链接，使得 heap(0) 指向当前仍然存活的最小距离链接
        ''' </summary>
        Private Sub CleanStale()
            Do While heap.Count > 0 AndAlso heap(Scan0).removed
                Call PopRoot()
                staleCount -= 1
            Loop
        End Sub

        ''' <summary>
        ''' Poll：弹出当前最小的存活链接（不修改 <see cref="linkTable"/>）
        ''' </summary>
        Private Function PopMin() As HierarchyLink
            Call CleanStale()

            If heap.Count = 0 Then
                Return Nothing
            End If

            Dim top As HierarchyLink = heap(Scan0)
            Call PopRoot()
            Return top
        End Function

        Private Sub SiftDown(i As Integer, n As Integer)
            Do
                Dim l As Integer = 2 * i + 1
                Dim r As Integer = 2 * i + 2
                Dim smallest As Integer = i

                If l < n AndAlso compareLink(heap(l), heap(smallest)) < 0 Then
                    smallest = l
                End If
                If r < n AndAlso compareLink(heap(r), heap(smallest)) < 0 Then
                    smallest = r
                End If
                If smallest = i Then
                    Exit Do
                End If

                Call swap(i, smallest)
                i = smallest
            Loop
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Sub swap(i As Integer, j As Integer)
            Dim tmp As HierarchyLink = heap(i)
            heap(i) = heap(j)
            heap(j) = tmp
        End Sub

#End Region

        Public Function ToList() As IList(Of HierarchyTreeNode)
            Dim l As New List(Of HierarchyTreeNode)

            ' linkTable 中的条目即为当前仍然存活的链接（失效链接在 Remove 时已被移出字典）
            For Each clusterPair As HierarchyLink In linkTable.Values
                l.Add(clusterPair.Tree)
            Next

            Return l
        End Function

        ''' <summary>
        ''' dictionary hash search for the link
        ''' </summary>
        ''' <param name="c1"></param>
        ''' <param name="c2"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function FindByCodePair(c1 As Cluster, c2 As Cluster) As HierarchyTreeNode
            Dim link As HierarchyLink = Nothing

            If linkTable.TryGetValue(hashCodePair(c1, c2), link) Then
                Return link.Tree
            Else
                Return Nothing
            End If
        End Function

        Public Function RemoveFirst() As HierarchyTreeNode
            Dim poll As HierarchyLink = PopMin()

            If poll Is Nothing Then
                Return Nothing
            Else
                With poll.Tree
                    Call linkTable.Remove(poll.HashKey)
                    Return .ByRef
                End With
            End If
        End Function

        Public Function Remove(pending As IEnumerable(Of HierarchyTreeNode)) As Boolean
            For Each i As HierarchyTreeNode In pending
                Call Remove(i)
            Next

            Return True
        End Function

        Public Function Remove(link As HierarchyTreeNode) As Boolean
            Dim removed As HierarchyLink = Nothing

            If Not linkTable.TryGetValue(hashCodePair(link), removed) Then
                Return False
            End If

            ' O(1) 惰性删除：仅移出字典并打标记，堆中的陈旧条目留待出堆时跳过
            Call linkTable.Remove(removed.HashKey)
            removed.removed = True
            staleCount += 1

            ' 陈旧条目超过存活链接数时压缩一次堆，
            ' 避免堆无限膨胀（既抬高 log 因子，也让每次比较都发生缓存缺失）
            If staleCount > linkTable.Count Then
                Call Compact()
            End If

            Return True
        End Function

        ''' <summary>
        ''' 丢弃堆中所有已失效的陈旧条目并重新建堆（O(m)）
        ''' </summary>
        Private Sub Compact()
            heap.Clear()

            For Each link As HierarchyLink In linkTable.Values
                heap.Add(link)
            Next

            Call Heapify()
            staleCount = 0
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Sort()
            ' 堆结构天然维护最小序（heap(0) 恒为最小），这里保留为兼容性的空实现
        End Sub

        Public Function Add(link As HierarchyTreeNode, Optional direct As Boolean = False) As Boolean
            Dim hlink As New HierarchyLink(link)

            If linkTable.ContainsKey(hlink.HashKey) Then
#If DEBUG Then
                Dim existingItem As HierarchyLink = linkTable(hlink.HashKey)

                Call Console _
                    .Error _
                    .WriteLine("hashCode = " & existingItem.HashKey & " adding redundant link:" & link.ToString & " (exist:" & existingItem.ToString & ")")
#End If
                Return False
            Else
                Call linkTable.Add(hlink.HashKey, hlink)
                ' direct 参数保留以兼容旧调用方（旧实现中 direct=True 表示稍后统一排序），
                ' 堆结构下插入即维护堆序，无需区分
                Call Push(hlink)

                Return True
            End If
        End Function

        Public Overloads Function ToString() As String
            Return $"Have {linkTable.Count} linkage with minimal distance {MinimalDistance}"
        End Function
    End Class
End Namespace
