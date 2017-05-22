#Region "Microsoft.VisualBasic::12e4114cf0bb914aa2117ec2347c2ad1, ..\sciBASIC#\gr\Datavisualization.Network\Test\Module1.vb"

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

Imports Microsoft.VisualBasic.Data.visualize.Network.Analysis.PageRank
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.Styling
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS
Imports Microsoft.VisualBasic.Serialization.JSON

Module Test

    Sub Main()
        Call TestStyling()
        Call TestPageRank()

        Pause()
    End Sub

    Sub TestPageRank()
        Dim g As New Network

        Call g.AddEdges("B", {"C"})
        Call g.AddEdges("C", {"B"})
        Call g.AddEdges("D", {"A", "B"})
        Call g.AddEdges("E", {"D", "B", "F"})
        Call g.AddEdges("F", {"E", "B"})
        Call g.AddEdges("G", {"E", "B"})
        Call g.AddEdges("H", {"E", "B"})
        Call g.AddEdges("I", {"E", "B"})
        Call g.AddEdges("J", {"E"})
        Call g.AddEdges("K", {"E"})

        Dim matrix As New GraphMatrix(g)
        Dim pr As New PageRank(matrix)

        Dim result = matrix.TranslateVector(pr.ComputePageRank)

        Call result.GetJson(True).EchoLine
    End Sub

    Sub TestStyling()
        Dim json As New StyleJSON With {
            .nodes = New Dictionary(Of String, NodeStyle) From {
            {
                "*", New NodeStyle With {
                    .fill = "black",
                    .size = "size",
                    .stroke = Stroke.AxisStroke
                }
            },
            {
                "type = example", New NodeStyle With {
                    .fill = "red",
                    .size = "scale(size, 5, 30)"
                }
            }
            },
            .labels = New Dictionary(Of String, LabelStyle) From {
            {
                "degree > 2", New LabelStyle With {
                    .fill = "brown"
                }
            }
            }
        }
        Dim styles As StyleMapper = StyleMapper.FromJSON(json)
    End Sub
End Module
