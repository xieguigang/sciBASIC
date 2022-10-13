Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic
Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports PCAanalysis = Microsoft.VisualBasic.Math.LinearAlgebra.Prcomp.PCA

Namespace PCA

    Public Class PCA2D : Inherits Plot

        ReadOnly pca As PCAanalysis
        ReadOnly groups As String()

        Public Sub New(PCA As PCAanalysis, groups As String(), theme As Theme)
            MyBase.New(theme)
            Me.pca = PCA
            Me.groups = groups
        End Sub

        Protected Overrides Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)

        End Sub
    End Class
End Namespace