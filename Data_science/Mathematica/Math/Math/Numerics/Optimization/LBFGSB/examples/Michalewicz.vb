Imports Microsoft.VisualBasic.Math.Framework.Optimization.LBFGSB

Public Class Michalewicz
    Inherits IGradFunction
    Public m2 As Double
    Public d As Integer

    Public Sub New(d As Integer, m As Double)
        m2 = 2.0 * m
        Me.d = d
    End Sub

    Public Sub New(d As Integer)
        Me.New(d, 10.0)
    End Sub

    Public Sub New()
        Me.New(5)
    End Sub

    Public Overrides Function evaluate([in] As Double()) As Double
        Dim res = 0.0
        For i = 1 To d
            Dim x = [in](i - 1)
            res += Math.Sin(x) * Math.Pow(Math.Sin(i * x * x / Math.PI), m2)
        Next
        Return -res
    End Function


    Public Shared Sub Main11(args As String())

        Debug.flag = True

        Dim param As Parameters = New Parameters()
        Dim lbfgsb As LBFGSB = New LBFGSB(param)

        Dim pi = Math.PI

        ' converges to global minimum
        Try
            '	double[] res = lbfgsb.minimize(new Michalewicz(), new double[] { 2,2,2,2,2 }, new double[] { 0,0,0,0,0 }, new double[] { pi,pi,pi,pi,pi });
            Dim res As Double() = lbfgsb.minimize(New Michalewicz(2), New Double() {2, 2}, New Double() {0, 0}, New Double() {pi, pi})

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


