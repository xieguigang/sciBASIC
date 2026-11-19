Option Strict On
Option Explicit On

Imports System
Imports TSK = Microsoft.VisualBasic.DataMining.FuzzyLogic.TSK

Namespace BooleanNetworks

    ''' <summary>固定阈值二值化：值大于等于阈值时为真。</summary>
    Public NotInheritable Class FixedThresholdBinarizer
        Implements IStateBinarizer

        Private _threshold As Double

        ''' <summary>以指定阈值创建二值化器。</summary>
        Public Sub New(threshold As Double)
            If Double.IsNaN(threshold) OrElse Double.IsInfinity(threshold) Then
                Throw New ArgumentException("threshold must be a finite number", NameOf(threshold))
            End If

            _threshold = threshold
        End Sub

        ''' <summary>二值化阈值。</summary>
        Public Property Threshold As Double
            Get
                Return _threshold
            End Get
            Set(value As Double)
                If Double.IsNaN(value) OrElse Double.IsInfinity(value) Then
                    Throw New ArgumentException("threshold must be a finite number", NameOf(value))
                End If

                _threshold = value
            End Set
        End Property

        ''' <summary>单值二值化。</summary>
        Public Function Binarize(value As Double) As Boolean Implements IStateBinarizer.Binarize
            Return value >= _threshold
        End Function

        ''' <summary>整行二值化。</summary>
        Public Function BinarizeRow(values As Double()) As Boolean() Implements IStateBinarizer.BinarizeRow
            Return BinarizationHelper.BinarizeRow(Me, values)
        End Function
    End Class

    ''' <summary>先把表达值归一化到 <c>[0,1]</c> 再与阈值比较的二值化器。</summary>
    Public NotInheritable Class RangeThresholdBinarizer
        Implements IStateBinarizer

        Private _min As Double
        Private _max As Double
        Private _threshold As Double

        ''' <summary>以表达值域和归一化阈值创建二值化器。</summary>
        Public Sub New(minimum As Double, maximum As Double, threshold As Double)
            If Double.IsNaN(minimum) OrElse Double.IsNaN(maximum) OrElse Double.IsNaN(threshold) Then
                Throw New ArgumentException("range bounds and threshold must be numbers")
            End If
            If maximum <= minimum Then
                Throw New ArgumentException("maximum must be greater than minimum", NameOf(maximum))
            End If
            If threshold < 0.0 OrElse threshold > 1.0 Then
                Throw New ArgumentOutOfRangeException(NameOf(threshold), threshold, "normalized threshold must be in [0,1]")
            End If

            _min = minimum
            _max = maximum
            _threshold = threshold
        End Sub

        ''' <summary>表达值下界。</summary>
        Public ReadOnly Property Minimum As Double
            Get
                Return _min
            End Get
        End Property

        ''' <summary>表达值上界。</summary>
        Public ReadOnly Property Maximum As Double
            Get
                Return _max
            End Get
        End Property

        ''' <summary>归一化阈值。</summary>
        Public ReadOnly Property Threshold As Double
            Get
                Return _threshold
            End Get
        End Property

        ''' <summary>归一化后二值化。</summary>
        Public Function Binarize(value As Double) As Boolean Implements IStateBinarizer.Binarize
            Dim normalized As Double = (value - _min) / (_max - _min)

            Return normalized >= _threshold
        End Function

        ''' <summary>整行二值化。</summary>
        Public Function BinarizeRow(values As Double()) As Boolean() Implements IStateBinarizer.BinarizeRow
            Return BinarizationHelper.BinarizeRow(Me, values)
        End Function
    End Class

    ''' <summary>
    ''' 基于 TSK 隶属函数的模糊二值化：隶属度达到阈值时为真。
    ''' </summary>
    ''' <remarks>
    ''' 该实现直接复用 <see cref="TSK.IMembershipFunction"/>，使布尔网络与模糊逻辑内核共享
    ''' 同一套隶属函数定义，是联合建模的底层入口。
    ''' </remarks>
    Public NotInheritable Class FuzzyMembershipBinarizer
        Implements IStateBinarizer

        Private ReadOnly _membership As TSK.IMembershipFunction
        Private _threshold As Double

        ''' <summary>以隶属函数和隶属度阈值创建二值化器。</summary>
        Public Sub New(membership As TSK.IMembershipFunction, Optional threshold As Double = 0.5)
            If membership Is Nothing Then
                Throw New ArgumentNullException(NameOf(membership))
            End If
            If Double.IsNaN(threshold) OrElse threshold < 0.0 OrElse threshold > 1.0 Then
                Throw New ArgumentOutOfRangeException(NameOf(threshold), threshold, "membership threshold must be in [0,1]")
            End If

            _membership = membership.Clone()
            _threshold = threshold
        End Sub

        ''' <summary>复用的隶属函数。</summary>
        Public ReadOnly Property Membership As TSK.IMembershipFunction
            Get
                Return _membership
            End Get
        End Property

        ''' <summary>隶属度阈值。</summary>
        Public Property Threshold As Double
            Get
                Return _threshold
            End Get
            Set(value As Double)
                If Double.IsNaN(value) OrElse value < 0.0 OrElse value > 1.0 Then
                    Throw New ArgumentOutOfRangeException(NameOf(value), value, "membership threshold must be in [0,1]")
                End If

                _threshold = value
            End Set
        End Property

        ''' <summary>计算隶属度并与阈值比较。</summary>
        Public Function Binarize(value As Double) As Boolean Implements IStateBinarizer.Binarize
            Return _membership.Evaluate(value) >= _threshold
        End Function

        ''' <summary>整行二值化。</summary>
        Public Function BinarizeRow(values As Double()) As Boolean() Implements IStateBinarizer.BinarizeRow
            Return BinarizationHelper.BinarizeRow(Me, values)
        End Function
    End Class

    ''' <summary>二值化器的公共辅助方法。</summary>
    Public NotInheritable Class BinarizationHelper

        Private Sub New()
        End Sub

        ''' <summary>逐元素应用二值化器。</summary>
        Public Shared Function BinarizeRow(binarizer As IStateBinarizer, values As Double()) As Boolean()
            If binarizer Is Nothing Then
                Throw New ArgumentNullException(NameOf(binarizer))
            End If
            If values Is Nothing Then
                Throw New ArgumentNullException(NameOf(values))
            End If

            Dim result(values.Length - 1) As Boolean

            For i As Integer = 0 To values.Length - 1
                result(i) = binarizer.Binarize(values(i))
            Next

            Return result
        End Function

        ''' <summary>为每个基因使用独立阈值的一行二值化。</summary>
        Public Shared Function BinarizeRow(binarizers As IStateBinarizer(), values As Double()) As Boolean()
            If binarizers Is Nothing Then
                Throw New ArgumentNullException(NameOf(binarizers))
            End If
            If values Is Nothing Then
                Throw New ArgumentNullException(NameOf(values))
            End If
            If binarizers.Length <> values.Length Then
                Throw New ArgumentException("binarizer count must match value count", NameOf(binarizers))
            End If

            Dim result(values.Length - 1) As Boolean

            For i As Integer = 0 To values.Length - 1
                result(i) = binarizers(i).Binarize(values(i))
            Next

            Return result
        End Function
    End Class

End Namespace
