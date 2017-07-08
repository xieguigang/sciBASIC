Imports System.Math
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Mathematical.LinearAlgebra

Public Module Math

    Public Const X = 0, Y = 1, Z = 2

    Public Function ParallelogramLaw(f1 As Force, f2 As Force) As Force
        Dim alpha = f1.Angle - f2.Angle
        Dim F = f1 ^ 2 + f2 ^ 2 + 2 * f1 * f2 * Cos(alpha)
        Dim sina = Sin(alpha) * f1 / F

        alpha = Sinh(sina)

        Return New Force With {
            .Strength = F,
            .Angle = alpha
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
            .Angle = alpha
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

    Public Function CoulombsLaw(m1 As MassPoint, m2 As MassPoint, Optional k# = 9000000000.0) As Force
        Dim d = m1.Point - m2.Point
        Dim f = Math.CoulombsLaw(m1.Charge, m2.Charge, d.SumMagnitude, k)
        Return RepulsiveForce(f, m1.Point, m2.Point)
    End Function

    Public Function Cos(a As Vector, b As Vector) As Double
        Return a.DotProduct(b) / (a.SumMagnitude * b.SumMagnitude)
    End Function

    ''' <summary>
    ''' 
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
            .Angle = alpha
        }
    End Function
End Module
