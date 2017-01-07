Public Module AxisScalling

    Public Function GetAxisValues(max#, Optional min# = 0R) As Double()
        Dim p10% = Fix(Math.Log10(max))

    End Function

    Public Function GetAxisByTick(max#, tick#, Optional min# = 0R) As Double()
        Dim l As New List(Of Double)

        For i As Double = min To max Step tick
            Call l.Add(i)
        Next

        Return l.ToArray
    End Function
End Module
