Namespace ComponentModel.Ranges.Unit

    Public Structure UnitTag(Of T)

        Dim unit As T
        Dim value As Double

        Sub New(unit As T, value As Double)
            Me.unit = unit
            Me.value = value
        End Sub

    End Structure
End Namespace