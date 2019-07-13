Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization

''' <summary>
''' The Nonlinear Grid Dynamics System
''' </summary>
''' <remarks>
''' 理论上可以拟合任意一个系统
''' </remarks>
Public Class GridSystem : Implements ICloneable(Of GridSystem)

    ''' <summary>
    ''' 线性方程的常数项
    ''' </summary>
    ''' <returns></returns>
    Public Property AC As Double
    Public Property A As Vector
    Public Property C As Correlation()
    ' Public Property P As PWeight()

    ''' <summary>
    ''' Evaluate the system dynamics
    ''' 
    ''' ```
    ''' C + A * X ^ C
    ''' ```
    ''' </summary>
    ''' <param name="X"></param>
    ''' <returns></returns>
    Public Function Evaluate(X As Vector) As Double
        Dim C As Vector = Me.C.Select(Function(ci) ci.Evaluate(X)).AsVector
        ' Dim P As Vector = Me.P.Select(Function(pi) pi.Evaluate(X)).AsVector
        Dim fx As Vector = A * (X ^ C)
        Dim result = AC + fx.Sum

        Return result
    End Function

    Public Function Clone() As GridSystem Implements ICloneable(Of GridSystem).Clone
        Return New GridSystem With {
            .A = New Vector(A.AsEnumerable),
            .AC = AC,
            .C = C _
                .Select(Function(ci) ci.Clone) _
                .ToArray            ' .P = P.Select(Function(pi) pi.Clone).ToArray
        }
    End Function
End Class
