#Region "Microsoft.VisualBasic::c6ad14e48a8d73c2f68d3dc04f6dd561, gr\network-visualization\Visualizer\Canvas.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module CanvasDrawer
    ' 
    '     Function: DrawImage
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Styling
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver

Public Module CanvasDrawer

    <Extension>
    Public Function DrawImage(net As NetworkGraph,
                            Optional canvasSize$ = "1024,1024",
                            Optional padding$ = g.DefaultPadding,
                            Optional styling As StyleMapper = Nothing,
                            Optional background$ = "white") As GraphicsData

    End Function

End Module
