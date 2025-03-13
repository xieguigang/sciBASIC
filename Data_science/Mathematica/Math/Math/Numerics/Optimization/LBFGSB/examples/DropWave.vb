Imports Microsoft.VisualBasic.Math.Framework.Optimization.LBFGSB



' DROP-WAVE
' https://www.sfu.ca/~ssurjano/drop.html
' f(0,0)=-1


Public Class DropWave
    Inherits IGradFunction
    Public Overrides Function evaluate([in] As Double()) As Double
        Dim x1 = [in](0)
        Dim x2 = [in](1)

        Dim v = x1 * x1 + x2 * x2
        Return -(1.0 + Math.Cos(12.0 * v)) / (0.5 * v + 2.0)
    End Function

    Public Shared Sub Main8(args As String())

        Debug.flag = True

        Dim param As Parameters = New Parameters()
        Dim lbfgsb As LBFGSB = New LBFGSB(param)

        Try
            Dim res As Double() = lbfgsb.minimize(New DropWave(), New Double() {-0.1, 0.1}, New Double() {-5, -5}, New Double() {5, 5})

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


