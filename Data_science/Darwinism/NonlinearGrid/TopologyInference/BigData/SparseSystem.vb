Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization

Namespace BigData

    Public Class SparseGridSystem : Implements IDynamicsComponent(Of SparseGridSystem)

        ''' <summary>
        ''' 线性方程的常数项
        ''' </summary>
        ''' <returns></returns>
        Public Property AC As Double
        Public Property A As SparseVector
        Public Property C As SparseCorrelation()

        Public ReadOnly Property Width As Integer Implements IDynamicsComponent(Of SparseGridSystem).Width
            Get
                Return A.Length
            End Get
        End Property

        Public Function Evaluate(X As Vector) As Double Implements IDynamicsComponent(Of SparseGridSystem).Evaluate
            Dim C As Vector = Me.C.Select(Function(ci) ci.Evaluate(X)).AsVector
            ' 20190722 当X中存在负数的时候,假设对应的C相关因子为小数负数,则会出现NaN计算结果值
            Dim F As Vector = Math.E ^ C
            Dim fx As SparseVector = A * X * F
            Dim S = AC + fx.Sum

            Return S
        End Function

        Public Function Clone() As SparseGridSystem Implements ICloneable(Of SparseGridSystem).Clone
            Return New SparseGridSystem With {
                .A = New SparseVector(A),
                .AC = AC,
                .C = C _
                    .Select(Function(p) p.Clone) _
                    .ToArray
            }
        End Function
    End Class
End Namespace