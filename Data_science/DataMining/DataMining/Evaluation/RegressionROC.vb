Imports System.Data
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model

Namespace Evaluation

    Public Module RegressionROC

        ''' <summary>
        ''' Evaluate the regression model ROC
        ''' </summary>
        ''' <param name="test"></param>
        ''' <param name="range"></param>
        ''' <param name="n"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function ROC(test As IEnumerable(Of RegressionClassify),
                                     Optional range As DoubleRange = Nothing,
                                     Optional n As Integer = 25) As IEnumerable(Of Validation)

            Dim allTest As RegressionClassify() = test.ToArray

            If range Is Nothing Then
                range = allTest _
                    .Select(Function(t) t.errors) _
                    .Range
            ElseIf range.Length = 0 Then
                Throw New InvalidConstraintException("error range can not be empty!")
            End If

            Dim d As Double = 1 / n
            Dim cutoff As Double
            Dim i As Double = 0
            Dim width As Double = range.Length
            Dim validate As Func(Of RegressionClassify, Boolean) = Function(any) True

            Do While i <= 1.0
                cutoff = (1 - i) * width

                Yield Validation.Calc(
                    entity:=allTest,
                    getValidate:=validate,
                    getPredict:=Function(t) t.errors < cutoff,
                    percentile:=i
                )

                i += d
            Loop
        End Function
    End Module
End Namespace