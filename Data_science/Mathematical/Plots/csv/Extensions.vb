#Region "Microsoft.VisualBasic::8433ab6054402a6db84c5569571779ec, ..\sciBASIC#\Data_science\Mathematical\Plots\csv\Extensions.vb"

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
