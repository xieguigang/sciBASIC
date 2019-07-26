Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Data.visualize.Network
Imports Microsoft.VisualBasic.Data.visualize.Network.Analysis
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Layouts
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Driver

Module CLI

    <ExportAPI("/network")>
    <Usage("/network /model <network_tables.directory> [/size <default=5000,3000> /fd <arguments.ini> /out <image.png/svg>]")>
    Public Function VisualizeNetwork(args As CommandLine) As Integer
        Dim in$ = args("/model")
        Dim size$ = args("/size") Or "5000,3000"
        Dim out$ = args("/out") Or $"{[in].TrimDIR}/image.png"
        Dim fdArgv As ForceDirectedArgs = Parameters.Load(args("/fd"), ForceDirectedArgs.DefaultNew)
        Dim model = NetworkTables.Load(DIR:=[in]).AnalysisDegrees
        Dim graph As NetworkGraph = model.CreateGraph(nodeColor:=Function(n) n!color.GetBrush)
        Dim image As GraphicsData = graph _
            .doRandomLayout _
            .doForceLayout(fdArgv, showProgress:=True) _
            .DrawImage(
                canvasSize:=size,
                nodeRadius:=DirectMapRadius(20),
                fontSize:=DirectMapRadius(8),
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
