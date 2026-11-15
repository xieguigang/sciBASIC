' Copyright (c) sciBASIC.NET Foundation
' GNU GENERAL PUBLIC LICENSE (GPL3) or later

Imports System

Namespace BayesianRegression

    ''' <summary>
    ''' 连续目标的等宽分箱器。
    ''' </summary>
    ''' <remarks>
    ''' 朴素贝叶斯本身是分类器，回归的做法是把连续目标 y 离散为 K 个等宽区间，
    ''' 用区间编号作为类别标签拟合分类器，预测时再以各区间的后验概率对
    ''' 「区间代表值」加权求期望，还原出连续的预测值。
    ''' 区间代表值取箱内训练目标的均值；没有样本的箱退化为区间中点。
    ''' </remarks>
    Public Class TargetBinning

        Private ReadOnly _minimum As Double
        Private ReadOnly _maximum As Double
        Private ReadOnly _interval As Double
        Private ReadOnly _binCount As Integer
        Private ReadOnly _representatives As Double()

        ''' <summary>默认的目标分箱数量</summary>
        Public Const DefaultBins As Integer = 10

        Private Sub New(y As Double(), requestedBins As Integer)
            If y Is Nothing Then
                Throw New ArgumentNullException(NameOf(y))
            End If
            If y.Length = 0 Then
                Throw New ArgumentException("the target vector contains no values.", NameOf(y))
            End If

            Dim lo As Double = Double.PositiveInfinity
            Dim hi As Double = Double.NegativeInfinity

            For i As Integer = 0 To y.Length - 1
                Dim value As Double = y(i)

                If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then
                    Throw New ArgumentException($"the target value at index[{i}] is not a finite number.", NameOf(y))
                End If
                If value < lo Then lo = value
                If value > hi Then hi = value
            Next

            _minimum = lo
            _maximum = hi

            Dim span As Double = hi - lo
            Dim bins As Integer = If(requestedBins < 1, 1, requestedBins)

            If span <= 0.0 Then
                ' 目标恒定的退化情形：只保留一个箱，预测值恒为该常数
                _binCount = 1
                _interval = 0.0
                _representatives = New Double() {lo}
                Return
            End If

            _binCount = bins
            _interval = span / bins

            Dim sums As Double() = New Double(bins - 1) {}
            Dim counts As Integer() = New Integer(bins - 1) {}

            For i As Integer = 0 To y.Length - 1
                Dim b As Integer = BinIndex(y(i))
                sums(b) += y(i)
                counts(b) += 1
            Next

            _representatives = New Double(bins - 1) {}

            For b As Integer = 0 To bins - 1
                If counts(b) > 0 Then
                    _representatives(b) = sums(b) / counts(b)
                Else
                    _representatives(b) = _minimum + (b + 0.5) * _interval
                End If
            Next
        End Sub

        ''' <summary>
        ''' 从已持久化的分箱边界与代表值恢复分箱器，不做任何目标统计。
        ''' </summary>
        Private Sub New(minimum As Double, maximum As Double, interval As Double,
                        binCount As Integer, representatives As Double())
            If binCount < 1 Then
                Throw New ArgumentOutOfRangeException(NameOf(binCount), $"the bin count must be positive, but got {binCount}!")
            End If
            If representatives Is Nothing OrElse representatives.Length <> binCount Then
                Throw New ArgumentException(
                    $"the representative count ({If(representatives Is Nothing, 0, representatives.Length)}) " &
                    $"does not match the bin count ({binCount})!", NameOf(representatives))
            End If
            If Double.IsNaN(minimum) OrElse Double.IsInfinity(minimum) Then
                Throw New ArgumentException($"the binning minimum is not a finite number ({minimum})!", NameOf(minimum))
            End If
            If Double.IsNaN(maximum) OrElse Double.IsInfinity(maximum) Then
                Throw New ArgumentException($"the binning maximum is not a finite number ({maximum})!", NameOf(maximum))
            End If
            If Double.IsNaN(interval) OrElse Double.IsInfinity(interval) OrElse interval < 0.0R Then
                Throw New ArgumentException($"the bin interval is invalid ({interval})!", NameOf(interval))
            End If

            _minimum = minimum
            _maximum = maximum
            _interval = interval
            _binCount = binCount
            _representatives = CType(representatives.Clone(), Double())
        End Sub

        ''' <summary>
        ''' 由训练目标向量拟合分箱边界与代表值。
        ''' </summary>
        ''' <param name="y">连续目标向量</param>
        ''' <param name="bins">期望的箱数，小于 1 时按 1 处理</param>
        Public Shared Function Fit(y As Double(), Optional bins As Integer = DefaultBins) As TargetBinning
            Return New TargetBinning(y, bins)
        End Function

        ''' <summary>
        ''' 从已保存的模型状态恢复分箱器（用于模型持久化加载）。
        ''' </summary>
        ''' <remarks>
        ''' <c>BayesianRegression</c> 的预测路径只读取 <see cref="Representatives"/>，
        ''' 但为了保证 <see cref="BinIndex"/> 与诊断输出在恢复后同样可用，
        ''' 边界与箱宽也一并持久化并还原。
        ''' </remarks>
        Public Shared Function Restore(minimum As Double, maximum As Double, interval As Double,
                                       binCount As Integer, representatives As Double()) As TargetBinning
            Return New TargetBinning(minimum, maximum, interval, binCount, representatives)
        End Function

        ''' <summary>实际生效的箱数（目标恒定时为 1）</summary>
        Public ReadOnly Property BinCount As Integer
            Get
                Return _binCount
            End Get
        End Property

        ''' <summary>训练目标的最小值</summary>
        Public ReadOnly Property Minimum As Double
            Get
                Return _minimum
            End Get
        End Property

        ''' <summary>训练目标的最大值</summary>
        Public ReadOnly Property Maximum As Double
            Get
                Return _maximum
            End Get
        End Property

        ''' <summary>单个箱的宽度，目标恒定时为 0</summary>
        Public ReadOnly Property Interval As Double
            Get
                Return _interval
            End Get
        End Property

        ''' <summary>按箱编号索引的代表值数组，请勿修改返回的数组</summary>
        Public ReadOnly Property Representatives As Double()
            Get
                Return _representatives
            End Get
        End Property

        ''' <summary>
        ''' 把任意目标值映射到箱编号 [0, BinCount-1]，超出训练范围的值钳制到边界箱。
        ''' </summary>
        Public Function BinIndex(value As Double) As Integer
            If _binCount = 1 Then
                Return 0
            End If
            If Double.IsNaN(value) Then
                Throw New ArgumentException("cannot map NaN to a target bin.", NameOf(value))
            End If
            If Double.IsInfinity(value) Then
                Return If(value < 0, 0, _binCount - 1)
            End If

            Dim index As Integer = CInt(System.Math.Floor((value - _minimum) / _interval))

            If index < 0 Then
                Return 0
            End If
            If index >= _binCount Then
                Return _binCount - 1
            End If

            Return index
        End Function

        ''' <summary>读取指定箱的代表值</summary>
        Public Function RepresentativeAt(index As Integer) As Double
            If index < 0 OrElse index >= _binCount Then
                Throw New ArgumentOutOfRangeException(NameOf(index),
                    $"bin index {index} is out of range [0, {_binCount - 1}].")
            End If

            Return _representatives(index)
        End Function

        ''' <summary>读取指定箱的下边界</summary>
        Public Function LowerEdge(index As Integer) As Double
            If index < 0 OrElse index >= _binCount Then
                Throw New ArgumentOutOfRangeException(NameOf(index),
                    $"bin index {index} is out of range [0, {_binCount - 1}].")
            End If

            Return _minimum + index * _interval
        End Function

        ''' <summary>把整条目标向量编码为整数类别标签</summary>
        Public Function Encode(y As Double()) As Integer()
            If y Is Nothing Then
                Throw New ArgumentNullException(NameOf(y))
            End If

            Dim codes As Integer() = New Integer(y.Length - 1) {}

            For i As Integer = 0 To y.Length - 1
                codes(i) = BinIndex(y(i))
            Next

            Return codes
        End Function

        Public Overrides Function ToString() As String
            Return $"TargetBinning: {_binCount} bin(s) over [{_minimum}, {_maximum}], interval={_interval}."
        End Function

    End Class
End Namespace
