#Region "Microsoft.VisualBasic::9cb0d79fc161698b79a4724e1d832d40, ..\sciBASIC#\gr\Datavisualization.Network\Test\Test.vb"

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
