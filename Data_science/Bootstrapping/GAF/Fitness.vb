Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo
Imports Microsoft.VisualBasic.DataMining.GAF
Imports Microsoft.VisualBasic.DataMining.GAF.Helper
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Mathematical.diffEq
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Text

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
        Dim model As Type
        Dim n%, a#, b#
        ''' <summary>
        ''' 计算的采样数
        ''' </summary>
        Dim samples%

        Public log10Fitness As Boolean

        ''' <summary>
        ''' 从真实的实验观察数据来构建出拟合(这个构造函数是测试用的)
        ''' </summary>
        ''' <param name="observation"></param>
        Sub New(observation As Dictionary(Of String, Double), model As Model, n%, a#, b#)
            With Me
                .observation = model.RunTest(observation, n, a, b)
                .model = model.GetType
                .n = n
                .a = a
                .b = b
                .samples = n / 100

                Call .model.FullName.Warning
            End With
        End Sub

        Public Function Calculate(chromosome As ParameterVector) As Double Implements Fitness(Of ParameterVector, Double).Calculate
            Dim vars As Dictionary(Of String, Double) =
                chromosome _
                    .vars _
                    .ToDictionary(Function(var) var.Name,
                                  Function(var) var.value)
            Dim out As ODEsOut = MonteCarlo.Model.RunTest(model, vars, n, a, b)  ' 通过拟合的参数得到具体的计算数据
            Dim fit As New List(Of Double)
            Dim NaN%

            For Each y As NamedValue(Of Double()) In observation.y.Values
                ' 再计算出fitness
                Dim sample1 = y.x.Split(samples)
                Dim sample2 = out.y(y.Name).x.Split(samples)
                Dim a#()
                Dim b#()

                If log10Fitness Then
                    a = sample1.ToArray(Function(x) Math.Log10(x.Max))
                    b = sample2.ToArray(Function(x) Math.Log10(x.Max))
                Else
                    a = sample1.ToArray(Function(x) x.Max)
                    b = sample2.ToArray(Function(x) x.Max)
                End If

                NaN% = b.Where(AddressOf Is_NA_UHandle).Count
                fit += Math.Sqrt(FitnessHelper.Calculate(a#, b#)) ' FitnessHelper.Calculate(y.x, out.y(y.Name).x)   
            Next

            ' Return fit.Average
            Dim fitness# = fit.Average

            If fitness.Is_NA_UHandle Then
                fitness = Single.MaxValue
                fitness += NaN% * 10
            End If

            Return fitness
        End Function
    End Class
End Namespace