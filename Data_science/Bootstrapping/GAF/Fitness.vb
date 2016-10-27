Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.Bootstrapping.MonteCarlo
Imports Microsoft.VisualBasic.DataMining.Darwinism.GAF
Imports Microsoft.VisualBasic.DataMining.Darwinism.GAF.Helper
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Mathematical.Calculus
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

Namespace GAF

    ''' <summary>
    ''' 计算当前的最好的参数的fitness
    ''' </summary>
    ''' <param name="best">当前代之中的最好的参数</param>
    ''' <param name="fit"></param>
    ''' <returns></returns>
    Public Delegate Function FitnessCompute(best As ParameterVector, fit As GAFFitness) As Double

    Public Class GAFFitness
        Implements Fitness(Of ParameterVector, Double)

        ''' <summary>
        ''' 真实的实验观察数据
        ''' </summary>
        Dim observation As ODEsOut
        ''' <summary>
        ''' 具体的计算模型
        ''' </summary>
        Public ReadOnly Property Model As Type

        ''' <summary>
        ''' 计算的采样数
        ''' </summary>
        Dim samples%

#Region "Friend visit for dump debug module and run test for fitness calc"

        ''' <summary>
        ''' ODEs y0
        ''' </summary>
        Friend y0 As Dictionary(Of String, Double)
        ''' <summary>
        ''' RK4 parameters
        ''' </summary>
        Friend n%, a#, b#
#End Region

        Public log10Fitness As Boolean
        ''' <summary>
        ''' 被忽略比较的y变量名称
        ''' </summary>
        ''' <returns></returns>
        Public Property Ignores As String()

        ''' <summary>
        ''' 从真实的实验观察数据来构建出拟合(这个构造函数是测试用的)
        ''' </summary>
        ''' <param name="observation"></param>
        Sub New(observation As Dictionary(Of String, Double), model As MonteCarlo.Model, n%, a#, b#)
            With Me
                .observation = model.RunTest(observation, n, a, b)
                ._Model = model.GetType
                .n = n
                .a = a
                .b = b
            End With

            Call __init()
        End Sub

        ''' <summary>
        ''' 初始化一些共同的数据
        ''' </summary>
        Private Sub __init()
            With Me
                .samples = n / 100
                .y0 = observation _
                    .y _
                    .Values _
                    .ToDictionary(Function(v) v.Name,
                                  Function(y) y.x(0))
                .Ignores = {}

                Call .Model.FullName.Warning
            End With
        End Sub

        ''' <summary>
        ''' 从真实的实验观察数据来构建出拟合(这个构造函数是测试用的)
        ''' </summary>
        ''' <param name="observation"></param>
        Sub New(model As Type, observation As ODEsOut, initOverrides As Dictionary(Of String, Double))
            With Me
                .observation = observation
                ._Model = model
                .n = observation.x.Length
                .a = observation.x(0)
                .b = observation.x.Last
            End With

            Call __init()

            If Not initOverrides.IsNullOrEmpty Then
                For Each k$ In initOverrides.Keys
                    y0(k$) = initOverrides(k$)
                Next

                Call $"Overrides y0: {initOverrides.GetJson}".__DEBUG_ECHO
            End If
        End Sub

        Public Function Calculate(chromosome As ParameterVector) As Double Implements Fitness(Of ParameterVector, Double).Calculate
            Dim vars As Dictionary(Of String, Double) =
                chromosome _
                    .vars _
                    .ToDictionary(Function(var) var.Name,
                                  Function(var) var.value)
            Dim out As ODEsOut = ' y0使用实验观测值，而非突变的随机值
                MonteCarlo.Model.RunTest(Model, y0, vars, n, a, b)  ' 通过拟合的参数得到具体的计算数据
            Dim fit As New List(Of Double)
            Dim NaN%

            ' 再计算出fitness
            For Each y As NamedValue(Of Double()) In observation.y _
                .Values _
                .Where(Function(v)
                           Return Array.IndexOf(Ignores, v.Name) = -1
                       End Function)

                Dim sample1 = y.x.Split(samples, echo:=False)
                Dim sample2 = out.y(y.Name).x.Split(samples, echo:=False)
                Dim a#()
                Dim b#()

                If log10Fitness Then
                    a = sample1.ToArray(Function(x) log10(x.Max))
                    b = sample2.ToArray(Function(x) log10(x.Max))
                Else
                    a = sample1.ToArray(Function(x) x.Max)
                    b = sample2.ToArray(Function(x) x.Max)
                End If

                NaN% = b.Where(AddressOf IsNaNImaginary).Count
                fit += Math.Sqrt(FitnessHelper.Calculate(a#, b#)) ' FitnessHelper.Calculate(y.x, out.y(y.Name).x)   
            Next

            ' Return fit.Average
            Dim fitness# = fit.Average

            If fitness.IsNaNImaginary Then
                fitness = Integer.MaxValue * 100.0R
                fitness += NaN% * 10
            End If

            Return fitness
        End Function

        Private Function log10(x#) As Double
            If x = 0R Then
                Return -1000
            Else
                Return Math.Log10(Math.Abs(x))
            End If
        End Function
    End Class
End Namespace