Public Interface Individual

    Sub Tick(adjacents As IEnumerable(Of Individual))

End Interface

Public Delegate Function ToInteger(Of T As Individual)(a As T) As Integer