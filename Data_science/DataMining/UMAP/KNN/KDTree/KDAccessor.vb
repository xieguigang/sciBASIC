
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.GraphTheory.KdTree
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Correlations.DistanceMethods

Namespace KNN.KDTreeMethod

    Public Class KDAccessor : Inherits KdNodeAccessor(Of KDPoint)

        ReadOnly indexMaps As Dictionary(Of String, Integer)

        Sub New(dims As Integer)
            indexMaps = dims _
                .Sequence _
                .ToDictionary(Function(i) i.ToString,
                              Function(i)
                                  Return i
                              End Function)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Sub setByDimensin(x As KDPoint, dimName As String, value As Double)
            x.vector(indexMaps(dimName)) = value
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function GetDimensions() As String()
            Return indexMaps.Keys.ToArray
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function metric(a As KDPoint, b As KDPoint) As Double
            Return a.vector.EuclideanDistance(b.vector)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function getByDimension(x As KDPoint, dimName As String) As Double
            Return x.vector(indexMaps(dimName))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function nodeIs(a As KDPoint, b As KDPoint) As Boolean
            Return a Is b
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function activate() As KDPoint
            Return New KDPoint
        End Function
    End Class
End Namespace