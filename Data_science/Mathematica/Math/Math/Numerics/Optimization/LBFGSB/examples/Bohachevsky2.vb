
Imports Microsoft.VisualBasic.Math.Framework.Optimization.LBFGSB

' https://www.sfu.ca/~ssurjano/boha.html

Public Class Bohachevsky2
    Inherits IGradFunction

    Public Overrides Function evaluate([in] As Double()) As Double
        Dim x1 = [in](0)
        Dim x2 = [in](1)

        Return x1 * x1 + 2.0 * x2 * x2 - 0.3 * Math.Cos(3.0 * Math.PI * x1) * Math.Cos(4.0 * Math.PI * x2) + 0.3
    End Function

    Public Shared Sub Main3(args As String())

        Debug.flag = True

        Dim param As Parameters = New Parameters()
        param.linesearch = LINESEARCH.MORETHUENTE_LBFGSPP
        Dim lbfgsb As LBFGSB = New LBFGSB(param)

        Try
            Dim res As Double() = lbfgsb.minimize(New Bohachevsky2(), New Double() {-50, 90}, New Double() {-100, -100}, New Double() {100, 100})

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
