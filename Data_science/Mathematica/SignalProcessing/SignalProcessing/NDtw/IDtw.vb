Imports System

Namespace NDtw

    Public Interface IDtw
        Function GetCost() As Double
        Function GetPath() As Tuple(Of Integer, Integer)()
        Function GetDistanceMatrix() As Double()()
        Function GetCostMatrix() As Double()()
        ReadOnly Property XLength As Integer
        ReadOnly Property YLength As Integer
        ReadOnly Property SeriesVariables As SeriesVariable()
    End Interface
End Namespace
