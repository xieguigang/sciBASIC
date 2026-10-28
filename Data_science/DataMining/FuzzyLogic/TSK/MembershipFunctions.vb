Option Strict On
Option Explicit On

Imports System
Imports std = System.Math

Namespace TSK

    ''' <summary>
    ''' 高斯隶属函数 <c>mu(x) = exp(-(x-c)^2 / (2*sigma^2))</c>，参数 <c>(center, width)</c>。
    ''' </summary>
    ''' <remarks>
    ''' TSK 推理的默认选择。对数导数为
    ''' <c>[(x-c)/sigma^2, (x-c)^2/sigma^3]</c>，在 <c>sigma &gt; 0</c> 时处处有界，
    ''' 因此整个训练过程不需要对 <c>mu</c> 做任何除零保护。
    ''' </remarks>
    Public Class GaussianMembership
        Inherits MembershipFunctionBase

        Private _center As Double
        Private _width As Double

        ''' <summary>按中心与宽度创建（宽度被夹紧到 <see cref="MembershipFunctionBase.MinimumWidth"/> 之上）。</summary>
        Public Sub New(center As Double, width As Double)
            _center = center
            _width = If(width > MinimumWidth, width, MinimumWidth)
        End Sub

        ''' <summary>隶属函数类型。</summary>
        Public Overrides ReadOnly Property Kind As MembershipKind
            Get
                Return MembershipKind.Gaussian
            End Get
        End Property

        ''' <summary>参数个数。</summary>
        Public Overrides ReadOnly Property ParameterCount As Integer
            Get
                Return 2
            End Get
        End Property

        ''' <summary>中心参数 c。</summary>
        Public Property Center As Double
            Get
                Return _center
            End Get
            Set(value As Double)
                _center = value
            End Set
        End Property

        ''' <summary>宽度参数 sigma（恒为正）。</summary>
        Public Property Width As Double
            Get
                Return _width
            End Get
            Set(value As Double)
                _width = If(value > MinimumWidth, value, MinimumWidth)
            End Set
        End Property

        ''' <summary>读取参数向量。</summary>
        Public Overrides Function GetParameters() As Double()
            Return New Double() {_center, _width}
        End Function

        ''' <summary>写入参数向量。</summary>
        Public Overrides Sub SetParameters(values As Double())
            Call CheckLength(values, 2, NameOf(values))
            _center = values(0)
            _width = If(values(1) > MinimumWidth, values(1), MinimumWidth)
        End Sub

        ''' <summary>计算隶属度。</summary>
        Public Overrides Function Evaluate(x As Double) As Double
            Dim z As Double = (x - _center) / _width
            Return std.Exp(-0.5 * z * z)
        End Function

        ''' <summary>对数导数。</summary>
        Public Overrides Function LogGradient(x As Double) As Double()
            Dim d As Double = x - _center
            Dim w2 As Double = _width * _width
            Return New Double() {d / w2, d * d / (w2 * _width)}
        End Function

        ''' <summary>宽度必须为正。</summary>
        Public Overrides Function IsPositiveParameter(index As Integer) As Boolean
            Return index = 1
        End Function

        ''' <summary>参数名称。</summary>
        Public Overrides Function ParameterNames() As String()
            Return New String() {"center", "width"}
        End Function

        ''' <summary>深拷贝。</summary>
        Public Overrides Function Clone() As IMembershipFunction
            Return New GaussianMembership(_center, _width)
        End Function

    End Class

    ''' <summary>
    ''' 广义钟形隶属函数 <c>mu(x) = 1 / (1 + |(x-c)/a|^(2b))</c>，参数 <c>(a, b, c)</c>。
    ''' </summary>
    ''' <remarks>
    ''' 与高斯相比钟形有更长的尾部（近似幂律衰减），在输入分布有重尾时更稳健。
    ''' <c>a</c> 与 <c>b</c> 必须为正；<c>b</c> 过小会让 <c>t^(2b-1)</c> 在 <c>t</c> 接近 0 时发散，
    ''' 因此 <see cref="SetParameters"/> 把 <c>b</c> 夹紧到 <c>[0.5, 50]</c>。
    ''' </remarks>
    Public Class GeneralizedBellMembership
        Inherits MembershipFunctionBase

        Private _a As Double
        Private _b As Double
        Private _c As Double

        ''' <summary>按 (a, b, c) 创建。</summary>
        Public Sub New(a As Double, b As Double, c As Double)
            _a = If(a > MinimumWidth, a, MinimumWidth)
            _b = ClampB(b)
            _c = c
        End Sub

        Private Shared Function ClampB(b As Double) As Double
            If b < 0.5 Then
                Return 0.5
            ElseIf b > 50.0 Then
                Return 50.0
            Else
                Return b
            End If
        End Function

        ''' <summary>隶属函数类型。</summary>
        Public Overrides ReadOnly Property Kind As MembershipKind
            Get
                Return MembershipKind.GeneralizedBell
            End Get
        End Property

        ''' <summary>参数个数。</summary>
        Public Overrides ReadOnly Property ParameterCount As Integer
            Get
                Return 3
            End Get
        End Property

        ''' <summary>读取参数向量。</summary>
        Public Overrides Function GetParameters() As Double()
            Return New Double() {_a, _b, _c}
        End Function

        ''' <summary>写入参数向量。</summary>
        Public Overrides Sub SetParameters(values As Double())
            Call CheckLength(values, 3, NameOf(values))
            _a = If(values(0) > MinimumWidth, values(0), MinimumWidth)
            _b = ClampB(values(1))
            _c = values(2)
        End Sub

        ''' <summary>计算隶属度。</summary>
        Public Overrides Function Evaluate(x As Double) As Double
            Dim t As Double = std.Abs((x - _c) / _a)

            If t < 0.000000000001 Then
                Return 1.0
            End If

            Return 1.0 / (1.0 + std.Pow(t, 2.0 * _b))
        End Function

        ''' <summary>对数导数。</summary>
        Public Overrides Function LogGradient(x As Double) As Double()
            Dim logGrad(2) As Double
            Dim t As Double = std.Abs((x - _c) / _a)

            If t < 0.000000000001 Then
                ' x == c 时 mu 恒为 1，对任何参数的一阶导都是 0
                Return logGrad
            End If

            Dim lnT As Double = std.Log(t)
            Dim p As Double = std.Exp(2.0 * _b * lnT)
            Dim mu As Double = 1.0 / (1.0 + p)
            Dim sign As Double = If(x >= _c, 1.0, -1.0)

            logGrad(0) = 2.0 * _b * p * mu / _a
            logGrad(1) = -2.0 * lnT * p * mu
            logGrad(2) = 2.0 * _b * std.Pow(t, 2.0 * _b - 1.0) * mu * sign / _a

            Return logGrad
        End Function

        ''' <summary>a 与 b 必须为正。</summary>
        Public Overrides Function IsPositiveParameter(index As Integer) As Boolean
            Return index = 0 OrElse index = 1
        End Function

        ''' <summary>参数名称。</summary>
        Public Overrides Function ParameterNames() As String()
            Return New String() {"a", "b", "c"}
        End Function

        ''' <summary>深拷贝。</summary>
        Public Overrides Function Clone() As IMembershipFunction
            Return New GeneralizedBellMembership(_a, _b, _c)
        End Function

    End Class

    ''' <summary>
    ''' Sigmoid 隶属函数 <c>mu(x) = 1 / (1 + exp(-a(x-c)))</c>，参数 <c>(a, c)</c>。
    ''' </summary>
    ''' <remarks>
    ''' 适合表达"大于某阈值"这类单调概念（例如"温度偏高"）。注意它不满足
    ''' "两侧都回到 0"，所以通常只应该用在定义域的端点，或者与另一侧的 Sigmoid 配对使用。
    ''' </remarks>
    Public Class SigmoidMembership
        Inherits MembershipFunctionBase

        Private _a As Double
        Private _c As Double

        ''' <summary>按斜率 a 与中心 c 创建。</summary>
        Public Sub New(a As Double, c As Double)
            _a = If(a > MinimumWidth, a, MinimumWidth)
            _c = c
        End Sub

        ''' <summary>隶属函数类型。</summary>
        Public Overrides ReadOnly Property Kind As MembershipKind
            Get
                Return MembershipKind.Sigmoid
            End Get
        End Property

        ''' <summary>参数个数。</summary>
        Public Overrides ReadOnly Property ParameterCount As Integer
            Get
                Return 2
            End Get
        End Property

        ''' <summary>读取参数向量。</summary>
        Public Overrides Function GetParameters() As Double()
            Return New Double() {_a, _c}
        End Function

        ''' <summary>写入参数向量。</summary>
        Public Overrides Sub SetParameters(values As Double())
            Call CheckLength(values, 2, NameOf(values))
            _a = If(values(0) > MinimumWidth, values(0), MinimumWidth)
            _c = values(1)
        End Sub

        ''' <summary>计算隶属度（对指数做 ±40 的夹断以避免溢出）。</summary>
        Public Overrides Function Evaluate(x As Double) As Double
            Dim z As Double = _a * (x - _c)

            If z >= 40.0 Then
                Return 1.0
            ElseIf z <= -40.0 Then
                Return 0.0
            Else
                Return 1.0 / (1.0 + std.Exp(-z))
            End If
        End Function

        ''' <summary>对数导数。</summary>
        ''' <remarks>
        ''' <c>ln(mu) = -ln(1 + exp(-a(x-c)))</c>，因此
        ''' <c>d(ln mu)/da = (1-mu)(x-c)</c>、<c>d(ln mu)/dc = -(1-mu)a</c>。
        ''' 注意第二项是**负**的：<c>z = a(x-c)</c> 对 <c>c</c> 的偏导是 <c>-a</c>。
        ''' </remarks>
        Public Overrides Function LogGradient(x As Double) As Double()
            Dim mu As Double = Evaluate(x)
            Dim d As Double = 1.0 - mu
            Return New Double() {(x - _c) * d, -_a * d}
        End Function

        ''' <summary>斜率必须为正。</summary>
        Public Overrides Function IsPositiveParameter(index As Integer) As Boolean
            Return index = 0
        End Function

        ''' <summary>参数名称。</summary>
        Public Overrides Function ParameterNames() As String()
            Return New String() {"slope", "center"}
        End Function

        ''' <summary>深拷贝。</summary>
        Public Overrides Function Clone() As IMembershipFunction
            Return New SigmoidMembership(_a, _c)
        End Function

    End Class

    ''' <summary>
    ''' 三角隶属函数，参数 <c>(a, b, c)</c>，在 <c>b</c> 处取 1、在 <c>a</c> 与 <c>c</c> 处取 0。
    ''' </summary>
    ''' <remarks>
    ''' 分段线性，只在分段内部可微；节点上的导数取 0。用于把专家给定的
    ''' "大约在 b 附近" 直接录入模型，不建议作为自由训练的初值（梯度在节点附近无界）。
    ''' </remarks>
    Public Class TriangularMembership
        Inherits MembershipFunctionBase

        Private _a As Double
        Private _b As Double
        Private _c As Double

        ''' <summary>按 (a, b, c) 创建，要求 <c>a &lt;= b &lt;= c</c>。</summary>
        Public Sub New(a As Double, b As Double, c As Double)
            Call SetParameters(New Double() {a, b, c})
        End Sub

        ''' <summary>隶属函数类型。</summary>
        Public Overrides ReadOnly Property Kind As MembershipKind
            Get
                Return MembershipKind.Triangular
            End Get
        End Property

        ''' <summary>参数个数。</summary>
        Public Overrides ReadOnly Property ParameterCount As Integer
            Get
                Return 3
            End Get
        End Property

        ''' <summary>左端点 a（在该点隶属度为 0）。</summary>
        Public ReadOnly Property A As Double
            Get
                Return _a
            End Get
        End Property

        ''' <summary>峰值点 b（在该点隶属度为 1）。</summary>
        Public ReadOnly Property B As Double
            Get
                Return _b
            End Get
        End Property

        ''' <summary>右端点 c（在该点隶属度为 0）。</summary>
        Public ReadOnly Property C As Double
            Get
                Return _c
            End Get
        End Property

        ''' <summary>读取参数向量。</summary>
        Public Overrides Function GetParameters() As Double()
            Return New Double() {_a, _b, _c}
        End Function

        ''' <summary>写入参数向量（不强制排序，由调用方保证 <c>a &lt;= b &lt;= c</c>）。</summary>
        Public Overrides Sub SetParameters(values As Double())
            Call CheckLength(values, 3, NameOf(values))
            _a = values(0)
            _b = values(1)
            _c = values(2)
        End Sub

        ''' <summary>计算隶属度。</summary>
        Public Overrides Function Evaluate(x As Double) As Double
            If x <= _a OrElse x >= _c Then
                Return 0.0
            End If

            If x < _b Then
                Dim den As Double = _b - _a

                If den <= MinimumWidth Then
                    Return 1.0
                Else
                    Return (x - _a) / den
                End If
            ElseIf x > _b Then
                Dim den As Double = _c - _b

                If den <= MinimumWidth Then
                    Return 1.0
                Else
                    Return (_c - x) / den
                End If
            Else
                Return 1.0
            End If
        End Function

        ''' <summary>对数导数。</summary>
        Public Overrides Function LogGradient(x As Double) As Double()
            Dim logGrad(2) As Double

            If x < _a OrElse x > _c Then
                Return logGrad
            End If

            If x < _b Then
                Dim left As Double = x - _a
                Dim right As Double = _b - x
                Dim width As Double = _b - _a

                If left <= MinimumWidth OrElse width <= MinimumWidth Then
                    Return logGrad
                End If

                logGrad(0) = -right / (width * left)
                logGrad(1) = -1.0 / width
            ElseIf x > _b Then
                Dim right As Double = _c - x
                Dim left As Double = x - _b
                Dim width As Double = _c - _b

                If right <= MinimumWidth OrElse width <= MinimumWidth Then
                    Return logGrad
                End If

                logGrad(1) = 1.0 / width
                logGrad(2) = left / (width * right)
            End If

            Return logGrad
        End Function

        ''' <summary>三角函数的参数不要求为正。</summary>
        Public Overrides Function IsPositiveParameter(index As Integer) As Boolean
            Return False
        End Function

        ''' <summary>参数名称。</summary>
        Public Overrides Function ParameterNames() As String()
            Return New String() {"a", "b", "c"}
        End Function

        ''' <summary>深拷贝。</summary>
        Public Overrides Function Clone() As IMembershipFunction
            Return New TriangularMembership(_a, _b, _c)
        End Function

    End Class

    ''' <summary>
    ''' 梯形隶属函数，参数 <c>(a, b, c, d)</c>：<c>[b, c]</c> 上取 1，两侧线性回落到 0。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 求值与 Mamdani 前端的历史实现（<c>Logical.FuzzyLogic.LinguisticVariable.Fuzzify</c> 的
    ''' 分段 if/else）逐点一致，包括两个退化肩点的单点取值：
    ''' 左肩 <c>a = b</c> 时 <c>mu(a) = 1</c>、右肩 <c>c = d</c> 时 <c>mu(c) = 1</c>。
    ''' 这条约定是"旧模型重放到新内核不产生数值漂移"的前提，改动前请先跑
    ''' <c>_smoketest</c> 里的 Mamdani 一致性一节。
    ''' </para>
    ''' <para>
    ''' 未排序的坐标（例如 <c>a &gt; b</c>）属于病态输入，不在兼容承诺范围内：
    ''' 分段判据仍然按坐标比较，但结果不保证与历史实现一致。
    ''' </para>
    ''' </remarks>
    Public Class TrapezoidalMembership
        Inherits MembershipFunctionBase

        Private _a As Double
        Private _b As Double
        Private _c As Double
        Private _d As Double

        ''' <summary>按 (a, b, c, d) 创建。</summary>
        Public Sub New(a As Double, b As Double, c As Double, d As Double)
            Call SetParameters(New Double() {a, b, c, d})
        End Sub

        ''' <summary>隶属函数类型。</summary>
        Public Overrides ReadOnly Property Kind As MembershipKind
            Get
                Return MembershipKind.Trapezoidal
            End Get
        End Property

        ''' <summary>参数个数。</summary>
        Public Overrides ReadOnly Property ParameterCount As Integer
            Get
                Return 4
            End Get
        End Property

        ''' <summary>左端点 a（隶属度从该点起线性上升）。</summary>
        Public ReadOnly Property A As Double
            Get
                Return _a
            End Get
        End Property

        ''' <summary>左肩点 b（隶属度升到 1）。</summary>
        Public ReadOnly Property B As Double
            Get
                Return _b
            End Get
        End Property

        ''' <summary>右肩点 c（隶属度开始下降）。</summary>
        Public ReadOnly Property C As Double
            Get
                Return _c
            End Get
        End Property

        ''' <summary>右端点 d（隶属度回到 0）。</summary>
        Public ReadOnly Property D As Double
            Get
                Return _d
            End Get
        End Property

        ''' <summary>读取参数向量。</summary>
        Public Overrides Function GetParameters() As Double()
            Return New Double() {_a, _b, _c, _d}
        End Function

        ''' <summary>写入参数向量（不强制排序）。</summary>
        Public Overrides Sub SetParameters(values As Double())
            Call CheckLength(values, 4, NameOf(values))
            _a = values(0)
            _b = values(1)
            _c = values(2)
            _d = values(3)
        End Sub

        ''' <summary>计算隶属度。</summary>
        ''' <remarks>
        ''' 退化肩点（<c>b - a</c> 或 <c>d - c</c> 小于 <see cref="MembershipFunctionBase.MinimumWidth"/>）
        ''' 在端点处取 1，与历史 Mamdani 实现保持一致；正常梯形在 <c>a</c> 与 <c>d</c> 处仍取 0。
        ''' </remarks>
        Public Overrides Function Evaluate(x As Double) As Double
            If x < _a OrElse x > _d Then
                Return 0.0
            End If

            If x < _b Then
                Dim den As Double = _b - _a

                If den <= MinimumWidth Then
                    Return 1.0
                Else
                    Return (x - _a) / den
                End If
            ElseIf x <= _c Then
                Return 1.0
            Else
                Dim den As Double = _d - _c

                If den <= MinimumWidth Then
                    Return 1.0
                Else
                    Return (_d - x) / den
                End If
            End If
        End Function

        ''' <summary>对数导数（分段线性，节点处取 0）。</summary>
        Public Overrides Function LogGradient(x As Double) As Double()
            Dim logGrad(3) As Double

            If x <= _a OrElse x >= _d Then
                Return logGrad
            End If

            If x < _b Then
                Dim left As Double = x - _a
                Dim right As Double = _b - x
                Dim width As Double = _b - _a

                If left <= MinimumWidth OrElse width <= MinimumWidth Then
                    Return logGrad
                End If

                logGrad(0) = -right / (width * left)
                logGrad(1) = -1.0 / width
            ElseIf x > _c Then
                Dim left As Double = x - _c
                Dim right As Double = _d - x
                Dim width As Double = _d - _c

                If right <= MinimumWidth OrElse width <= MinimumWidth Then
                    Return logGrad
                End If

                logGrad(2) = 1.0 / width
                logGrad(3) = left / (width * right)
            End If

            Return logGrad
        End Function

        ''' <summary>梯形函数的参数不要求为正。</summary>
        Public Overrides Function IsPositiveParameter(index As Integer) As Boolean
            Return False
        End Function

        ''' <summary>参数名称。</summary>
        Public Overrides Function ParameterNames() As String()
            Return New String() {"a", "b", "c", "d"}
        End Function

        ''' <summary>深拷贝。</summary>
        Public Overrides Function Clone() As IMembershipFunction
            Return New TrapezoidalMembership(_a, _b, _c, _d)
        End Function

    End Class

End Namespace
