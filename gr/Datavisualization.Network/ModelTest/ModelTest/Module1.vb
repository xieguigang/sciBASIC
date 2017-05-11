#Region "Microsoft.VisualBasic::50c6ea33bfbbc9a7c61974bf99ad5a2f, ..\sciBASIC#\gr\Datavisualization.Network\ModelTest\ModelTest\Module1.vb"

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
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Data.visualize.Network.Visualize.NetworkVisualizer

Module Module1

    Sub Main()

        'Call DrawTest()
        'Call Pause()

        Dim graph = CytoscapeExportAsGraph("C:\Users\xieguigang\Source\Repos\sciBASIC\gr\Datavisualization.Network\net_test\xcb-main-Edges.csv", "C:\Users\xieguigang\Source\Repos\sciBASIC\gr\Datavisualization.Network\net_test\xcb-main-Nodes.csv")
        Call graph.doForceLayout(iterations:=100, showProgress:=True)
        Call graph.Tabular.Save("./")
        Call graph.DrawImage(New Size(2000, 2000), scale:=3.5).Save("./test.png")
        Call vbnet.Save(graph.Tabular, "./network.vbnet")
    End Sub

    Private Function DrawTest()
        Dim net = vbnet.Load("C:\Users\xieguigang\Source\Repos\sciBASIC\gr\Datavisualization.Network\ModelTest\ModelTest\bin\Debug\network.vbnet")
        Dim graph = net.CreateGraph
        Call graph.DrawImage(New Size(2000, 2000), scale:=3).Save("./test.png")
    End Function
End Module
