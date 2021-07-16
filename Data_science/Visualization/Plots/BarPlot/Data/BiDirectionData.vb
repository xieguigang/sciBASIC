Namespace BarPlot.Data

    Public Class BiDirectionData

        ''' <summary>
        ''' left
        ''' </summary>
        ''' <returns></returns>
        Public Property Factor1 As String
        ''' <summary>
        ''' right
        ''' </summary>
        ''' <returns></returns>
        Public Property Factor2 As String
        ''' <summary>
        ''' data samples
        ''' </summary>
        ''' <returns></returns>
        Public Property samples As BarDataSample()

        Public ReadOnly Property size As Integer
            Get
                Return samples.Length
            End Get
        End Property

        Default Public ReadOnly Property data(i As Integer) As BarDataSample
            Get
                Return samples(i)
            End Get
        End Property

    End Class
End Namespace