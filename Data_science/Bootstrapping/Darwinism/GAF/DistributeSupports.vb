Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.Darwinism.GAF.Helper
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical.Calculus

Namespace Darwinism.GAF

    ''' <summary>
    ''' Supports for GA distribute computing
    ''' </summary>
    Public Module DistributeSupports

        <Extension>
        Public Function GetFitness(model As Type, v As ParameterVector, observation As ODEsOut, ynames$(), y0 As Dictionary(Of String, Double), n%, t0#, tt#, log10Fitness As Boolean, ref As ODEsOut) As Double
            Dim vars As Dictionary(Of String, Double) = v _
                .vars _
                .ToDictionary(Function(var) var.Name,
                              Function(var) var.value)
            Dim out As ODEsOut = ' y0使用实验观测值，而非突变的随机值
                MonteCarlo.Model.RunTest(model, y0, vars, n, t0, tt, ref)  ' 通过拟合的参数得到具体的计算数据
            Dim fit As New List(Of Double)
            Dim NaN%

            ' 再计算出fitness
            For Each y As String In ynames
                Dim a#() = observation.y(y$).Value
                Dim b#() = out.y(y$).Value

                If log10Fitness Then
                    a = a.ToArray(Function(x) log10(x))
                    b = b.ToArray(Function(x) log10(x))
                    'Else
                    '    a = sample1.ToArray(Function(x) x.Max)
                    '    b = sample2.ToArray(Function(x) x.Max)
                End If

                NaN% = b.Where(AddressOf IsNaNImaginary).Count
                fit += Math.Sqrt(FitnessHelper.Calculate(a#, b#)) ' FitnessHelper.Calculate(y.x, out.y(y.Name).x)   
            Next

            Dim fitness As Double = fit.Average

            If fitness.IsNaNImaginary Then
                fitness = Integer.MaxValue * 100.0R
                fitness += NaN% * 10
            End If

            Return fitness
        End Function

        Public Function log10(x#) As Double
            If x = 0R Then
                Return -1000
            ElseIf x.IsNaNImaginary Then
                Return Double.NaN
            Else
                ' 假若不乘以符号，则相同指数级别的正数和负数之间的差异就会为0，
                ' 所以在这里需要乘以符号值
                Return Math.Sign(x) * Math.Log10(Math.Abs(x))
            End If
        End Function
    End Module
End Namespace