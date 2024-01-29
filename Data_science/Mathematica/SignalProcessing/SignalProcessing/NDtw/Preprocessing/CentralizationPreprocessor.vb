Imports System.Runtime.CompilerServices

Namespace NDtw.Preprocessing

    ''' <summary>
    ''' f(x) = x - mean
    ''' </summary>
    Public Class CentralizationPreprocessor : Inherits IPreprocessor

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function Preprocess(data As Double()) As Double()
            Return SIMD.Subtract.f64_op_subtract_f64_scalar(data, data.Average)
        End Function

        Public Overrides Function ToString() As String
            Return "Centralization"
        End Function
    End Class
End Namespace
