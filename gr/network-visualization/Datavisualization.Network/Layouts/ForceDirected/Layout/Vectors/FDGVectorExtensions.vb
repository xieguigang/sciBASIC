Imports System.Runtime.CompilerServices

Namespace Layouts

    Module FDGVectorExtensions

        <Extension>
        Public Function Average(Of PointF As {New, AbstractVector})(points As IEnumerable(Of PointF)) As PointF
            Dim vector = points.ToArray
            Dim x = vector.Select(Function(p) p.x).Average
            Dim y = vector.Select(Function(p) p.y).Average
            Dim z = vector.Select(Function(p) p.z).Average

            Return New PointF() With {.x = x, .y = y, .z = z}
        End Function
    End Module
End Namespace