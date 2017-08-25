#Region "Microsoft.VisualBasic::7d3d817888f51b77b2431de29d304d93, ..\sciBASIC#\Data_science\Mathematica\Plot\Plots-statistics\ZScores.vb"

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
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Axis
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Scripting.Runtime

''' <summary>
''' Plot of the <see cref="Bootstraping.Z"/>
''' </summary>
Public Module ZScoresPlot

    <Extension>
    Public Function Plot(data As ZScores, Optional size$ = "2700,3000", Optional margin$ = g.DefaultPadding, Optional bg$ = "white") As GraphicsData
        Dim ticks#() = data.Range.CreateAxisTicks
        Dim range As DoubleRange = ticks
        Dim plotInternal =
            Sub(ByRef g As IGraphics, rect As GraphicsRegion)

            End Sub

        Return g.GraphicsPlots(
            size.SizeParser, margin,
            bg,
            plotInternal)
    End Function
End Module

Public Structure ZScores

    Dim serials As DataSet()
    Dim groups As Dictionary(Of String, String())
    ''' <summary>
    ''' Colors for the <see cref="groups"/>
    ''' </summary>
    Dim colors As Dictionary(Of String, Color)

    Public Function Range() As DoubleRange
        Return serials _
            .Select(Function(d) d.Properties.Values) _
            .IteratesALL _
            .Range
    End Function

    Public Shared Function Load(path$, groups As Dictionary(Of String, String()), colors As Color()) As ZScores
        Dim colorlist As LoopArray(Of Color) = colors
        Dim datalist As DataSet() = DataSet.LoadDataSet(path)
        Dim names As New NamedVectorFactory(datalist.PropertyNames)
        Dim zscores = datalist _
            .Select(Function(serial)
                        Dim z As Vector = names _
                            .AsVector(serial.Properties) _
                            .Z()
                        Return New DataSet With {
                            .ID = serial.ID,
                            .Properties = names.Translate(z)
                        }
                    End Function) _
            .ToArray
        Return New ZScores With {
            .serials = zscores,
            .groups = groups,
            .colors = groups.ToDictionary(Function(x) x.Key,
                                          Function(x) colorlist.Next)
        }
    End Function

    Public Shared Function Load(path$, groups As Dictionary(Of String, String()), Optional colors$ = ColorBrewer.QualitativeSchemes.Paired12) As ZScores
        Return ZScores.Load(path, groups, Designer.GetColors(colors))
    End Function
End Structure