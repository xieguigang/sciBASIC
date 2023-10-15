Imports Microsoft.VisualBasic.Math.Calculus
Imports pnormf = Microsoft.VisualBasic.Math.Distributions.pnorm
Imports std = System.Math

Namespace Hypothesis

    Module NormalDistribution

        Sub New()
            Call pnormf.SetRK45(AddressOf TrapezodialRule)
        End Sub

        Public Function pnorm(q#,
                              Optional mean# = 0,
                              Optional sd# = 1,
                              Optional lower_tail As Boolean = True,
                              Optional logP As Boolean = False,
                              Optional resolution% = 30000) As Double
            Dim p#

            If lower_tail Then
                p = pnormf.BelowStandardDistribution(q, resolution, mean, sd)
            Else
                p = pnormf.AboveStandardDistribution(q, resolution, mean, sd)
            End If

            If logP Then
                Return std.Log10(p)
            Else
                Return p
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="a#"></param>
        ''' <param name="b#"></param>
        ''' <param name="resolution">计算的分辨率，越大越好</param>
        ''' <param name="m#"></param>
        ''' <param name="sd#"></param>
        ''' <returns></returns>
        Public Function TrapezodialRule(a#, b#, resolution%, m#, sd#) As Double
            Dim a1 As Double = pnormf.ProbabilityDensity(a, m, sd)
            Dim b1 As Double = pnormf.ProbabilityDensity(b, m, sd)
            Dim c As Double = 0.5 * (a1 + b1)
            Dim ode As New ODE With {.df = Function(xi, yi) pnormf.ProbabilityDensity(xi, m, sd), .ID = "pnorm", .y0 = c}
            Dim dx As Double = (b - a) / resolution
            Dim dxy = ode.RK4(resolution, a, b)

            c = dxy.sum

            Return dx * c
        End Function
    End Module
End Namespace