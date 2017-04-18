Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.SVM.Method
Imports Microsoft.VisualBasic.DataMining.SVM.Model

''' <summary>
''' @author Ralf Wondratschek
''' </summary>
Public Module OptimizerCalculator

    Public C As Double = 100.0

    ''' <summary>
    ''' SVM caller
    ''' </summary>
    ''' <param name="optimizer"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Calculate(optimizer As Optimizer) As Line
        Dim result As Line = optimizer.Optimize()

        ''            
        '' + Sometimes a method diverges, so we need to catch this case
        ''             
        Dim diverged As Boolean =
            Double.IsNaN(result.Offset()) OrElse
            Double.IsNaN(result.NormalVector().W1()) OrElse
            Double.IsNaN(result.NormalVector().W2())

        If (diverged) Then
            result = New Line(New NormalVector(-1, 1), 0)
        End If

        Return result
    End Function
End Module
