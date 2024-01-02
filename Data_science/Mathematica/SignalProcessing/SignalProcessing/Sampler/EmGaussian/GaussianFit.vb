Imports Microsoft.VisualBasic.Math.Distributions
Imports std = System.Math

Namespace EmGaussian

    ''' <summary>
    ''' em-gaussian fit of a single data, fit time/spectrum/other sequential 
    ''' data with a set of gaussians by expectation-maximization algoritm.
    ''' </summary>
    Public Class GaussianFit

        Dim membership As Double()
        Dim opts As Opts
        Dim eps As Double

        Sub New(opts As Opts)
            Me.opts = opts
            Me.eps = opts.eps
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="samples">the signal data should be normalized to range [0,1]</param>
        ''' <param name="npeaks"></param>
        ''' <returns></returns>
        Public Function fit(samples As Double(), Optional npeaks As Integer = 6, Optional ByRef logp As Double() = Nothing) As Variable()
            Dim random As Variable() = Enumerable.Range(0, npeaks) _
                .Select(Function(v, i)
                            Return New Variable With {
                                .weight = 1 / npeaks,
                                .mean = i / npeaks,
                                .variance = 1 / npeaks
                            }
                        End Function) _
                .ToArray

            Return fit(samples, components:=random, logp:=logp)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="samples"></param>
        ''' <param name="components">initialize data, could be created from a random data</param>
        ''' <returns></returns>
        Public Function fit(samples As Double(), components As Variable(), Optional ByRef logp As Double() = Nothing) As Variable()
            ' optimize components
            Dim lastLikelihood As Double = Double.NegativeInfinity
            Dim dx As Double = (1 / samples.Length) / 2
            Dim x_axis As Double() = Enumerable.Range(0, samples.Length) _
                .Select(Function(xi) xi / samples.Length) _
                .ToArray
            Dim _logp As New List(Of Double)

            membership = New Double(components.Length * samples.Length - 1) {}
            ' samples = SIMD.Add.f64_op_add_f64_scalar(samples, pos_min)

            For i As Integer = 0 To opts.maxIterations - 1
                Dim lh = likelihood(samples, components)

                If std.Abs(lh - lastLikelihood) < opts.tolerance Then
                    Exit For
                End If

                _logp.Add(lh)
                components = optimize(samples, x_axis, dx, components)
                'Dim lh_new = likelihood(samples, components_new)

                'If lh_new > lh Then
                '    components = components_new
                'End If

                lastLikelihood = lh
            Next

            logp = _logp.ToArray

            Return components _
                .OrderByDescending(Function(c) c.weight) _
                .ToArray
        End Function

        ''' <summary>
        ''' calculate likelihood
        ''' </summary>
        ''' <param name="samples"></param>
        ''' <param name="components"></param>
        ''' <returns></returns>
        Private Function likelihood(samples As Double(), components As Variable()) As Double
            Dim l As Double = 0
            Dim p As Double = 0
            Dim n = samples.Length
            Dim comp As Variable
            Dim xi As Double

            For i As Integer = 0 To samples.Length - 1
                p = 0
                xi = i / n

                For c As Integer = 0 To components.Length - 1
                    comp = components(c)
                    p += std.Abs(samples(i) - comp.weight * pnorm.ProbabilityDensity(xi, comp.mean, comp.variance))
                Next
                If p = 0.0 Then
                    l += Double.MaxValue
                Else
                    l += -1 / std.Log(p)
                End If
            Next

            Return l
        End Function

        ''' <summary>
        ''' single iteration of em algorithm of one step for each component
        ''' </summary>
        ''' <param name="samples"></param>
        ''' <param name="components"></param>
        ''' <returns></returns>
        Private Function optimize(samples As Double(), x_axis As Double(), dx As Double, components As Variable()) As Variable()
            Dim new_components As Variable() = New Variable(components.Length - 1) {}

            For i As Integer = 0 To components.Length - 1
                Dim c As Variable = components(i)
                Dim offset As Double
                Dim xi As Double = c.mean

                If randf(0, 1) > 0.5 Then
                    offset = dx
                Else
                    offset = -dx
                End If

                xi += offset

                ' peaks can not be overlaps with each other
                If components.Any(Function(ci)
                                      If ci Is c Then
                                          Return False
                                      Else
                                          Return std.Abs(ci.mean - xi) < opts.tolerance
                                      End If
                                  End Function) Then

                    xi -= offset
                End If

                c = New Variable(c)
                c.mean = xi

                If c.mean < 0 Then
                    c.mean = 0
                End If

                Dim y = x_axis.Select(Function(xj) c.gauss(xj)).ToArray
                Dim xmax As Integer = c.mean * samples.Length - 1

                If xmax < 0 Then
                    xmax = 0
                ElseIf xmax >= samples.Length Then
                    xmax = samples.Length - 1
                End If

                Dim ymax As Double = samples(xmax)
                Dim dw As Double = ymax / y.Max

                If dw = 0.0 Then
                    dw = eps
                ElseIf dw.IsNaNImaginary Then
                    dw = 1.125
                End If

                If dw > 1 Then
                    c.weight *= dw
                ElseIf dw > eps Then
                    c.weight /= dw
                End If

                Dim width = (y.Where(Function(yi) yi >= 0.001).Count + 1) * dx

                dw = c.variance / width

                If dw > 1 Then
                    c.variance *= dw
                ElseIf dw > eps Then
                    c.variance /= dw
                End If

                new_components(i) = c
            Next

            Return new_components
        End Function
    End Class
End Namespace