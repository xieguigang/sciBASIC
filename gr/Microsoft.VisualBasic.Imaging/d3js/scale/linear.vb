Namespace d3js.scale

    ''' <summary>
    ''' 连续性的映射
    ''' </summary>
    Public Class LinearScale : Inherits IScale

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