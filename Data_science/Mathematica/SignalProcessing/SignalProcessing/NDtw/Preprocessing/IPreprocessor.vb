Imports System.Runtime.CompilerServices

Namespace NDtw.Preprocessing

    Public MustInherit Class IPreprocessor

        ''' <summary>
        ''' apply of the data processor function
        ''' </summary>
        ''' <param name="data"></param>
        ''' <returns></returns>
        Default Public ReadOnly Property [Function](data As Double()) As Double()
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return Preprocess(data)
            End Get
        End Property

        Public MustOverride Function Preprocess(data As Double()) As Double()
        Public MustOverride Overrides Function ToString() As String

        <DebuggerStepThrough>
        Public Shared Function Centralization() As CentralizationPreprocessor
            Return New CentralizationPreprocessor
        End Function

        <DebuggerStepThrough>
        Public Shared Function None() As NonePreprocessor
            Return New NonePreprocessor
        End Function

        <DebuggerStepThrough>
        Public Shared Function Normalization() As NormalizationPreprocessor
            Return New NormalizationPreprocessor
        End Function

        <DebuggerStepThrough>
        Public Shared Function Standardization() As StandardizationPreprocessor
            Return New StandardizationPreprocessor
        End Function

    End Class

End Namespace
