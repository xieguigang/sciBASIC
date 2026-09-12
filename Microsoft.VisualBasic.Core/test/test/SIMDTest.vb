#Region "Microsoft.VisualBasic::c78dace7f4bb96ffc127d32e352782b0, Microsoft.VisualBasic.Core\test\test\SIMDTest.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 687
    '    Code Lines: 493 (71.76%)
    ' Comment Lines: 45 (6.55%)
    '    - Xml Docs: 84.44%
    ' 
    '   Blank Lines: 149 (21.69%)
    '     File Size: 31.23 KB


    ' Module SIMDTest
    ' 
    '     Function: CheckInPlaceAdd, LoopArgMax, LoopArgMin, LoopDot, LoopL1
    '               LoopMax, LoopMin, LoopSum, LoopSumSquares, NaiveMatrixDot
    '               NearlyEqual, RandomData, RandomIntegers, Ref, Ref1
    '               RefCmp, ScalarAdd, ScalarMultiplyScalar, SequenceEqual, TestSizes
    '               TimeBest
    ' 
    '     Sub: BenchAdd, BenchAddInPlace, BenchDot, Benchmark, BenchMultiplyScalar
    '          BenchSum, Check, Main1, Run, ScalarAddInPlace
    '          VerifyCompare, VerifyDivideSemantics, VerifyElementwise, VerifyModuloSemantics, VerifyParallel
    '          VerifyReduce, VerifyScalarForms, VerifyUnary, WriteLine
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Diagnostics
Imports System.Numerics
Imports Microsoft.VisualBasic.Math.SIMD
Imports std = System.Math

''' <summary>
''' SIMD 数学模块的正确性回归与性能基准测试。
''' </summary>
''' <remarks>
''' 运行入口：<c>test.exe --simd</c>
''' <list type="number">
''' <item><description>逐算子、逐类型地与独立实现的标量参照结果做一致性校验</description></item>
''' <item><description>边界用例：长度 0 / 1 / 向量宽度 ±1 / 不整除长度</description></item>
''' <item><description>Divide 的“分子为零则结果为零”语义回归</description></item>
''' <item><description>Modulo 的 VB <c>Mod</c> 语义回归</description></item>
''' <item><description>分块并行结果与单线程向量化结果的一致性</description></item>
''' <item><description>1e3 / 1e5 / 1e7 三种规模下的标量 vs 向量 vs 并行耗时与加速比</description></item>
''' </list>
''' </remarks>
Module SIMDTest

    Private ReadOnly rand As New Random(20260911)

    Private failures As Integer = 0

    ''' <summary>
    ''' 运行全部正确性回归检查。
    ''' </summary>
    Sub Run()
        Console.WriteLine("=========================================================")
        Console.WriteLine(" SIMD math module verification")
        Console.WriteLine("=========================================================")
        Console.WriteLine($" machine : {SIMDEnvironment.Description}")
        Console.WriteLine($" config  : {SIMDEnvironment.config}, enabled={SIMDEnvironment.IsEnabled}")
        Console.WriteLine()

        Call VerifyElementwise()
        Call VerifyScalarForms()
        Call VerifyDivideSemantics()
        Call VerifyModuloSemantics()
        Call VerifyUnary()
        Call VerifyReduce()
        Call VerifyCompare()
        Call VerifyParallel()

        Console.WriteLine()
        If failures = 0 Then
            Console.WriteLine("[PASS] all correctness checks passed!")
        Else
            Console.WriteLine($"[FAIL] {failures} correctness check(s) failed!")
        End If

        Console.WriteLine()
        Call Benchmark()
    End Sub

    ''' <summary>
    ''' 【兼容保留】历史入口。
    ''' </summary>
    Sub Main1()
        Call Run()
    End Sub

