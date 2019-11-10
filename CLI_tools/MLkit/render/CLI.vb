#Region "Microsoft.VisualBasic::b7c92b4e2fdd363137ca5bf9b613a5e5, CLI_tools\MLkit\render\CLI.vb"

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

    ' Module CLI
    ' 
    '     Function: OrthogonalLayout, VisualizeNetwork
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.InteropService.SharedORM
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.EdgeBundling
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts.Orthogonal
Imports Microsoft.VisualBasic.Data.visualize.Network.Styling
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Scripting.Runtime
Imports Microsoft.VisualBasic.Serialization.JSON

<CLI> Module CLI

    <ExportAPI("/layout.orthogonal")>
    <Usage("/layout.orthogonal /in <network.model.directory> [/node.size <width,height, default=13,8> /grid <size, default=500,300> /min.dist <default=20> /out <result.model.directory>]")>
    <Description("Do orthogonal network layout for the input given network data model.")>
    <Argument("/node.size", True, CLITypes.String,
              AcceptTypes:={GetType(Size)},
              Description:="The unify node size value in [width,height].")>
    <Argument("/grid", True, CLITypes.String,
              AcceptTypes:={GetType(Size)},
              Description:="")>
    Public Function OrthogonalLayout(args As CommandLine) As Integer
        Dim in$ = args <= "/in"
        Dim nodeSize$ = args("/node.size") Or "13,8"
        Dim grid$ = args("/grid") Or "700,500"
        Dim minDist# = args("/min.dist") Or 20.0
        Dim out$ = args("/out") Or ([in].TrimDIR & $".layout.orthogonal/")
        Dim network As NetworkTables = NetworkFileIO.Load([in])
        Dim graph As NetworkGraph = network _
            .CreateGraph(defaultNodeSize:=nodeSize) _
            .ResetNodeSize(nodeSize)

        Call Orthogonal.DoLayout(graph, grid.SizeParser, delta:=minDist)

        Return graph.Tabular _
            .Save(out) _
            .CLICode
    End Function

    <ExportAPI("/network")>
    <Description("Rendering network data model as png/svg image.")>
    <Usage("/network /model <network_tables.directory> [/size <default=5000,3000> /node.size <fieldName=30,300> /fd <arguments.ini> /style <styles.css> /cytoscape /out <image.png/svg>]")>
    <Argument("/model", False, CLITypes.File, PipelineTypes.std_in,
              Description:="A directory path which contains the network table and node attribute table in it.")>
    <Argument("/out", True, CLITypes.File, PipelineTypes.std_out,
              Extensions:="*.png, *.svg",
              Description:="By default the network image render result is saved in bitmap png file, if you want to save it as svg file, 
              then you should add a environment variable config ``/@set graphic_driver=svg`` in the commandline input.")>
    <Argument("/style", True, CLITypes.File,
              Extensions:="*.css",
              Description:="A css style file for your network, this will required your network model have the supported attributes for css rendering, like class, id, etc.")>
    <Argument("/cytoscape", True, CLITypes.Boolean,
              AcceptTypes:={GetType(Boolean)},
              Description:="Input table is in format of the cytoscape output csv tables?")>
    Public Function VisualizeNetwork(args As CommandLine) As Integer
        Dim in$ = args("/model")
        Dim size$ = args("/size") Or "5000,3000"
        Dim isCytoscape As Boolean = args("/cytoscape")
        Dim out$ = args("/out") Or $"{[in].TrimDIR}/image.png"
        Dim fdArgv As ForceDirectedArgs = Parameters.Load(args("/fd"), ForceDirectedArgs.DefaultNew)
        Dim model = NetworkFileIO.Load(DIR:=[in], cytoscapeFormat:=isCytoscape).AnalysisDegrees
        Dim graph As NetworkGraph = model.CreateGraph(nodeColor:=Function(n) n!color.GetBrush)
        Dim nodeSizeMapper As Func(Of Graph.Node, Single) = NodeStyles.NodeDegreeSize(graph.vertex, "30,300")

        If Not args("/node.size").DefaultValue.StringEmpty Then
            Dim config As NamedValue(Of String) = args("/node.size").DefaultValue.GetTagValue("=", trim:=True)
            Dim radiusRange As DoubleRange = config.Value
            Dim valueRange As DoubleRange = graph.vertex _
                .Select(Function(node)
                            Return Val(node.data(config.Name))
                        End Function) _
                .ToArray

            nodeSizeMapper = Function(node) As Single
                                 Return valueRange.ScaleMapping(node.data(config.Name), radiusRange)
                             End Function
        End If

        Call fdArgv.GetJson(indent:=True).__DEBUG_ECHO

        Dim image As GraphicsData = graph _
            .doRandomLayout _
            .doForceLayout(fdArgv, showProgress:=True) _
            .DoBarycentreEdgeLayout _
            .DrawImage(
                canvasSize:=size,
                nodeRadius:=nodeSizeMapper,
                fontSize:=NodeStyles.NodeDegreeSize(graph.vertex, "16,120"),
                minLinkWidth:=20,
                edgeDashTypes:=model.nodes _
                    .ToDictionary(Function(n) n.ID,
                                  Function(n)
                                      Return If(n!dash = "dash", DashStyle.DashDot, DashStyle.Solid)
                                  End Function),
                doEdgeBundling:=True
            )

        Return image.Save(out).CLICode
    End Function
End Module
