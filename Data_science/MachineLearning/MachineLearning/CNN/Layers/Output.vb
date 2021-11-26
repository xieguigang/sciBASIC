Imports System
Imports System.Runtime.InteropServices

Namespace Convolutional
    Public Class Output : Inherits Layer

        Public classes As String()
        Public sortedClasses As String()
        Public probabilities As Single()

        Public Overrides ReadOnly Property type As LayerTypes
            Get
                Return LayerTypes.Output
            End Get
        End Property

        Public Sub New(inputTensorDims As Integer(), classes As String())
            Call MyBase.New(inputTensorDims)

            Me.classes = classes
            probabilities = New Single(inputTensorDims(2) - 1) {}
            sortedClasses = New String(inputTensorDims(2) - 1) {}
        End Sub

        Public Function getDecision() As String
            If inputTensor.data IsNot Nothing Then
                Array.Copy(classes, sortedClasses, classes.Length)
                Array.ConstrainedCopy(inputTensor.data, 0, probabilities, 0, classes.Length)
                Array.Sort(probabilities, sortedClasses)
                Array.Reverse(probabilities)
                Array.Reverse(sortedClasses)
                disposeInputTensor()
            End If

            Return sortedClasses(0)
        End Function

        Public Overrides Function feedNext() As Layer
            Throw New InvalidOperationException("the output layer cann't be feed to next layer!")
        End Function
    End Class
End Namespace