#Region "test helpers"

    Private Function RandomData(n As Integer) As Double()
        Dim v As Double() = New Double(n - 1) {}

        For i As Integer = 0 To n - 1
            v(i) = rand.NextDouble() * 200 - 100
        Next

        Return v
    End Function

    Private Function RandomIntegers(n As Integer) As Integer()
        Dim v As Integer() = New Integer(n - 1) {}

        For i As Integer = 0 To n - 1
            v(i) = rand.Next(-100, 101)
        Next

        Return v
    End Function

    ''' <summary>
    ''' 覆盖各种边界与不整除长度：0、1、2、3、width-1、width、width+1、7、13、100、1000
    ''' </summary>
    Private Function TestSizes() As Integer()
        Dim width As Integer = Vector(Of Double).Count

        Return {0, 1, 2, 3, width - 1, width, width + 1, 7, 13, 100, 1000}
    End Function

    ''' <summary>
    ''' 独立的标量参照实现（二元运算）。
    ''' </summary>
    Private Function Ref(Of T)(a As T(), b As T(), f As Func(Of T, T, T)) As T()
        If a.Length = 0 Then Return Array.Empty(Of T)()

        Dim r As T() = New T(a.Length - 1) {}

        For i As Integer = 0 To a.Length - 1
            r(i) = f(a(i), b(i))
        Next

        Return r
    End Function

    ''' <summary>
    ''' 独立的标量参照实现（一元运算）。
    ''' </summary>
    Private Function Ref1(Of T)(a As T(), f As Func(Of T, T)) As T()
        If a.Length = 0 Then Return Array.Empty(Of T)()

        Dim r As T() = New T(a.Length - 1) {}

        For i As Integer = 0 To a.Length - 1
            r(i) = f(a(i))
        Next

        Return r
    End Function

    ''' <summary>
    ''' 独立的标量参照实现（比较运算：元素类型 -&gt; <see cref="Boolean"/>）。
    ''' </summary>
    Private Function RefCmp(Of T)(a As T(), b As T(), f As Func(Of T, T, Boolean)) As Boolean()
        If a.Length = 0 Then Return Array.Empty(Of Boolean)()

        Dim r As Boolean() = New Boolean(a.Length - 1) {}

        For i As Integer = 0 To a.Length - 1
            r(i) = f(a(i), b(i))
        Next

        Return r
    End Function

    Private Function SequenceEqual(Of T)(a As T(), b As T()) As Boolean
        If a Is Nothing OrElse b Is Nothing Then Return False
        If a.Length <> b.Length Then Return False

        For i As Integer = 0 To a.Length - 1
            If Not a(i).Equals(b(i)) Then Return False
        Next

        Return True
    End Function

    Private Function NearlyEqual(a As Double(), b As Double(), Optional eps As Double = 1.0E-12) As Boolean
        If a.Length <> b.Length Then Return False

        For i As Integer = 0 To a.Length - 1
            Dim scale As Double = std.Max(1.0, std.Abs(a(i)))

            If std.Abs(a(i) - b(i)) > eps * scale Then Return False
        Next

        Return True
    End Function

    Private Sub Check(name As String, ok As Boolean)
        If ok Then
            Console.WriteLine("  [ok]   " & name)
        Else
            failures += 1
            Console.WriteLine("  [FAIL] " & name)
        End If
    End Sub

#End Region

