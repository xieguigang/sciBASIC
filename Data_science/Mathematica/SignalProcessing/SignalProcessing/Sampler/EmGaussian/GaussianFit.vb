Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix

Namespace EmGaussian

    ''' <summary>
    ''' em-gaussian fit of a single data, fit time/spectrum/other sequential 
    ''' data with a set of gaussians by expectation-maximization algoritm.
    ''' </summary>
    Public Class GaussianFit

        Dim opts As Opts
        Dim peaks As Variable()

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
                                .height = samples.Max / 10000,
                                .center = i / npeaks,
                                .width = 0.000005,
                                .offset = 0.0001
                            }
                        End Function) _
                .ToArray

            peaks = random
            gaussFit(samples)
            Return peaks
        End Function

        Private Function getBeta0() As Double()
            Dim args As Double() = New Double(peaks.Length * Variable.argument_size - 1) {}
            Dim offset As Integer

            For i As Integer = 0 To peaks.Length - 1
                offset = i * Variable.argument_size
                args(offset) = peaks(i).height
                args(offset + 1) = peaks(i).center
                args(offset + 2) = peaks(i).width
                args(offset + 3) = peaks(i).offset
            Next

            Return args
        End Function

        Private Sub gaussFit(samples As Double())
            Dim gauss As New GaussNewtonSolver(
                fitFunction:=AddressOf target,
                maxIterations:=opts.maxIterations,
                rmseTol:=opts.tolerance,
                iterTol:=opts.eps)
            Dim nsize As Integer = samples.Length
            Dim points As DataPoint() = samples _
                .Select(Function(yi, i) New DataPoint(i / nsize, yi)) _
                .ToArray
            Dim result As Double() = gauss.Fit(points, getBeta0)

            peaks = decode(result)
        End Sub

        Private Function decode(args As Double()) As Variable()
            Dim i As Integer = 0

            For offset As Integer = 0 To args.Length - 1 Step Variable.argument_size
                peaks(i).height = args(offset)
                peaks(i).center = args(offset + 1)
                peaks(i).width = args(offset + 2)
                peaks(i).offset = args(offset + 3)

                i += 1
            Next

            Return peaks
        End Function

        Private Function target(x As Double, args As NumericMatrix) As Double
            Dim v As Double() = New Double(peaks.Length * Variable.argument_size - 1) {}

            For i As Integer = 0 To v.Length - 1
                v(i) = args(i, 0)
            Next

            Return Variable.Multi_gaussian(x, decode(v), offset:=0)
        End Function
    End Class
End Namespace