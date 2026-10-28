#Region "Microsoft.VisualBasic::de47e5c4a6f902ae3af011a558346470, Data_science\DataMining\DataMining\Clustering\DBSCAN\DbscanSession.vb"

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

    '   Total Lines: 252
    '    Code Lines: 171 (67.86%)
    ' Comment Lines: 40 (15.87%)
    '    - Xml Docs: 82.50%
    ' 
    '   Blank Lines: 41 (16.27%)
    '     File Size: 9.85 KB


    '     Class DbscanSession
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: CheckNeighborPts, isNoise, LoadDensityVector, RegionQuery
    ' 
    '         Sub: ExpandCluster, ExpandClusterParallel
    '         Class QueryTask
    ' 
    '             Constructor: (+1 Overloads) Sub New
    '             Sub: Solve
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.DataMining.Clustering
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.SIMD
Imports Microsoft.VisualBasic.Parallel
Imports std = System.Math

Namespace DBSCAN

    Public Class DbscanSession(Of T As IReadOnlyId)

        ReadOnly dbscan As DbscanAlgorithm(Of T)
        ''' <summary>
        ''' Dataset
        ''' </summary>
        Friend ReadOnly allPoints As DbscanPoint(Of T)()
        ''' <summary>
        ''' radius of center point
        ''' </summary>
        ReadOnly epsilon As Double
        Friend ReadOnly minPts As Integer
        ReadOnly densityCut As Double = -1
        ReadOnly maxStackSize As Integer

        Dim densityList As Dictionary(Of String, Double)

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="dbscan"></param>
        ''' <param name="allPoints"></param>
        ''' <param name="epsilon">Desired region ball range</param>
        ''' <param name="minPts">Minimum number of points to be in a region, density cutoff</param>
        Sub New(dbscan As DbscanAlgorithm(Of T),
                allPoints As DbscanPoint(Of T)(),
                epsilon As Double,
                minPts As Integer)

            Me.maxStackSize = std.Min(allPoints.Length / 2, 1024)
            Me.allPoints = allPoints
            Me.epsilon = epsilon
            Me.minPts = minPts
            Me.dbscan = dbscan

            Call dbscan.println($"max stack size for expands cluster is {maxStackSize}")
        End Sub

        Public Function isNoise(id As String) As Boolean
            If densityCut > 0 AndAlso densityList(id) < densityCut Then
                Return True
            Else
                Return False
            End If
        End Function

        Public Function LoadDensityVector(densityCut As Double) As DbscanSession(Of T)
            Dim metric As Func(Of DbscanPoint(Of T), DbscanPoint(Of T), Double) =
                Function(a, b)
                    Return dbscan._metricFunc(a.ClusterPoint, b.ClusterPoint)
                End Function

            If densityCut > 0 Then
                Dim allDensity = Density _
                    .GetDensity(Of DbscanPoint(Of T))(allPoints, metric, k:=minPts) _
                    .ToArray
                Dim orderDensity As Double() = allDensity _
                    .Select(Function(d) d.Value) _
                    .OrderBy(Function(d) d) _
                    .ToArray

                densityCut = orderDensity(densityCut * allDensity.Length)
                densityList = allDensity _
                    .ToDictionary(Function(i) i.Name,
                                  Function(i)
                                      Return i.Value
                                  End Function)

                Call dbscan.println($"Density cutoff for dbscan is: {densityCut}!")
                Call dbscan.println($"There are {orderDensity.Where(Function(d) d < densityCut).Count}/{densityList.Count} lower than this threshold value.")
            Else
                Call dbscan.println("No density cutoff of your sample data.")
            End If

            Return Me
        End Function

        ''' <summary>
        ''' this function was called by a parallel linq
        ''' </summary>
        ''' <param name="pn"></param>
        ''' <returns></returns>
        Private Function CheckNeighborPts(pn As DbscanPoint(Of T)) As DbscanPoint(Of T)()
            If dbscan._full OrElse Not pn.IsVisited Then
                Dim neighborPts2 = RegionQuery(pn.ClusterPoint, parallel:=False).ToArray

                SyncLock pn
                    pn.IsVisited = True
                    pn.ClusterId = If(isNoise(pn.ID), ClusterIDs.Noise, pn.ClusterId)
                End SyncLock

                If neighborPts2.Length >= minPts Then
                    Return neighborPts2
                Else
                    Return {}
                End If
            Else
                Return {}
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="neighborPts">other points in same region with point parameter</param>
        ''' <param name="clusterId">given clusterId</param>
        Public Sub ExpandClusterParallel(neighborPts As DbscanPoint(Of T)(), clusterId As Integer, stackDepth As Integer)
            ' 只对"尚未访问"的点做邻域查询（与旧实现的 Where Not IsVisited 等价），
            ' 但结果按 pending 的下标回填到数组里，合并时按原始顺序去重，
            ' 因此扩展顺序与输入顺序一致（PLINQ 版本的聚合顺序取决于线程调度，结果不可复现）。
            Dim pending As DbscanPoint(Of T)() = neighborPts _
                .Where(Function(pn) Not pn.IsVisited) _
                .ToArray
            Dim found As New List(Of DbscanPoint(Of T))(neighborPts.Length)
            Dim seen As New HashSet(Of DbscanPoint(Of T))(neighborPts)

            If pending.Length > 0 Then
                Dim chunkResults As DbscanPoint(Of T)()() = New DbscanPoint(Of T)(pending.Length - 1)() {}

                Call System.Threading.Tasks.Parallel.For(0, pending.Length,
                    Sub(i)
                        chunkResults(i) = CheckNeighborPts(pending(i))
                    End Sub)

                For i As Integer = 0 To chunkResults.Length - 1
                    Dim neighbors As DbscanPoint(Of T)() = chunkResults(i)

                    If neighbors Is Nothing Then Continue For

                    For Each x As DbscanPoint(Of T) In neighbors
                        If seen.Add(x) Then
                            Call found.Add(x)
                        End If
                    Next
                Next
            End If

            For Each pn As DbscanPoint(Of T) In neighborPts
                pn.IsVisited = True

                If pn.ClusterId = ClusterIDs.Unclassified OrElse pn.ClusterId = ClusterIDs.Noise Then
                    pn.ClusterId = clusterId
                End If
            Next

            If (stackDepth + 1) < maxStackSize AndAlso found.Count > 0 Then
                Dim foundArray As DbscanPoint(Of T)() = found.ToArray

                For Each pn As DbscanPoint(Of T) In foundArray
                    If pn.IsVisited Then
                        Continue For
                    Else
                        Call ExpandClusterParallel(foundArray, clusterId, stackDepth + 1)
                    End If
                Next
            End If
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="neighborPts">other points in same region with point parameter</param>
        ''' <param name="clusterId">given clusterId</param>
        Public Sub ExpandCluster(neighborPts As DbscanPoint(Of T)(), clusterId As Integer, stackDepth As Integer)
            Dim neighborPts2 As DbscanPoint(Of T)() = Nothing
            Dim pending As New List(Of DbscanPoint(Of T))(neighborPts)
            Dim seen As New HashSet(Of DbscanPoint(Of T))(neighborPts)
            Dim cursor As Integer = 0

            Do While cursor < pending.Count
                Dim pn As DbscanPoint(Of T) = Nothing

                ' 用游标顺序扫描代替每次 Where(...).FirstOrDefault()（后者每次都要建迭代器/委托）
                Do While cursor < pending.Count
                    Dim candidate As DbscanPoint(Of T) = pending(cursor)

                    cursor += 1

                    If Not candidate.IsVisited Then
                        pn = candidate

                        Exit Do
                    End If
                Loop

                If pn Is Nothing Then
                    Exit Do
                End If

                If dbscan._full OrElse Not pn.IsVisited Then
                    pn.IsVisited = True
                    neighborPts2 = RegionQuery(pn.ClusterPoint, parallel:=True)

                    If densityCut > 0 AndAlso densityList(pn.ID) < densityCut Then
                        pn.ClusterId = ClusterIDs.Noise
                    ElseIf neighborPts2.Length >= minPts Then
                        ' 旧实现用 Union().ToArray() 每轮重建整张邻域表（O(n) 内存搬运 × 迭代数）；
                        ' 这里用 HashSet 做增量去重，语义与 Union 完全一致。
                        For Each x As DbscanPoint(Of T) In neighborPts2
                            If seen.Add(x) Then
                                Call pending.Add(x)
                            End If
                        Next

                        If stackDepth < maxStackSize Then
                            ' call in recursive
                            Call ExpandCluster(neighborPts2, clusterId, stackDepth + 1)
                        End If
                    End If
                End If

                If pn.ClusterId = ClusterIDs.Unclassified OrElse pn.ClusterId = ClusterIDs.Noise Then
                    pn.ClusterId = clusterId
                End If
            Loop
        End Sub

        ''' <summary>
        ''' the point set index
        ''' </summary>
        ReadOnly queryCache As New Dictionary(Of String, DbscanPoint(Of T)())

        ''' <summary>
        ''' Checks and searchs neighbor points for given point
        ''' </summary>
        ''' <param name="point">centered point to be searched neighbors</param>
        ''' <returns>result neighbors</returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function RegionQuery(point As T, parallel As Boolean) As DbscanPoint(Of T)()
            Dim key As String = point.Identity
            Dim cached As DbscanPoint(Of T)() = Nothing

            ' 一次查找代替 ContainsKey + 索引器访问（旧实现每次命中都要做两次哈希）
            SyncLock queryCache
                If queryCache.TryGetValue(key, cached) Then
                    Return cached
                End If
            End SyncLock

            Dim result As DbscanPoint(Of T)() = ScanRegion(point, parallel)

            SyncLock queryCache
                Dim raced As DbscanPoint(Of T)() = Nothing

                If queryCache.TryGetValue(key, raced) Then
                    Return raced
                End If

                queryCache(key) = result
            End SyncLock

            Return result
        End Function

        ''' <summary>
        ''' 对全库做一次邻域扫描：先写布尔掩码再按原始下标物化结果。
        ''' </summary>
        ''' <remarks>
        ''' 旧实现为每个点新建一个 <c>QueryTask</c>（<see cref="VectorTask"/> 派生）并在线程内
        ''' 各自维护 <c>List</c>、加锁合并；n 个点就是 n 次任务对象 + n×workers 次小列表分配，
        ''' 且输出顺序取决于线程调度。掩码方案把所有临时对象压缩成两个数组，
        ''' 并且结果顺序恒为原始下标升序。
        ''' </remarks>
        Private Function ScanRegion(point As T, parallel As Boolean) As DbscanPoint(Of T)()
            Dim n As Integer = allPoints.Length

            If n = 0 Then Return Array.Empty(Of DbscanPoint(Of T))()

            Dim eval As Func(Of T, T, Double) = dbscan._metricFunc

            If Not parallel Then
                Dim buffer As New List(Of DbscanPoint(Of T))()

                For i As Integer = 0 To n - 1
                    Dim x As DbscanPoint(Of T) = allPoints(i)

                    If eval(point, x.ClusterPoint) <= epsilon Then
                        Call buffer.Add(x)
                    End If
                Next

                Return buffer.ToArray()
            End If

            Dim mask As Boolean() = New Boolean(n - 1) {}

            Call SimdParallel.ForRange(n,
                Sub(start, ends)
                    For i As Integer = start To ends - 1
                        mask(i) = eval(point, allPoints(i).ClusterPoint) <= epsilon
                    Next
                End Sub, 2048)

            Dim hits As New List(Of DbscanPoint(Of T))()

            For i As Integer = 0 To n - 1
                If mask(i) Then
                    Call hits.Add(allPoints(i))
                End If
            Next

            Return hits.ToArray()
        End Function
    End Class
End Namespace
