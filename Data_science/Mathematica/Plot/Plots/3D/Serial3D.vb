Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Legend
Imports Microsoft.VisualBasic.Imaging.Drawing3D

Namespace Plot3D

    ''' <summary>
    ''' Scatter serial data in 3D
    ''' </summary>
    Public Class Serial3D

        Public Property Title As String
        Public Property Color As Color
        Public Property Shape As LegendStyles
        Public Property Points As Point3D()
        Public Property PointSize As Single

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"[{Shape.ToString}, {Color.ToString}] {Title}"
        End Function
    End Class
End Namespace