
Imports Microsoft.VisualBasic.Math.Framework.Optimization.LBFGSB

' EGGHOLDER
' https://www.sfu.ca/~ssurjano/egg.html
' f(512,404.2319) = -959.6407


Public Class Eggholder
    Inherits IGradFunction
    Public Overrides Function evaluate([in] As Double()) As Double
        Dim x1 = [in](0)
        Dim x2 = [in](1)

        Dim x2_47 = x2 + 47.0
        Return -x2_47 * Math.Sin(Math.Sqrt(Math.Abs(x2_47 + x1 / 2.0))) - x1 * Math.Sin(Math.Sqrt(Math.Abs(x1 - x2_47)))
    End Function

    Public Shared Sub Main9(args As String())

        Debug.flag = True

        Dim param As Parameters = New Parameters()
        Dim lbfgsb As LBFGSB = New LBFGSB(param)

        Try
            Dim res As Double() = lbfgsb.minimize(New Eggholder(), New Double() {500, 350}, New Double() {-512, -512}, New Double() {512, 512})

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


