Imports System.Runtime.CompilerServices

Namespace EmGaussian

    Public Class GaussianFit

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function fit(samples, ParamArray components As Double()())
            Return fit(samples, New Opts With {.components = components})
        End Function

        Public Function fit(samples, Optional opts As Opts = Nothing)
            Dim detectComponents As Boolean = opts.components.IsNullOrEmpty
            ' initialize components
            Dim cn As Integer = opts.components.Length
            Dim components = opts.components _
                .Select(Function(v, i)
                            If v.IsNullOrEmpty Then
                                Return New Variable With {.weight = 1 / cn, .mean = i / cn, .variance = 1 / cn}
                            Else
                                Return New Variable With {.weight = v(0), .mean = v(1), .variance = v(2)}
                            End If
                        End Function) _
                .ToArray

            ' optimize components

        End Function
    End Class

    Public Class Variable

        Public Property weight As Double
        Public Property mean As Double
        Public Property variance As Double

    End Class
End Namespace