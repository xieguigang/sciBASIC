#Region "Microsoft.VisualBasic::2d57111958328bfda24dc914bd36fa7f, Data_science\Visualization\Visualization\Tabular\Extensions.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 62
    '    Code Lines: 48 (77.42%)
    ' Comment Lines: 6 (9.68%)
    '    - Xml Docs: 83.33%
    ' 
    '   Blank Lines: 8 (12.90%)
    '     File Size: 2.22 KB


    '     Module Extensions
    ' 
    '         Function: RemovesYOutlier, ScatterSerials
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.Framework.IO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Quantile

Namespace TabularRender

    Public Module Extensions

        <Extension>
        Public Function ScatterSerials(csv As File, fieldX$, fieldY$, color$, Optional ptSize! = 5) As ChartPlots.SerialData
            With DataFrameResolver.CreateObject(csv)
                Dim index As (X%, y%) = (.GetOrdinal(fieldX), .GetOrdinal(fieldY))
                Dim columns = .GetColumnVectors.ToArray
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
                    .pointSize = ptSize,
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
