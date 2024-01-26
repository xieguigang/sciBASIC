Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Correlations.DistanceMethods

Namespace KdTree.ApproximateNearNeighbor

    Friend Class VectorAccessor : Inherits KdNodeAccessor(Of TagVector)

        Dim dims As Dictionary(Of String, Integer)
        Dim dimKeys As String()

        ''' <summary>
        ''' create an accessor for access the n-dimension vector
        ''' </summary>
        ''' <param name="m"></param>
        Sub New(m As Integer)
            dims = m _
                .Sequence _
                .ToDictionary(Function(k) k.ToString,
                              Function(k)
                                  Return k
                              End Function)
            dimKeys = dims.Keys.ToArray
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub setByDimensin(x As TagVector, dimName As String, value As Double)
            x.vector(dims(dimName)) = value
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function GetDimensions() As String()
            Return dimKeys
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function metric(a As TagVector, b As TagVector) As Double
            Return a.vector.EuclideanDistance(b.vector)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function getByDimension(x As TagVector, dimName As String) As Double
            Return x.vector(dims(dimName))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function nodeIs(a As TagVector, b As TagVector) As Boolean
            Return a.index = b.index
        End Function

        Public Overrides Function activate() As TagVector
            Return New TagVector With {.vector = 0.0.Repeats(times:=dims.Count).ToArray, .index = -1}
        End Function
    End Class
End Namespace