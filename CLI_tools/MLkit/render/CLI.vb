Imports System.ComponentModel
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.InteropService.SharedORM
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.Analysis
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Data.visualize.Network.Styling
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Driver

<CLI> Module CLI

    <ExportAPI("/network")>
    <Description("Rendering network data model as png/svg image.")>
    <Usage("/network /model <network_tables.directory> [/size <default=5000,3000> /node.size <fieldName=30,300> /fd <arguments.ini> /out <image.png/svg>]")>
    Public Function VisualizeNetwork(args As CommandLine) As Integer
        Dim in$ = args("/model")
        Dim size$ = args("/size") Or "5000,3000"
        Dim out$ = args("/out") Or $"{[in].TrimDIR}/image.png"
        Dim fdArgv As ForceDirectedArgs = Parameters.Load(args("/fd"), ForceDirectedArgs.DefaultNew)
        Dim model = NetworkTables.Load(DIR:=[in]).AnalysisDegrees
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

        Dim image As GraphicsData = graph _
            .doRandomLayout _
            .doForceLayout(fdArgv, showProgress:=True) _
            .DrawImage(
                canvasSize:=size,
                nodeRadius:=nodeSizeMapper,
                fontSize:=NodeStyles.NodeDegreeSize(graph.vertex, "16,120"),
                minLinkWidth:=20,
                edgeDashTypes:=model.nodes _
                    .ToDictionary(Function(n) n.ID,
                                  Function(n)
                                      Return If(n!dash = "dash", DashStyle.DashDot, DashStyle.Solid)
                                  End Function)
            )

        Return image.Save(out).CLICode
    End Function
End Module