#Region "correctness"

    Private Sub VerifyElementwise()
        Console.WriteLine("[1] elementwise arithmetic")

        For Each n As Integer In TestSizes()
            Dim a As Double() = RandomData(n)
            Dim b As Double() = RandomData(n)
            Dim ia As Integer() = RandomIntegers(n)
            Dim ib As Integer() = RandomIntegers(n)
            Dim la As Long() = Array.ConvertAll(ia, Function(x) CLng(x))
            Dim lb As Long() = Array.ConvertAll(ib, Function(x) CLng(x))
            Dim sa As Short() = Array.ConvertAll(ia, Function(x) CShort(x))
            Dim sb As Short() = Array.ConvertAll(ib, Function(x) CShort(x))
            Dim fa As Single() = Array.ConvertAll(a, Function(x) CSng(x))
            Dim fb As Single() = Array.ConvertAll(b, Function(x) CSng(x))
            Dim tag As String = $"n={n}"

            Check($"f64 add          {tag}", SequenceEqual(Ref(a, b, Function(x, y) x + y), SimdEngine.Add(Of Double)(a, b)))
            Check($"f64 subtract     {tag}", SequenceEqual(Ref(a, b, Function(x, y) x - y), SimdEngine.Subtract(Of Double)(a, b)))
            Check($"f64 multiply     {tag}", SequenceEqual(Ref(a, b, Function(x, y) x * y), SimdEngine.Multiply(Of Double)(a, b)))
            Check($"f64 min          {tag}", SequenceEqual(Ref(a, b, Function(x, y) std.Min(x, y)), SimdEngine.Min(Of Double)(a, b)))
            Check($"f64 max          {tag}", SequenceEqual(Ref(a, b, Function(x, y) std.Max(x, y)), SimdEngine.Max(Of Double)(a, b)))
            Check($"f32 add          {tag}", SequenceEqual(Ref(fa, fb, Function(x, y) x + y), SimdEngine.Add(Of Single)(fa, fb)))
            Check($"f32 multiply     {tag}", SequenceEqual(Ref(fa, fb, Function(x, y) x * y), SimdEngine.Multiply(Of Single)(fa, fb)))
            Check($"i32 add          {tag}", SequenceEqual(Ref(ia, ib, Function(x, y) x + y), SimdEngine.Add(Of Integer)(ia, ib)))
            Check($"i32 subtract     {tag}", SequenceEqual(Ref(ia, ib, Function(x, y) x - y), SimdEngine.Subtract(Of Integer)(ia, ib)))
            Check($"i32 multiply     {tag}", SequenceEqual(Ref(ia, ib, Function(x, y) x * y), SimdEngine.Multiply(Of Integer)(ia, ib)))
            Check($"i64 multiply     {tag}", SequenceEqual(Ref(la, lb, Function(x, y) x * y), SimdEngine.Multiply(Of Long)(la, lb)))
            Check($"i16 multiply     {tag}", SequenceEqual(Ref(sa, sb, Function(x, y) CShort(x * y)), SimdEngine.Multiply(Of Short)(sa, sb)))
            Check($"f64 in-place add {tag}", CheckInPlaceAdd(a, b))
        Next
    End Sub

    Private Function CheckInPlaceAdd(a As Double(), b As Double()) As Boolean
        Dim expect As Double() = Ref(a, b, Function(x, y) x + y)
        Dim target As Double() = CType(a.Clone(), Double())

        Call SimdEngine.AddInPlace(Of Double)(target, b)

        If Not SequenceEqual(expect, target) Then Return False

        ' 就地运算不能使用“末块重叠”技巧，否则尾部元素会被重复累加
        Dim expectScalarAdd As Double() = Ref1(a, Function(x) x + 2.5)
        Dim expectScalarMul As Double() = Ref1(a, Function(x) x * 2.5)
        Dim expectSubtract As Double() = Ref(a, b, Function(x, y) x - y)
        Dim expectMultiply As Double() = Ref(a, b, Function(x, y) x * y)
        Dim t2 As Double() = CType(a.Clone(), Double())
        Dim t3 As Double() = CType(a.Clone(), Double())
        Dim t4 As Double() = CType(a.Clone(), Double())
        Dim t5 As Double() = CType(a.Clone(), Double())

        Call SimdEngine.AddScalarInPlace(Of Double)(t2, 2.5)
        Call SimdEngine.MultiplyScalarInPlace(Of Double)(t3, 2.5)
        Call SimdEngine.SubtractInPlace(Of Double)(t4, b)
        Call SimdEngine.MultiplyInPlace(Of Double)(t5, b)

        Return SequenceEqual(expectScalarAdd, t2) AndAlso
            SequenceEqual(expectScalarMul, t3) AndAlso
            SequenceEqual(expectSubtract, t4) AndAlso
            SequenceEqual(expectMultiply, t5)
    End Function

    Private Sub VerifyScalarForms()
        Console.WriteLine("[2] vector - scalar forms")

        For Each n As Integer In TestSizes()
            Dim a As Double() = RandomData(n)
            Dim tag As String = $"n={n}"

            Check($"f64 add scalar      {tag}", SequenceEqual(Ref1(a, Function(x) x + 2.5), SimdEngine.AddScalar(Of Double)(a, 2.5)))
            Check($"f64 sub scalar      {tag}", SequenceEqual(Ref1(a, Function(x) x - 2.5), SimdEngine.SubtractScalar(Of Double)(a, 2.5)))
            Check($"f64 scalar sub      {tag}", SequenceEqual(Ref1(a, Function(x) 2.5 - x), SimdEngine.ScalarSubtract(Of Double)(2.5, a)))
            Check($"f64 mul scalar      {tag}", SequenceEqual(Ref1(a, Function(x) 2.5 * x), SimdEngine.MultiplyScalar(Of Double)(2.5, a)))
            Check($"f64 div scalar      {tag}", SequenceEqual(Ref1(a, Function(x) x / 2.5), SimdEngine.DivideScalar(a, 2.5)))
            Check($"f64 scalar div      {tag}", SequenceEqual(Ref1(a, Function(x) 2.5 / x), SimdEngine.ScalarDivide(2.5, a)))
        Next
    End Sub

    ''' <summary>
    ''' <c>Divide.f64_op_divide_f64</c> 的历史语义：分子为 0 时结果直接为 0（避免 0/0 产生 NaN）。
    ''' </summary>
    Private Sub VerifyDivideSemantics()
        Dim a As Double() = {0.0, -0.0, 1.0, -1.0, 0.0, 3.5, 0.0, 7.0}
        Dim b As Double() = {0.0, 0.0, 2.0, 4.0, 5.0, 0.0, -3.0, 2.0}

        Check("divide zero-numerator semantics", SequenceEqual(Ref(a, b, Function(x, y) If(x = 0.0, 0.0, x / y)), Divide.f64_op_divide_f64(a, b)))

        Dim q = Divide.f64_op_divide_f64(a, b)

        Check("divide zero-numerator has no NaN", Not Array.Exists(q, Function(x) Double.IsNaN(x)))
        Check("divide non-zero numerator keeps quotient", q(4) = 0.0 AndAlso q(7) = 3.5)

        ' 无零分子的普通除法必须与 IEEE 除法逐位一致
        Dim c As Double() = {1.0, -1.0, 3.5, 7.0}
        Dim d As Double() = {2.0, 4.0, 0.5, 2.0}

        Check("divide plain f64 matches ieee", SequenceEqual(Ref(c, d, Function(x, y) x / y), SimdEngine.Divide(c, d)))
    End Sub

    ''' <summary>
    ''' Modulo 必须保持 VB <c>Mod</c> 运算符的语义（不能用 <c>IEEERemainder</c> 替代）。
    ''' </summary>
    Private Sub VerifyModuloSemantics()
        Dim a As Double() = {7.0, -7.0, 7.5, -7.5, 5.0}
        Dim b As Double() = {3.0, 3.0, 2.0, 2.0, -3.0}

        Check("f64 modulo semantics", SequenceEqual(Ref(a, b, Function(x, y) x Mod y), Modulo.f64_op_modulo_f64(a, b)))
        Check("f64 modulo scalar semantics", SequenceEqual(Ref1(a, Function(x) x Mod 3.0), Modulo.f64_op_modulo_f64_scalar(a, 3.0)))
        Check("i32 modulo semantics", SequenceEqual(Ref({7, -7, 5}, {3, 3, -3}, Function(x, y) x Mod y),
                                                    Modulo.int32_op_modulo_int32({7, -7, 5}, {3, 3, -3})))
        Check("empty modulo returns empty", Modulo.f64_op_modulo_f64(Array.Empty(Of Double)(), Array.Empty(Of Double)()).Length = 0)
    End Sub

    Private Sub VerifyUnary()
        Console.WriteLine("[3] unary math")

        For Each n As Integer In TestSizes()
            Dim a As Double() = RandomData(n)
            Dim tag As String = $"n={n}"

            Check($"f64 abs         {tag}", SequenceEqual(Ref1(a, Function(x) std.Abs(x)), SimdMath.Abs(Of Double)(a)))
            Check($"f64 negate      {tag}", SequenceEqual(Ref1(a, Function(x) -x), SimdMath.Negate(Of Double)(a)))
            Check($"f64 square      {tag}", NearlyEqual(Ref1(a, Function(x) x ^ 2), SimdMath.Square(Of Double)(a)))
            Check($"f64 clamp       {tag}", SequenceEqual(Ref1(a, Function(x) std.Min(std.Max(x, -10.0), 10.0)), SimdMath.Clamp(Of Double)(a, -10.0, 10.0)))
        Next

        Dim positive As Double() = {0.0, 1.0, 4.0, 9.0, 2.25, 1.0E6}
        Check("f64 sqrt", SequenceEqual(Ref1(positive, Function(x) std.Sqrt(x)), SimdMath.Sqrt(positive)))

        Dim nonzero As Double() = {1.0, 2.0, 4.0, 0.5, -8.0}
        Check("f64 reciprocal", SequenceEqual(Ref1(nonzero, Function(x) 1.0 / x), SimdMath.Reciprocal(nonzero)))

        Dim power As Double() = {2.0, 3.0, -4.0, 0.5, 10.0}
        Check("f64 pow(exponent=2) matches ^", SequenceEqual(Ref1(power, Function(x) x ^ 2), SimdMath.PowScalar(power, 2.0)))
        Check("f64 pow(exponent=0.5) matches ^", SequenceEqual(Ref1(power, Function(x) x ^ 0.5), SimdMath.PowScalar(power, 0.5)))
        Check("f64 pow(exponent=3) within ulp", NearlyEqual(Ref1(power, Function(x) x ^ 3), SimdMath.PowScalar(power, 3.0), 1.0E-14))
        Check("f64 pow(exponent=4) within ulp", NearlyEqual(Ref1(power, Function(x) x ^ 4), SimdMath.PowScalar(power, 4.0), 1.0E-14))
        Check("f64 pow(exponent=5) matches ^", NearlyEqual(Ref1(power, Function(x) x ^ 5), SimdMath.PowScalar(power, 5.0)))
        Check("f64 exp matches std", SequenceEqual(Ref1(power, Function(x) std.Exp(x)), SimdMath.Exp(power)))
        Check("f64 log matches std", SequenceEqual(Ref1(power, Function(x) std.Log(x)), SimdMath.Log(power)))
        Check("f64 exp empty", SimdMath.Exp(Array.Empty(Of Double)()).Length = 0)
    End Sub

    Private Sub VerifyReduce()
        Console.WriteLine("[4] reduction")

        For Each n As Integer In {0, 1, 2, 3, 5, 16, 17, 1000, 100000}
            Dim a As Double() = RandomData(n)
            Dim tag As String = $"n={n}"

            If n = 0 Then
                Check($"sum empty       {tag}", SimdReduce.Sum(a) = 0.0)
                Check($"dotsq empty     {tag}", SimdReduce.SumSquares(a) = 0.0)
                Check($"l1 empty        {tag}", SimdReduce.L1Norm(a) = 0.0)
                Check($"l2 empty        {tag}", SimdReduce.L2Norm(a) = 0.0)
                Continue For
            End If

            Check($"sum             {tag}", NearlyEqual({SimdReduce.Sum(a)}, {LoopSum(a)}))
            Check($"sumsquares      {tag}", NearlyEqual({SimdReduce.SumSquares(a)}, {LoopSumSquares(a)}))
            Check($"dot self        {tag}", NearlyEqual({SimdReduce.Dot(a, a)}, {LoopSumSquares(a)}))
            Check($"dot fma         {tag}", NearlyEqual({SIMDIntrinsics.DotFma(a, a)}, {LoopSumSquares(a)}))
            Check($"sumsquares fma  {tag}", NearlyEqual({SIMDIntrinsics.SumSquaresFma(a)}, {LoopSumSquares(a)}))
            Check($"mean            {tag}", NearlyEqual({SimdReduce.Mean(a)}, {LoopSum(a) / n}))
            Check($"min             {tag}", NearlyEqual({SimdReduce.Min(a)}, {LoopMin(a)}))
            Check($"max             {tag}", NearlyEqual({SimdReduce.Max(a)}, {LoopMax(a)}))
            Check($"l1norm          {tag}", NearlyEqual({SimdReduce.L1Norm(a)}, {LoopL1(a)}))
            Check($"l2norm          {tag}", NearlyEqual({SimdReduce.L2Norm(a)}, {std.Sqrt(LoopSumSquares(a))}))
            Check($"argmax          {tag}", SimdReduce.ArgMax(a) = LoopArgMax(a))
            Check($"argmin          {tag}", SimdReduce.ArgMin(a) = LoopArgMin(a))
            Check($"axpy            {tag}", NearlyEqual(SIMDIntrinsics.Axpy(2.5, a, a), Ref1(a, Function(x) 2.5 * x + x)))
            Check($"multiply-add    {tag}", NearlyEqual(SIMDIntrinsics.MultiplyAdd(a, a, a), Ref1(a, Function(x) x * x + x)))
            Check($"svd sum via ext {tag}", NearlyEqual({a.SimdSum()}, {LoopSum(a)}))
            Check($"svd dot via ext {tag}", NearlyEqual({a.SimdDot(a)}, {LoopSumSquares(a)}))
            Check($"svd l2 via ext  {tag}", NearlyEqual({a.SimdL2Norm()}, {std.Sqrt(LoopSumSquares(a))}))
        Next

        ' 长度不一致必须抛出
        Dim mismatch As Boolean = False

        Try
            Call SimdReduce.Dot({1.0, 2.0}, {1.0})
        Catch ex As ArgumentException
            mismatch = True
        End Try

        Check("dot length mismatch throws", mismatch)

        Dim emptyThrows As Boolean = False

        Try
            Call SimdReduce.Min(Array.Empty(Of Double)())
        Catch ex As ArgumentException
            emptyThrows = True
        End Try

        Check("min on empty throws", emptyThrows)
    End Sub

    Private Sub VerifyCompare()
        Console.WriteLine("[5] compare and mask")

        Dim a As Double() = {1.0, 5.0, -3.0, 8.0, 0.0, 2.0, 9.0}
        Dim b As Double() = {2.0, 5.0, 3.0, 1.0, 0.0, -2.0, 9.0}

        ' 掩码通道定位必须是“按元素宽度”而不是“按固定整数宽度”，
        ' 否则 Single/Integer(4 字节) 与 Short(2 字节) 会出现通道错位。
        ' 这里对所有支持的类型 × 所有边界长度逐一校验。
        For Each n As Integer In TestSizes()
            Dim da As Double() = RandomData(n)
            Dim db As Double() = RandomData(n)
            Dim ia As Integer() = RandomIntegers(n)
            Dim ib As Integer() = RandomIntegers(n)
            Dim la As Long() = Array.ConvertAll(ia, Function(x) CLng(x))
            Dim lb As Long() = Array.ConvertAll(ib, Function(x) CLng(x))
            Dim sa As Short() = Array.ConvertAll(ia, Function(x) CShort(x))
            Dim sb As Short() = Array.ConvertAll(ib, Function(x) CShort(x))
            Dim fa As Single() = Array.ConvertAll(da, Function(x) CSng(x))
            Dim fb As Single() = Array.ConvertAll(db, Function(x) CSng(x))
            Dim tag As String = $"n={n}"

            Check($"f64 gt cross-size  {tag}", SequenceEqual(RefCmp(da, db, Function(x, y) x > y), SimdCompare.GreaterThan(Of Double)(da, db)))
            Check($"f64 eq cross-size  {tag}", SequenceEqual(RefCmp(da, db, Function(x, y) x = y), SimdCompare.Equal(Of Double)(da, db)))
            Check($"f32 gt cross-size  {tag}", SequenceEqual(RefCmp(fa, fb, Function(x, y) x > y), SimdCompare.GreaterThan(Of Single)(fa, fb)))
            Check($"f32 ne cross-size  {tag}", SequenceEqual(RefCmp(fa, fb, Function(x, y) x <> y), SimdCompare.NotEqual(Of Single)(fa, fb)))
            Check($"i32 lt cross-size  {tag}", SequenceEqual(RefCmp(ia, ib, Function(x, y) x < y), SimdCompare.LessThan(Of Integer)(ia, ib)))
            Check($"i32 ge cross-size  {tag}", SequenceEqual(RefCmp(ia, ib, Function(x, y) x >= y), SimdCompare.GreaterThanOrEqual(Of Integer)(ia, ib)))
            Check($"i64 le cross-size  {tag}", SequenceEqual(RefCmp(la, lb, Function(x, y) x <= y), SimdCompare.LessThanOrEqual(Of Long)(la, lb)))
            Check($"i16 eq cross-size  {tag}", SequenceEqual(RefCmp(sa, sb, Function(x, y) x = y), SimdCompare.Equal(Of Short)(sa, sb)))
        Next

        Check("greater than", SequenceEqual(RefCmp(a, b, Function(x, y) x > y), SimdCompare.GreaterThan(Of Double)(a, b)))
        Check("less than", SequenceEqual(RefCmp(a, b, Function(x, y) x < y), SimdCompare.LessThan(Of Double)(a, b)))
        Check("greater or equal", SequenceEqual(RefCmp(a, b, Function(x, y) x >= y), SimdCompare.GreaterThanOrEqual(Of Double)(a, b)))
        Check("less or equal", SequenceEqual(RefCmp(a, b, Function(x, y) x <= y), SimdCompare.LessThanOrEqual(Of Double)(a, b)))
        Check("equal", SequenceEqual(RefCmp(a, b, Function(x, y) x = y), SimdCompare.Equal(Of Double)(a, b)))
        Check("not equal", SequenceEqual(RefCmp(a, b, Function(x, y) x <> y), SimdCompare.NotEqual(Of Double)(a, b)))
        Check("i32 greater than", SequenceEqual(RefCmp({1, 5, -3, 8}, {2, 5, 3, 1}, Function(x, y) x > y),
                                                SimdCompare.GreaterThan(Of Integer)({1, 5, -3, 8}, {2, 5, 3, 1})))

        Dim mask As Boolean() = SimdCompare.GreaterThan(Of Double)(a, b)
        Dim expectSelected As Double() = Ref(a, b, Function(x, y) If(x > y, x, y))

        Check("conditional select", SequenceEqual(expectSelected, SimdCompare.Select(mask, a, b)))
        Check("select equals max here", SequenceEqual(expectSelected, SimdEngine.Max(Of Double)(a, b)))
        Check("mask any", SimdCompare.Any(mask))
        Check("mask all", Not SimdCompare.All(mask))
        Check("mask count", SimdCompare.CountTrue(mask) = 2)
        Check("where compacts", SequenceEqual({8.0, 2.0}, SimdCompare.Where(mask, a)))
        Check("empty compare", SimdCompare.GreaterThan(Of Double)(Array.Empty(Of Double)(), Array.Empty(Of Double)()).Length = 0)
    End Sub

    Private Sub VerifyParallel()
        Console.WriteLine("[6] parallel + vector hybrid")

        Dim n As Integer = SimdParallel.MinParallelLength * 3
        Dim a As Double() = RandomData(n)
        Dim b As Double() = RandomData(n)

        Check("parallel sum == single thread sum", NearlyEqual({SimdParallel.Sum(a)}, {SimdReduce.Sum(a)}, 1.0E-9))
        Check("parallel dot == fma dot", NearlyEqual({SimdParallel.Dot(a, b)}, {SIMDIntrinsics.DotFma(a, b)}, 1.0E-9))
        Check("parallel sumsquares == single", NearlyEqual({SimdParallel.SumSquares(a)}, {SimdReduce.SumSquares(a)}, 1.0E-9))
        Check("parallel min == single", SimdParallel.Min(a) = SimdReduce.Min(a))
        Check("parallel max == single", SimdParallel.Max(a) = SimdReduce.Max(a))
        Check("parallel add == engine add", SequenceEqual(SimdEngine.Add(Of Double)(a, b), SimdParallel.Add(a, b)))
        Check("parallel subtract == engine", SequenceEqual(SimdEngine.Subtract(Of Double)(a, b), SimdParallel.Subtract(a, b)))
        Check("parallel multiply == engine", SequenceEqual(SimdEngine.Multiply(Of Double)(a, b), SimdParallel.Multiply(a, b)))
        Check("parallel mul scalar == engine", SequenceEqual(SimdEngine.MultiplyScalar(Of Double)(2.5, a), SimdParallel.MultiplyScalar(2.5, a)))
        Check("parallel l1 == single", NearlyEqual({SimdParallel.L1Norm(a)}, {SimdReduce.L1Norm(a)}, 1.0E-9))

        ' 矩阵乘法：结果必须与朴素三重循环一致
        Dim ma As Double()() = {RandomData(17), RandomData(17), RandomData(17)}
        Dim mb As Double()() = New Double(16)() {}

        For k As Integer = 0 To 16
            mb(k) = RandomData(5)
        Next

        Dim viaParallel As Double()() = SimdParallel.MatrixDot(ma, mb)
        Dim viaNaive As Double()() = NaiveMatrixDot(ma, mb)
        Dim matrixOk As Boolean = True

        For i As Integer = 0 To ma.Length - 1
            If Not NearlyEqual(viaParallel(i), viaNaive(i), 1.0E-9) Then
                matrixOk = False
            End If
        Next

        Check("matrix dot == naive", matrixOk)
    End Sub

    Private Function NaiveMatrixDot(a As Double()(), b As Double()()) As Double()()
        Dim nrowA As Integer = a.Length
        Dim n As Integer = b.Length
        Dim ncolB As Integer = b(0).Length
        Dim c As Double()() = New Double(nrowA - 1)() {}

        For i As Integer = 0 To nrowA - 1
            c(i) = New Double(ncolB - 1) {}
        Next
        For i As Integer = 0 To nrowA - 1
            For j As Integer = 0 To ncolB - 1
                Dim s As Double = 0

                For k As Integer = 0 To n - 1
                    s += a(i)(k) * b(k)(j)
                Next

                c(i)(j) = s
            Next
        Next

        Return c
    End Function

