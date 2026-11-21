Option Strict On
Option Explicit On

Imports System
Imports std = System.Math

Namespace Hybrid

    ''' <summary>
    ''' TSK 的数值域与 FCM 的激活域 <c>[0,1]</c> 之间的显式换算。
    ''' </summary>
    ''' <remarks>
    ''' <para>
    ''' 之前 TSK 输出与 FCM 激活被直接做算术平均（<c>(a + z) / 2</c>），
    ''' 两者量纲完全不同（原始表达量/z 值 vs 隶属度），平均出来的数没有意义。
    ''' 把换算抽成接口之后，耦合点只剩「一个显式的映射」，
    ''' 量纲错误在类型上就不成立了。
    ''' </para>
    ''' <para>
    ''' 默认实现 <see cref="RangeActivationMapper"/> 假设 TSK 一侧是 z 值、用固定区间线性映射；
    ''' 需要更精细的口径（例如按每个基因自己的 <c>[min,max]</c>）时传入自定义实现即可。
    ''' </para>
    ''' </remarks>
    Public Interface ITskActivationMapper

        ''' <summary>TSK 数值 → FCM 激活（应当落在 <c>[0,1]</c>）。</summary>
        Function ToActivation(value As Double) As Double

        ''' <summary>FCM 激活 → TSK 数值（<see cref="ToActivation"/> 的逆）。</summary>
        Function FromActivation(activation As Double) As Double

    End Interface

    ''' <summary>
    ''' 线性区间映射：<c>activation = (value - minimum) / (maximum - minimum)</c>。
    ''' </summary>
    Public NotInheritable Class RangeActivationMapper
        Implements ITskActivationMapper

        Private ReadOnly _minimum As Double
        Private ReadOnly _maximum As Double

        ''' <summary>以区间端点创建映射（<paramref name="maximum"/> 必须大于 <paramref name="minimum"/>）。</summary>
        Public Sub New(minimum As Double, maximum As Double)
            If Double.IsNaN(minimum) OrElse Double.IsNaN(maximum) OrElse Double.IsInfinity(minimum) OrElse Double.IsInfinity(maximum) Then
                Throw New ArgumentException("the mapping range must be finite")
            End If
            If maximum <= minimum Then
                Throw New ArgumentException("the maximum must be greater than the minimum", NameOf(maximum))
            End If

            _minimum = minimum
            _maximum = maximum
        End Sub

        ''' <summary>区间下界。</summary>
        Public ReadOnly Property Minimum As Double
            Get
                Return _minimum
            End Get
        End Property

        ''' <summary>区间上界。</summary>
        Public ReadOnly Property Maximum As Double
            Get
                Return _maximum
            End Get
        End Property

        ''' <summary>线性映射到 <c>[0,1]</c>（超界时夹断）。</summary>
        Public Function ToActivation(value As Double) As Double Implements ITskActivationMapper.ToActivation
            Dim ratio As Double = (value - _minimum) / (_maximum - _minimum)

            If Double.IsNaN(ratio) Then
                Return 0.5
            ElseIf ratio < 0.0 Then
                Return 0.0
            ElseIf ratio > 1.0 Then
                Return 1.0
            Else
                Return ratio
            End If
        End Function

        ''' <summary>把 <c>[0,1]</c> 映射回数值域。</summary>
        Public Function FromActivation(activation As Double) As Double Implements ITskActivationMapper.FromActivation
            Dim ratio As Double = If(Double.IsNaN(activation), 0.5, activation)

            ratio = std.Max(0.0, std.Min(1.0, ratio))

            Return _minimum + ratio * (_maximum - _minimum)
        End Function

    End Class

End Namespace
