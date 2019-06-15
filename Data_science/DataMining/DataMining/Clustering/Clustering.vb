#Region "Microsoft.VisualBasic::09871f196f3f8c533a462292b18388b1, Data_science\DataMining\DataMining\Clustering\Clustering.vb"

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

    '     Class SimpleCluster
    ' 
    '         Properties: d, Items, Kernel, offset
    ' 
    '         Function: Merge, Split, ToString
    ' 
    '     Module Clustering
    ' 
    '         Function: Clustering, InternalMergeJ
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Linq

Namespace Clustering

    Public Class SimpleCluster

        Public Property Kernel As Double
        Public Property d As Double
        Public Property Items As Double()

        ''' <summary>
        ''' 1 - 右偏移，即<see cref="Items"></see>里面的对象大部分都大于<see cref="Kernel"></see>
        ''' 0 - 不偏移，则比较有可能为一个核
        ''' -1 - 左偏移，即大部分对象都小于<see cref="Kernel"></see>
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property offset As Integer
            Get
                Dim kernel As Double = Me.Kernel   '分别计算左右偏移量
                Dim d As Double = 2 * Me.d
                Dim Right_LQuery = (From n In Items Where n - kernel > d Select 1).ToArray.Count / Items.Count

                d = -d
                Dim Left__LQuery = (From n In Items Let dd = n - kernel Where dd < 0 AndAlso dd < d Select 1).ToArray.Count / Items.Count

                '在比较左右偏移量来决定偏移的方向
                If Right_LQuery > 0.7 Then '
                    Return 1
                ElseIf Left__LQuery > 0.7 Then
                    Return -1
                Else
                    Return 0
                End If
            End Get
        End Property

        ''' <summary>
        ''' 核分裂
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Split() As SimpleCluster()
            Dim Chunk = (From n In Items Select n Order By n Ascending).ToArray
            Dim ChunkData = Chunk.Split(Chunk.Count / 2)
            Return {New SimpleCluster With {.Items = ChunkData.First, .d = d, .Kernel = ChunkData.First.Average}, New SimpleCluster With {.Items = ChunkData.Last, .Kernel = ChunkData.Last.Average, .d = d}}
        End Function

        Public Function Merge(value As SimpleCluster) As SimpleCluster
            Me.Items = {Items, value.Items}.ToVector
            Return Me
        End Function

        Public Overrides Function ToString() As String
            Return String.Format("{0} ===> {1}; offset is {2}", String.Join(",", Items), Kernel, offset)
        End Function
    End Class


    <Package("Clustering", Publisher:="xie.guigang@gmail.com")>
    Public Module Clustering

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="d">点之间的间距大小，当小于这个距离的任意两个点都会被划分为一个分类</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Clustering")>
        Public Function Clustering(data As IEnumerable(Of Double), <Parameter("Distance")> d As Double) As SimpleCluster()
            Dim Ordered = (From n In data Select n Order By n Ascending).ToArray '从小到大排序
            Dim an = data.Average
            If an - Ordered.First <= d AndAlso Ordered.Last - an <= d Then
                Return {New SimpleCluster With {.Kernel = an, .Items = data.ToArray, .d = d}}
            End If

            Dim inits As Double = (data.Count / 4)
            If inits < 1.5 Then
                inits = data.Count / 2
                If inits <= 2 Then
                    Dim NNN = (From n In data Select New SimpleCluster With {.Items = {n}, .Kernel = n, .d = d}).ToArray
                    Return NNN
                End If
            End If
            Dim ChunkData = Ordered.Split(inits)
            Dim LQuery = (From kernel As Double()
                          In ChunkData
                          Select New SimpleCluster With {
                              .Kernel = kernel.Average,
                              .Items = kernel,
                              .d = d}).AsList '进行初筛
            '出现偏移的核都被合并进入偏向性的核之中，进行递归聚类
            Dim get_Merged = Function(p As Integer) As SimpleCluster
                                 If p < 0 OrElse p = LQuery.Count Then
                                     Return Nothing     '核分裂
                                 Else
                                     Return LQuery(p)
                                 End If
                             End Function
            Dim Offset = (From i In LQuery.SeqIterator
                          Let kk = i.value
                          Let p = kk.offset
                          Where p <> 0
                          Let merged_into = get_Merged(i.i - p)
                          Select kernel = kk,
                              MergedInto = merged_into).ToArray

            For Each ker In Offset
                If ker.MergedInto Is Nothing Then '核分裂
                    Call LQuery.Remove(ker.kernel)
                    Call LQuery.AddRange(ker.kernel.Split)

                    Continue For
                End If

                Call ker.MergedInto.Merge(ker.kernel)
                Call LQuery.Remove(ker.kernel) '移除被合并的核
            Next

            '递归聚类
            Dim HigherLevel = Clustering((From item In LQuery Select item.Kernel).ToArray, d)
            LQuery = (From item In HigherLevel
                      Let sub_kernels = (From kkk In LQuery Where Array.IndexOf(item.Items, kkk.Kernel) > -1 Select kkk).ToArray
                      Select New SimpleCluster With
                         {
                             .Kernel = item.Kernel,
                             .Items = (From sk In sub_kernels Select sk.Items).ToArray.ToVector, .d = item.d}).AsList

            If LQuery.Count = 1 Then '在阈值d之下已经无法再聚类的，则必须要退出递归
                Return LQuery.ToArray
            End If

            Dim ChunkBuffer = (From item In LQuery Select Clustering(item.Items, d)).ToArray.ToVector
            '  ChunkBuffer = InternalMergeJ(ChunkBuffer, d)
            Return ChunkBuffer
        End Function

        Public Function InternalMergeJ(data As SimpleCluster(), d As Double) As SimpleCluster()
            If data.Count = 1 Then
                Return data
            End If

            For i As Integer = 0 To data.Count - 2
                Dim [next] = data(i + 1)
                Dim item = data(i)
                Dim LQuery = (From n In item.Items Where n - [next].Kernel <= d Select n).ToArray

                [next].Items = {[next].Items, LQuery}.ToVector
                [next].Kernel = [next].Items.Average
                Dim tmpList = item.Items.AsList
                For Each n In LQuery
                    Call tmpList.Remove(n)
                Next
            Next

            Return data
        End Function
    End Module
End Namespace
