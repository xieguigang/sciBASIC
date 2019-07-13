#Region "Microsoft.VisualBasic::52c8d74ec1594d3032e723797c12429b, Data_science\Mathematica\Math\MathApp\Modules\ODEsTest.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Class ODEsTest
    ' 
    '     Function: y0
    ' 
    '     Sub: func
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical.LinearAlgebra
Imports Microsoft.VisualBasic.Mathematical.Calculus

Public Class ODEsTest : Inherits ODEs

    Dim a As Double = 0.1, b As Double = 0.1, c As Double = 0.1

    Dim yC As var
    Dim P As var

    Protected Overrides Sub func(dx As Double, ByRef dy As Vector)
        dy(P) = a * P - b * yC * P
        dy(yC) = b * P * yC - c * yC
    End Sub

    Protected Overrides Function y0() As var()
        Return {P = 2, yC = 1}
    End Function
End Class
