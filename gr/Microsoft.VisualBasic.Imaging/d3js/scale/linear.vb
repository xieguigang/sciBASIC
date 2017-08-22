Namespace d3js.scale

    ''' <summary>
    ''' 连续性的映射
    ''' </summary>
    Public Class LinearScale : Inherits IScale

        ''' <summary>
        ''' Constructs a new continuous scale with the unit domain [0, 1], the unit range [0, 1], 
        ''' the default interpolator and clamping disabled. Linear scales are a good default 
        ''' choice for continuous quantitative data because they preserve proportional differences. 
        ''' Each range value y can be expressed as a function of the domain value x: ``y = mx + b``.
        ''' </summary>
        Sub New()
        End Sub

        Public Overrides Function domain(values() As Double) As OrdinalScale
            Throw New NotImplementedException()
        End Function

        Public Overrides Function domain(values() As String) As OrdinalScale
            Throw New NotImplementedException()
        End Function

        Public Overrides Function domain(values() As Integer) As OrdinalScale
            Throw New NotImplementedException()
        End Function

        Public Overrides Function rangeBands() As OrdinalScale
            Throw New NotImplementedException()
        End Function

        Public Overrides Function range(Optional values() As Double = Nothing) As OrdinalScale
            Throw New NotImplementedException()
        End Function
    End Class
End Namespace