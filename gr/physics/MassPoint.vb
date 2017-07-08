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

    ''' <summary>
    ''' 物体受力后产生位移
    ''' </summary>
    Public Sub ApplyForce(F As Force, Optional c# = 1)
        If Point.Count = 2 Then
            Point += F.Decomposition2D * c
        Else
            Point += F.Decomposition3D * c
        End If
    End Sub
End Class
