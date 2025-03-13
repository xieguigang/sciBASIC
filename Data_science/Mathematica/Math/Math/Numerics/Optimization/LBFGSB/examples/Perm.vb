Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Math.Framework.Optimization.LBFGSB


Public Class Perm
    Inherits IGradFunction
    Private d As Integer
    Private ij As Double()()
    Private jbeta As Double()

    Public Sub New(beta As Double, d As Integer)
        Me.d = d

        ij = RectangularArray.Matrix(Of Double)(d, d)
        jbeta = New Double(d - 1) {}

        For j = 1 To d
            jbeta(j - 1) = j + beta
            For i = 1 To d
                ij(i - 1)(j - 1) = 1.0 / Math.Pow(j, i)
            Next
        Next
    End Sub

    Public Sub New(d As Integer)
        Me.New(1.0, d)
    End Sub
    Public Sub New()
        Me.New(5)
    End Sub

    Public Overrides Function evaluate([in] As Double()) As Double
        Dim res = 0.0
        For i = 1 To d
            Dim resj = 0.0
            For j = 1 To d
                resj += jbeta(j - 1) * (Math.Pow([in](j - 1), i) - ij(i - 1)(j - 1))
            Next
            res += resj * resj
        Next
        Return res
    End Function

    Public Shared Sub Main14(args As String())

        Debug.flag = True

        Dim param As Parameters = New Parameters()
        Dim lbfgsb As LBFGSB = New LBFGSB(param)

        Dim d = 3

        Dim z = New Double() {0, 0, 0}
        Dim f As Perm = New Perm(d)
        Call Debug.debug("res = " & f.evaluate(z).ToString())

        ' converges to global minimum
        Try
            Dim res As Double() = lbfgsb.minimize(New Perm(d), New Double() {1, -1, -3}, New Double() {-d, -d, -d}, New Double() {d, d, d})
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


