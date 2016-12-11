#Region "Microsoft.VisualBasic::7133d03cae929af0a6170122329b6c93, ..\sciBASIC#\Data_science\Bootstrapping\Monte-Carlo\Bifurcation\ParameterBifurcations.vb"

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
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Mathematical.Calculus
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
    End Module
End Namespace