#End Region

#Region "scalar reference loops"

    Private Function LoopSum(v As Double()) As Double
        Dim s As Double = 0

        For i As Integer = 0 To v.Length - 1
            s += v(i)
        Next

        Return s
    End Function

    Private Function LoopSumSquares(v As Double()) As Double
        Dim s As Double = 0

        For i As Integer = 0 To v.Length - 1
            s += v(i) * v(i)
        Next

        Return s
    End Function

    Private Function LoopL1(v As Double()) As Double
        Dim s As Double = 0

        For i As Integer = 0 To v.Length - 1
            s += std.Abs(v(i))
        Next

        Return s
    End Function

    Private Function LoopMin(v As Double()) As Double
        Dim m As Double = v(0)

        For i As Integer = 1 To v.Length - 1
            m = std.Min(m, v(i))
        Next

        Return m
    End Function

    Private Function LoopMax(v As Double()) As Double
        Dim m As Double = v(0)

        For i As Integer = 1 To v.Length - 1
            m = std.Max(m, v(i))
        Next

        Return m
    End Function

    Private Function LoopArgMax(v As Double()) As Integer
        Return Array.IndexOf(v, LoopMax(v))
    End Function

    Private Function LoopArgMin(v As Double()) As Integer
        Return Array.IndexOf(v, LoopMin(v))
    End Function

