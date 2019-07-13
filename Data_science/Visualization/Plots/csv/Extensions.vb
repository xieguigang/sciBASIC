#Region "Microsoft.VisualBasic::2b167eeb21e351587b3f73a6504e0a19, Data_science\Visualization\Plots\csv\Extensions.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Module Extensions
    ' 
    '         Function: RemovesYOutlier, ScatterSerials
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Quantile

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
                    .Select(Function(xi) New PointF(xi.value, Y(xi))) _
                    .ToArray
                Dim points As PointData() = pts _
                    .Select(Function(pt) New PointData(pt)) _
                    .ToArray

                Return New ChartPlots.SerialData With {
                    .color = color.TranslateColor(throwEx:=False),
                    .PointSize = ptSize,
                    .title = $"Plot({fieldX}, {fieldY})",
                    .pts = points
                }
            End With
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="s"></param>
        ''' <param name="q">默认值为1，表示不会移除任何值</param>
        ''' <returns></returns>
        <Extension>
        Public Function RemovesYOutlier(s As ChartPlots.SerialData, Optional q# = 1) As ChartPlots.SerialData
            If q = 1.0R Then
                Return s
            End If

            With s.pts _
                .Select(Function(pt) CDbl(pt.pt.Y)) _
                .GKQuantile

                q = .Query(quantile:=q)
                s.pts = s.pts _
                    .Where(Function(pt) pt.pt.Y <= q) _
                    .ToArray

                Return s
            End With
        End Function
    End Module
End Namespace
