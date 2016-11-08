Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Mathematical.Calculus
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Linq

Namespace MonteCarlo

    ''' <summary>
    ''' System status cluster
    ''' </summary>
    Public Module SystemCluster

        ''' <summary>
        ''' 使用蒙特卡洛的方法来搜索可能的系统状态空间
        ''' </summary>
        ''' <param name="model"></param>
        ''' <returns>可能的系统状态的KMeans聚类结果</returns>
        <Extension>
        Public Iterator Function KMeansCluster(model As Type,
                                               k&, n%, a#, b#,
                                               args As Dictionary(Of String, Double),
                                               Optional stop% = -1) As IEnumerable(Of NamedValue(Of VariableModel()))

            Dim y0 = TryCast(Activator.CreateInstance(model), Model).yinit
            Dim y0rand = y0.Select(
                Function(v) New NamedValue(Of INextRandomNumber) With {
                    .Name = v.Name,
                    .x = AddressOf v.GetValue
                })
            Dim validResults As IEnumerable(Of ODEsOut) =
                model.Bootstrapping(
                    args, y0rand,
                    k, n, a, b,
                    trimNaN:=True,
                    parallel:=True)
            Dim inputs As New List(Of Entity)  ' Kmeans的输入数据
            Dim ys$() = MonteCarlo.Model.GetVariables(model).ToArray

            For Each v As ODEsOut In validResults
                inputs += New Entity With {
                    .uid = v.y0.GetJson, ' 因为发生变化的是y0，参数没有变，所以只使用y0来标识Entity就行了
                    .Properties = ys.Select(Function(name$) v.y(name).x).ToVector
                }
            Next

            Dim result As ClusterCollection(Of Entity) =
                ClusterDataSet(n, inputs,
                debug:=True,
                [stop]:=[stop],
                parallel:=True)

            For Each cluster In result
                Dim inits As Dictionary(Of String, Double)() = cluster _
                    .Select(Function(x) x.uid) _
                    .ToArray(AddressOf LoadObject(Of Dictionary(Of String, Double)))
                ' 由于不同的组合也可能产生相同的系统状态，所以在这里是不是还需要做进一步的聚类？
                ' 从这里populates一个可能的系统状态的范围
            Next
        End Function
    End Module
End Namespace