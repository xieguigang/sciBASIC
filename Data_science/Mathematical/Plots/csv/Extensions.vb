Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Linq

Namespace csv

    Public Module Extensions

        <Extension>
        Public Function ScatterSerials(csv As File, fieldX$, fieldY$, color$, Optional ptSize! = 5) As ChartPlots.SerialData
            With DataFrame.CreateObject(csv)
                Dim index As (X%, y%) = (.GetOrdinal(fieldX), .GetOrdinal(fieldY))
                Dim columns = .Columns.ToArray
                Dim X = columns(index.X)
                Dim Y = columns(index.y)
                Dim pts As PointF() = X _
                    .SeqIterator _
                    .ToArray(Function(xi) New PointF(xi.value, Y(xi)))
                Dim points As PointData() = pts.ToArray(Function(pt) New PointData(pt))

                Return New ChartPlots.SerialData With {
                    .color = color.TranslateColor,
                    .PointSize = ptSize,
                    .title = $"Plot({fieldX}, {fieldY})",
                    .pts = points
                }
            End With
        End Function
    End Module
End Namespace