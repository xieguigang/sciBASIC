Imports Microsoft.VisualBasic.Math.Framework.Optimization.LBFGSB

Public Class Parabola2
    Inherits IGradFunction

    Public Overrides Function evaluate([in] As Double()) As Double
        Dim x = [in](0)
        Dim y = [in](1)
        Dim t = x * x + y * y
        If x < 1.0 OrElse y < 1.0 Then
            Debug.debug("W"c, "outside " & "[" & String.Join(", ", [in]) & "]")
        End If
        Return t + Math.Sin(t) * Math.Sin(t)
    End Function

    Public Shared Sub Main13(args As String())

        Debug.flag = True

        Dim param As Parameters = New Parameters()
        Dim lbfgsb As LBFGSB = New LBFGSB(param)

        ' converges to global minimum
        Try
            Dim res As Double() = lbfgsb.minimize(New Parabola2(), New Double() {3, 3}, New Double() {1, 1}, New Double() {5, 5})
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


