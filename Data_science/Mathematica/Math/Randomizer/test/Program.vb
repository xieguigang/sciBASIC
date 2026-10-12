Imports System.Threading
Imports Microsoft.VisualBasic.Math

''' <summary>
''' TrueRandom / EnvironmentEntropy 真随机数模块自测。
''' 
''' 真随机数不做"可复现"断言，仅验证：长度正确、范围正确、多次调用不重复
''' （不可预测）、接口契约、纯环境噪声非恒定、多线程并发无异常、比特平衡。
''' </summary>
Module Program

    Sub Main(args As String())
        Dim failures As Integer = 0

        failures += TestLength()
        failures += TestRange()
        failures += TestUnpredictable()
        failures += TestInterface()
        failures += TestEntropy()
        failures += TestThreadSafety()
        failures += TestBitBalance()

        Console.WriteLine()

        If failures = 0 Then
            Console.WriteLine("ALL TESTS PASSED")
        Else
            Console.WriteLine(failures & " TEST(S) FAILED")
            Environment.Exit(1)
        End If
    End Sub

    Function Check(condition As Boolean, name As String) As Integer
        If condition Then
            Console.WriteLine("  PASS: " & name)
            Return 0
        Else
            Console.WriteLine("  FAIL: " & name)
            Return 1
        End If
    End Function

    Function TestLength() As Integer
        Console.WriteLine("[Length]")
        Dim trng As New TrueRandom()
        Dim f As Integer = 0

        f += Check(trng.GenerateBytes(1024).Length = 1024, "GenerateBytes(1024) length")
        f += Check(trng.GenerateInt32s(100).Length = 100, "GenerateInt32s(100) length")
        f += Check(trng.GenerateDoubles(50).Length = 50, "GenerateDoubles(50) length")

        Dim buf(255) As Byte
        trng.NextBytes(buf)
        f += Check(buf.Length = 256, "NextBytes fills buffer")

        Dim dbls(9) As Double
        trng.NextFloats(dbls)
        f += Check(dbls.Length = 10, "NextFloats fills buffer")

        Return f
    End Function

    Function TestRange() As Integer
        Console.WriteLine("[Range]")
        Dim trng As New TrueRandom()
        Dim f As Integer = 0

        Dim dbls = trng.GenerateDoubles(10000)
        Dim allInRange = True
        For Each d As Double In dbls
            If d < 0 OrElse d >= 1 Then
                allInRange = False
            End If
        Next
        f += Check(allInRange, "GenerateDoubles in [0,1)")

        Dim minOk = True, maxOk = True
        For i As Integer = 0 To 9999
            Dim r = trng.Next(1, 7)
            If r < 1 Then minOk = False
            If r >= 7 Then maxOk = False
        Next
        f += Check(minOk, "Next(1,7) >= 1")
        f += Check(maxOk, "Next(1,7) < 7")
        f += Check(trng.Next(5, 5) = 5, "Next(equal bounds) returns min")

        Return f
    End Function

    Function TestUnpredictable() As Integer
        Console.WriteLine("[Unpredictable]")
        Dim f As Integer = 0

        Dim a = New TrueRandom().GenerateBytes(256)
        Dim b = New TrueRandom().GenerateBytes(256)
        Dim differ = False
        For i As Integer = 0 To 255
            If a(i) <> b(i) Then
                differ = True
                Exit For
            End If
        Next
        f += Check(differ, "Two TrueRandom instances produce different output")

        Dim anyNonZero = False
        For i As Integer = 0 To 255
            If a(i) <> 0 Then
                anyNonZero = True
                Exit For
            End If
        Next
        f += Check(anyNonZero, "GenerateBytes not all zero")

        Return f
    End Function

    Function TestInterface() As Integer
        Console.WriteLine("[IProvideRandomValues]")
        Dim f As Integer = 0
        Dim r As IProvideRandomValues = New TrueRandom()

        f += Check(r.IsThreadSafe, "IsThreadSafe = True")

        Dim d = r.NextFloat()
        f += Check(d >= 0 AndAlso d < 1, "NextFloat in [0,1)")

        Dim buf(3) As Double
        r.NextFloats(buf)
        Dim ok = True
        For Each v In buf
            If v < 0 OrElse v >= 1 Then
                ok = False
            End If
        Next
        f += Check(ok, "NextFloats in [0,1)")

        Dim n = r.Next(10, 20)
        f += Check(n >= 10 AndAlso n < 20, "Interface Next(10,20) in range")

        Return f
    End Function

    Function TestEntropy() As Integer
        Console.WriteLine("[EnvironmentEntropy]")
        Dim f As Integer = 0
        Dim ent As New EnvironmentEntropy()

        Dim seed = ent.GetSeed()
        f += Check(seed.Length = 32, "GetSeed length = 32")

        Dim more = ent.GetBytes(64)
        f += Check(more.Length = 64, "GetBytes(64) length")

        Dim allSame = True
        For i As Integer = 1 To more.Length - 1
            If more(i) <> more(0) Then
                allSame = False
                Exit For
            End If
        Next
        f += Check(Not allSame, "EnvironmentEntropy output not constant")

        Return f
    End Function

    Function TestThreadSafety() As Integer
        Console.WriteLine("[ThreadSafety]")
        Dim f As Integer = 0
        Dim trng As New TrueRandom()
        Dim exceptions As Integer = 0
        Dim totalBytes As Long = 0
        Dim threads As New List(Of Thread)
        Dim latch As New ManualResetEvent(False)

        For t As Integer = 0 To 7
            Dim th As New Thread(Sub()
                                    latch.WaitOne()
                                    Try
                                        For i As Integer = 0 To 49
                                            Dim b = trng.GenerateBytes(64)
                                            Interlocked.Add(totalBytes, b.Length)
                                        Next
                                    Catch
                                        Interlocked.Increment(exceptions)
                                    End Try
                                End Sub)
            th.Start()
            threads.Add(th)
        Next

        latch.Set()

        For Each th In threads
            th.Join()
        Next

        f += Check(exceptions = 0, "No exceptions under concurrency")
        f += Check(totalBytes = 8 * 50 * 64, "All bytes produced (got " & totalBytes & ")")

        Return f
    End Function

    Function TestBitBalance() As Integer
        Console.WriteLine("[BitBalance]")
        Dim f As Integer = 0
        Dim bytes = New TrueRandom().GenerateBytes(20000)
        Dim ones As Long = 0

        For Each b As Byte In bytes
            ones += PopulationCount(b)
        Next

        Dim frac = CDbl(ones) / (bytes.Length * 8)
        f += Check(frac > 0.45 AndAlso frac < 0.55, "Bit-1 fraction ~0.5 (got " & frac.ToString("F3") & ")")

        Return f
    End Function

    Function PopulationCount(b As Byte) As Integer
        Dim c As Integer = 0
        Dim x = b

        While x <> 0
            c += x And 1
            x = x >> 1
        End While

        Return c
    End Function
End Module
