Imports System.Drawing

Namespace Drawing2D.Math2D.MarchingSquares

    Public Class ContourLayer

        Public Property threshold As Double
        Public Property shapes As Polygon2D()
        Public Property dimension As Integer()

        Public Shared Iterator Function GetContours(sample As IEnumerable(Of MeasureData)) As IEnumerable(Of GeneralPath)
            Dim contour As New MarchingSquares()
            Dim matrix As New MapMatrix(sample)
            Dim level_cutoff As Double() = matrix.GetPercentages
            Dim data As Double()() = matrix _
                .GetMatrixInterpolation _
                .MatrixTranspose _
                .ToArray

            For Each polygon As GeneralPath In contour.mkIsos(data, levels:=level_cutoff)
                polygon.dimension = matrix.dimension
                Yield polygon
            Next
        End Function

    End Class

    Public Class Polygon2D

        Public Property x As Integer()
        Public Property y As Integer()

        Public Function ToArray() As PointF()
            Return x _
                .Select(Function(xi, i) New PointF(xi, y(i))) _
                .ToArray
        End Function
    End Class
End Namespace