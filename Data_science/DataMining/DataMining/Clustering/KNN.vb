#Region "Microsoft.VisualBasic::2905f4ef3a09e4ea46450f0ffe68761d, Data_science\DataMining\DataMining\Clustering\KNN.vb"

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

    '   Total Lines: 123
    '    Code Lines: 84 (68.29%)
    ' Comment Lines: 23 (18.70%)
    '    - Xml Docs: 73.91%
    ' 
    '   Blank Lines: 16 (13.01%)
    '     File Size: 5.11 KB


    '     Class KNN
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Classify
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar
Imports Microsoft.VisualBasic.ApplicationServices.Terminal.ProgressBar.Tqdm
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.DataMining.ComponentModel.EntityModels
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Math.SIMD
Imports Parallels = System.Threading.Tasks.Parallel
Imports std = System.Math

Namespace Clustering

    ''' <summary>
    ''' KNN classifier
    ''' </summary>
    Public Class KNN

        ''' <summary>
        ''' this holds the values of the training data
        ''' </summary>
        ReadOnly trainingSet As ClusterEntity()
        ''' <summary>
        ''' this holds the class associated with the values
        ''' </summary>
        ReadOnly clusterNames As Dictionary(Of Integer, String)

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="trainingSet">
        ''' should contains the <see cref="ClusterEntity.cluster"/> index value
        ''' </param>
        ''' <param name="clusterNames">
        ''' the class label that associated with the <see cref="ClusterEntity.cluster"/>
        ''' </param>
        Sub New(trainingSet As IEnumerable(Of ClusterEntity), clusterNames As Dictionary(Of Integer, String))
            Me.trainingSet = trainingSet.ToArray
            Me.clusterNames = clusterNames
        End Sub

        Sub New(trainingSet As IEnumerable(Of EntityClusterModel))
            Dim raw = trainingSet.ToArray
            Dim allNames As String() = raw _
                .Select(Function(d) d.Properties.Keys) _
                .IteratesALL _
                .Distinct _
                .ToArray

            Me.trainingSet = raw.Select(Function(r) r.ToModel(allNames)).ToArray
            Me.clusterNames = raw.Select(Function(i) i.Cluster) _
                .Distinct _
                .SeqIterator _
                .ToDictionary(Function(i) i + 1,
                              Function(i)
                                  Return i.value
                              End Function)

            Dim clusterIndex As Dictionary(Of String, Integer) = clusterNames _
                .ToDictionary(Function(d) d.Value,
                              Function(d)
                                  Return d.Key
                              End Function)

            For i As Integer = 0 To Me.trainingSet.Length - 1
                Me.trainingSet(i).cluster = clusterIndex(raw(i).Cluster)
            Next
        End Sub

        Public Iterator Function Classify(testSet As NamedCollection(Of Double)(), K As Integer) As IEnumerable(Of NamedCollection(Of NamedValue(Of Integer)))
            ' 两个平铺缓冲代替 Double(1)()（旧实现为每个训练样本分配一个小数组：
            ' n 个 24 字节的数组头 + 间接寻址，在 10^5 级训练集上非常可观）。
            Dim distances As Double() = New Double(trainingSet.Length - 1) {}
            Dim bar As Tqdm.ProgressBar = Nothing
            Dim [option] As New ParallelOptions With {.MaxDegreeOfParallelism = std.Max(1, Environment.ProcessorCount)}

            ' start computing
            For Each test As Integer In TqdmWrapper.Range(0, testSet.Length, bar:=bar, wrap_console:=App.EnableTqdm)
                Dim test_vec As Double() = testSet(test).value
                Dim tr As ClusterEntity() = trainingSet

                Call bar.SetLabel(testSet(test).name)
                Call Parallels.For(
                    fromInclusive:=0,
                    toExclusive:=tr.Length,
                    parallelOptions:=[option],
                    body:=Sub(index)
#Disable Warning
                              ' 平方距离与距离同序，比较用平方可以省掉 n 次开方
                              distances(index) = SimdReduce.DistanceSquared(test_vec, tr(index).entityVector)
#Enable Warning
                          End Sub)

                ' 取前 K 小（有界堆 O(n log K)），替代 AsParallel().OrderBy().Take() 的全排序
                Dim top As Integer() = SimdSelect.TopKIndices(distances, std.Min(K, distances.Length))
                Dim result As New List(Of NamedValue(Of Integer))(top.Length)

                For Each index As Integer In top
                    Dim d As Integer = tr(index).cluster
                    Dim predictedClass = clusterNames(d)
                    Dim output As New NamedValue(Of Integer) With {
                        .Name = testSet(test).name,
                        .Value = d,
                        .Description = predictedClass
                    }

                    Call result.Add(output)
                Next

                Yield New NamedCollection(Of NamedValue(Of Integer)) With {
                    .name = testSet(test).name,
                    .value = result.ToArray
                }
            Next
        End Function
    End Class
End Namespace
