Imports Microsoft.VisualBasic.Math.Framework.Optimization.LBFGSB


' ACKLEY
' https://www.sfu.ca/~ssurjano/ackley.html
' Global minimum: f(0,0,...,0) = 0
Public Class Ackley
    Inherits IGradFunction
    Private d As Integer
    Private a, b, c As Double

    Public Overrides Function evaluate([in] As Double()) As Double
        Dim sumx2 = 0.0
        Dim sumcos = 0.0
        For Each x In [in]
            sumx2 += x * x
            sumcos += Math.Cos(c * x)
        Next

        Return -a * Math.Exp(-b * Math.Sqrt(sumx2 / d)) - Math.Exp(sumcos / d) + a + Math.E

    End Function

    Public Sub New()
        Me.New(5)
    End Sub

    Public Sub New(d As Integer)
        Me.New(d, 20.0, 0.2, Math.PI * 2.0)
    End Sub

    Public Sub New(d As Integer, a As Double, b As Double, c As Double)
        Me.d = d
        Me.a = a
        Me.b = b
        Me.c = c
    End Sub

    Public Shared Sub Main1(args As String())

        Debug.flag = True

        Dim param As Parameters = New Parameters()
        Dim lbfgsb As LBFGSB = New LBFGSB(param)

        Try
            '			double[] res = lbfgsb.minimize(new Ackley(), new double[] { 0.5, 0.2, -0.7, 0.7, -0.6 },
            '					new double[] { -32, -32, -32, -32, -32 }, new double[] { 32, 32, 32, 32, 32 });
            '			double[] res = lbfgsb.minimize(new Ackley(), new double[] { 0.65, -0.5, 0.0, 0.5, 0.6 },
            '					new double[] { 0, -10, 0, 0, 0 }, new double[] { 32, 32, 32, 32, 32 });
            Dim res As Double() = lbfgsb.minimize(New Ackley(10), New Double() {0.65, -0.5, 0.0, 0.5, 0.2, 0.2, 0.2, -0.2, -0.2, -0.5}, New Double() {-32, -32, -32, -32, 0, -32, -32, -32, -32, -32}, New Double() {32, 32, 32, 32, 32, 32, 32, 32, 32, 32})

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


