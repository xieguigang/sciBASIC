Option Strict On
Option Explicit On

Imports System

Namespace TSK

    ''' <summary>
    ''' 隶属函数的类型编码。
    ''' </summary>
    ''' <remarks>
    ''' 前三种（高斯、广义钟形、Sigmoid）在参数域上处处可微，可作为自由训练的前提参数；
    ''' 三角与梯形是分段线性函数，只在分段内部可微（节点处的导数按单侧极限给出，节点上取 0），
    ''' 更适合把专家知识原样录入模型后冻结，而不是作为自由训练的初值。
    ''' </remarks>
    Public Enum MembershipKind As Integer

        ''' <summary>高斯函数 <c>exp(-(x-c)^2/(2*sigma^2))</c>，参数 (center, width)。</summary>
        Gaussian = 0
        ''' <summary>广义钟形函数 <c>1/(1+|(x-c)/a|^(2b))</c>，参数 (a, b, c)。</summary>
        GeneralizedBell = 1
        ''' <summary>Sigmoid 函数 <c>1/(1+exp(-a(x-c)))</c>，参数 (a, c)。</summary>
        Sigmoid = 2
        ''' <summary>三角函数，参数 (a, b, c)，<c>a &lt;= b &lt;= c</c>。</summary>
        Triangular = 3
        ''' <summary>梯形函数，参数 (a, b, c, d)，<c>a &lt; b &lt;= c &lt; d</c>。</summary>
        Trapezoidal = 4

    End Enum

    ''' <summary>
    ''' 可微隶属函数的契约。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 本体系的实现都是**纯函数**：参数保存在对象字段里，求值过程不写任何可变状态，
    ''' 因此可以被多个线程同时调用，也可以在同一个样本上反复求值而不需要重置。
    ''' Mamdani 前端的历史类型 <c>Logical.FuzzyLogic.MembershipFunction</c> 已经改为
    ''' <see cref="TrapezoidalMembership"/> 的门面，因此同样遵守这份契约。
    ''' </para>
    ''' <para>
    ''' 求导数时对外提供两种形式：
    ''' <list type="bullet">
    ''' <item><see cref="Gradient"/> 给出 <c>d(mu)/d(p_k)</c>，用于与中心差分做对拍校验；</item>
    ''' <item><see cref="LogGradient"/> 给出 <c>d(mu)/d(p_k) / mu</c>，即对数导数。
    ''' 前提参数梯度在对数域推导，使用对数导数可以避免 <c>mu</c> 下溢到 0 时的除零
    ''' （这是数值上的关键：直接算 <c>d(mu)/dp</c> 再除 <c>mu</c> 会爆）。</item>
    ''' </list>
    ''' </para>
    ''' </remarks>
    Public Interface IMembershipFunction

        ''' <summary>隶属函数类型。</summary>
        ReadOnly Property Kind As MembershipKind
        ''' <summary>参数个数（高斯 2、钟形 3、Sigmoid 2、三角 3、梯形 4）。</summary>
        ReadOnly Property ParameterCount As Integer

        ''' <summary>读取参数向量（长度等于 <see cref="ParameterCount"/>）。</summary>
        Function GetParameters() As Double()
        ''' <summary>写入参数向量；要求为正的参数会被夹紧到正数下界。</summary>
        Sub SetParameters(values As Double())

        ''' <summary>计算隶属度 <c>mu(x)</c>。</summary>
        Function Evaluate(x As Double) As Double

        ''' <summary>解析导数 <c>d(mu)/d(p_k)</c>。</summary>
        Function Gradient(x As Double) As Double()

        ''' <summary>对数导数 <c>d(mu)/d(p_k) / mu</c>，<c>mu = 0</c> 时退化到其有界极限。</summary>
        Function LogGradient(x As Double) As Double()

        ''' <summary>第 <paramref name="index"/> 个参数是否必须为正（宽度 / 斜率一类）。</summary>
        Function IsPositiveParameter(index As Integer) As Boolean

        ''' <summary>参数名称，用于诊断输出。</summary>
        Function ParameterNames() As String()

        ''' <summary>可读描述，例如 <c>Gaussian(center=0.5, width=0.25)</c>。</summary>
        Function Describe() As String

        ''' <summary>深拷贝（模型克隆与序列化都要求参数互不影响）。</summary>
        Function Clone() As IMembershipFunction

    End Interface

    ''' <summary>
    ''' 隶属函数基类：统一 <see cref="Gradient"/> 的实现（<c>mu * LogGradient</c>）与参数长度校验。
    ''' </summary>
    Public MustInherit Class MembershipFunctionBase
        Implements IMembershipFunction

        ''' <summary>宽度类参数的下界，避免除零与 <c>mu</c> 恒为 0 的死区。</summary>
        Public Const MinimumWidth As Double = 0.000001

        ''' <summary>隶属函数类型。</summary>
        Public MustOverride ReadOnly Property Kind As MembershipKind Implements IMembershipFunction.Kind
        ''' <summary>参数个数。</summary>
        Public MustOverride ReadOnly Property ParameterCount As Integer Implements IMembershipFunction.ParameterCount
        ''' <summary>读取参数向量。</summary>
        Public MustOverride Function GetParameters() As Double() Implements IMembershipFunction.GetParameters
        ''' <summary>写入参数向量。</summary>
        Public MustOverride Sub SetParameters(values As Double()) Implements IMembershipFunction.SetParameters
        ''' <summary>计算隶属度。</summary>
        Public MustOverride Function Evaluate(x As Double) As Double Implements IMembershipFunction.Evaluate
        ''' <summary>对数导数。</summary>
        Public MustOverride Function LogGradient(x As Double) As Double() Implements IMembershipFunction.LogGradient
        ''' <summary>参数是否必须为正。</summary>
        Public MustOverride Function IsPositiveParameter(index As Integer) As Boolean Implements IMembershipFunction.IsPositiveParameter
        ''' <summary>参数名称。</summary>
        Public MustOverride Function ParameterNames() As String() Implements IMembershipFunction.ParameterNames
        ''' <summary>深拷贝。</summary>
        Public MustOverride Function Clone() As IMembershipFunction Implements IMembershipFunction.Clone

        ''' <summary>
        ''' 解析导数 <c>d(mu)/d(p_k) = mu * d(ln mu)/d(p_k)</c>。
        ''' </summary>
        ''' <remarks>
        ''' <c>mu</c> 已经下溢到 0 时直接返回全 0：此时该隶属函数对样本没有任何贡献，
        ''' 其梯度也不应该再影响参数（否则会得到 0 * 无穷 = NaN）。
        ''' </remarks>
        Public Function Gradient(x As Double) As Double() Implements IMembershipFunction.Gradient
            Dim lg As Double() = LogGradient(x)
            Dim result(lg.Length - 1) As Double
            Dim mu As Double = Evaluate(x)

            If mu <= 0.0 Then
                Return result
            End If

            For i As Integer = 0 To lg.Length - 1
                result(i) = mu * lg(i)
            Next

            Return result
        End Function

        ''' <summary>可读描述。</summary>
        Public Function Describe() As String Implements IMembershipFunction.Describe
            Dim names As String() = ParameterNames()
            Dim values As Double() = GetParameters()
            Dim parts As String() = New String(names.Length - 1) {}

            For i As Integer = 0 To names.Length - 1
                parts(i) = $"{names(i)}={values(i).ToString("G6")}"
            Next

            Return $"{Kind}({String.Join(", ", parts)})"
        End Function

        ''' <summary>可读描述。</summary>
        Public Overrides Function ToString() As String
            Return Describe()
        End Function

        ''' <summary>校验参数向量长度。</summary>
        Protected Shared Sub CheckLength(values As Double(), expected As Integer, paramName As String)
            If values Is Nothing Then
                Throw New ArgumentNullException(paramName)
            End If
            If values.Length <> expected Then
                Throw New ArgumentException($"{paramName} expects {expected} parameters but got {values.Length}", paramName)
            End If
        End Sub

    End Class

End Namespace
