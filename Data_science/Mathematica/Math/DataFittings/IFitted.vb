Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Interface IFitted

    ''' <summary>
    ''' 相关系数
    ''' </summary>
    ''' <returns></returns>
    ReadOnly Property CorrelationCoefficient As Double
    ReadOnly Property Polynomial As Polynomial

    Default ReadOnly Property GetY(x As Double) As Double

    ''' <summary>
    ''' 保存拟合后的y值，在拟合时可设置为不保存节省内存
    ''' </summary>
    Property ErrorTest As TestPoint()

End Interface
