Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo
Imports Microsoft.VisualBasic.DataMining.GAF
Imports Microsoft.VisualBasic.DataMining.GAF.Helper
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Mathematical.diffEq

Namespace GAF

    Public Class GAFfitness
        Implements Fitness(Of ParameterVector, Double)

        ''' <summary>
        ''' 真实的实验观察数据
        ''' </summary>
        Dim observation As ODEsOut
        ''' <summary>
        ''' 具体的计算模型
        ''' </summary>
        Dim model As Model
        Dim n%, a#, b#

        ''' <summary>
        ''' 从真实的实验观察数据来构建出拟合(这个构造函数是测试用的)
        ''' </summary>
        ''' <param name="observation"></param>
        Sub New(observation As Dictionary(Of String, Double), model As Model, n%, a#, b#)
            With Me
                .observation = model.RunTest(observation, n, a, b)
                .model = model
                .n = n
                .a = a
                .b = b
            End With
        End Sub

        Public Function Calculate(chromosome As ParameterVector) As Double Implements Fitness(Of ParameterVector, Double).Calculate
            Dim vars As Dictionary(Of String, Double) =
                chromosome _
                    .vars _
                    .ToDictionary(Function(var) var.Name,
                                  Function(var) var.GetValue)
            Dim out As ODEsOut = model.RunTest(vars, n, a, b)  ' 通过拟合的参数得到具体的计算数据
            Dim fit As New List(Of Double)

            For Each y As NamedValue(Of Double()) In observation.y.Values
                fit += FitnessHelper.Calculate(y.x, out.y(y.Name).x)   ' 再计算出fitness
            Next

            Return fit.Max
        End Function
    End Class
End Namespace