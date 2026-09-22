#Region "Microsoft.VisualBasic::2b447d89e92d870bc4bb2497886db669, Data_science\DataMining\DataMining\test\NumericTableTests\Program.vb"

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

    '   Total Lines: 143
    '    Code Lines: 98 (68.53%)
    ' Comment Lines: 21 (14.69%)
    '    - Xml Docs: 66.67%
    ' 
    '   Blank Lines: 24 (16.78%)
    '     File Size: 5.85 KB


    ' Module Program
    ' 
    '     Function: JoinLabel, NewTable
    ' 
    '     Sub: LabelCheck, Main, PartitionCheck
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports Microsoft.VisualBasic.Data
Imports Microsoft.VisualBasic.DataMining.Clustering
Imports Microsoft.VisualBasic.DataMining.FuzzyCMeans
Imports Microsoft.VisualBasic.DataMining.KMeans

''' <summary>
''' 统一二维表<see cref="NumericTable"/>聚类接口的冒烟测试。
''' 
''' 该控制台程序不依赖任何第三方测试框架，直接对各聚类算法执行一次真实运算，
''' 并校验结果是否写入了标签矩阵。运行方式：
''' 
''' ```bash
''' dotnet run --project test/NumericTableTests/NumericTableTests.vbproj
''' ```
''' </summary>
Module Program

    ''' <summary>
    ''' 两个分离良好的二维簇，每个簇 3 个样本
    ''' </summary>
    Private ReadOnly rows As Double()() = {
        New Double() {0.0, 0.0},
        New Double() {0.1, 0.2},
        New Double() {0.2, 0.1},
        New Double() {5.0, 5.0},
        New Double() {5.1, 4.9},
        New Double() {4.9, 5.2}
    }

    Private ReadOnly ids As String() = {"a", "b", "c", "d", "e", "f"}
    Private ReadOnly features As String() = {"f1", "f2"}

    Private Function NewTable() As NumericTable
        Return New NumericTable(rows, ids, features)
    End Function

    Sub Main()
        Dim failures As New List(Of String)

        ' 硬划分聚类：应当将前 3 个与后 3 个样本正确的划分为两个簇
        Call PartitionCheck(failures, "kmeans", NewTable().kmeans(k:=2))
        Call PartitionCheck(failures, "cmeans", NewTable().cmeans(c:=2))
        Call PartitionCheck(failures, "kmedoids", NewTable().kmedoids(k:=2))
        Call PartitionCheck(failures, "dbscan", NewTable().dbscan(epsilon:=1.0, minPts:=2))
        Call PartitionCheck(failures, "hdbscan", NewTable().hdbscan(minPoints:=2, minClusterSize:=2))
        Call PartitionCheck(failures, "bisectingKMeans", NewTable().bisectingKMeans(k:=2, iterations:=2))
        Call PartitionCheck(failures, "lloyds", NewTable().lloydsCluster(2))
        Call PartitionCheck(failures, "canopy", NewTable().canopyCluster())

        ' 结果标签列的写入检查（这些算法的划分结果依赖随机初始化，仅检查结果列存在）
        Call LabelCheck(failures, "spectral", NewTable().spectralCluster(2), "cluster")
        Call LabelCheck(failures, "knnCluster", NewTable().knnCluster(k:=2, p:=0.5), "cluster")
        Call LabelCheck(failures, "density", NewTable().densityScore(2), "density")
        Call LabelCheck(failures, "cmeans.membership", NewTable().cmeans(c:=2), "membership_1")

        ' KNN 有监督分类：以 kmeans 的划分结果作为训练标签
        Dim train = NewTable().kmeans(k:=2)
        Dim predicted = train.knnClassify(NewTable(), 3)
        Call LabelCheck(failures, "knnClassify", predicted, "knn")

        ' 聚类评估指标
        Dim evaluated = NewTable().kmeans(k:=2)

        If evaluated.silhouette() <= 0 Then
            Call failures.Add("silhouette: expected a positive score")
        End If
        If evaluated.dunn() <= 0 Then
            Call failures.Add("dunn: expected a positive score")
        End If
        If evaluated.calinskiHarabasz() <= 0 Then
            Call failures.Add("calinskiHarabasz: expected a positive score")
        End If

        ' 统一评估框架（ROC/AUC 唯一核心 + 三类结果适配器）的数值一致性回归测试
        Call EvaluationConsistencyTest.Run(failures)

        If failures.Count > 0 Then
            Console.WriteLine("FAILED:")

            For Each msg As String In failures
                Console.WriteLine("  - " & msg)
            Next

            Environment.Exit(1)
        Else
            Console.WriteLine("all table clustering checks passed.")
        End If
    End Sub

    ''' <summary>
    ''' 校验聚类结果是否将前三个样本与后三个样本正确的划分为两个不同的簇
    ''' </summary>
    Private Sub PartitionCheck(failures As List(Of String), name As String, table As NumericTable, Optional column As String = "cluster")
        Dim values As Double() = Nothing

        If Not table.TryGetLabel(column, values) Then
            Call failures.Add($"{name}: missing label column '{column}'")
            Return
        End If

        If values.Length <> rows.Length Then
            Call failures.Add($"{name}: label size {values.Length} <> {rows.Length}")
            Return
        End If

        Dim first As Integer = CInt(values(0))
        Dim second As Integer = CInt(values(3))

        If first = second Then
            Call failures.Add($"{name}: the two clusters are not separated: {JoinLabel(values)}")
            Return
        End If

        For i As Integer = 0 To 2
            If CInt(values(i)) <> first Then
                Call failures.Add($"{name}: unexpected partition: {JoinLabel(values)}")
                Return
            End If
        Next

        For i As Integer = 3 To 5
            If CInt(values(i)) <> second Then
                Call failures.Add($"{name}: unexpected partition: {JoinLabel(values)}")
                Return
            End If
        Next
    End Sub

    Private Sub LabelCheck(failures As List(Of String), name As String, table As NumericTable, column As String)
        Dim values As Double() = Nothing

        If Not table.TryGetLabel(column, values) Then
            Call failures.Add($"{name}: missing label column '{column}'")
        ElseIf values.Length <> rows.Length Then
            Call failures.Add($"{name}: label size {values.Length} <> {rows.Length}")
        End If
    End Sub

    Private Function JoinLabel(values As Double()) As String
        Return String.Join(", ", values.Select(Function(d) CInt(d).ToString))
    End Function
End Module

