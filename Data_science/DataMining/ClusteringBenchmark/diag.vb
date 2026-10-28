Imports System.Diagnostics
Imports System.Numerics
Imports Microsoft.VisualBasic.Math.SIMD
Imports std = System.Math

''' <summary>
''' 临时诊断：定位 SIMD 距离内核与标量路径的性能差异来源。
''' </summary>
Public Module Diag

    Private Function ScalarLoop(a As Double(), b As Double()) As Double
        Dim sum As Double = 0

        For k As Integer = 0 To a.Length - 1
            Dim d As Double = a(k) - b(k)

            sum += d * d
        Next

        Return sum
    End Function

    Private Function VectorLoop(a As Double(), b As Double()) As Double
        Dim count As Integer = Vector(Of Double).Count
        Dim step4 As Integer = count * 4
        Dim len As Integer = a.Length
        Dim ones As New Vector(Of Double)(1.0)
        Dim i As Integer = 0
        Dim sum As Double = 0

        If len >= step4 Then
            Dim acc0 As Vector(Of Double) = Vector(Of Double).Zero
            Dim acc1 As Vector(Of Double) = Vector(Of Double).Zero
            Dim acc2 As Vector(Of Double) = Vector(Of Double).Zero
            Dim acc3 As Vector(Of Double) = Vector(Of Double).Zero
            Dim last4 As Integer = len - step4

            Do While i <= last4
                Dim d0 As Vector(Of Double) = Vector.Subtract(Of Double)(New Vector(Of Double)(a, i), New Vector(Of Double)(b, i))
                Dim d1 As Vector(Of Double) = Vector.Subtract(Of Double)(New Vector(Of Double)(a, i + count), New Vector(Of Double)(b, i + count))
                Dim d2 As Vector(Of Double) = Vector.Subtract(Of Double)(New Vector(Of Double)(a, i + count * 2), New Vector(Of Double)(b, i + count * 2))
                Dim d3 As Vector(Of Double) = Vector.Subtract(Of Double)(New Vector(Of Double)(a, i + count * 3), New Vector(Of Double)(b, i + count * 3))

                acc0 = Vector.Add(Of Double)(acc0, Vector.Multiply(Of Double)(d0, d0))
                acc1 = Vector.Add(Of Double)(acc1, Vector.Multiply(Of Double)(d1, d1))
                acc2 = Vector.Add(Of Double)(acc2, Vector.Multiply(Of Double)(d2, d2))
                acc3 = Vector.Add(Of Double)(acc3, Vector.Multiply(Of Double)(d3, d3))

                i += step4
            Loop

            acc0 = Vector.Add(Of Double)(Vector.Add(Of Double)(acc0, acc1), Vector.Add(Of Double)(acc2, acc3))
            sum = Vector.Dot(Of Double)(acc0, ones)
        End If

        For k As Integer = i To len - 1
            Dim d As Double = a(k) - b(k)

            sum += d * d
        Next

        Return sum
    End Function

    Public Sub Run()
        Dim n As Integer = 50000
        Dim a As Double() = New Double(99) {}
        Dim b As Double() = New Double(99) {}

        For i As Integer = 0 To a.Length - 1
            a(i) = i * 0.5
            b(i) = i * 0.25
        Next

        Dim timer As Stopwatch = Stopwatch.StartNew()

        For pass As Integer = 1 To 3
            SIMDEnvironment.config = SIMDConfiguration.disable
            timer.Restart()

            Dim s1 As Double = 0

            For i As Integer = 0 To n - 1
                s1 += ScalarLoop(a, b)
            Next

            timer.Stop()
            Console.WriteLine($"scalar-loop         pass{pass}: {timer.Elapsed.TotalMilliseconds,8:F2} ms  ({s1:G6})")

            SIMDEnvironment.config = SIMDConfiguration.auto
            timer.Restart()

            Dim s2 As Double = 0

            For i As Integer = 0 To n - 1
                s2 += VectorLoop(a, b)
            Next

            timer.Stop()
            Console.WriteLine($"vector-loop(inline) pass{pass}: {timer.Elapsed.TotalMilliseconds,8:F2} ms  ({s2:G6})")

            timer.Restart()

            Dim s3 As Double = 0

            For i As Integer = 0 To n - 1
                s3 += SimdReduce.DistanceSquared(a, b)
            Next

            timer.Stop()
            Console.WriteLine($"SimdReduce (simd)   pass{pass}: {timer.Elapsed.TotalMilliseconds,8:F2} ms  ({s3:G6})")

            SIMDEnvironment.config = SIMDConfiguration.disable
            timer.Restart()

            Dim s4 As Double = 0

            For i As Integer = 0 To n - 1
                s4 += SimdReduce.DistanceSquared(a, b)
            Next

            timer.Stop()
            Console.WriteLine($"SimdReduce (scalar) pass{pass}: {timer.Elapsed.TotalMilliseconds,8:F2} ms  ({s4:G6})")
            Console.WriteLine()
        Next
    End Sub
End Module
