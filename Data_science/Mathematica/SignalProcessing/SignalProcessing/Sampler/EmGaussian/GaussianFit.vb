Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.Bootstrapping
Imports Microsoft.VisualBasic.Math.LinearAlgebra.Matrix
Imports Microsoft.VisualBasic.Math.Scripting

Namespace EmGaussian

    ''' <summary>
    ''' em-gaussian fit of a single data, fit time/spectrum/other sequential 
    ''' data with a set of gaussians by expectation-maximization algoritm.
    ''' </summary>
    Public Class GaussianFit

        Dim opts As Opts
        Dim peaks As Variable()
        Dim kernel As KernelFunction = AddressOf Variable.Multi_gaussian
        Dim sine_kernel As Boolean

        Public Delegate Function KernelFunction(x As Double, peaks As Variable(), offset As Double) As Double

        Sub New(opts As Opts, Optional sine_kernel As Boolean = False)
            Me.sine_kernel = sine_kernel
            Me.opts = opts

            If sine_kernel Then
                kernel = AddressOf Variable.Multi_sine
            End If
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="samples">the signal data should be normalized to range [0,1]</param>
        ''' <param name="npeaks"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function fit(samples As Double(), Optional npeaks As Integer = 6) As Variable()
            Return fit(seq(0, samples.Length).ToArray, samples, npeaks)
        End Function

        Public Function fit(x As Double(), samples As Double(), Optional npeaks As Integer = 6) As Variable()
            Dim ymax As Double = samples.Max
            Dim random As Variable() = Enumerable.Range(0, npeaks) _
                .Select(Function(v, i)
                            Return New Variable With {
                                .height = ymax * randf(0.5, 1.1),
                                .center = x((i / npeaks) * (x.Length - 1)),
                                .width = randf(0.01, 0.1),
                                .offset = 0.01
                            }
                        End Function) _
                .ToArray

            peaks = random
            gaussFit(x, samples)
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

        Private Sub gaussFit(x As Double(), samples As Double())
            Dim gauss As New GaussNewtonSolver(
                fitFunction:=AddressOf target,
                maxIterations:=opts.maxIterations,
                rmseTol:=opts.tolerance,
                iterTol:=opts.eps)
            Dim nsize As Integer = samples.Length
            Dim points As DataPoint() = samples _
                .Select(Function(yi, i) New DataPoint(x(i), yi)) _
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

            Return kernel(x, decode(v), offset:=0)
        End Function
    End Class
End Namespace