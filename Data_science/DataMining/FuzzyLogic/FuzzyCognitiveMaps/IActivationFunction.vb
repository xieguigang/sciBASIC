Option Strict On
Option Explicit On

Namespace FuzzyCognitiveMaps

    ''' <summary>FCM 激活/传播函数接口。</summary>
    Public Interface IActivationFunction
        ''' <summary>输出下界。</summary>
        ReadOnly Property LowerBound As Double
        ''' <summary>输出上界。</summary>
        ReadOnly Property UpperBound As Double
        ''' <summary>把加权和映射为激活值。</summary>
        Function Evaluate(value As Double) As Double
    End Interface

End Namespace
