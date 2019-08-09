Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization

Namespace BigData

    Public Class SparseCorrelation : Implements IDynamicsComponent(Of SparseCorrelation), ICorrelation(Of HalfVector)

        ''' <summary>
        ''' B is a vector that related with X
        ''' </summary>
        ''' <returns></returns>
        Public Property B As HalfVector Implements ICorrelation(Of HalfVector).B
        Public Property BC As Double Implements ICorrelation(Of HalfVector).BC

        Public ReadOnly Property Width As Integer Implements IDynamicsComponent(Of SparseCorrelation).Width
            Get
                Return B.Length
            End Get
        End Property

        Public Function Evaluate(X As Vector) As Double Implements IDynamicsComponent(Of SparseCorrelation).Evaluate
            Dim c As Vector = BC + B * X
            Return c.Sum
        End Function

        Public Function Clone() As SparseCorrelation Implements ICloneable(Of SparseCorrelation).Clone
            Return New SparseCorrelation With {
                .B = New HalfVector(B),
                .BC = BC
            }
        End Function
    End Class
End Namespace