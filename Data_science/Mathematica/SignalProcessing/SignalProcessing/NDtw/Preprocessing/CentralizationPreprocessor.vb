Imports System.Runtime.CompilerServices

Namespace NDtw.Preprocessing

    ''' <summary>
    ''' f(x) = x - mean
    ''' </summary>
    Public Class CentralizationPreprocessor : Implements IPreprocessor

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Preprocess(data As Double()) As Double() Implements IPreprocessor.Preprocess
            Return SIMD.Subtract.f64_op_subtract_f64_scalar(data, data.Average)
        End Function

        Public Overrides Function ToString() As String Implements IPreprocessor.ToString
            Return "Centralization"
        End Function
    End Class
End Namespace
