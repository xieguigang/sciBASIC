Imports Microsoft.VisualBasic.Math.Framework.Optimization.LBFGSB

' CROSS-IN-TRAY
' https://www.sfu.ca/~ssurjano/crossit.html
' f(+/-1.3491, +/-1.3491) = -2.06261

Public Class CrossInTray
    Inherits IGradFunction

    Public Overrides Function evaluate([in] As Double()) As Double
        Dim x1 = [in](0)
        Dim x2 = [in](1)

        Return -0.0001 * Math.Pow(Math.Abs(Math.Sin(x1) * Math.Sin(x2) * Math.Exp(Math.Abs(100.0 - Math.Sqrt(x1 * x1 + x2 * x2) / Math.PI))) + 1.0, 0.1)
    End Function

    Public Shared Sub Main7(args As String())

        Debug.flag = True

        Dim param As Parameters = New Parameters()
        Dim lbfgsb As LBFGSB = New LBFGSB(param)

        Try
            Dim res As Double() = lbfgsb.minimize(New CrossInTray(), New Double() {3, -3}, New Double() {-10, -10}, New Double() {10, 10})

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


