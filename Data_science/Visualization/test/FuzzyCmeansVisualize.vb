#Region "Microsoft.VisualBasic::99f40fc14ef8b44c38be5b5fe595c1a9, Data_science\Visualization\test\FuzzyCmeansVisualize.vb"

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

    '   Total Lines: 123
    '    Code Lines: 90
    ' Comment Lines: 14
    '   Blank Lines: 19
    '     File Size: 4.28 KB


    ' Module FuzzyCMeansVisualize
    ' 
    '     Function: CMeans
    ' 
    '     Sub: AddPoints, CMeansVisualize, Main, Visualize
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.ChartPlots
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.DataMining.ComponentModel
Imports Microsoft.VisualBasic.DataMining.FuzzyCMeans
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Module FuzzyCMeansVisualize

    ''' <summary>
    ''' Assign random points within given range
    ''' </summary>
    ''' <param name="raw"></param>
    ''' <param name="rnd"></param>
    ''' <param name="n%"></param>
    ''' <param name="up%"></param>
    <Extension>
    Private Sub AddPoints(ByRef raw As List(Of FuzzyCMeansEntity), rnd As Random, n%, up%)
        For i As Integer = 0 To n
            raw += New FuzzyCMeansEntity With {
                .uid = i,
                .entityVector = {
                    rnd.Next(0, up),
                    rnd.Next(0, up)
                }
            }
        Next
    End Sub

    ' 进行cmeans聚类
    Private Function CMeans() As (raw As FuzzyCMeansEntity(), n%, trace As Dictionary(Of Integer, List(Of FuzzyCMeansEntity)))
        Dim raw As New List(Of FuzzyCMeansEntity)
        Dim rnd As New Random(Now.Millisecond)
        Dim up% = 1500

        ' initizlize of the points
        For i As Integer = 0 To 25
            Call raw.AddPoints(rnd, 50, up)
            up -= 50
        Next

        Dim n% = 10  ' required of 10 clusters

        ' invoke cmeans cluster and gets the centra points
        Dim centras = raw.CMeans(n, 2)

#Region "DEBUG INFO OUTPUTS"
        For Each x In raw
            Call ($"{x.uid}: {x.entityVector.GetJson} => " & x.memberships.GetJson).__DEBUG_ECHO
        Next
#End Region

        Return (raw, n, Nothing)
    End Function

    Private Sub CMeansVisualize()
        Call CMeans.Visualize
    End Sub

    Sub Main()
        Call CMeansVisualize()
    End Sub

    <Extension>
    Private Sub Visualize(data As (raw As FuzzyCMeansEntity(), n%, trace As Dictionary(Of Integer, List(Of FuzzyCMeansEntity))))
        Dim trace As Dictionary(Of Integer, List(Of FuzzyCMeansEntity)) = data.trace

        ' data plots visualize
        Dim plotData As New List(Of SerialData)
        ' using ColorBrewer color patterns from the visualbasic internal color designer
        Dim colors As Color() = Designer.GetColors("Paired:c10", data.n)

        ' generates serial data for each point in the raw inputs
        For Each x As FuzzyCMeansEntity In data.raw
            Dim r = colors(x.ProbablyMembership).R
            Dim g = colors(x.ProbablyMembership).G
            Dim b = colors(x.ProbablyMembership).B
            Dim c As Color = Color.FromArgb(CInt(r), CInt(g), CInt(b))

            plotData += Scatter.FromPoints(
                {New PointF(x(0), x(1))},
                c.RGBExpression,
                ptSize:=30,
                title:="Point " & x.uid)
        Next

        Dim traceSerials As New List(Of List(Of FuzzyCMeansEntity))

        For i As Integer = 0 To data.n - 1
            traceSerials += New List(Of FuzzyCMeansEntity)
        Next

        For Each k In trace.Keys.OrderBy(Function(x) x)
            For i As Integer = 0 To data.n - 1
                traceSerials(i) += trace(k)(i)
            Next
        Next

        ' generates the serial data for each centra points
        For i = 0 To data.n - 1
            Dim points As IEnumerable(Of PointF) =
                traceSerials(i) _
                .Select(Function(x) New PointF(x(0), x(1)))
            plotData += Scatter.FromPoints(
                points,
                colors(i).RGBExpression,
                ptSize:=10,
                title:="Cluster " & i)
            plotData.Last.AddMarker(
                plotData.Last.pts.Last.pt.X,
                "Cluster " & i,
                "red",
                style:=LegendStyles.Triangle)
        Next

        Call Scatter.Plot(plotData, "5000,3000", fillPie:=True, showLegend:=False) _
            .Save("./CMeans.png")
    End Sub
End Module
