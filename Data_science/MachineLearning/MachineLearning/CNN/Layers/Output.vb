Namespace Convolutional

    Public Class Output : Inherits Layer

        Friend ReadOnly m_classes As String()

        Public ReadOnly Property sortedClasses As String()
        Public ReadOnly Property probabilities As Single()

        Public Overrides ReadOnly Property type As LayerTypes
            Get
                Return LayerTypes.Output
            End Get
        End Property

        Public Sub New(inputTensorDims As Integer(), classes As String())
            Call MyBase.New(inputTensorDims)

            m_classes = classes
            probabilities = New Single(inputTensorDims(2) - 1) {}
            sortedClasses = New String(inputTensorDims(2) - 1) {}
        End Sub

        ''' <summary>
        ''' get a class label which its probability is the highest value.
        ''' </summary>
        ''' <returns></returns>
        Public Function getDecision() As String
            If inputTensor.data IsNot Nothing Then
                Call Array.Copy(m_classes, sortedClasses, m_classes.Length)
                Call Array.ConstrainedCopy(inputTensor.data, 0, probabilities, 0, m_classes.Length)
                Call Array.Sort(probabilities, sortedClasses)
                Call Array.Reverse(probabilities)
                Call Array.Reverse(sortedClasses)

                Call disposeInputTensor()
            End If

            Return sortedClasses(0)
        End Function

        Protected Overrides Function layerFeedNext() As Layer
            Throw New InvalidOperationException("the output layer cann't be feed to next layer!")
        End Function

        Public Overrides Function feedNext() As Layer
            Return layerFeedNext()
        End Function

        Public Overrides Function ToString() As String
            Return $"{m_classes.Length} class tags: [{m_classes.Take(6).JoinBy("; ")}...]"
        End Function
    End Class
End Namespace
