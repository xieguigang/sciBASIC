Imports System.Runtime.CompilerServices
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

        Sub New(opts As Opts)
            Me.opts = opts
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="samples">the signal data should be normalized to range [0,1]</param>
        ''' <param name="npeaks"></param>
        ''' <returns></returns>
        Public Function fit(samples As Double(), Optional npeaks As Integer = 6) As Variable()
            Dim random As Variable() = Enumerable.Range(0, npeaks) _
                .Select(Function(v, i)
                            Return New Variable With {
                                .weight = 1 / npeaks,
                                .mean = i / npeaks,
                                .variance = 1 / npeaks
                            }
                        End Function) _
                .ToArray

            Return fit(samples, components:=random)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="samples"></param>
        ''' <param name="components">initialize data, could be created from a random data</param>
        ''' <returns></returns>
        Public Function fit(samples As Double(), components As Variable()) As Variable()
            ' optimize components
            Dim lastLikelihood As Double = Double.NegativeInfinity

            membership = New Double(components.Length * samples.Length - 1) {}

            For i As Integer = 0 To opts.maxIterations - 1
                Dim lh = likelihood(samples, components)

                If std.Abs(lh - lastLikelihood) < opts.tolerance Then
                    Exit For
                End If

                components = optimize(samples, components)
                lastLikelihood = lh
            Next

            Return components
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

            For i As Integer = 0 To samples.Length - 1
                p = 0
                For c As Integer = 0 To components.Length - 1
                    comp = components(c)
                    p += samples(i) * comp.weight * pnorm.ProbabilityDensity(i / n, comp.mean, comp.variance)
                Next
                If p = 0.0 Then
                    l += -99999999
                Else
                    l += std.Log(p)
                End If
            Next

            Return l
        End Function

        ''' <summary>
        ''' single iteration of em algorithm
        ''' </summary>
        ''' <param name="samples"></param>
        ''' <param name="components"></param>
        ''' <returns></returns>
        Private Function optimize(samples As Double(), components As Variable()) As Variable()
            Dim comp As Variable

            For i As Integer = 0 To samples.Length - 1
                Dim x = i / samples.Length
                Dim p = New Double(components.Length - 1) {}

                ' total probability at the point
                Dim sump As Double = 0

                For c As Integer = 0 To components.Length - 1
                    comp = components(c)
                    p(c) = comp.weight * pnorm.ProbabilityDensity(x, comp.mean, comp.variance)
                    sump += p(c)
                Next

                ' [c0, c1, c2, c0, c1, c2, ...]
                For c As Integer = 0 To components.Length - 1
                    If p(c) = 0.0 Then
                        membership(i * components.Length + c) = 0
                    Else
                        membership(i * components.Length + c) = samples(i) * p(c) / sump
                    End If
                Next
            Next

            ' M-step: update components to better cover member samples
            Dim w = New Double(components.Length - 1) {}
            Dim sumw As Double = 0

            For c As Integer = 0 To components.Length - 1
                For i As Integer = 0 To samples.Length - 1
                    w(c) += membership(i * components.Length + c)
                Next
                sumw += w(c)
            Next

            Dim n = samples.Length

            Return components _
                .Select(Function(component, c)
                            ' get new amp as ratio of the total weight
                            If w(c) = 0.0 Then
                                component.weight = 0
                            Else
                                component.weight = w(c) / sumw
                            End If

                            ' get new mean as weighted by ratios value
                            Dim sumu As Double = 0.0

                            For i As Integer = 0 To samples.Length - 1
                                sumu += (i / n) * membership(i * components.Length + c)
                            Next

                            If w(c) = 0.0 Then
                                component.mean = randf(0, 1)
                            Else
                                component.mean = sumu / w(c)
                            End If

                            ' get new variations as weighted by ratios stdev
                            Dim sumv As Double = 0
                            For i As Integer = 0 To samples.Length - 1
                                sumv += membership(i * components.Length + c) * (i / n - component.mean) ^ 2
                            Next

                            If w(c) = 0.0 Then
                                component.variance = 0.00001
                            Else
                                component.variance = std.Max(sumv / w(c), 0.00001)
                            End If

                            Return component
                        End Function) _
                .ToArray
        End Function
    End Class
End Namespace