Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Mathematical.Calculus

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
        Public Iterator Function KMeansCluster(model As Type,
                                               k&,
                                               data As ODEsOut,
                                               args As Dictionary(Of String, (ld#, ud#)),
                                               Optional stop% = -1,
                                               Optional ncluster% = -1,
                                               Optional nsubCluster% = 3) As IEnumerable(Of NamedValue(Of VariableModel()))

        End Function
    End Module
End Namespace