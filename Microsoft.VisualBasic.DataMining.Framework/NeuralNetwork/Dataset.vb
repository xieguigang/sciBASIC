Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace NeuralNetwork

    ''' <summary>
    ''' The training dataset
    ''' </summary>
    Public Class DataSet

#Region "-- Properties --"

        ''' <summary>
        ''' Neuron network input parameters
        ''' </summary>
        ''' <returns></returns>
        Public Property Values() As Double()
        ''' <summary>
        ''' The network expected output values
        ''' </summary>
        ''' <returns></returns>
        Public Property Targets() As Double()
#End Region

#Region "-- Constructor --"

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="values__1">Neuron network input parameters</param>
        ''' <param name="targets__2">The network expected output values</param>
        Public Sub New(values__1 As Double(), targets__2 As Double())
            Values = values__1
            Targets = targets__2
        End Sub

        Sub New()
        End Sub
#End Region

        Public Overrides Function ToString() As String
            Return Values.Join(Targets).GetJson
        End Function
    End Class
End Namespace
