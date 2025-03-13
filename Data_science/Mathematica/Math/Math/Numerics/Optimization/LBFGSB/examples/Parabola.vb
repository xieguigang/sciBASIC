Imports Microsoft.VisualBasic.Math.Framework.Optimization.LBFGSB



' Parabola, f(x)=2x^2-x+3
' Global minimum: f(0.25) = 2.875
Public Class Parabola
    Inherits IGradFunction

    Public Overrides Function evaluate(x As Double(), grad As Double()) As Double
        Dim xx = x(0)
        grad(0) = 4 * xx - 1
        Return 2 * xx * xx - xx + 3
    End Function

    Public Overrides Function in_place_gradient() As Boolean
        Return True
    End Function

    Public Shared Sub Main12(args As String())

        Debug.flag = True

        Dim param As Parameters = New Parameters()
        Dim lbfgsb As LBFGSB = New LBFGSB(param)

        ' converges to global minimum
        Try
            Dim res As Double() = lbfgsb.minimize(New Parabola(), New Double() {-2}, New Double() {-5}, New Double() {5})
            Debug.debug("!"c, "RESULT")
            Call Debug.debug("k = " & lbfgsb.k.ToString())
            Debug.debug("x = ", res)
            Call Debug.debug("fx = " & lbfgsb.fx.ToString())
            Debug.debug("grad = ", lbfgsb.m_grad)
        Catch e As LBFGSBException
            Console.WriteLine(e.ToString())
            Console.Write(e.StackTrace)
        End Try
    End Sub

End Class


