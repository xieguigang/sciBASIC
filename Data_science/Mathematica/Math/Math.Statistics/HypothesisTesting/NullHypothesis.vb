Imports Microsoft.VisualBasic.Math.Statistics.Hypothesis
Imports std = System.Math

Namespace Hypothesis

    Public MustInherit Class NullHypothesis(Of T)

        Public ReadOnly Property Permutation As Integer

        Sub New(Optional permutation As Integer = 1000)
            _Permutation = permutation
        End Sub

        ''' <summary>
        ''' generates the random set with <see cref="permutation"/> elements.
        ''' </summary>
        ''' <returns></returns>
        Public MustOverride Function ZeroSet() As IEnumerable(Of T)
        Public MustOverride Function Score(x As T) As Double

        Public Function Pvalue(score As Double, Optional alternative As Hypothesis = Hypothesis.Greater) As Double
            Dim zero As T() = ZeroSet.ToArray
            Dim n As Integer

            Select Case alternative
                Case Hypothesis.Greater
                    ' mu > mu0
                    n = Aggregate x As T
                        In zero.AsParallel
                        Let per_score As Double = Me.Score(x)
                        Where per_score >= score
                        Into Count
                Case Hypothesis.Less
                    ' mu < mu0
                    n = Aggregate x As T
                        In zero.AsParallel
                        Let per_score As Double = Me.Score(x)
                        Where per_score <= score
                        Into Count
                Case Hypothesis.TwoSided
                    ' 计算观测统计量的绝对值，用于双侧检验判断极端性
                    Dim abs_score As Double = std.Abs(score)

                    ' 双侧检验：置换统计量的绝对值 >= 观测统计量的绝对值
                    n = Aggregate x As T
                        In zero.AsParallel
                        Let per_score As Double = Me.Score(x)
                        Let abs_per_score As Double = std.Abs(per_score)
                        Where abs_per_score >= abs_score
                        Into Count
                Case Else
                    Throw New InvalidProgramException($"unknown alternative hypothesis: {alternative}!")
            End Select

            If alternative = Hypothesis.TwoSided Then
                Return 2 * (n + 1) / (Permutation + 1)
            Else
                Return (n + 1) / (Permutation + 1)
            End If
        End Function
    End Class
End Namespace