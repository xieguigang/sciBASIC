
Imports Microsoft.VisualBasic.Math.Framework.Optimization.LBFGSB

' ROSENBROCK
' https://www.sfu.ca/~ssurjano/rosen.html
' Global minimum: f(1,1,...,1) = 0;


Public Class Rosenbrock
    Inherits IGradFunction
    Private n As Integer

    Public Sub New(n As Integer)
        Me.n = n
    End Sub

    Public Sub New()
        Me.New(5)
    End Sub

    Public Overrides Function in_place_gradient() As Boolean
        Return True
    End Function

    Public Overrides Function evaluate(x As Double(), grad As Double()) As Double
        Dim fx = (x(0) - 1.0) * (x(0) - 1.0)
        grad(0) = 2.0 * (x(0) - 1.0) + 16.0 * (x(0) * x(0) - x(1)) * x(0)
        For i = 1 To n - 1
            Dim v = x(i) - x(i - 1) * x(i - 1)
            fx += 4.0 * v * v
            If i = n - 1 Then
                grad(i) = 8.0 * v
            Else
                grad(i) = 8.0 * v + 16.0 * (x(i) * x(i) - x(i + 1)) * x(i)
            End If
        Next
        Return fx
    End Function

    Public Shared Sub Main15(args As String())

        Debug.flag = True

        Dim param As Parameters = New Parameters()
        Dim lbfgsb As LBFGSB = New LBFGSB(param)

        ' converges to global minimum
        Try
            Dim res As Double() = lbfgsb.minimize(New Rosenbrock(), New Double() {2, -4, 2, 4, -2}, New Double() {-5, -5, -5, -5, -5}, New Double() {10, 10, 10, 10, 10})
            Debug.debug("!"c, "RESULT")
            Call Debug.debug("k = " & lbfgsb.k.ToString())
            Debug.debug("x = ", res)
            Call Debug.debug("fx = " & lbfgsb.fx.ToString())
            Debug.debug("grad = ", lbfgsb.m_grad)
        Catch e As LBFGSBException
            Console.WriteLine(e.ToString())
            Console.Write(e.StackTrace)
        End Try

        Dim f As Rosenbrock = New Rosenbrock()
        Dim g = New Double(4) {}
        Call Debug.debug("res=" & f.eval(New Double() {2, -4, 2, 4, -2}, g).ToString())

    End Sub

End Class

