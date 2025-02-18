#Region "Microsoft.VisualBasic::8380d05c751399cf932284067ef9077f, sciBASIC#\gr\network-visualization\test\mingleTest.vb"

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

'   Total Lines: 147
'    Code Lines: 132
' Comment Lines: 0
'   Blank Lines: 15
'     File Size: 3.13 KB


' Module mingleTest
' 
'     Sub: Main
' 
' Class testJsonNode
' 
'     Properties: data, id, name
' 
' Class data
' 
'     Properties: color, coords, weight
' 
' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.EdgeBundling.Mingle
Imports Microsoft.VisualBasic.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Serialization.JSON

Module mingleTest

    Dim json As String = <json>

                             [
  {
    "id": 1,
    "name": 1,
    "data": {
      "coords": [
        10,
        10,
        10,
        450
      ],
      "weight": 5,
      "color": "rgb(241, 205, 15)"
    }
  },
  {
    "id": 2,
    "name": 2,
    "data": {
      "coords": [
        30,
        10,
        30,
        450
      ],
      "weight": 5,
      "color": "rgb(16, 99, 179)"
    }
  },
  {
    "id": 3,
    "name": 3,
    "data": {
      "coords": [
        50,
        10,
        50,
        450
      ],
      "weight": 5,
      "color": "rgb(147, 148, 58)"
    }
  },
  {
    "id": 4,
    "name": 4,
    "data": {
      "coords": [
        100,
        10,
        100,
        450
      ],
      "weight": 5,
      "color": "rgb(187, 71, 149)"
    }
  },
  {
    "id": 5,
    "name": 5,
    "data": {
      "coords": [
        200,
        10,
        200,
        450
      ],
      "weight": 5,
      "color": "rgb(244, 141, 72)"
    }
  },
  {
    "id": 6,
    "name": 6,
    "data": {
      "coords": [
        230,
        10,
        230,
        450
      ],
      "weight": 5,
      "color": "rgb(110, 196, 145)"
    }
  }
]


                         </json>.Value

    Sub Main()
        Dim bundle As New Bundler(New Options With {
          .angleStrength = 10
        })
        Dim json = mingleTest.json.LoadJSON(Of testJsonNode())
        Dim nodes As New List(Of Node)
        Dim data As MingleNodeData

        For Each item In json
            data = New MingleNodeData With {.mass = item.data.weight, .color = item.data.color.GetBrush, .coords = item.data.coords}
            nodes.Add(New Node(item.name, data))
        Next

        bundle.setNodes(nodes.ToArray)
        bundle.buildNearestNeighborGraph(10)
        bundle.MINGLE()

        Dim canvas As New RenderContext
        Dim render As New MingleRender(New RenderOptions, canvas)

        For Each node As BundleNode In bundle.EnumerateNodes.Select(Function(v) New BundleNode(v))
            Dim edges = node.unbundleEdges(delta:=1)
            render.renderBezier(edges)
        Next

        Using g As Graphics2D = New Size(5000, 5000).CreateGDIDevice
            Call canvas.Render(g)
            Call g.ImageResource.SaveAs("./mingle_test22222222.png")
        End Using
    End Sub
End Module

Public Class testJsonNode

    Public Property id As String
    Public Property name As String
    Public Property data As data

End Class

Public Class data
    Public Property coords As Double()
    Public Property weight As Double
    Public Property color As String
End Class
