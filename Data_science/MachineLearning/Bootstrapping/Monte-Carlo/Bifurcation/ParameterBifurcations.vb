#Region "Microsoft.VisualBasic::edea566d8500930cf4d2ce97d5987d33, sciBASIC#\Data_science\MachineLearning\Bootstrapping\Monte-Carlo\Bifurcation\ParameterBifurcations.vb"

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

    '   Total Lines: 120
    '    Code Lines: 92
    ' Comment Lines: 17
    '   Blank Lines: 11
    '     File Size: 5.40 KB


    '     Module BifurcationAnalysis
    ' 
    '         Function: KMeansCluster, Run
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Calculus
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace MonteCarlo

    Partial Public Module BifurcationAnalysis

        ''' <summary>
        ''' Search for all possible system status clusters by using MonteCarlo method from random system parameter.
        ''' (在变量的初始值固定不变的情况下，使用不同的参数变量值来计算，使用蒙特卡洛的方法来搜索可能的系统状态空间)
        ''' </summary>
        ''' <param name="model"></param>
        ''' <param name="args">参数值所限定的变化范围，``参数值，数位浮动下限，数位浮动的上限``</param>
        ''' <returns>可能的系统状态的KMeans聚类结果</returns>
        <Extension>
        Public Function KMeansCluster(model As Type,
                                      k&,
                                      data As ODEsOut,
                                      args As Dictionary(Of String, (ld#, ud#)),
                                      Optional stop% = -1,
                                      Optional ncluster% = -1,
                                      Optional nsubCluster% = 3,
                                      Optional rnd As IRandomSeeds = Nothing) As IEnumerable(Of Cluster)
            Dim n% = data.x.Length
            Dim a# = data.x(0)
            Dim b# = data.x.Last
            Dim y0 = LinqAPI.Exec(Of NamedValue(Of IValueProvider)) <=
                From v As String
                In MonteCarlo.Model.GetVariables(model)
                Let value As Double = data.params(v)
                Select New NamedValue(Of IValueProvider) With {
                    .Name = v,
                    .Value = Function() value
                }
            Dim params As NamedValue(Of IValueProvider)() =
                LinqAPI.Exec(Of NamedValue(Of IValueProvider)) <=
 _
                From v As String
                In MonteCarlo.Model.GetParameters(model)
                Let value As Double = data.params(v)
                Let range As (ld#, ud#) = args(v)
                Let rndSeed As PreciseRandom = value.GetRandomRange(range.ld, range.ud, rnd)
                Select New NamedValue(Of IValueProvider) With {
                    .Name = v,
                    .Value = AddressOf rndSeed.NextNumber
                }
            Dim results As IEnumerable(Of ODEsOut) =
                model.Bootstrapping(params, y0, k, n, a, b,, True)
            Dim uidProvider As Func(Of ODEsOut, String) = Function(v) v.params.GetJson

            Return results.__clusterInternal(
                y0.Select(Function(x) x.Name).ToArray,
                ncluster,
                [stop],
                uidProvider:=uidProvider)
        End Function

        ''' <summary>
        ''' 返回来的结果是按照突变的参数进行从小到大排序了的
        ''' </summary>
        ''' <param name="model"></param>
        ''' <param name="base"></param>
        ''' <param name="param$"></param>
        ''' <param name="range"></param>
        ''' <param name="n%"></param>
        ''' <param name="parallel"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function Run(model As Type,
                                     base As ODEsOut,
                                     param$,
                                     range As DoubleRange,
                                     Optional n% = 10,
                                     Optional parallel As Boolean = False) As IEnumerable(Of ODEsOut)

            Dim l = base.x.Length
            Dim a = base.x(Scan0)
            Dim b = base.x.Last

            For Each x As KeyValuePair(Of String, Double) In base.params
                Call App.JoinVariable(x.Key, x.Value)
            Next

            Dim ranges#() = range.Enumerate(n)

            If Not parallel Then
                Dim __run As Func(Of Double, ODEsOut) =
                    Function(x#)
                        Dim params As New Dictionary(Of String, Double)(base.params)
                        params(param) = x
                        Dim out As ODEsOut = MonteCarlo.Model _
                            .RunTest(model, params, params, l, a, b)
                        Return out
                    End Function

                For Each x As Double In ranges
                    Yield __run(x)
                Next
            Else
                Dim LQuery = From x As Double
                             In ranges.AsParallel
                             Let params = New Dictionary(Of String, Double)(base.params) _
                                 .Join(param, x)
                             Select x,
                                 out = MonteCarlo.Model _
                                    .RunTest(model, params, params, l, a, b)
                             Order By x Ascending

                For Each value In LQuery
                    Yield value.out
                Next
            End If
        End Function
    End Module
End Namespace
