Imports Microsoft.VisualBasic.Mathematical.LinearAlgebra

Public Class MassPoint

    Public Property Mass As Double

    ''' <summary>
    ''' 用来兼容2D/3D
    ''' </summary>
    ''' <returns></returns>
    Public Property Point As Vector

    ''' <summary>
    ''' 用于计算库仑力的电荷量
    ''' </summary>
    ''' <returns></returns>
    Public Property Charge As Double

End Class
