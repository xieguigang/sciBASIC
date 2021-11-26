Imports System
Imports System.Runtime.InteropServices

Namespace Convolutional
    Public Class Output : Inherits Layer

        Public classes As String()
        Public sortedClasses As String()
        Public probabilities As Single()

        Public Sub New(inputTensorDims As Integer(), classes As String())
            MyBase.New(inputTensorDims)
            type = "Output"
            Me.classes = classes
            probabilities = New Single(inputTensorDims(2) - 1) {}
            sortedClasses = New String(inputTensorDims(2) - 1) {}
        End Sub

        Public Function getDecision() As String
            If inputTensor.memPtr IsNot Nothing Then
                Array.Copy(classes, sortedClasses, classes.Length)
                Array.ConstrainedCopy(inputTensor.memPtr, 0, probabilities, 0, classes.Length)
                Array.Sort(probabilities, sortedClasses)
                Array.Reverse(probabilities)
                Array.Reverse(sortedClasses)
                disposeInputTensor()
            End If

            Return sortedClasses(0)
        End Function

        Public Overrides Sub feedNext()
            Throw New NotImplementedException()
        End Sub
    End Class
End Namespace
