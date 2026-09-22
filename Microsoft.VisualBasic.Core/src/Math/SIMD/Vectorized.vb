Imports System.Linq

Namespace Math.SIMD.Vectorization

    ''' <summary>
    ''' 向量化运算的统一词汇表: 脚本向量化改写器所发射的**全部**调用都指向本模块。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' <b>为什么需要独立的一套词汇</b>: <see cref="SimdExtensions"/> 已被大量既有代码以
    ''' <c>Imports Microsoft.VisualBasic.Math.SIMD</c> 的方式直接引用, 其成员名
    ''' (<c>SimdAdd</c> / <c>SimdAbs</c> / <c>SimdSum</c> …) 都被占用。
    ''' 而 VB 对「两个已导入模块之中的同名成员」会直接报 <c>BC30562</c>(名称不明确) ——
    ''' 即使签名不同、即使调用处写了显式泛型实参也无法化解。
    ''' 因此本模块统一采用 <c>Vec*</c> 前缀, 与 <see cref="SimdExtensions"/> 完全不存在重名。
    ''' </para>
    ''' <para>
    ''' <b>为什么放在 <c>Math.SIMD.Vectorization</c> 子命名空间</b>:
    ''' 生成代码只需要 <c>Imports Microsoft.VisualBasic.Math.SIMD.Vectorization</c> 一条导入语句,
    ''' 这样既能看到本模块的 <c>Vec*</c> 成员, 又**不会**把 <c>Microsoft.VisualBasic.Math.SIMD</c>
    ''' 之中那些泛化命名的历史门面类(<c>Add</c> / <c>Subtract</c> / <c>Multiply</c> / <c>Divide</c> /
    ''' <c>Modulo</c> / <c>Exponent</c>)带进脚本作用域, 从而不会与脚本自身定义的类型产生歧义。
    ''' </para>
    ''' <para>
    ''' <b>实现策略</b>:
    ''' <list type="bullet">
    ''' <item>有真 SIMD 内核可用的一律转调 <see cref="SimdEngine"/> / <see cref="SimdMath"/> /
    ''' <see cref="SimdReduce"/>(基于 <see cref="System.Numerics.Vector(Of T)"/>);</item>
    ''' <item>框架/硬件没有对应内核的运算(整除 <c>\</c>、取余 <c>Mod</c>、乘积归约、逐元素映射)
    ''' 退化为标量循环, 与运行时既有 <c>Modulo</c> / <c>Exponent</c> 门面处于同一实现水平;</item>
    ''' <item>归约刻意走 <see cref="SimdReduce"/> 的**顺序**内核而不是 <c>SimdParallel</c> 的分块并行,
    ''' 以保证脚本的浮点结果可复现(并行归约的加法次序不确定)。</item>
    ''' </list>
    ''' </para>
    ''' <para>
    ''' <b>长度契约</b>: 向量⊕向量运算假设两侧长度一致, 与 <see cref="SimdEngine"/> 保持一致,
    ''' 不额外做长度校验(长度不足时由运行时自身的边界检查抛出)。
    ''' </para>
    ''' <para>
    ''' <b>词汇完整性</b>: 本模块按「数值类型 × 运算形态」把词汇一次性补全, 因此其中
    ''' <c>VecSquare</c> 与 <c>VecReciprocal</c> 目前没有对应的改写来源
    ''' (脚本里的 <c>v * v</c> / <c>1 / v</c> 分别被发射为 <c>VecMultiply</c> / <c>VecScalarDivide</c>,
    ''' 以保持 VB 的逐元素类型语义), 它们作为词汇表的一部分保留, 供脚本直接调用。
    ''' </para>
    ''' </remarks>
    Public Module Vectorized

#Region "helpers"

        ''' <summary>逐元素二元运算的标量循环实现(VB 不支持泛型运算符约束, 因此运算由调用方注入)</summary>
        Private Function ZipMap(Of T)(v1 As T(), v2 As T(), f As Func(Of T, T, T)) As T()
            If v1 Is Nothing Then
                Throw New ArgumentNullException(NameOf(v1), "the input vector can not be NULL!")
            End If
            If v2 Is Nothing Then
                Throw New ArgumentNullException(NameOf(v2), "the input vector can not be NULL!")
            End If

            Dim len As Integer = v1.Length
            If len = 0 Then Return Array.Empty(Of T)()

            Dim out As T() = New T(len - 1) {}
            For i As Integer = 0 To len - 1
                out(i) = f(v1(i), v2(i))
            Next

            Return out
        End Function

        ''' <summary>向量 ⊕ 标量 的标量循环实现</summary>
        Private Function ZipScalarMap(Of T)(v As T(), scalar As T, f As Func(Of T, T, T)) As T()
            If v Is Nothing Then
                Throw New ArgumentNullException(NameOf(v), "the input vector can not be NULL!")
            End If

            Dim len As Integer = v.Length
            If len = 0 Then Return Array.Empty(Of T)()

            Dim out As T() = New T(len - 1) {}
            For i As Integer = 0 To len - 1
                out(i) = f(v(i), scalar)
            Next

            Return out
        End Function

        ''' <summary>标量 ⊕ 向量 的标量循环实现(标量位于左操作数一侧)</summary>
        Private Function ScalarZipMap(Of T)(scalar As T, v As T(), f As Func(Of T, T, T)) As T()
            If v Is Nothing Then
                Throw New ArgumentNullException(NameOf(v), "the input vector can not be NULL!")
            End If

            Dim len As Integer = v.Length
            If len = 0 Then Return Array.Empty(Of T)()

            Dim out As T() = New T(len - 1) {}
            For i As Integer = 0 To len - 1
                out(i) = f(scalar, v(i))
            Next

            Return out
        End Function

#End Region

#Region "二元运算: 向量 op 向量"

        ''' <summary>逐元素相加: <c>out(i) = v1(i) + v2(i)</c></summary>
        Public Function VecAdd(Of T As Structure)(v1 As T(), v2 As T()) As T()
            Return SimdEngine.Add(Of T)(v1, v2)
        End Function

        ''' <summary>逐元素相减: <c>out(i) = v1(i) - v2(i)</c></summary>
        Public Function VecSubtract(Of T As Structure)(v1 As T(), v2 As T()) As T()
            Return SimdEngine.Subtract(Of T)(v1, v2)
        End Function

        ''' <summary>逐元素相乘: <c>out(i) = v1(i) * v2(i)</c></summary>
        Public Function VecMultiply(Of T As Structure)(v1 As T(), v2 As T()) As T()
            Return SimdEngine.Multiply(Of T)(v1, v2)
        End Function

        ''' <summary>
        ''' 逐元素真除法(VB 的 <c>/</c>): <c>out(i) = v1(i) / v2(i)</c>
        ''' </summary>
        ''' <remarks>
        ''' 只提供 <see cref="Double"/>/<see cref="Single"/> 形态 —— VB 的 <c>/</c>
        ''' 对整型操作数会先把结果提升为 <see cref="Double"/>, 改写器在调用之前已经完成提升。
        ''' </remarks>
        Public Function VecDivide(v1 As Double(), v2 As Double()) As Double()
            Return SimdEngine.Divide(v1, v2)
        End Function

        ''' <summary>逐元素真除法(VB 的 <c>/</c>, <see cref="Single"/>)</summary>
        Public Function VecDivide(v1 As Single(), v2 As Single()) As Single()
            Return SimdEngine.Divide(v1, v2)
        End Function

        ''' <summary>逐元素整除(VB 的 <c>\</c>, <see cref="Integer"/>)</summary>
        Public Function VecIntegerDivide(v1 As Integer(), v2 As Integer()) As Integer()
            Return ZipMap(Of Integer)(v1, v2, Function(a, b) a \ b)
        End Function

        ''' <summary>逐元素整除(VB 的 <c>\</c>, <see cref="Long"/>)</summary>
        Public Function VecIntegerDivide(v1 As Long(), v2 As Long()) As Long()
            Return ZipMap(Of Long)(v1, v2, Function(a, b) a \ b)
        End Function

        ''' <summary>逐元素取余(VB 的 <c>Mod</c>, <see cref="Integer"/>)</summary>
        Public Function VecModulo(v1 As Integer(), v2 As Integer()) As Integer()
            Return ZipMap(Of Integer)(v1, v2, Function(a, b) a Mod b)
        End Function

        ''' <summary>逐元素取余(VB 的 <c>Mod</c>, <see cref="Long"/>)</summary>
        Public Function VecModulo(v1 As Long(), v2 As Long()) As Long()
            Return ZipMap(Of Long)(v1, v2, Function(a, b) a Mod b)
        End Function

        ''' <summary>逐元素取余(VB 的 <c>Mod</c>, <see cref="Single"/>)</summary>
        Public Function VecModulo(v1 As Single(), v2 As Single()) As Single()
            Return ZipMap(Of Single)(v1, v2, Function(a, b) a Mod b)
        End Function

        ''' <summary>逐元素取余(VB 的 <c>Mod</c>, <see cref="Double"/>)</summary>
        Public Function VecModulo(v1 As Double(), v2 As Double()) As Double()
            Return ZipMap(Of Double)(v1, v2, Function(a, b) a Mod b)
        End Function

        ''' <summary>
        ''' 逐元素幂(VB 的 <c>^</c>): <c>out(i) = v1(i) ^ v2(i)</c>
        ''' </summary>
        ''' <remarks>VB 的 <c>^</c> 无论操作数为何种数值类型, 结果恒为 <see cref="Double"/>。</remarks>
        Public Function VecPower(v1 As Double(), v2 As Double()) As Double()
            Return SimdMath.Pow(v1, v2)
        End Function

#End Region

#Region "二元运算: 向量 op 标量"

        ''' <summary>向量加标量: <c>out(i) = v(i) + scalar</c></summary>
        Public Function VecAddScalar(Of T As Structure)(v As T(), scalar As T) As T()
            Return SimdEngine.AddScalar(Of T)(v, scalar)
        End Function

        ''' <summary>向量减标量: <c>out(i) = v(i) - scalar</c></summary>
        Public Function VecSubtractScalar(Of T As Structure)(v As T(), scalar As T) As T()
            Return SimdEngine.SubtractScalar(Of T)(v, scalar)
        End Function

        ''' <summary>标量减向量: <c>out(i) = scalar - v(i)</c></summary>
        Public Function VecScalarSubtract(Of T As Structure)(scalar As T, v As T()) As T()
            Return SimdEngine.ScalarSubtract(Of T)(scalar, v)
        End Function

        ''' <summary>向量乘标量: <c>out(i) = v(i) * scalar</c></summary>
        Public Function VecMultiplyScalar(Of T As Structure)(v As T(), scalar As T) As T()
            Return SimdEngine.MultiplyScalar(Of T)(scalar, v)
        End Function

        ''' <summary>向量除以标量: <c>out(i) = v(i) / scalar</c></summary>
        Public Function VecDivideScalar(v As Double(), scalar As Double) As Double()
            Return SimdEngine.DivideScalar(v, scalar)
        End Function

        ''' <summary>向量除以标量(<see cref="Single"/>)</summary>
        Public Function VecDivideScalar(v As Single(), scalar As Single) As Single()
            Return SimdEngine.DivideScalar(v, scalar)
        End Function

        ''' <summary>标量除以向量: <c>out(i) = scalar / v(i)</c></summary>
        Public Function VecScalarDivide(scalar As Double, v As Double()) As Double()
            Return SimdEngine.ScalarDivide(scalar, v)
        End Function

        ''' <summary>标量除以向量(<see cref="Single"/>)</summary>
        Public Function VecScalarDivide(scalar As Single, v As Single()) As Single()
            Return SimdEngine.ScalarDivide(scalar, v)
        End Function

        ''' <summary>向量整除标量(VB 的 <c>\</c>, <see cref="Integer"/>)</summary>
        Public Function VecIntegerDivideScalar(v As Integer(), scalar As Integer) As Integer()
            Return ZipScalarMap(Of Integer)(v, scalar, Function(a, b) a \ b)
        End Function

        ''' <summary>向量整除标量(VB 的 <c>\</c>, <see cref="Long"/>)</summary>
        Public Function VecIntegerDivideScalar(v As Long(), scalar As Long) As Long()
            Return ZipScalarMap(Of Long)(v, scalar, Function(a, b) a \ b)
        End Function

        ''' <summary>标量整除向量(VB 的 <c>\</c>, <see cref="Integer"/>)</summary>
        Public Function VecScalarIntegerDivide(scalar As Integer, v As Integer()) As Integer()
            Return ScalarZipMap(Of Integer)(scalar, v, Function(a, b) a \ b)
        End Function

        ''' <summary>标量整除向量(VB 的 <c>\</c>, <see cref="Long"/>)</summary>
        Public Function VecScalarIntegerDivide(scalar As Long, v As Long()) As Long()
            Return ScalarZipMap(Of Long)(scalar, v, Function(a, b) a \ b)
        End Function

        ''' <summary>向量对标量取余(VB 的 <c>Mod</c>, <see cref="Integer"/>)</summary>
        Public Function VecModuloScalar(v As Integer(), scalar As Integer) As Integer()
            Return ZipScalarMap(Of Integer)(v, scalar, Function(a, b) a Mod b)
        End Function

        ''' <summary>向量对标量取余(VB 的 <c>Mod</c>, <see cref="Long"/>)</summary>
        Public Function VecModuloScalar(v As Long(), scalar As Long) As Long()
            Return ZipScalarMap(Of Long)(v, scalar, Function(a, b) a Mod b)
        End Function

        ''' <summary>向量对标量取余(VB 的 <c>Mod</c>, <see cref="Single"/>)</summary>
        Public Function VecModuloScalar(v As Single(), scalar As Single) As Single()
            Return ZipScalarMap(Of Single)(v, scalar, Function(a, b) a Mod b)
        End Function

        ''' <summary>向量对标量取余(VB 的 <c>Mod</c>, <see cref="Double"/>)</summary>
        Public Function VecModuloScalar(v As Double(), scalar As Double) As Double()
            Return ZipScalarMap(Of Double)(v, scalar, Function(a, b) a Mod b)
        End Function

        ''' <summary>标量对向量取余(VB 的 <c>Mod</c>, <see cref="Integer"/>)</summary>
        Public Function VecScalarModulo(scalar As Integer, v As Integer()) As Integer()
            Return ScalarZipMap(Of Integer)(scalar, v, Function(a, b) a Mod b)
        End Function

        ''' <summary>标量对向量取余(VB 的 <c>Mod</c>, <see cref="Long"/>)</summary>
        Public Function VecScalarModulo(scalar As Long, v As Long()) As Long()
            Return ScalarZipMap(Of Long)(scalar, v, Function(a, b) a Mod b)
        End Function

        ''' <summary>标量对向量取余(VB 的 <c>Mod</c>, <see cref="Single"/>)</summary>
        Public Function VecScalarModulo(scalar As Single, v As Single()) As Single()
            Return ScalarZipMap(Of Single)(scalar, v, Function(a, b) a Mod b)
        End Function

        ''' <summary>标量对向量取余(VB 的 <c>Mod</c>, <see cref="Double"/>)</summary>
        Public Function VecScalarModulo(scalar As Double, v As Double()) As Double()
            Return ScalarZipMap(Of Double)(scalar, v, Function(a, b) a Mod b)
        End Function

        ''' <summary>向量的标量次幂(VB 的 <c>v ^ n</c>)</summary>
        Public Function VecPowerScalar(v As Double(), exponent As Double) As Double()
            Return SimdMath.PowScalar(v, exponent)
        End Function

        ''' <summary>标量的向量次幂(VB 的 <c>b ^ v</c>)</summary>
        Public Function VecScalarPower(baseValue As Double, v As Double()) As Double()
            Return ScalarZipMap(Of Double)(baseValue, v, Function(a, b) a ^ b)
        End Function

#End Region

#Region "一元运算"

        ''' <summary>逐元素取负: <c>out(i) = -v(i)</c></summary>
        Public Function VecNegate(Of T As Structure)(v As T()) As T()
            Return SimdMath.Negate(Of T)(v)
        End Function

        ''' <summary>逐元素绝对值: <c>out(i) = |v(i)|</c></summary>
        Public Function VecAbs(Of T As Structure)(v As T()) As T()
            Return SimdMath.Abs(Of T)(v)
        End Function

        ''' <summary>逐元素平方: <c>out(i) = v(i) * v(i)</c></summary>
        Public Function VecSquare(Of T As Structure)(v As T()) As T()
            Return SimdMath.Square(Of T)(v)
        End Function

        ''' <summary>逐元素平方根</summary>
        Public Function VecSqrt(v As Double()) As Double()
            Return SimdMath.Sqrt(v)
        End Function

        ''' <summary>逐元素平方根(<see cref="Single"/>)</summary>
        Public Function VecSqrt(v As Single()) As Single()
            Return SimdMath.Sqrt(v)
        End Function

        ''' <summary>逐元素自然指数</summary>
        Public Function VecExp(v As Double()) As Double()
            Return SimdMath.Exp(v)
        End Function

        ''' <summary>逐元素自然指数(<see cref="Single"/>)</summary>
        Public Function VecExp(v As Single()) As Single()
            Return SimdMath.Exp(v)
        End Function

        ''' <summary>逐元素自然对数</summary>
        Public Function VecLog(v As Double()) As Double()
            Return SimdMath.Log(v)
        End Function

        ''' <summary>逐元素自然对数(<see cref="Single"/>)</summary>
        Public Function VecLog(v As Single()) As Single()
            Return SimdMath.Log(v)
        End Function

        ''' <summary>逐元素任意底对数</summary>
        Public Function VecLog(v As Double(), baseValue As Double) As Double()
            Return SimdMath.Log(v, baseValue)
        End Function

        ''' <summary>逐元素符号函数</summary>
        Public Function VecSign(v As Double()) As Double()
            Return SimdMath.Sign(v)
        End Function

        ''' <summary>逐元素符号函数(<see cref="Single"/>)</summary>
        Public Function VecSign(v As Single()) As Single()
            Return SimdMath.Sign(v)
        End Function

        ''' <summary>逐元素向下取整</summary>
        Public Function VecFloor(v As Double()) As Double()
            Return SimdMath.Floor(v)
        End Function

        ''' <summary>逐元素向下取整(<see cref="Single"/>)</summary>
        Public Function VecFloor(v As Single()) As Single()
            Return SimdMath.Floor(v)
        End Function

        ''' <summary>逐元素向上取整</summary>
        Public Function VecCeiling(v As Double()) As Double()
            Return SimdMath.Ceiling(v)
        End Function

        ''' <summary>逐元素向上取整(<see cref="Single"/>)</summary>
        Public Function VecCeiling(v As Single()) As Single()
            Return SimdMath.Ceiling(v)
        End Function

        ''' <summary>逐元素截断取整</summary>
        Public Function VecTruncate(v As Double()) As Double()
            Return SimdMath.Truncate(v)
        End Function

        ''' <summary>逐元素截断取整(<see cref="Single"/>)</summary>
        Public Function VecTruncate(v As Single()) As Single()
            Return SimdMath.Truncate(v)
        End Function

        ''' <summary>逐元素倒数: <c>out(i) = 1 / v(i)</c></summary>
        Public Function VecReciprocal(v As Double()) As Double()
            Return SimdMath.Reciprocal(v)
        End Function

        ''' <summary>逐元素倒数(<see cref="Single"/>)</summary>
        Public Function VecReciprocal(v As Single()) As Single()
            Return SimdMath.Reciprocal(v)
        End Function

#End Region

#Region "逐元素映射与类型转换"

        ''' <summary>
        ''' 任意一元函数的逐元素映射: <c>out(i) = f(v(i))</c>
        ''' </summary>
        ''' <remarks>
        ''' 「逐元素数学函数」的通用兜底入口: 运行时没有专用内核的函数(三角函数、四舍五入、
        ''' 以及脚本自定义的数值函数)都通过本方法向量化。实现为标量循环 ——
        ''' 委托调用本身无法被 SIMD 内核吸收。
        ''' </remarks>
        Public Function VecMap(Of TIn As Structure, TOut As Structure)(v As TIn(),
                                                                      f As Func(Of TIn, TOut)) As TOut()
            If v Is Nothing Then
                Throw New ArgumentNullException(NameOf(v), "the input vector can not be NULL!")
            End If

            Dim len As Integer = v.Length
            If len = 0 Then Return Array.Empty(Of TOut)()

            Dim out As TOut() = New TOut(len - 1) {}
            For i As Integer = 0 To len - 1
                out(i) = f(v(i))
            Next

            Return out
        End Function

        ''' <summary>
        ''' 逐元素元素类型转换。
        ''' </summary>
        ''' <remarks>
        ''' VB 对数组**不存在**逐元素转换(<c>Short()</c> 无法赋给 <c>Integer()</c> 形参),
        ''' 因此当向量化运算两侧的元素类型不一致、或结果类型需要提升时,
        ''' 改写器必须显式插入一次转换, 本方法即该用途。实现为标量循环, 每次只多一次遍历。
        ''' </remarks>
        Public Function VecConvert(Of TIn As Structure, TOut As Structure)(v As TIn()) As TOut()
            Dim target As Type = GetType(TOut)

            Return VecMap(Of TIn, TOut)(
                v,
                Function(x) DirectCast(System.Convert.ChangeType(x, target), TOut))
        End Function

#End Region

#Region "聚合归约"

        ''' <summary>向量求和 <c>SUM(v)</c>(<see cref="Double"/>)</summary>
        Public Function VecSum(v As Double()) As Double
            Return SimdReduce.Sum(v)
        End Function

        ''' <summary>向量求和 <c>SUM(v)</c>(<see cref="Single"/>)</summary>
        Public Function VecSum(v As Single()) As Single
            Return SimdReduce.Sum(v)
        End Function

        ''' <summary>向量求和 <c>SUM(v)</c>(<see cref="Integer"/>)</summary>
        Public Function VecSum(v As Integer()) As Integer
            Return v.Sum()
        End Function

        ''' <summary>向量求和 <c>SUM(v)</c>(<see cref="Long"/>)</summary>
        Public Function VecSum(v As Long()) As Long
            Return v.Sum()
        End Function

        ''' <summary>
        ''' 向量求和 <c>SUM(v)</c>(<see cref="Short"/>)
        ''' </summary>
        ''' <remarks>
        ''' LINQ 的数值归约把 <see cref="Byte"/>/<see cref="Short"/> 一律拓宽到 <see cref="Integer"/>,
        ''' 没有 <c>Short</c> 重载, 因此这里手写累加以保持「按元素类型累加」的语义。
        ''' </remarks>
        Public Function VecSum(v As Short()) As Short
            Dim total As Short = 0

            For Each x As Short In v
                total = CShort(total + x)
            Next

            Return total
        End Function

        ''' <summary>向量均值(<see cref="Double"/>)</summary>
        Public Function VecMean(v As Double()) As Double
            Return SimdReduce.Mean(v)
        End Function

        ''' <summary>向量均值(<see cref="Single"/>)</summary>
        Public Function VecMean(v As Single()) As Single
            Return SimdReduce.Mean(v)
        End Function

        ''' <summary>向量均值(<see cref="Integer"/>, 结果恒为 <see cref="Double"/>)</summary>
        Public Function VecMean(v As Integer()) As Double
            Return v.Average()
        End Function

        ''' <summary>向量均值(<see cref="Long"/>, 结果恒为 <see cref="Double"/>)</summary>
        Public Function VecMean(v As Long()) As Double
            Return v.Average()
        End Function

        ''' <summary>向量最小值</summary>
        Public Function VecMin(v As Double()) As Double
            Return SimdReduce.Min(v)
        End Function

        ''' <summary>向量最小值(<see cref="Single"/>)</summary>
        Public Function VecMin(v As Single()) As Single
            Return v.Min()
        End Function

        ''' <summary>向量最小值(<see cref="Integer"/>)</summary>
        Public Function VecMin(v As Integer()) As Integer
            Return v.Min()
        End Function

        ''' <summary>向量最小值(<see cref="Long"/>)</summary>
        Public Function VecMin(v As Long()) As Long
            Return v.Min()
        End Function

        ''' <summary>向量最小值(<see cref="Short"/>)</summary>
        Public Function VecMin(v As Short()) As Short
            Return v.Min()
        End Function

        ''' <summary>向量最大值</summary>
        Public Function VecMax(v As Double()) As Double
            Return SimdReduce.Max(v)
        End Function

        ''' <summary>向量最大值(<see cref="Single"/>)</summary>
        Public Function VecMax(v As Single()) As Single
            Return SimdReduce.Max(v)
        End Function

        ''' <summary>向量最大值(<see cref="Integer"/>)</summary>
        Public Function VecMax(v As Integer()) As Integer
            Return v.Max()
        End Function

        ''' <summary>向量最大值(<see cref="Long"/>)</summary>
        Public Function VecMax(v As Long()) As Long
            Return v.Max()
        End Function

        ''' <summary>向量最大值(<see cref="Short"/>)</summary>
        Public Function VecMax(v As Short()) As Short
            Return v.Max()
        End Function

        ''' <summary>向量乘积 <c>PRODUCT(v)</c>; 空向量返回乘法单位元 <c>1</c></summary>
        Public Function VecProduct(v As Double()) As Double
            Return v.Aggregate(1.0, Function(a, b) a * b)
        End Function

        ''' <summary>向量乘积 <c>PRODUCT(v)</c>(<see cref="Single"/>)</summary>
        Public Function VecProduct(v As Single()) As Single
            Return v.Aggregate(1.0F, Function(a, b) a * b)
        End Function

        ''' <summary>向量乘积 <c>PRODUCT(v)</c>(<see cref="Integer"/>)</summary>
        Public Function VecProduct(v As Integer()) As Integer
            Return v.Aggregate(1, Function(a, b) a * b)
        End Function

        ''' <summary>向量乘积 <c>PRODUCT(v)</c>(<see cref="Long"/>)</summary>
        Public Function VecProduct(v As Long()) As Long
            Return v.Aggregate(1L, Function(a, b) a * b)
        End Function

        ''' <summary>向量乘积 <c>PRODUCT(v)</c>(<see cref="Short"/>)</summary>
        Public Function VecProduct(v As Short()) As Short
            Return v.Aggregate(1S, Function(a, b) a * b)
        End Function

        ''' <summary>
        ''' 向量元素个数 <c>COUNT(v)</c>
        ''' </summary>
        ''' <remarks>数组的 <c>Length</c> 访问是 <c>O(1)</c>, 与 <c>Enumerable.Count</c> 的数组快速路径一致。</remarks>
        Public Function VecCount(Of T)(v As T()) As Integer
            If v Is Nothing Then
                Return 0
            Else
                Return v.Length
            End If
        End Function

#End Region
    End Module
End Namespace
