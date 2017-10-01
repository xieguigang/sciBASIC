#Region "Microsoft.VisualBasic::1d57ddada419ff0924ed924263d303d9, ..\sciBASIC#\Data_science\Mathematica\Plot\Plots\csv\Extensions.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

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
