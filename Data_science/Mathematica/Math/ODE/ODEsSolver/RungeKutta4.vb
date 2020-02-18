Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Class RungeKutta4

    Dim K1, K2, K3, K4 As Vector
    Dim ODEs As ODEs

    Sub New(system As ODEs)
        ODEs = system
    End Sub

    ''' <summary>
    ''' RK4 ODEs solver
    ''' </summary>
    ''' <param name="dxn">The x initial value.(x初值)</param>
    ''' <param name="dyn">The y initial value.(初值y(n))</param>
    ''' <param name="dh">Steps delta.(步长)</param>
    ''' <param name="dynext">
    ''' Returns the y(n+1) result from this parameter.(下一步的值y(n+1))
    ''' </param>
    Private Sub rungeKutta(dxn As Double,
                           ByRef dyn As Vector,
                           dh As Double,
                           ByRef dynext As Vector)

        Call ODEs.ODEs(dxn, dyn, K1)                        ' 求解K1
        Call ODEs.ODEs(dxn + dh / 2, dyn + dh / 2 * K1, K2) ' 求解K2
        Call ODEs.ODEs(dxn + dh / 2, dyn + dh / 2 * K2, K3) ' 求解K3
        Call ODEs.ODEs(dxn + dh, dyn + dh * K3, K4)         ' 求解K4

        ' 求解下一步的值y(n+1)
        dynext = dyn + (K1 + K2 + K3 + K4) * dh / 6.0
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="n">A larger value of this parameter, will makes your result more precise.</param>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    Public Sub Solve(y0 As Double(), n As Integer, a As Double, b As Double, ByRef x As Double(), ByRef y As List(Of Double)())
        ' 步长
        Dim dh As Double = (b - a) / n
        Dim dx As Double = a
        Dim darrayn As New Vector(y0)
        ' 下一步的值,最好初始化
        Dim darraynext As New Vector(y0.Length)
        Dim xi As New List(Of Double)

        K1 = New Vector(y0.Length)
        K2 = New Vector(y0.Length)
        K3 = New Vector(y0.Length)
        K4 = New Vector(y0.Length)

        y = New List(Of Double)(y0.Length - 1) {}

        For i As Integer = 0 To n
            Call rungeKutta(dx, darrayn, dh, darraynext)

            xi += dx
            dx += dh
            darrayn = darraynext

            For index As Integer = 0 To darrayn.Dim - 1
                y(index).Add(darrayn(index))
            Next
        Next

        x = xi
    End Sub
End Class
