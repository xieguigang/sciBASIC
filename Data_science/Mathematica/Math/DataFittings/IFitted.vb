Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Interface IFitted

    ''' <summary>
    ''' 相关系数
    ''' </summary>
    ''' <returns></returns>
    ReadOnly Property CorrelationCoefficient As Double
    ReadOnly Property Polynomial As Polynomial

    Default ReadOnly Property GetY(x As Double) As Double

End Interface
