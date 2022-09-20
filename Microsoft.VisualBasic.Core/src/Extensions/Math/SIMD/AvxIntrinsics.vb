Imports System.Runtime.Intrinsics

Namespace Math

#If NETCOREAPP Then
    Public Class AvxIntrinsics

        Public Shared Function Add(v1 As Double(), v2 As Double(), math As Func(Of Vector256(Of Double), Vector256(Of Double), Vector256(Of Double))) As Double()
            Dim a As Vector256(Of Double)
            Dim b As Vector256(Of Double)
            Dim c As Vector256(Of Double)
            Dim size As Integer = v1.Length
            Dim vec As Double() = New Double(size - 1) {}
            Dim remaining As Integer = v1.Length Mod SIMD.countDouble

            For i As Integer = 0 To vec.Length - 1 Step 4
                a = Vector256.Create(v1(i), v1(i + 1), v1(i + 2), v1(i + 3))
                b = Vector256.Create(v2(i), v2(i + 1), v2(i + 2), v2(i + 3))
                c = math(a, b)
                vec(i) = c.GetElement(0)
                vec(i + 1) = c.GetElement(1)
                vec(i + 2) = c.GetElement(2)
                vec(i + 3) = c.GetElement(3)
            Next

            For i As Integer = v1.Length - remaining To v1.Length - 1
                vec(i) = v1(i) + v2(i)
            Next

            Return vec
        End Function
    End Class
#End If
End Namespace