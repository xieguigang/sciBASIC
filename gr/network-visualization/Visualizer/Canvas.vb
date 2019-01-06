Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph
Imports Microsoft.VisualBasic.Data.visualize.Network.Styling
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver

Public Module Canvas

    <Extension>
    Public Function DrawImage(net As NetworkGraph,
                            Optional canvasSize$ = "1024,1024",
                            Optional padding$ = g.DefaultPadding,
                            Optional styling As StyleMapper = Nothing,
                            Optional background$ = "white") As GraphicsData

    End Function

End Module
