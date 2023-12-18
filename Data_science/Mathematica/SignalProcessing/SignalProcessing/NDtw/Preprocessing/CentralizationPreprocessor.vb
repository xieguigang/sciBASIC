Imports System.Linq

Namespace NDtw.Preprocessing
    Public Class CentralizationPreprocessor
        Implements IPreprocessor
        Public Function Preprocess(data As Double()) As Double() Implements IPreprocessor.Preprocess
            Dim avg = data.Average()
            Return data.[Select](Function(x) x - avg).ToArray()
        End Function

        Public Overrides Function ToString() As String Implements IPreprocessor.ToString
            Return "Centralization"
        End Function
    End Class
End Namespace
