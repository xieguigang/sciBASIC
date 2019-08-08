Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization

Namespace BigData

    Public Class SparseCorrelation : Implements IDynamicsComponent(Of SparseCorrelation), ICorrelation(Of SparseVector)

        ''' <summary>
        ''' B is a vector that related with X
        ''' </summary>
        ''' <returns></returns>
        Public Property B As SparseVector Implements ICorrelation(Of SparseVector).B
        Public Property BC As Double Implements ICorrelation(Of SparseVector).BC

        Public ReadOnly Property Width As Integer Implements IDynamicsComponent(Of SparseCorrelation).Width
            Get
                Return B.Length
            End Get
        End Property

        Public Function Evaluate(X As Vector) As Double Implements IDynamicsComponent(Of SparseCorrelation).Evaluate
            Dim c As SparseVector = BC + B * X
            Return c.Sum
        End Function

        Public Function Clone() As SparseCorrelation Implements ICloneable(Of SparseCorrelation).Clone
            Return New SparseCorrelation With {
                .B = New SparseVector(B),
                .BC = BC
            }
        End Function
    End Class
End Namespace