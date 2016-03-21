Namespace Kernel.GeneticAlgorithm

    ''' <summary>
    ''' When doing math operations, we may want to sum, multiply or divide an input value 
    ''' (or a value already calculated on top of it) by a numeric "constant". Such constant
    ''' is given by this neuron.
    ''' It is "constant" for its generation, yet it can reproduce with different values.
    ''' </summary>
    Public NotInheritable Class IntNeuron
        Implements INeuron
        Private _value As Integer
        Public Sub New(value As Integer)
            _value = value
        End Sub

        Public ReadOnly Property Complexity() As Integer Implements INeuron.Complexity
            Get
                Return 1
            End Get
        End Property

        Public Function Execute(input As Integer) As System.Nullable(Of Integer) Implements INeuron.Execute
            Return _value
        End Function

        Public Function Reproduce() As INeuron Implements INeuron.Reproduce
            Dim random As Integer = GetRandom(11)

            If random >= 2 Then
                ' we don't really need to create a new object if we don't want to make any changes, so
                ' we simply return this.
                Return Me
            End If

            Dim amountToChange As Integer = 1
            Dim percentualChange As Integer = GetRandom(11)
            If percentualChange > 0 Then
                amountToChange = _value * percentualChange \ 100

                If amountToChange = 0 Then
                    amountToChange = 1
                End If
            End If

            If random = 0 Then
                Return New IntNeuron(_value - amountToChange)
            End If

            Return New IntNeuron(_value + amountToChange)
        End Function

        Public Overrides Function ToString() As String
            Return _value.ToString()
        End Function
    End Class
End Namespace