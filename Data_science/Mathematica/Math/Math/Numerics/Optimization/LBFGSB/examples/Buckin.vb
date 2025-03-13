Imports Microsoft.VisualBasic.Math.Framework.Optimization.LBFGSB


' BUKIN FUNCTION N. 6
' https://www.sfu.ca/~ssurjano/bukin6.html
' f(-10,1) = 0;
Public Class Buckin
    Inherits IGradFunction
    Public Overrides Function evaluate([in] As Double()) As Double
        Dim x1 = [in](0)
        Dim x2 = [in](1)

        Return 100.0 * Math.Sqrt(Math.Abs(x2 - 0.01 * x1 * x1)) + 0.01 * Math.Abs(x1 + 10.0)
    End Function

    Public Shared Sub Main6(args As String())

        Debug.flag = True

        Dim param As Parameters = New Parameters()
        Dim lbfgsb As LBFGSB = New LBFGSB(param)

        ' FAILS

        Try
            Dim res As Double() = lbfgsb.minimize(New Buckin(), New Double() {-10.1, 1}, New Double() {-15, -3}, New Double() {-5, 3})

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


