Imports Microsoft.VisualBasic.Mathematical.BasicR
Imports Microsoft.VisualBasic.Mathematical.Calculus

''' <summary>
''' ``dy`` reference to the exists values.
''' </summary>
Public MustInherit Class RefODEs : Inherits ODEs

    Public Property RefValues As ValueVector

    Protected NotOverridable Overrides Sub func(dx As Double, ByRef dy As Vector)
        RefValues += 1
        Call func(dx, dy, RefValues)
    End Sub

    Protected MustOverride Overloads Sub func(dx#, ByRef dy As Vector, Y As ValueVector)

End Class
