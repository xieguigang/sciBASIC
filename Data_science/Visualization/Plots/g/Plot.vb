Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Graphic

    Public MustInherit Class Plot

        Protected ReadOnly theme As Theme

        Public Property xlabel As String = "X"
        Public Property ylabel As String = "Y"

        ''' <summary>
        ''' the main title string
        ''' </summary>
        ''' <returns></returns>
        Public Property main As String

        Sub New(theme As Theme)
            Me.theme = theme
        End Sub

        Public Function Plot(Optional size$ = Resolution2K.Size, Optional ppi As Integer = 300, Optional driver As Drivers = Drivers.Default) As GraphicsData
            Return g.GraphicsPlots(
                size:=size.SizeParser,
                padding:=theme.padding,
                bg:=theme.background,
                plotAPI:=AddressOf PlotInternal,
                driver:=driver,
                dpi:=$"{ppi},{ppi}"
            )
        End Function

        Protected MustOverride Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)

    End Class
End Namespace