Option Strict On
Option Explicit On

Imports System

''' <summary>
''' 递推最小二乘（RLS）：按样本增量更新结论参数，不做全量重解。
''' </summary>
''' <remarks>
''' <para>
''' 用 Sherman–Morrison 公式维护逆相关矩阵 <c>P = (A'A)^-1</c>：
''' <code>
'''   u      = P * a
'''   denom  = forget + a' * u
'''   gain   = u / denom
'''   theta += gain * (y - a' * theta)
'''   P      = (P - gain * u') / forget
''' </code>
''' 每条样本的代价是 <c>O(P^2)</c>，内存也是 <c>O(P^2)</c>，与批量 LSE 同阶，
''' 但它**不需要保存历史**，因此天然适合流式数据；<c>forget = 1</c> 时等价于累积最小二乘。
''' </para>
''' <para>
''' 数值上要注意：初始协方差取一个较大的对角阵（默认 1000）相当于告诉算法"初始解很不可信"。
''' 长期运行后 <c>P</c> 会趋于 0，更新逐渐失效；<c>forget &lt; 1</c> 可以让它保持"记忆"，
''' 这也是 ANFIS 处理非平稳时序时的常用做法。
''' </para>
''' </remarks>
Public Class RecursiveLeastSquares

    Private ReadOnly _parameterCount As Integer
    Private ReadOnly _outputCount As Integer
    Private ReadOnly _forgetting As Double
    Private ReadOnly _initialCovariance As Double

    Private _inverse As Double(,)
    Private _theta As Double(,)
    Private _count As Long

    ''' <summary>创建 RLS 求解器。</summary>
    Public Sub New(parameterCount As Integer,
                   outputCount As Integer,
                   Optional forgetting As Double = 1.0,
                   Optional initialCovariance As Double = 1000.0)

        If parameterCount <= 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(parameterCount), parameterCount, "the parameter count must be positive")
        End If
        If outputCount <= 0 Then
            Throw New ArgumentOutOfRangeException(NameOf(outputCount), outputCount, "the output count must be positive")
        End If
        If forgetting <= 0.0 OrElse forgetting > 1.0 Then
            Throw New ArgumentOutOfRangeException(NameOf(forgetting), forgetting, "the forgetting factor must be in (0, 1]")
        End If
        If initialCovariance <= 0.0 Then
            Throw New ArgumentOutOfRangeException(NameOf(initialCovariance), initialCovariance, "the initial covariance must be positive")
        End If

        _parameterCount = parameterCount
        _outputCount = outputCount
        _forgetting = forgetting
        _initialCovariance = initialCovariance

        Call Reset()
    End Sub

    ''' <summary>已经喂入的样本数。</summary>
    Public ReadOnly Property SampleCount As Long
        Get
            Return _count
        End Get
    End Property

    ''' <summary>清空状态，回到"初始解 = 0、协方差 = 初值"。</summary>
    Public Sub Reset()
        _inverse = New Double(_parameterCount - 1, _parameterCount - 1) {}
        _theta = New Double(_parameterCount - 1, _outputCount - 1) {}

        For i As Integer = 0 To _parameterCount - 1
            _inverse(i, i) = _initialCovariance
        Next

        _count = 0
    End Sub

    ''' <summary>
    ''' 用一条样本 <c>(a, y)</c> 更新结论参数。
    ''' </summary>
    ''' <param name="a">设计矩阵的一行，长度 = P（第 r 块为 <c>w-bar_r*x</c> 与 <c>w-bar_r</c>）</param>
    ''' <param name="y">该样本的观测值，长度 = 输出维数</param>
    Public Sub Update(a As Double(), y As Double())
        If a Is Nothing OrElse a.Length <> _parameterCount Then
            Throw New ArgumentException($"the design row must have {_parameterCount} elements", NameOf(a))
        End If
        If y Is Nothing OrElse y.Length <> _outputCount Then
            Throw New ArgumentException($"the target must have {_outputCount} elements", NameOf(y))
        End If

        Dim p As Integer = _parameterCount
        Dim o As Integer = _outputCount

        ' u = P * a
        Dim u(p - 1) As Double

        For i As Integer = 0 To p - 1
            Dim sum As Double = 0.0

            For j As Integer = 0 To p - 1
                sum += _inverse(i, j) * a(j)
            Next

            u(i) = sum
        Next

        Dim denom As Double = _forgetting

        For j As Integer = 0 To p - 1
            denom += a(j) * u(j)
        Next

        If denom <= 0.0 OrElse Double.IsNaN(denom) Then
            Return
        End If

        ' gain = u / denom；残差 = y - a' * theta
        Dim gain(p - 1) As Double

        For i As Integer = 0 To p - 1
            gain(i) = u(i) / denom
        Next

        For k As Integer = 0 To o - 1
            Dim prediction As Double = 0.0

            For j As Integer = 0 To p - 1
                prediction += a(j) * _theta(j, k)
            Next

            Dim errorValue As Double = y(k) - prediction

            For i As Integer = 0 To p - 1
                _theta(i, k) += gain(i) * errorValue
            Next
        Next

        ' P = (P - gain * u') / forget
        For i As Integer = 0 To p - 1
            For j As Integer = 0 To p - 1
                Dim updated As Double = _inverse(i, j) - gain(i) * u(j)
                _inverse(i, j) = updated / _forgetting
            Next
        Next

        _count += 1
    End Sub

    ''' <summary>当前的结论参数估计（副本，形状 <c>[P, OutputCount]</c>）。</summary>
    Public Function GetTheta() As Double(,)
        Return CType(_theta.Clone(), Double(,))
    End Function

    ''' <summary>直接覆盖当前估计（用于热启动）。</summary>
    Public Sub SetTheta(values As Double(,))
        If values Is Nothing OrElse values.GetLength(0) <> _parameterCount OrElse values.GetLength(1) <> _outputCount Then
            Throw New ArgumentException($"the consequent matrix must be ({_parameterCount}, {_outputCount})", NameOf(values))
        End If

        _theta = CType(values.Clone(), Double(,))
    End Sub

End Class
