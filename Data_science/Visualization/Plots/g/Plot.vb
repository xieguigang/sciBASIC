Imports Microsoft.VisualBasic.Data.ChartPlots.Graphic.Canvas
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Graphic

    Public MustInherit Class Plot

        ''' <summary>
        ''' the main title
        ''' </summary>
        ''' <returns></returns>
        Public Property main As String = Me.GetType.Name

        ''' <summary>
        ''' the sub-title
        ''' </summary>
        ''' <returns></returns>
        Public Property subtitle As String
        Public Property legendTitle As String

        Public Property mainStyle As FontStyle
        Public Property subTitleStyle As FontStyle
        Public Property legendTitleStyle As FontStyle

        Public Property size As String = "1600,1200"
        Public Property padding As String = g.DefaultPadding

        Public Property background As String

        Sub New(theme As Theme)

        End Sub

        Public Function Plot(Optional ppi As Integer = 300, Optional driver As Drivers = Drivers.Default) As GraphicsData
            Return g.GraphicsPlots(
                size:=size.SizeParser,
                padding:=padding,
                bg:=background,
                plotAPI:=AddressOf PlotInternal,
                driver:=driver,
                dpi:=$"{ppi},{ppi}"
            )
        End Function

        Protected MustOverride Sub PlotInternal(ByRef g As IGraphics, canvas As GraphicsRegion)

    End Class
End Namespace