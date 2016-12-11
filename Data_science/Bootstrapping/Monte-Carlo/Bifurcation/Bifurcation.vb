#Region "Microsoft.VisualBasic::4de3874f88cd69200d3d55dbe765e56f, ..\sciBASIC#\Data_science\Bootstrapping\Monte-Carlo\StatesCharacters.vb"

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
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Mathematical.Calculus
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace MonteCarlo

    ''' <summary>
    ''' Search for all possible system status clusters
    ''' </summary>
    ''' <remarks>
    ''' ###### Figure 3: Bifurcation analysis.
    ''' 
    ''' > For Each important parameters, performing bifurcation analysis to (1) find out 
    ''' > how many possible stable steady states in the system (model), for example, for 
    ''' > example, V, may have more than two states such as very low amount (may Not 
    ''' > enough to cause symptoms), very high amount (immediately cause symptoms), And 
    ''' > mediate amount (may have long “latency” period in the cell); (2) find out how 
    ''' > Virus (V, Or all the other five species) changes with its parameters' change 
    ''' > (the parameter regions). For easy understanding, for example, in some parameter 
    ''' > region, virus amount would decrease/increase significantly; whereas in another 
    ''' > region, V would have interesting phenomenon such as oscillating. 
    ''' </remarks>
    Public Module BifurcationAnalysis

        ''' <summary>
        ''' Search for all possible system status clusters by using MonteCarlo method from random system inits.
        ''' (在参数固定不变的情况下，使用不同的y变量初始值来计算，使用蒙特卡洛的方法来搜索可能的系统状态空间)
        ''' </summary>
        ''' <param name="model"></param>
        ''' <returns>可能的系统状态的KMeans聚类结果</returns>
        <Extension>
        Public Function KMeansCluster(model As Type,
                                      k&,
                                      n%, a#, b#,
                                      args As Dictionary(Of String, Double),
                                      Optional stop% = -1,
                                      Optional ncluster% = -1,
                                      Optional nsubCluster% = 3) As IEnumerable(Of NamedValue(Of VariableModel()))

            ' 整个系统使用随机初始值进行计算，从而可以使用蒙特卡洛的方法得到所有可能的系统状态
            Dim y0 = TryCast(Activator.CreateInstance(model), Model).yinit
            Dim y0rand = y0.Select(
                Function(v) New NamedValue(Of IValueProvider) With {
                    .Name = v.Name,
                    .Value = AddressOf v.GetValue
                })
            Dim validResults As IEnumerable(Of ODEsOut) =
                model.Bootstrapping(
                    y0, y0rand,
                    k, n, a, b,
                    trimNaN:=True,
                    parallel:=True)
            Dim ys$() = MonteCarlo.Model.GetVariables(model).ToArray

            ' 因为发生变化的是y0，参数没有变，所以只使用y0来标识Entity就行了
            Dim uid As Func(Of ODEsOut, String) =
                Function(v) v.y0.GetJson

            Return validResults.__clusterInternal(
                ys, ncluster, nsubCluster,
                [stop],
                uidProvider:=uid)
        End Function

        <Extension>
        Private Iterator Function __clusterInternal(validResults As IEnumerable(Of ODEsOut),
                                                    ys$(),
                                                    ncluster%, nsubCluster%, stop%,
                                                    uidProvider As Func(Of ODEsOut, String)) As IEnumerable(Of NamedValue(Of VariableModel()))

            Dim inputs As New List(Of Entity)  ' Kmeans的输入数据

            For Each v As ODEsOut In validResults
                inputs += New Entity With {
                    .uid = uidProvider(v),
                    .Properties = ys.Select(Function(name$) v.y(name).Value).ToVector
                }
            Next

            If ncluster <= 1 Then
                ncluster = inputs.Count / 10
            End If

            Dim result As ClusterCollection(Of Entity) =
                ClusterDataSet(ncluster, inputs,
                debug:=True,
                [stop]:=[stop],
                parallel:=True)

            For Each cluster As SeqValue(Of KMeansCluster(Of Entity)) In result.SeqIterator
                Dim inits As EntityLDM() =
                    (+cluster) _
                    .Select(Function(x) x.uid) _
                    .ToArray(Function(data) New EntityLDM With {
                        .Name = data,
                        .Properties = data.LoadObject(Of Dictionary(Of String, Double))
                    })

                ' 由于不同的组合也可能产生相同的系统状态，所以在这里是不是还需要做进一步的聚类？
                ' 从这里populates一个可能的系统状态的范围
                Dim parts% = nsubCluster
                Dim subclusters As EntityLDM()()

                If inits.Length < 3 Then ' 无法聚类
                    subclusters = {
                        inits
                    }
                Else
                    If parts >= inits.Length Then
                        If inits.Length = 3 Then
                            parts = 2
                        Else
                            parts = 3
                        End If
                    End If

                    subclusters = inits _
                        .Kmeans(parts) _
                        .GroupBy(Function(x) x.Cluster) _
                        .ToArray(Function(g) g.ToArray)
                End If

                For Each subc As EntityLDM() In subclusters
                    Dim status As Dictionary(Of String, Double()) =
                        ys.ToDictionary(
                        Function(name$) name,
                        Function(name$) subc _
                            .Select(Function(x) x.Properties(name)) _
                            .ToArray)
                    Dim means As Dictionary(Of String, Double) =
                        status.ToDictionary(Function(kk) kk.Key,
                                            Function(kk) kk.Value.Average)

                    Yield New NamedValue(Of VariableModel()) With {
                        .Name = cluster.i & "::" & means.GetJson,
                        .Value = status _
                            .ToArray(Function(s) New VariableModel With {
                                .Name = s.Key,
                                .Min = s.Value.Min,
                                .Max = s.Value.Max
                            })
                    }
                Next
            Next
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="x#">通过拟合所得到的一个具体的值</param>
        ''' <param name="ldelta#">小数位往下浮动多少</param>
        ''' <param name="udelta#">小数位往上浮动多少</param>
        ''' <param name="rnd"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetRandomRange(x#, ldelta#, udelta#, Optional rnd As IRandomSeeds = Nothing) As PreciseRandom
            Dim pow As Double = Math.Log10(x)
            ldelta = pow - ldelta
            udelta = pow + udelta

            Dim out As New PreciseRandom(CInt(ldelta), CInt(udelta), rnd)
            Return out
        End Function
    End Module
End Namespace
