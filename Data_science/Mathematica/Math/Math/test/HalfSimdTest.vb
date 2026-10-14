#Region "Microsoft.VisualBasic::HalfSimdTest, Data_science\Mathematica\Math\Math\test\HalfSimdTest.vb"

    ' 半精度 SIMD 内核（HalfSimd）的差分自测
    '
    ' 设计要点：
    '   1. 转换内核以 BCL System.Half 的标量语义为基准做**逐位**比对，
    '      覆盖全部 65536 个 half 位型，以及次正规/正负零/NaN payload/溢出到无穷等边界；
    '   2. 融合算子与标量路径 a op b 逐位一致（BCL 的 Half.op_Addition 本身就是
    '      "转 Single 运算再转回"，所以只要转换是 RNE，两者必然一致）；
    '   3. 向量路径与 SIMD 关闭时的标量回退路径必须给出完全相同的结果；
    '   4. 归约（Sum/Dot/Norm）的归约顺序在向量化后会发生改变，因此按容差比对。

#End Region

Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Numerics
Imports SIMDConfiguration = Microsoft.VisualBasic.Math.SIMD.SIMDConfiguration
Imports SIMDEnvironment = Microsoft.VisualBasic.Math.SIMD.SIMDEnvironment
Imports SimdCapabilities = Microsoft.VisualBasic.Math.SIMD.SimdCapabilities
Imports std = System.Math

