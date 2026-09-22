#Region "Microsoft.VisualBasic::608181087d43e52ad50950e093680dd9, Data_science\DataMining\hierarchical-clustering\HCTNumericTableTests\Benchmark.vb"

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

    '   Total Lines: 224
    '    Code Lines: 143 (63.84%)
    ' Comment Lines: 31 (13.84%)
    '    - Xml Docs: 87.10%
    ' 
    '   Blank Lines: 50 (22.32%)
    '     File Size: 7.54 KB


    ' Module Benchmark
    ' 
    '     Function: purity, SyntheticTable
    ' 
    '     Sub: report, RunAll, RunApproximate, RunExact
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Diagnostics
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering
Imports Microsoft.VisualBasic.DataMining.HierarchicalClustering.BIRCH

''' <summary>
''' 层次聚类性能基准：
''' 
''' + **精确通道**（<c>distanceMatrix()</c> → <c>hca()/hcut()</c>）在中等规模数据上的耗时；
''' + **BIRCH 近似通道**（<c>hcaApprox()/hcutApprox()</c>）在 2 万样本以上数据上的耗时与压缩率。
''' 
''' 运行方式：
''' ```bash
''' dotnet run --project HCTNumericTableTests/HCTNumericTableTests.vbproj -- benchmark
''' ```
''' </summary>
Module Benchmark

    ''' <summary>
    ''' 生成 k 个分离良好的球状簇：n 个样本、d 个特征，并把真实分组写入 ``group`` 标签列
    ''' </summary>
    Public Function SyntheticTable(n As Integer, d As Integer, k As Integer, Optional seed As Integer = 12345) As NumericTable
        Dim rand As New Random(seed)
        Dim centers As Double()() = New Double(k - 1)() {}
        Dim rows As Double()() = New Double(n - 1)() {}
        Dim ids As String() = New String(n - 1) {}
        Dim groups As Double() = New Double(n - 1) {}

        For c As Integer = 0 To k - 1
            Dim center As Double() = New Double(d - 1) {}

            For j As Integer = 0 To d - 1
                center(j) = rand.NextDouble() * 50.0
            Next

            centers(c) = center
        Next

        For i As Integer = 0 To n - 1
            Dim center As Double() = centers(i Mod k)
            Dim row As Double() = New Double(d - 1) {}

            For j As Integer = 0 To d - 1
                row(j) = center(j) + rand.NextDouble() * 2 - 1
            Next

            rows(i) = row
            ids(i) = "s" & (i + 1)
            groups(i) = (i Mod k) + 1
        Next

        Dim names As String() = New String(d - 1) {}

        For j As Integer = 0 To d - 1
            names(j) = "f" & (j + 1)
        Next

        Dim table As New NumericTable(rows, ids, names)

        Call table.SetLabel("group", groups)

        Return table
    End Function

    ''' <summary>
    ''' 默认基准套件
    ''' </summary>
    Public Sub RunAll()
        Console.WriteLine("==== hierarchical clustering benchmark ====")
        Console.WriteLine()

        ' 精确通道：受 n×n 距离矩阵与 O(n^2) 链接表的内存限制，规模不能太大
        Call RunExact(1500, 4, 4)
        Console.WriteLine()

        ' BIRCH 近似通道：面向 2 万样本以上，全程不构造 n×n 距离矩阵
        Call RunApproximate(20000, 8, 6, 1500)
        Console.WriteLine()
        Call RunApproximate(50000, 8, 6, 2000)
        Console.WriteLine()

        Console.WriteLine("==== benchmark done ====")
    End Sub

    ''' <summary>
    ''' 精确通道基准（构造 n×n 距离矩阵 + O(n^2) 链接表）
    ''' </summary>
    Public Sub RunExact(n As Integer, d As Integer, k As Integer)
        Dim x = SyntheticTable(n, d, k)

        Console.WriteLine($"[exact ] n={n} d={d} k={k}")

        Dim sw As Stopwatch = Stopwatch.StartNew()
        Dim dist = x.distanceMatrix()
        Dim tMatrix As Long = sw.ElapsedMilliseconds

        sw.Restart()
        Dim tree = dist.hca()
        Dim tTree As Long = sw.ElapsedMilliseconds
        Dim leafs As Integer = tree.Leafs

        sw.Restart()
        Dim flat = dist.hcut(k:=k)
        Dim tCut As Long = sw.ElapsedMilliseconds

        Console.WriteLine($"          distanceMatrix = {tMatrix} ms, hca = {tTree} ms (leafs = {leafs})")
        Call report("hcut", flat, tCut)
    End Sub

    ''' <summary>
    ''' BIRCH 近似通道基准（不构造 n×n 距离矩阵，适用于 2 万样本以上）
    ''' </summary>
    Public Sub RunApproximate(n As Integer, d As Integer, k As Integer, Optional targetSubclusters As Integer = 2000)
        Dim x = SyntheticTable(n, d, k)
        Dim options As New BirchOptions With {
            .targetSubclusters = targetSubclusters,
            .silent = True
        }

        Console.WriteLine($"[approx] n={n} d={d} k={k} targetSubclusters={targetSubclusters}")

        Dim sw As Stopwatch = Stopwatch.StartNew()
        Dim tree = x.hcaApprox(options)
        Dim tTree As Long = sw.ElapsedMilliseconds
        Dim m As Integer = tree.Leafs

        sw.Restart()
        Dim flat = x.hcutApprox(k, options)
        Dim tCut As Long = sw.ElapsedMilliseconds

        Console.WriteLine($"          subclusters m = {m} (compression {n \ Math.Max(m, 1)}x), hcaApprox = {tTree} ms")
        Call report("hcutApprox", flat, tCut)
    End Sub

    ''' <summary>
    ''' 输出一次切分结果的耗时、簇大小分布与相对真实分组的纯度
    ''' </summary>
    Private Sub report(name As String, flat As NumericTable, ms As Long)
        Dim labels As Double() = Nothing

        If flat Is Nothing OrElse Not flat.TryGetLabel("cluster", labels) Then
            Console.WriteLine($"          {name}: missing 'cluster' label!")
            Return
        End If

        Dim sizes As New Dictionary(Of Integer, Integer)

        For i As Integer = 0 To labels.Length - 1
            Dim id As Integer = CInt(labels(i))
            Dim c As Integer = 0

            If sizes.TryGetValue(id, c) Then
                sizes(id) = c + 1
            Else
                sizes(id) = 1
            End If
        Next

        Dim sizeText As New List(Of String)

        For Each size As Integer In sizes.Values
            Call sizeText.Add(size.ToString())
        Next

        sizeText.Sort()
        sizeText.Reverse()

        Console.WriteLine($"          {name} = {ms} ms, clusters = {sizes.Count} [{String.Join(", ", sizeText)}]")

        Dim groups As Double() = Nothing

        If flat.TryGetLabel("group", groups) Then
            Console.WriteLine($"          purity = {purity(labels, groups):P1}")
        End If
    End Sub

    ''' <summary>
    ''' 计算聚类标签相对真实分组的纯度：每个簇取占多数的真实分组，累计后除以样本数
    ''' </summary>
    Public Function purity(labels As Double(), groups As Double()) As Double
        Dim crosstab As New Dictionary(Of Integer, Dictionary(Of Integer, Integer))()
        Dim total As Integer = labels.Length

        If total = 0 Then
            Return 0
        End If

        For i As Integer = 0 To total - 1
            Dim c As Integer = CInt(labels(i))
            Dim g As Integer = CInt(groups(i))
            Dim row As Dictionary(Of Integer, Integer) = Nothing

            If Not crosstab.TryGetValue(c, row) Then
                row = New Dictionary(Of Integer, Integer)()
                crosstab(c) = row
            End If

            Dim cnt As Integer = 0

            If row.TryGetValue(g, cnt) Then
                row(g) = cnt + 1
            Else
                row(g) = 1
            End If
        Next

        Dim correct As Integer = 0

        For Each row As Dictionary(Of Integer, Integer) In crosstab.Values
            Dim max As Integer = 0

            For Each cnt As Integer In row.Values
                If cnt > max Then
                    max = cnt
                End If
            Next

            correct += max
        Next

        Return correct / total
    End Function
End Module

