Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.Correlations

Namespace KMeans

    Public Class Metric

        ReadOnly propertyNames As String()

        Sub New(propertyNames As IEnumerable(Of String))
            Me.propertyNames = propertyNames.ToArray
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function DistanceTo(x As EntityClusterModel, y As EntityClusterModel) As Double
            Return x(propertyNames).EuclideanDistance(y(propertyNames))
        End Function

    End Class
End Namespace