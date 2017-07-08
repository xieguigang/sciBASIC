Imports System.Math
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Mathematical.LinearAlgebra
Imports sys = System.Math

''' <summary>
''' Math provider for <see cref="Force"/>
''' </summary>
Public Module Math

    ''' <summary>
    ''' Vector index for XYZ
    ''' </summary>
    Public Const X = 0, Y = 1, Z = 2

    ''' <summary>
    ''' 将力分解为水平和垂直两个方向上面的分力
    ''' </summary>
    ''' <param name="F"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Decomposition2D(F As Force) As Vector
        Dim v = F.Strength
        Dim a = F.Angle
        Return New Vector({v * sys.Cos(a), v * Sin(a)})
    End Function

    <Extension>
    Public Function Decomposition3D(F As Force) As Vector
        Throw New NotImplementedException
    End Function

    ''' <summary>
    ''' 力的合成的平行四边形法则
    ''' </summary>
    ''' <param name="f1"></param>
    ''' <param name="f2"></param>
    ''' <returns></returns>
    Public Function ParallelogramLaw(f1 As Force, f2 As Force) As Force
        If f1 = 0 Then
            Return f2
        ElseIf f2 = 0 Then
            Return f1
        ElseIf f1 = 0 AndAlso f2 = 0 Then
            Return New Force
        Else
            If f2.Angle > f1.Angle Then
                Call f2.SwapWith(f1)
            End If
        End If

        Dim alpha = f1.Angle - f2.Angle
        Dim F = Sqrt(f1 ^ 2 + f2 ^ 2 + 2 * f1 * f2 * sys.Cos(alpha))
        Dim sina = Sin(alpha) * f1 / F

        alpha = Sinh(sina)

        Return New Force With {
            .Strength = F,
            .Angle = alpha,
            .source = NameOf(ParallelogramLaw)
        }
    End Function

    <Extension>
    Public Function Sum(F As IEnumerable(Of Force)) As Force
        Dim result As New Force

        For Each n As Force In F
            result = result + n
        Next

        Return result
    End Function

    ''' <summary>
    ''' 重力总是向下的
    ''' </summary>
    ''' <param name="m"></param>
    ''' <param name="g#"></param>
    ''' <returns></returns>
    Public Function Gravity(m As MassPoint, Optional g# = 9.8) As Force
        Dim f = m.Mass * g
        Dim alpha = PI + 1 / 2 * PI

        Return New Force With {
            .Strength = f,
            .Angle = alpha,
            .source = NameOf(Gravity)
        }
    End Function

    ''' <summary>
    ''' 与合力的方向相反的摩檫力
    ''' </summary>
    ''' <returns></returns>
    Public Function Friction() As Force
        Throw New NotImplementedException
    End Function

    ''' <summary>
    ''' 只计算出力的大小，没有方向
    ''' </summary>
    ''' <param name="q1#"></param>
    ''' <param name="q2#"></param>
    ''' <param name="r#"></param>
    ''' <param name="k#"></param>
    ''' <returns></returns>
    Public Function CoulombsLaw(q1#, q2#, r#, Optional k# = 9000000000.0) As Double
        Return k * q1 * q2 / r ^ 2
    End Function

    ''' <summary>
    ''' 两个点电荷之间的库仑力
    ''' </summary>
    ''' <param name="m1"></param>
    ''' <param name="m2"></param>
    ''' <param name="k#"></param>
    ''' <returns></returns>
    Public Function CoulombsLaw(m1 As MassPoint, m2 As MassPoint, Optional k# = 9000000000.0) As Force
        Dim d = m1.Point - m2.Point
        Dim f = Math.CoulombsLaw(m1.Charge, m2.Charge, d.SumMagnitude, k)

        With RepulsiveForce(f, m1.Point, m2.Point)
            .source = NameOf(CoulombsLaw)
            Return .ref
        End With
    End Function

    ''' <summary>
    ''' 计算两个向量之间的alpha夹角的cos值
    ''' </summary>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    ''' <returns></returns>
    Public Function Cos(a As Vector, b As Vector) As Double
        Return a.DotProduct(b) / (a.SumMagnitude * b.SumMagnitude)
    End Function

    ''' <summary>
    ''' 排斥力模型
    ''' </summary>
    ''' <param name="strength#"></param>
    ''' <param name="a">空间位置坐标</param>
    ''' <param name="b">空间位置坐标</param>
    ''' <returns></returns>
    Public Function RepulsiveForce(strength#, a As Vector, b As Vector) As Force
        Dim cosA = Math.Cos(a, b)
        Dim alpha = Cosh(cosA)

        Return New Force With {
            .Strength = strength,
            .Angle = alpha,
            .source = NameOf(RepulsiveForce)
        }
    End Function

    ''' <summary>
    ''' 吸引力模型
    ''' </summary>
    ''' <param name="strength#"></param>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    ''' <returns></returns>
    Public Function AttractiveForce(strength#, a As Vector, b As Vector) As Force
        With -Math.RepulsiveForce(strength, a, b)
            .source = NameOf(AttractiveForce)
            Return .ref
        End With
    End Function
End Module
