
Imports Microsoft.VisualBasic.Math.Framework.Optimization.LBFGSB


' THREE-HUMP CAMEL FUNCTION
' https://www.sfu.ca/~ssurjano/camel3.html
' Global minimum: f(0,0)=0 


Public NotInheritable Class ThreeHumpCamel
    Inherits IGradFunction

    Public Overrides Function evaluate([in] As Double(), grad As Double()) As Double
        Dim x = [in](0)
        Dim x2 = x * x
        Dim x4 = x2 * x2
        Dim y = [in](1)

        grad(0) = 4 * x - 4.2 * x2 * x + x4 * x + y
        grad(1) = x + 2 * y

        Return 2 * x2 - 1.05 * x4 + x4 * x2 / 6.0 + x * y + y * y
    End Function

    Public Overrides Function in_place_gradient() As Boolean
        Return True
    End Function

    Public Shared Sub Main16(args As String())

        Debug.flag = True

        Dim param As Parameters = New Parameters()
        Dim lbfgsb As LBFGSB = New LBFGSB(param)

        ' converges to global minimum
        Try
            '			double[] res = lbfgsb.minimize(new ThreeHumpCamel(), new double[] { -5, 5 }, new double[] { -5, -5 },
            '					new double[] { 5, 5 });
            Dim res As Double() = lbfgsb.minimize(New ThreeHumpCamel(), New Double() {2, 2}, New Double() {0, -5}, New Double() {1, 1})

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


