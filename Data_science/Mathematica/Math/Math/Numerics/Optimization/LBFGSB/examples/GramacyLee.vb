Imports Microsoft.VisualBasic.Math.Framework.Optimization.LBFGSB
' Gramacy & Lee (2012)
' https://www.sfu.ca/~ssurjano/grlee12.html


Public Class GramacyLee
    Inherits IGradFunction
    Public Overrides Function evaluate([in] As Double()) As Double
        Dim x = [in](0)

        Dim v = (x - 1) * (x - 1)
        Return Math.Sin(10.0 * Math.PI * x) / (2.0 * x) + v * v
    End Function

    Public Shared Sub Main10(args As String())

        Debug.flag = True

        Dim param As Parameters = New Parameters()
        param.weak_wolfe = True
        param.max_linesearch = 1000
        Dim lbfgsb As LBFGSB = New LBFGSB(param)

        ' a lot of fails in line search

        Try
            Dim res As Double() = lbfgsb.minimize(New GramacyLee(), New Double() {0.55}, New Double() {0.5}, New Double() {2.5})

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


