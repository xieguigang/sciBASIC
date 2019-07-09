Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization

''' <summary>
''' The Nonlinear Grid Dynamics System
''' </summary>
''' <remarks>
''' 理论上可以拟合任意一个系统
''' </remarks>
Public Class GridSystem : Implements ICloneable(Of GridSystem)

    Public Property A As Vector
    Public Property C As Correlation()

    ''' <summary>
    ''' Evaluate the system dynamics
    ''' </summary>
    ''' <param name="X"></param>
    ''' <returns></returns>
    Public Function Evaluate(X As Vector) As Double
        Dim C As Vector = Me.C.Select(Function(ci) ci.Evaluate(X)).AsVector
        Dim fx As Vector = A * (X ^ C)
        Dim result = fx.Sum

        Return result
    End Function

    Public Function Clone() As GridSystem Implements ICloneable(Of GridSystem).Clone
        Return New GridSystem With {
            .A = New Vector(A.AsEnumerable),
            .C = C _
                .Select(Function(ci) ci.Clone) _
                .ToArray
        }
    End Function
End Class
