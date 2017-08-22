Namespace d3js.scale

    Public MustInherit Class IScale

        Public MustOverride Function domain(values#()) As OrdinalScale
        Public MustOverride Function domain(values$()) As OrdinalScale
        Public MustOverride Function domain(values%()) As OrdinalScale
        Public MustOverride Function rangeBands() As OrdinalScale
        Public MustOverride Function range(Optional values#() = Nothing) As OrdinalScale

    End Class
End Namespace