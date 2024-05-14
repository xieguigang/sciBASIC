#Region "Microsoft.VisualBasic::12e589610e8bed1ff72b0f7ac6994d2a, Data_science\Visualization\Plots\BarPlot\Histogram\Extensions.vb"

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

    '   Total Lines: 47
    '    Code Lines: 33
    ' Comment Lines: 7
    '   Blank Lines: 7
    '     File Size: 1.59 KB


    '     Module Extensions
    ' 
    '         Function: NewModel
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.Distributions.BinBox

<Assembly: InternalsVisibleTo("ggplot")>

Namespace BarPlot.Histogram

    Friend Module Extensions

        ''' <summary>
        ''' Tag值为直方图的高，value值为直方图的平均值连线
        ''' Syntax helper
        ''' </summary>
        ''' <param name="hist"></param>
        ''' <param name="legend"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function NewModel(hist As IEnumerable(Of DataBinBox(Of Double)), legend As LegendObject) As HistProfile
            Dim data As DataBinBox(Of Double)() = hist.ToArray
            Dim boxes As New List(Of HistogramData)

            If data.Length = 0 Then
                Return New HistProfile With {
                    .legend = legend,
                    .data = {}
                }
            End If

            For Each box In data
                boxes += New HistogramData With {
                    .x1 = box.Boundary.Min,
                    .x2 = box.Boundary.Max,
                    .y = box.Count,
                    .pointY = If(box.Count = 0, 0, box.Raw.Average)
                }
            Next

            Return New HistProfile With {
                .legend = legend,
                .data = boxes
            }
        End Function
    End Module
End Namespace
