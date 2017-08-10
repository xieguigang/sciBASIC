#Region "Microsoft.VisualBasic::210920fd496d9844eb0923c06db66750, ..\sciBASIC#\Data_science\Bootstrapping\EigenvectorBootstrapping.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Language.UnixBash
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Calculus
Imports Microsoft.VisualBasic.Serialization.JSON

Public Module EigenvectorBootstrapping

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="DIR"></param>
    ''' <param name="eigenvector"></param>
    ''' <param name="partN">将原始数据分解为多少个数据块来抽取特征向量从而进行数据采样</param>
    ''' <returns></returns>
    <Extension>
    Public Function LoadData(DIR As String, eigenvector As Dictionary(Of String, Eigenvector), Optional partN As Integer = 20) As IEnumerable(Of VectorTagged(Of Dictionary(Of String, Double)))
        Return (ls - l - r - wildcards("*.csv") <= DIR) _
            .Select(AddressOf ODEsOut.LoadFromDataFrame) _
            .Sampling(eigenvector, partN)
    End Function

    Public Function GetVars(DIR As String) As String()
        Dim data As ODEsOut = ODEsOut.LoadFromDataFrame((ls - l - r - wildcards("*.csv") <= DIR).First)
        Return data.y.Keys.ToArray
    End Function

    Public Function DefaultEigenvector(DIR As String) As Dictionary(Of String, Eigenvector)
        Return GetVars(DIR).DefaultEigenvector
    End Function

    <Extension>
    Public Function DefaultEigenvector(vars As IEnumerable(Of String)) As Dictionary(Of String, Eigenvector)
        Dim vec As New Dictionary(Of String, Eigenvector)
        For Each var As String In vars
            vec(var) = AddressOf DefaultEigenvector
        Next
        Return vec
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="data"><see cref="LoadData"/>的输出数据</param>
    ''' <param name="n">所期望的Kmeans集合的数量</param>
    ''' <returns></returns>
    <Extension>
    Public Function KMeans(data As IEnumerable(Of VectorTagged(Of Dictionary(Of String, Double))),
                              n As Integer,
                Optional [stop] As Integer = -1) _
                                As Dictionary(Of Double(), NamedValue(Of Dictionary(Of String, Double)())())

        Dim strTags As NamedValue(Of VectorTagged(Of Dictionary(Of String, Double)))() =
            LinqAPI.Exec(Of NamedValue(Of VectorTagged(Of Dictionary(Of String, Double)))) <=
 _
            From x As VectorTagged(Of Dictionary(Of String, Double))
            In data.AsParallel
            Select New NamedValue(Of VectorTagged(Of Dictionary(Of String, Double))) With {
                .Name = x.Tag.GetJson,
                .Value = x,
                .Description = x.TagStr
            }

        Call "Load data complete!".__DEBUG_ECHO

        Dim datasets As Entity() = strTags.ToArray(
            Function(x) New Entity With {
                .uid = x.Description,
                .Properties = x.Value.Tag  ' 在这里使用特征向量作为属性来进行聚类操作
        })

        Call "Creates dataset complete!".__DEBUG_ECHO

        Dim clusters = datasets.ClusterDataSet(n, debug:=True, [stop]:=[stop], parallel:=True)
        Dim out As New Dictionary(Of Double(), NamedValue(Of Dictionary(Of String, Double)())())
        Dim raw = (From x As NamedValue(Of VectorTagged(Of Dictionary(Of String, Double)))
                   In strTags
                   Select x
                   Group x By x.Name Into Group) _
                        .ToDictionary(Function(x) x.Name,
                                      Function(x) x.Group.ToArray)

        For Each cluster As KMeansCluster(Of Entity) In clusters
            Dim key As Double() = cluster.ClusterMean  ' out之中的key
            Dim tmp As New List(Of NamedValue(Of Dictionary(Of String, Double)()))   ' out之中的value

            For Each x As Entity In cluster
                Dim rawKey As String = x.Properties.GetJson
                Dim rawParams =
                    raw(rawKey).ToArray(Function(o) o.Value.value)

                tmp += New NamedValue(Of Dictionary(Of String, Double)()) With {
                    .Name = x.uid,
                    .Value = rawParams
                }
            Next

            out(key) = tmp.ToArray
        Next

        Return out
    End Function

    ''' <summary>
    ''' 默认的特征向量: ``{data.Average, data.StdError}``
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    Public Function DefaultEigenvector(data As Double()) As Double()
        Return {data.Average, data.StdError}
    End Function

    <Extension>
    Public Function GetSample(data As Double(), eig As Dictionary(Of String, Eigenvector), partN As Integer) As ODEsOut
        Dim serialSize As Integer = 2 * partN  ' DefaultEigenvector函数返回两个数据 * 分块的数量 --> 一个系列的总量
        Dim serials = data.Split(serialSize)
        Dim out As New ODEsOut With {
            .params = New Dictionary(Of String, Double),
            .y = New Dictionary(Of NamedCollection(Of Double))
        }

        For Each key As SeqValue(Of String) In eig.Keys.SeqIterator
            out.y(+key) = New NamedCollection(Of Double) With {
                .Name = +key,
                .Value = serials(key.i).Split(2).ToArray(Function(o) o(0))
            }
        Next

        out.x = out.y.Values _
            .First.Value _
            .Sequence _
            .Select(Function(x) CDbl(x)) _
            .ToArray

        Return out
    End Function
End Module

''' <summary>
''' 从目标数据集合之中抽取特征向量
''' </summary>
''' <param name="data"></param>
''' <returns></returns>
Public Delegate Function Eigenvector(data As Double()) As Double()
