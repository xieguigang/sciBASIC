Imports Microsoft.VisualBasic.Math.Framework.Optimization.LBFGSB


' https://www.sfu.ca/~ssurjano/booth.html
' f(1,3) = 0


Public Class Booth
    Inherits IGradFunction
    Public Overrides Function evaluate([in] As Double(), grad As Double()) As Double
        Dim x1 = [in](0)
        Dim x2 = [in](1)

        Dim a = x1 + 2.0 * x2 - 7.0
        Dim b = 2.0 * x1 + x2 - 5.0

        grad(0) = 2.0 * a + 4.0 * b
        grad(1) = 4.0 * a + 2.0 * b

        Return a * a + b * b
    End Function

    Public Overrides Function in_place_gradient() As Boolean
        Return True
    End Function

    Public Shared Sub Main5(args As String())

        Debug.flag = True

        Dim param As Parameters = New Parameters()
        Dim lbfgsb As LBFGSB = New LBFGSB(param)

        Try
            '		double[] res = lbfgsb.minimize(new Booth(), new double[] { -10,0.1 }, new double[] { -10, -10 },
            '				new double[] { 10, 10 });
            Dim res As Double() = lbfgsb.minimize(New Booth(), New Double() {-1, -10}, New Double() {-10, 3}, New Double() {10, 3.1})
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

