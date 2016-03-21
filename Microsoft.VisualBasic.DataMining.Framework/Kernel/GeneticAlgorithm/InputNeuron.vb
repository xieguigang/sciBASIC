Namespace Kernel.GeneticAlgorithm
    ''' <summary>
    ''' The math neurons actually never see the real input value, they only get a value
    ''' from another neuron. This neuron provides the user input value to those neurons.
    ''' </summary>
    Public NotInheritable Class InputNeuron
        Implements INeuron
        Public Shared ReadOnly Instance As New InputNeuron()

        Public ReadOnly Property Complexity() As Integer Implements INeuron.Complexity
            Get
                Return 1
            End Get
        End Property

        Public Function Execute(input As Integer) As System.Nullable(Of Integer) Implements INeuron.Execute
            Return input
        End Function

        Public Function Reproduce() As INeuron Implements INeuron.Reproduce
            Return Me
        End Function

        Public Overrides Function ToString() As String
            Return "x"
        End Function
    End Class
End Namespace