#Region "Microsoft.VisualBasic::bb21fe9eae1bbe28eb46b4fedf5f2e70, ..\sciBASIC#\Data_science\Mathematica\Plot\Plots-statistics\Heatmap\PlotExtensions.vb"

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
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Correlations
Imports Microsoft.VisualBasic.Math.Correlations.Correlations

Namespace Heatmap

    Public Module PlotExtensions

        ''' <summary>
        ''' 相比于<see cref="LoadDataSet(String, String, Boolean, Correlations.ICorrelation)"/>函数，这个函数处理的是没有经过归一化处理的原始数据
        ''' </summary>
        ''' <param name="data"></param>
        ''' <param name="correlation">假若这个参数为空，则默认使用<see cref="Correlations.GetPearson(Double(), Double())"/></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function CorrelatesNormalized(
                                    data As IEnumerable(Of DataSet),
                    Optional correlation As Correlations.ICorrelation = Nothing) _
                                         As IEnumerable(Of NamedValue(Of Dictionary(Of String, Double)))

            Dim dataset As DataSet() = data.ToArray
            Dim keys$() = dataset(Scan0) _
                .Properties _
                .Keys _
                .ToArray

            If correlation Is Nothing Then
                correlation = AddressOf Correlations.GetPearson
            End If

            For Each x As DataSet In dataset
                Dim out As New Dictionary(Of String, Double)
                Dim array As Double() = keys.Select(Function(o$) x(o))

                For Each y As DataSet In dataset
                    out(y.ID) = correlation(
                    array,
                    keys.Select(Function(o) y(o)))
                Next

                Yield New NamedValue(Of Dictionary(Of String, Double)) With {
                    .Name = x.ID,
                    .Value = out
                }
            Next
        End Function

        ''' <summary>
        ''' (这个函数是直接加在已经计算好了的相关度数据).假若使用这个直接加载数据来进行heatmap的绘制，
        ''' 请先要确保数据集之中的所有数据都是经过归一化的，假若没有归一化，则确保函数参数
        ''' <paramref name="normalization"/>的值为真
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="uidMap$"></param>
        ''' <param name="normalization">是否对输入的数据集进行归一化处理？</param>
        ''' <param name="correlation">
        ''' 默认为<see cref="Correlations.GetPearson(Double(), Double())"/>方法
        ''' </param>
        ''' <returns></returns>
        <Extension>
        Public Function LoadDataSet(path As String,
                                    Optional uidMap$ = Nothing,
                                    Optional normalization As Boolean = False,
                                    Optional correlation As ICorrelation = Nothing) As NamedValue(Of Dictionary(Of String, Double))()

            Dim ds As IEnumerable(Of DataSet) = DataSet.LoadDataSet(path, uidMap)

            If normalization Then
                Return ds.CorrelatesNormalized(correlation).ToArray
            Else
                Return LinqAPI.Exec(Of NamedValue(Of Dictionary(Of String, Double))) _
 _
                    () <= From x As DataSet
                          In ds
                          Select New NamedValue(Of Dictionary(Of String, Double)) With {
                              .Name = x.ID,
                              .Value = x.Properties
                          }
            End If
        End Function

        <Extension>
        Public Function KmeansReorder(data As NamedValue(Of Dictionary(Of String, Double))(), Optional n% = 5) As NamedValue(Of Dictionary(Of String, Double))()
            Dim keys$() = data(Scan0%).Value.Keys.ToArray
            Dim entityList As Entity() = LinqAPI.Exec(Of Entity) _
 _
                () <= From x As NamedValue(Of Dictionary(Of String, Double))
                      In data
                      Select New Entity With {
                          .uid = x.Name,
                          .Properties = keys _
                              .Select(Function(k) x.Value(k)) _
                              .ToArray
                      }

            Dim clusters As ClusterCollection(Of Entity)

            n = entityList.Length / n

            If n = 0 OrElse entityList.Length <= 2 Then
                clusters = New ClusterCollection(Of Entity)

                For Each x As Entity In entityList
                    Dim c As New KMeansCluster(Of Entity)

                    Call c.Add(x)
                    Call clusters.Add(c)
                Next
            Else
                clusters = entityList.ClusterDataSet(n)
            End If

            Dim out As New List(Of NamedValue(Of Dictionary(Of String, Double)))

            ' 通过kmeans计算出keys的顺序
            Dim keysEntity = keys _
                .Select(Function(k)
                            Return New Entity With {
                                .uid = k,
                                .Properties = data _
                                    .Select(Function(x) x.Value(k)) _
                                    .ToArray
                            }
                        End Function) _
                .ToArray

            Dim keysOrder As New List(Of String)

            For Each cluster In keysEntity.ClusterDataSet(CInt(keys.Length / 5))
                For Each k In cluster
                    keysOrder += k.uid
                Next
            Next

            For Each cluster In clusters
                For Each entity As Entity In cluster
                    out += New NamedValue(Of Dictionary(Of String, Double)) With {
                    .Name = entity.uid,
                    .Value = keysOrder _
                        .SeqIterator _
                        .ToDictionary(Function(x) x.value,
                                      Function(x) entity.Properties(x.i))
                }
                Next
            Next

            Return out
        End Function
    End Module
End Namespace
