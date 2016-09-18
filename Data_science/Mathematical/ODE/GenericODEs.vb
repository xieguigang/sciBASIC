
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical.BasicR

Public Delegate Sub [Function](dx As Double, ByRef dy As Vector)

Public Class GenericODEs : Inherits ODEs

    Public Property df As [Function]

    Sub New(ParamArray vars As var())
        Me.vars = vars

        For Each x In vars.SeqIterator
            x.obj.Index = x.i
        Next
    End Sub

    Protected Overrides Sub func(dx As Double, ByRef dy As Vector)
        Call _df(dx, dy)
    End Sub

    Protected Overrides Function y0() As var()
        Return vars
    End Function
End Class