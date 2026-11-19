Option Strict On
Option Explicit On

Imports System
Imports std = System.Math
Imports TSK = Microsoft.VisualBasic.DataMining.FuzzyLogic.TSK

Namespace FuzzyCognitiveMaps

    ''' <summary>Sigmoid 激活函数，把加权和压缩到 <c>(0,1)</c>。</summary>
    Public NotInheritable Class SigmoidActivation
        Implements IActivationFunction

        Private ReadOnly _alpha As Double

        ''' <summary>以斜率参数创建 Sigmoid 激活函数。</summary>
        Public Sub New(Optional alpha As Double = 1.0)
            If alpha <= 0.0 OrElse Double.IsNaN(alpha) OrElse Double.IsInfinity(alpha) Then
                Throw New ArgumentOutOfRangeException(NameOf(alpha), alpha, "alpha must be positive and finite")
            End If

            _alpha = alpha
        End Sub

        ''' <summary>斜率。</summary>
        Public ReadOnly Property Alpha As Double
            Get
                Return _alpha
            End Get
        End Property

        ''' <summary>输出下界 0。</summary>
        Public ReadOnly Property LowerBound As Double Implements IActivationFunction.LowerBound
            Get
                Return 0.0
            End Get
        End Property

        ''' <summary>输出上界 1。</summary>
        Public ReadOnly Property UpperBound As Double Implements IActivationFunction.UpperBound
            Get
                Return 1.0
            End Get
        End Property

        ''' <summary>计算 Sigmoid。</summary>
        Public Function Evaluate(value As Double) As Double Implements IActivationFunction.Evaluate
            If value >= 0.0 Then
                Dim z As Double = std.Exp(-_alpha * value)

                Return 1.0 / (1.0 + z)
            Else
                Dim z As Double = std.Exp(_alpha * value)

                Return z / (1.0 + z)
            End If
        End Function
    End Class

    ''' <summary>二值激活函数（大于阈值输出上界，否则输出下界）。</summary>
    Public NotInheritable Class BinaryActivation
        Implements IActivationFunction

        Private _lower As Double
        Private _upper As Double
        Private _threshold As Double

        ''' <summary>以值域和阈值创建二值激活函数。</summary>
        Public Sub New(threshold As Double, Optional lower As Double = 0.0, Optional upper As Double = 1.0)
            If lower >= upper Then
                Throw New ArgumentException("lower bound must be less than upper bound", NameOf(lower))
            End If

            _lower = lower
            _upper = upper
            _threshold = threshold
        End Sub

        ''' <summary>输出下界。</summary>
        Public ReadOnly Property LowerBound As Double Implements IActivationFunction.LowerBound
            Get
                Return _lower
            End Get
        End Property

        ''' <summary>输出上界。</summary>
        Public ReadOnly Property UpperBound As Double Implements IActivationFunction.UpperBound
            Get
                Return _upper
            End Get
        End Property

        ''' <summary>激活阈值。</summary>
        Public ReadOnly Property Threshold As Double
            Get
                Return _threshold
            End Get
        End Property

        ''' <summary>阈值判定。</summary>
        Public Function Evaluate(value As Double) As Double Implements IActivationFunction.Evaluate
            Return If(value >= _threshold, _upper, _lower)
        End Function
    End Class

    ''' <summary>分段线性激活函数：在 <c>[lower, upper]</c> 内裁剪。</summary>
    Public NotInheritable Class ClampedLinearActivation
        Implements IActivationFunction

        Private _lower As Double
        Private _upper As Double

        ''' <summary>以值域创建裁剪线性激活函数。</summary>
        Public Sub New(lower As Double, upper As Double)
            If lower >= upper Then
                Throw New ArgumentException("lower bound must be less than upper bound", NameOf(lower))
            End If

            _lower = lower
            _upper = upper
        End Sub

        ''' <summary>输出下界。</summary>
        Public ReadOnly Property LowerBound As Double Implements IActivationFunction.LowerBound
            Get
                Return _lower
            End Get
        End Property

        ''' <summary>输出上界。</summary>
        Public ReadOnly Property UpperBound As Double Implements IActivationFunction.UpperBound
            Get
                Return _upper
            End Get
        End Property

        ''' <summary>裁剪到值域。</summary>
        Public Function Evaluate(value As Double) As Double Implements IActivationFunction.Evaluate
            If value < _lower Then
                Return _lower
            ElseIf value > _upper Then
                Return _upper
            Else
                Return value
            End If
        End Function
    End Class

    ''' <summary>
    ''' 用 TSK 隶属函数作为激活函数，把加权和映射到隶属度。
    ''' </summary>
    ''' <remarks>值域固定为 <c>[0,1]</c>，保证与模糊逻辑内核的数值语义一致。</remarks>
    Public NotInheritable Class MembershipActivation
        Implements IActivationFunction

        Private ReadOnly _membership As TSK.IMembershipFunction

        ''' <summary>以 TSK 隶属函数创建激活函数。</summary>
        Public Sub New(membership As TSK.IMembershipFunction)
            If membership Is Nothing Then
                Throw New ArgumentNullException(NameOf(membership))
            End If

            _membership = membership.Clone()
        End Sub

        ''' <summary>复用的隶属函数。</summary>
        Public ReadOnly Property Membership As TSK.IMembershipFunction
            Get
                Return _membership
            End Get
        End Property

        ''' <summary>输出下界 0。</summary>
        Public ReadOnly Property LowerBound As Double Implements IActivationFunction.LowerBound
            Get
                Return 0.0
            End Get
        End Property

        ''' <summary>输出上界 1。</summary>
        Public ReadOnly Property UpperBound As Double Implements IActivationFunction.UpperBound
            Get
                Return 1.0
            End Get
        End Property

        ''' <summary>计算隶属度。</summary>
        Public Function Evaluate(value As Double) As Double Implements IActivationFunction.Evaluate
            Dim result As Double = _membership.Evaluate(value)

            If result < 0.0 Then
                Return 0.0
            ElseIf result > 1.0 Then
                Return 1.0
            Else
                Return result
            End If
        End Function
    End Class

End Namespace
