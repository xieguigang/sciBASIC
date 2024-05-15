#Region "Microsoft.VisualBasic::219a75603dd174a73679d8dd1d36b30b, gr\physics\Math.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 211
    '    Code Lines: 109
    ' Comment Lines: 79
    '   Blank Lines: 23
    '     File Size: 6.71 KB


    ' Module Math
    ' 
    '     Function: AttractiveForce, Cos, (+2 Overloads) CoulombsLaw, Decomposition2D, Decomposition3D
    '               Friction, Gravity, ParallelogramLaw, RepulsiveForce, Sum
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Math
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

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
        Dim v = F.strength
        Dim a = F.angle
        Return New Vector({v * std.Cos(a), v * Sin(a)})
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
        If f1 = 0.0 Then
            Return f2
        ElseIf f2 = 0.0 Then
            Return f1
        ElseIf f1 = 0.0 AndAlso f2 = 0.0 Then
            Return New Force
        Else
        End If

        Dim alpha = f1.angle - f2.angle
        Dim F = Sqrt(f1 ^ 2 + f2 ^ 2 + 2 * f1 * f2 * std.Cos(alpha))

        If F = 0R Then
            ' F 为零的之后，只有二者方向相反
            alpha = Min(f2.angle, f1.angle)
        Else
            Dim sina = Sin(alpha) * f1 / F

            If std.Abs(sina) <= 0.000000000001 Then
                ' 要么二者相反，要么二者同向
                If F > f1 AndAlso F > f2 Then
                    ' 二者同向相加才会出现都大于的情况
                    alpha = Min(f1.angle, f2.angle)
                Else
                    ' 反向，取力最大的方向
                    If f1 > f2 Then
                        alpha = f1.angle
                    Else
                        alpha = f2.angle
                    End If
                End If
            Else
                alpha = Sinh(sina) + f2.angle
            End If
        End If

        Return New Force With {
            .strength = F,
            .angle = alpha,
            .source = NameOf(ParallelogramLaw)
        }
    End Function

    ''' <summary>
    ''' 使用平行四边形法则计算出合力
    ''' </summary>
    ''' <param name="F">分力</param>
    ''' <returns></returns>
    <Extension>
    Public Function Sum(F As IEnumerable(Of Force)) As Force
        Dim result As New Force

        ' 力从小到大升序排序，可以保证最后力的方向永远是偏向于大力所指向的方向
        For Each n As Force In F.OrderBy(Function(i) i.strength)
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
            .strength = f,
            .angle = alpha,
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
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
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
            Return .ByRef
        End With
    End Function

    ''' <summary>
    ''' 计算两个向量之间的alpha夹角的cos值
    ''' </summary>
    ''' <param name="a"></param>
    ''' <param name="b"></param>
    ''' <returns></returns>
    ''' 
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
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
        Dim d = a - b
        Dim cosA = Cos(a - b, {100, 0})  ' 两个向量的方向对X坐标轴的夹角才是力的方向
        Dim alpha = Arccos(cosA)

        If d(Y) < 0 Then
            ' y 小于零的时候是第三和第4象限的
            ' cos(170) = cos(190)
            ' 则假设通过判断这个y坐标值知道点是在第三和第四象限
            ' 那么 190 = 180 + (180-170)
            '      350 = 180 + (180-10)
            alpha = PI + (PI - alpha)
        End If

        Return New Force With {
            .strength = strength,
            .angle = alpha,
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
            Return .ByRef
        End With
    End Function
End Module
