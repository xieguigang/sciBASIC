Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Driver

Public Module Extensions

    <Extension>
    Public Sub SavePng(plot As PlotEngine, filepath As String, Optional dpi As Integer = 300)
        Dim g As GdiRasterGraphics = DirectCast(plot.GetGraphics, GdiRasterGraphics)
        Dim res = g.ImageResource

        Call res.SaveAs(filepath)
    End Sub
End Module