Public Module HalfSimdTest

    Private failures As Integer = 0
    Private checks As Integer = 0

    ''' <summary>
    ''' 运行全部半精度 SIMD 自测
    ''' </summary>
    ''' <returns>0 表示全部通过</returns>
    Public Function RunAll() As Integer
        Console.WriteLine("==================================================================")
        Console.WriteLine(" 半精度 SIMD 内核差分自测（HalfSimd vs System.Half 标量语义）")
        Console.WriteLine("==================================================================")
        Console.WriteLine()
        Console.WriteLine(SimdCapabilities.Description)
        Console.WriteLine($"SIMD enabled = {SIMDEnvironment.IsEnabled}, lanes = {HalfSimd.Lanes}")
        Console.WriteLine()

        failures = 0
        checks = 0

        TestHalfToSingleExhaustive()
        TestSingleToHalfBoundaries()
        TestSingleToHalfRandom()
        TestElementwiseOps()
        TestScalarBroadcastOps()
        TestReductions()
        TestScalarFallbackPath()
        TestHalfVectorApi()

        Console.WriteLine()
        Console.WriteLine("------------------------------------------------------------------")
        Console.WriteLine($" 断言总数 = {checks}, 失败 = {failures}")
        Console.WriteLine("------------------------------------------------------------------")

        Return If(failures = 0, 0, 1)
    End Function

    ' ------------------------------------------------------------------
    ' 断言工具
    ' ------------------------------------------------------------------

    Private Sub Ok(condition As Boolean, message As String)
        checks += 1

        If Not condition Then
            failures += 1
            Console.WriteLine($"  [FAIL] {message}")
        End If
    End Sub

    Private Sub Section(name As String)
        Console.WriteLine($"[{name}]")
    End Sub

    Private Sub Done(name As String, extra As String)
        Console.WriteLine($"  {name}: {extra}")
    End Sub

    ''' <summary>按位比较两个 Half</summary>
    Private Function Bits(h As Half) As UShort
        Return BitConverter.HalfToUInt16Bits(h)
    End Function

    ''' <summary>把整数位型还原为 Half</summary>
    Private Function BitsOf(bits As Integer) As Half
        Return BitConverter.UInt16BitsToHalf(CUShort(bits))
    End Function

    ' ------------------------------------------------------------------
    ' 1. (Single)Half：全部 65536 个位型
    ' ------------------------------------------------------------------

    Private Sub TestHalfToSingleExhaustive()
        Section("1. (Single)Half 全位型逐位比对")

        Dim source As Half() = GC.AllocateUninitializedArray(Of Half)(&H10000)
        Dim expected As Single() = GC.AllocateUninitializedArray(Of Single)(&H10000)

        For i As Integer = 0 To &HFFFF
            source(i) = BitsOf(i)
            expected(i) = CSng(source(i))
        Next

        Dim actual As Single() = HalfSimd.ToSingle(source)
        Dim bad As Integer = 0
        Dim firstBad As Integer = -1

        For i As Integer = 0 To &HFFFF
            Dim a As Integer = BitConverter.SingleToInt32Bits(actual(i))
            Dim b As Integer = BitConverter.SingleToInt32Bits(expected(i))

            ' NaN 只比较"是不是 NaN"，因为 payload 位允许不同实现选择
            If Single.IsNaN(expected(i)) Then
                If Not Single.IsNaN(actual(i)) Then
                    bad += 1
                    If firstBad < 0 Then firstBad = i
                End If
            ElseIf a <> b Then
                bad += 1
                If firstBad < 0 Then firstBad = i
            End If
        Next

        Ok(bad = 0, $"(Single)Half 存在 {bad} 个位型与 BCL 不一致，首个失败位型 = 0x{firstBad:X4}")
        Done("全位型", $"65536 个位型，不一致 {bad} 个")

        ' 反向：把转换结果再转回 Half，应当还原为原始位型（NaN 除外）
        Dim roundTrip As Half() = HalfSimd.ToHalf(actual)
        Dim rtBad As Integer = 0

        For i As Integer = 0 To &HFFFF
            If Half.IsNaN(source(i)) Then Continue For
            If Bits(roundTrip(i)) <> Bits(source(i)) Then rtBad += 1
        Next

        Ok(rtBad = 0, $"Half -> Single -> Half 往返存在 {rtBad} 个位型不还原")
        Done("往返", $"不一致 {rtBad} 个")
        Console.WriteLine()
    End Sub

    ' ------------------------------------------------------------------
    ' 2. (Half)Single：边界值
    ' ------------------------------------------------------------------

    Private Sub TestSingleToHalfBoundaries()
        Section("2. (Half)Single 边界值逐位比对")

        Dim values As New List(Of Single) From {
            0.0F, -0.0F, 1.0F, -1.0F, 0.5F, 0.3F, 1.0E-7F,
            6.1035156E-05F,   ' 2^-14：最小正规数
            6.0975552E-05F,   ' 最大次正规数
            5.9604645E-08F,   ' 2^-24：最小次正规数
            2.9802322E-08F,   ' 2^-25：应当舍入到 0
            4.4703484E-08F,   ' 1.5 x 2^-25：应当舍入到最小次正规数（tie-to-even）
            65504.0F,         ' Half.MaxValue
            65519.996F,       ' 仍然舍入到 MaxValue
            65520.0F,         ' 溢出到 +Inf 的阈值（tie）
            65536.0F,         ' 直接溢出
            -65504.0F, -65520.0F, -65536.0F,
            Single.PositiveInfinity, Single.NegativeInfinity, Single.NaN,
            1.00146484375F,   ' = 1 + 1.5 x 2^-10：两个 half 的正中间（tie-to-even 向上）
            1.00048828125F,   ' = 1 + 2^-11：tie-to-even 向下
            Single.MaxValue, Single.MinValue,
            Single.Epsilon
        }

        ' NaN 的不同 payload
        values.Add(BitConverter.Int32BitsToSingle(&H7FC00001))
        values.Add(BitConverter.Int32BitsToSingle(&HFFC00001))
        values.Add(BitConverter.Int32BitsToSingle(&H7F800001))
        values.Add(BitConverter.Int32BitsToSingle(&HFF800001))

        Dim arr As Single() = values.ToArray()
        Dim actual As Half() = HalfSimd.ToHalf(arr)
        Dim bad As Integer = 0

        For i As Integer = 0 To arr.Length - 1
            Dim expect As Half = CType(arr(i), Half)

            If Half.IsNaN(expect) Then
                If Not Half.IsNaN(actual(i)) Then bad += 1
            ElseIf Bits(actual(i)) <> Bits(expect) Then
                bad += 1
                Console.WriteLine($"        mismatch: f=0x{BitConverter.SingleToInt32Bits(arr(i)):X8} expect=0x{Bits(expect):X4} actual=0x{Bits(actual(i)):X4}")
            End If
        Next

        Ok(bad = 0, $"(Half)Single 边界值存在 {bad} 个不一致")
        Done("边界值", $"{arr.Length} 个样本，不一致 {bad} 个")
        Console.WriteLine()
    End Sub

    ' ------------------------------------------------------------------
    ' 3. (Half)Single：随机位型 + tie 扫描
    ' ------------------------------------------------------------------

    Private Sub TestSingleToHalfRandom()
        Section("3. (Half)Single 随机与舍入 tie 扫描")

        Dim rnd As New Random(20261015)
        Dim sample As Single() = GC.AllocateUninitializedArray(Of Single)(1 << 18)

        ' 一半随机位型（覆盖 NaN/Inf/次正规），一半"舍入关键位"扫描
        For i As Integer = 0 To sample.Length - 1
            If (i And 1) = 0 Then
                sample(i) = BitConverter.Int32BitsToSingle(rnd.Next(Integer.MinValue, Integer.MaxValue))
            Else
                ' 构造 tie：尾数低 13 位取 0x1000（正好一半）以及各种边界
                Dim exp As Integer = rnd.Next(90, 150)
                Dim mantissa As Integer = rnd.Next(0, &H800000) And &H7FE000
                Dim tail As Integer = (1 << (13 - (i Mod 3))) - 1

                sample(i) = BitConverter.Int32BitsToSingle((exp << 23) Or mantissa Or tail)
            End If
        Next

        Dim actual As Half() = HalfSimd.ToHalf(sample)
        Dim bad As Integer = 0
        Dim firstBad As Integer = -1

        For i As Integer = 0 To sample.Length - 1
            Dim expect As Half = CType(sample(i), Half)

            If Half.IsNaN(expect) Then
                If Not Half.IsNaN(actual(i)) Then
                    bad += 1
                    If firstBad < 0 Then firstBad = i
                End If
            ElseIf Bits(actual(i)) <> Bits(expect) Then
                bad += 1
                If firstBad < 0 Then
                    firstBad = i
                    Console.WriteLine($"        first mismatch: f=0x{BitConverter.SingleToInt32Bits(sample(i)):X8} expect=0x{Bits(expect):X4} actual=0x{Bits(actual(i)):X4}")
                End If
            End If
        Next

        Ok(bad = 0, $"(Half)Single 随机样本存在 {bad} 个不一致，首个下标 = {firstBad}")
        Done("随机样本", $"{sample.Length} 个样本，不一致 {bad} 个")
        Console.WriteLine()
    End Sub

    ' ------------------------------------------------------------------
    ' 4. 融合算子（vector vector）
    ' ------------------------------------------------------------------

    Private Sub TestElementwiseOps()
        Section("4. 逐元素融合算子（Half x Half）")

        Dim rnd As New Random(42)
        Dim lengths As Integer() = {0, 1, 2, 3, 7, 8, 9, 15, 16, 17, 31, 33, 100, 1000, 4096}

        For Each n As Integer In lengths
            Dim a As Half() = RandomHalves(rnd, n)
            Dim b As Half() = RandomHalves(rnd, n)

            CompareBits($"Add[{n}]", HalfSimd.Add(a, b), ReferenceBinary(a, b, Function(x, y) x + y))
            CompareBits($"Subtract[{n}]", HalfSimd.Subtract(a, b), ReferenceBinary(a, b, Function(x, y) x - y))
            CompareBits($"Multiply[{n}]", HalfSimd.Multiply(a, b), ReferenceBinary(a, b, Function(x, y) x * y))
            CompareBits($"Divide[{n}]", HalfSimd.Divide(a, b), ReferenceBinary(a, b, Function(x, y) x / y))
        Next

        Done("融合算子", $"{lengths.Length} 种长度 x 4 个算子")
        Console.WriteLine()
    End Sub

    ' ------------------------------------------------------------------
    ' 5. 标量广播算子
    ' ------------------------------------------------------------------

    Private Sub TestScalarBroadcastOps()
        Section("5. 标量广播算子（Half x Single）")

        Dim rnd As New Random(7)
        Dim scalars As Single() = {0.0F, 1.0F, -1.0F, 0.3F, 2.5F, 100.0F, 0.001F, -0.7F}

        For Each s As Single In scalars
            For Each n As Integer In {1, 5, 8, 9, 17, 513}
                Dim a As Half() = RandomHalves(rnd, n)

                CompareBits($"AddScalar[{n},{s}]",
                    HalfSimd.AddScalar(a, s),
                    ReferenceScalar(a, s, Function(p, q) p + q))
                CompareBits($"SubtractScalar[{n},{s}]",
                    HalfSimd.SubtractScalar(a, s),
                    ReferenceScalar(a, s, Function(p, q) p - q))
                CompareBits($"MultiplyScalar[{n},{s}]",
                    HalfSimd.MultiplyScalar(a, s),
                    ReferenceScalar(a, s, Function(p, q) p * q))
                CompareBits($"DivideScalar[{n},{s}]",
                    HalfSimd.DivideScalar(a, s),
                    ReferenceScalar(a, s, Function(p, q) p / q))
            Next
        Next

        ' 融合乘加：只舍入一次，与"先乘再转 Half 再加"不同
        Dim x As Half() = RandomHalves(rnd, 1000)
        Dim y As Half() = RandomHalves(rnd, 1000)
        Const addend As Single = 0.75F

        CompareBits("MultiplyAdd[1000]",
            HalfSimd.MultiplyAdd(x, y, addend),
            ReferenceBinary(x, y, Function(p, q) p * q + addend))

        Done("标量广播", "8 个标量 x 6 种长度 x 4 个算子 + MultiplyAdd")
        Console.WriteLine()
    End Sub

    ' ------------------------------------------------------------------
    ' 6. 归约
    ' ------------------------------------------------------------------

    Private Sub TestReductions()
        Section("6. 归约（Sum / Dot / Norm / Min / Max）")

        Dim rnd As New Random(99)

        For Each n As Integer In {1, 7, 8, 9, 100, 1000, 4096}
            Dim a As Half() = RandomHalves(rnd, n)
            Dim b As Half() = RandomHalves(rnd, n)

            Dim refSum As Double = 0R
            Dim refDot As Double = 0R

            For i As Integer = 0 To n - 1
                refSum += CSng(a(i))
                refDot += CSng(a(i)) * CSng(b(i))
            Next

            Close($"Sum[{n}]", HalfSimd.Sum(a), refSum)
            Close($"Dot[{n}]", HalfSimd.Dot(a, b), refDot)
            Close($"Norm[{n}]", HalfSimd.Norm(a), std.Sqrt(ReferenceSumOfSquares(a)))

            ' Min/Max：不含 NaN 时逐位可比
            Dim refMin As Single = CSng(a(0))
            Dim refMax As Single = CSng(a(0))

            For i As Integer = 1 To n - 1
                Dim xi As Single = CSng(a(i))

                If xi < refMin Then refMin = xi
                If xi > refMax Then refMax = xi
            Next

            Ok(Bits(HalfSimd.Min(a)) = Bits(CType(refMin, Half)), $"Min[{n}] 不一致")
            Ok(Bits(HalfSimd.Max(a)) = Bits(CType(refMax, Half)), $"Max[{n}] 不一致")
        Next

        Done("归约", "7 种长度")
        Console.WriteLine()
    End Sub

    ' ------------------------------------------------------------------
    ' 7. 向量路径 vs 标量回退路径
    ' ------------------------------------------------------------------

    Private Sub TestScalarFallbackPath()
        Section("7. 向量路径 vs 标量回退路径（SIMD disable）必须逐位一致")

        Dim rnd As New Random(2024)
        Dim a As Half() = RandomHalves(rnd, 1024)
        Dim b As Half() = RandomHalves(rnd, 1024)

        Dim vecAdd As Half() = HalfSimd.Add(a, b)
        Dim vecMul As Half() = HalfSimd.Multiply(a, b)
        Dim vecScalar As Half() = HalfSimd.MultiplyScalar(a, 1.25F)
        Dim vecSingle As Single() = HalfSimd.ToSingle(a)

        Dim saved = SIMDEnvironment.config
        SIMDEnvironment.config = SIMDConfiguration.disable

        Dim scalarAdd As Half() = HalfSimd.Add(a, b)
        Dim scalarMul As Half() = HalfSimd.Multiply(a, b)
        Dim scalarScalar As Half() = HalfSimd.MultiplyScalar(a, 1.25F)
        Dim scalarSingle As Single() = HalfSimd.ToSingle(a)

        SIMDEnvironment.config = saved

        CompareBits("Add: simd vs scalar", vecAdd, scalarAdd)
        CompareBits("Multiply: simd vs scalar", vecMul, scalarMul)
        CompareBits("MultiplyScalar: simd vs scalar", vecScalar, scalarScalar)

        Dim badFloat As Integer = 0

        For i As Integer = 0 To vecSingle.Length - 1
            If BitConverter.SingleToInt32Bits(vecSingle(i)) <> BitConverter.SingleToInt32Bits(scalarSingle(i)) Then
                badFloat += 1
            End If
        Next

        Ok(badFloat = 0, $"ToSingle: simd vs scalar 存在 {badFloat} 处不一致")
        Done("回退路径", "向量与标量结果逐位一致")
        Console.WriteLine()
    End Sub

    ' ------------------------------------------------------------------
    ' 8. HalfVector 公开 API 冒烟
    ' ------------------------------------------------------------------

    Private Sub TestHalfVectorApi()
        Section("8. HalfVector 公开 API 冒烟")

        Dim rnd As New Random(314)
        Dim raw As Half() = RandomHalves(rnd, 300)
        Dim v As New HalfVector(raw)
        Dim w As New HalfVector(RandomHalves(rnd, 300))

        CompareBits("v + 1.5F", (v + 1.5F).Array, ReferenceScalar(raw, 1.5F, Function(x, y) x + y))
        CompareBits("v * 2.0F", (v * 2.0F).Array, ReferenceScalar(raw, 2.0F, Function(x, y) x * y))
        CompareBits("v + w", (v + w).Array, ReferenceBinary(raw, w.Array, Function(x, y) x + y))

        ' Sum / Mean 与逐元素 Double 累加一致（容差内）
        Dim refSum As Double = 0R

        For Each x As Half In raw
            refSum += CSng(x)
        Next

        Close("HalfVector.Sum", v.Sum, refSum)
        Close("HalfVector.Mean", v.Mean, refSum / 300)

        ' 批量转换往返
        Dim singles As Single() = v.ToSingle()
        Dim back As HalfVector = HalfVector.FromSingles(singles)

        CompareBits("ToSingle -> FromSingles 往返", back.Array, raw)
        Ok(v.ToSingle().Length = 300, "ToSingle 长度不对")

        ' 与单精度向量相乘
        Dim dense As New Vector(HalfSimd.ToSingle(raw))
        Dim prod As Vector = v * dense
        Dim badProd As Integer = 0

        For i As Integer = 0 To 299
            If std.Abs(prod(i) - CSng(raw(i)) * CSng(raw(i))) > 1.0E-3F Then badProd += 1
        Next

        Ok(badProd = 0, $"v * Vector 存在 {badProd} 处不一致")
        Done("HalfVector", "算子 / 归约 / 批量转换 / 与 Vector 相乘")
        Console.WriteLine()
    End Sub

    ' ------------------------------------------------------------------
    ' 参考实现（全部走 System.Half 的标量语义）
    ' ------------------------------------------------------------------

    Private Function ReferenceBinary(a As Half(), b As Half(),
                                     op As Func(Of Single, Single, Single)) As Half()
        Dim out As Half() = GC.AllocateUninitializedArray(Of Half)(a.Length)

        For i As Integer = 0 To a.Length - 1
            out(i) = CType(op(CSng(a(i)), CSng(b(i))), Half)
        Next

        Return out
    End Function

    Private Function ReferenceScalar(a As Half(), s As Single,
                                      op As Func(Of Single, Single, Single)) As Half()
        Dim out As Half() = GC.AllocateUninitializedArray(Of Half)(a.Length)

        For i As Integer = 0 To a.Length - 1
            out(i) = CType(op(CSng(a(i)), s), Half)
        Next

        Return out
    End Function

    Private Function ReferenceSumOfSquares(a As Half()) As Double
        Dim total As Double = 0R

        For Each x As Half In a
            total += CSng(x) * CSng(x)
        Next

        Return total
    End Function

    Private Sub CompareBits(name As String, actual As Half(), expected As Half())
        If actual.Length <> expected.Length Then
            Ok(False, $"{name}: 长度不一致 {actual.Length} <> {expected.Length}")
            Return
        End If

        Dim bad As Integer = 0

        For i As Integer = 0 To actual.Length - 1
            If Bits(actual(i)) <> Bits(expected(i)) Then bad += 1
        Next

        Ok(bad = 0, $"{name}: {bad}/{actual.Length} 个元素与标量参考不一致")
    End Sub

    Private Sub Close(name As String, actual As Double, expected As Double)
        Dim diff As Double = std.Abs(actual - expected)
        Dim scale As Double = std.Max(1.0, std.Abs(expected))

        Ok(diff / scale < 1.0E-12, $"{name}: 实际 {actual} 与参考 {expected} 相差 {diff}")
    End Sub

    Private Function RandomHalves(rnd As Random, n As Integer) As Half()
        Dim out As Half() = GC.AllocateUninitializedArray(Of Half)(n)

        For i As Integer = 0 To n - 1
            Select Case rnd.Next(0, 12)
                Case 0
                    out(i) = CType(rnd.NextDouble() * 2 - 1, Half)
                Case 1
                    out(i) = CType(rnd.NextDouble() * 1000 - 500, Half)
                Case 2
                    out(i) = CType(rnd.NextDouble() * 1.0E-4, Half)
                Case 3
                    out(i) = BitsOf(rnd.Next(0, &H10000))
                Case Else
                    out(i) = CType(CSng(rnd.NextDouble() * 20 - 10), Half)
            End Select
        Next

        ' 刻意混入若干特殊值（保证尾部/对齐路径也覆盖到）
        If n >= 4 Then
            out(0) = CType(0.0F, Half)
            out(1) = CType(65504.0F, Half)
            out(2) = CType(-0.5F, Half)
            out(3) = BitsOf(&H1)   ' 最小次正规数
        End If

        Return out
    End Function
End Module
