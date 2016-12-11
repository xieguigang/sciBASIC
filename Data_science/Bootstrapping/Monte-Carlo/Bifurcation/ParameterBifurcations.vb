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
                                      Optional rnd As IRandomSeeds = Nothing) As IEnumerable(Of NamedValue(Of VariableModel()))
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
                ncluster, nsubCluster,
                [stop],
                uidProvider:=uidProvider)
        End Function
    End Module
End Namespace