#End Region

#Region "benchmark"

    Private Sub Benchmark()
        Console.WriteLine("=========================================================")
        Console.WriteLine(" benchmark (best of 3 runs)")
        Console.WriteLine("=========================================================")
        Console.WriteLine(" 标量基线为等价的朴素 for 循环；out-of-place 的标量基线同样使用")
        Console.WriteLine(" GC.AllocateUninitializedArray，保证与向量版本口径一致。")
        Console.WriteLine(" 就地(in-place)组的耗时不含结果数组分配，反映的是纯计算/内存带宽上限。")

        For Each n As Integer In {1000, 100000, 10000000}
            Dim a As Double() = RandomData(n)
            Dim b As Double() = RandomData(n)

            Console.WriteLine()
            Console.WriteLine($"--- n = {n:#,##0} ---")
            Call BenchAdd(a, b)
            Call BenchAddInPlace(a, b)
            Call BenchMultiplyScalar(a, 2.5)
            Call BenchSum(a)
            Call BenchDot(a, b)
        Next

        Console.WriteLine()
        Console.WriteLine("note: parallel only kicks in when n >= " & (SimdParallel.MinParallelLength * 2).ToString("#,##0"))
    End Sub

    Private Sub BenchAdd(a As Double(), b As Double())
        Dim ts As Double = TimeBest(3, Sub() Call ScalarAdd(a, b))
        Dim tv As Double = TimeBest(3, Sub() Call SimdEngine.Add(Of Double)(a, b))
        Dim tp As Double = TimeBest(3, Sub() Call SimdParallel.Add(a, b))

        WriteLine("add            ", ts, tv, tp)
    End Sub

    Private Sub BenchAddInPlace(a As Double(), b As Double())
        Dim ts As Double = TimeBest(3, Sub() Call ScalarAddInPlace(a, b))
        Dim tv As Double = TimeBest(3, Sub() Call SimdEngine.AddInPlace(Of Double)(a, b))

        WriteLine("add (in-place) ", ts, tv, Double.NaN, inPlace:=True)
    End Sub

    Private Sub BenchMultiplyScalar(a As Double(), scalar As Double)
        Dim ts As Double = TimeBest(3, Sub() Call ScalarMultiplyScalar(a, scalar))
        Dim tv As Double = TimeBest(3, Sub() Call SimdEngine.MultiplyScalar(Of Double)(scalar, a))
        Dim tp As Double = TimeBest(3, Sub() Call SimdParallel.MultiplyScalar(scalar, a))

        WriteLine("multiplyScalar ", ts, tv, tp)
    End Sub

    Private Sub BenchSum(a As Double())
        Dim ts As Double = TimeBest(3, Sub() Call LoopSum(a))
        Dim tv As Double = TimeBest(3, Sub() Call SimdReduce.Sum(a))
        Dim tp As Double = TimeBest(3, Sub() Call SimdParallel.Sum(a))

        WriteLine("sum            ", ts, tv, tp)
    End Sub

    Private Sub BenchDot(a As Double(), b As Double())
        Dim ts As Double = TimeBest(3, Sub() Call LoopDot(a, b))
        Dim tv As Double = TimeBest(3, Sub() Call SIMDIntrinsics.DotFma(a, b))
        Dim tp As Double = TimeBest(3, Sub() Call SimdParallel.Dot(a, b))

        WriteLine("dot            ", ts, tv, tp)
    End Sub

#Region "scalar baselines for the benchmark"

    Private Function ScalarAdd(a As Double(), b As Double()) As Double()
        Dim out As Double() = GC.AllocateUninitializedArray(Of Double)(a.Length)

        For i As Integer = 0 To a.Length - 1
            out(i) = a(i) + b(i)
        Next

        Return out
    End Function

    Private Function ScalarMultiplyScalar(a As Double(), scalar As Double) As Double()
        Dim out As Double() = GC.AllocateUninitializedArray(Of Double)(a.Length)

        For i As Integer = 0 To a.Length - 1
            out(i) = a(i) * scalar
        Next

        Return out
    End Function

    Private Sub ScalarAddInPlace(a As Double(), b As Double())
        For i As Integer = 0 To a.Length - 1
            a(i) += b(i)
        Next
    End Sub

#End Region

    Private Function LoopDot(a As Double(), b As Double()) As Double
        Dim s As Double = 0

        For i As Integer = 0 To a.Length - 1
            s += a(i) * b(i)
        Next

        Return s
    End Function

    Private Function TimeBest(runs As Integer, action As Action) As Double
        Dim best As Double = Double.MaxValue

        For i As Integer = 1 To runs
            Dim sw As Stopwatch = Stopwatch.StartNew

            Call action()
            Call sw.Stop()

            best = std.Min(best, sw.Elapsed.TotalMilliseconds)
        Next

        Return best
    End Function

    Private Sub WriteLine(name As String, scalar As Double, vector As Double, parallel As Double,
                          Optional inPlace As Boolean = False)
        If inPlace Then
            Console.WriteLine($"  {name} scalar={scalar,10:N2}ms  simd={vector,10:N2}ms  " &
                              $"speedup(simd)={scalar / std.Max(vector, 1.0E-06),6:N2}x")
        Else
            Console.WriteLine($"  {name} scalar={scalar,10:N2}ms  simd={vector,10:N2}ms  parallel={parallel,10:N2}ms  " &
                              $"speedup(simd)={scalar / std.Max(vector, 1.0E-06),6:N2}x  speedup(parallel)={scalar / std.Max(parallel, 1.0E-06),6:N2}x")
        End If
    End Sub

#End Region
End Module
