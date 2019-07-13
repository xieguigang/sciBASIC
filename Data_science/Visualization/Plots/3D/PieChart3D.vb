#Region "Microsoft.VisualBasic::91d509a6cffb6d982d6a6c6b92a3f770, Data_science\Visualization\Plots\3D\PieChart3D.vb"

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

    '     Module PieChart3D
    ' 
    '         Function: (+2 Overloads) Plot3D
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.ChartPlots.Fractions
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models.Isometric.Shapes
Imports Microsoft.VisualBasic.Imaging.Driver
Imports Microsoft.VisualBasic.Imaging.Math2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math

Namespace Plot3D

    Public Module PieChart3D

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Plot3D(data As IEnumerable(Of NamedValue(Of Integer)), camera As Camera, Optional schema$ = "Paired:c12") As GraphicsData
            Return data.Fractions(schema).Plot3D(camera)
        End Function

        <Extension>
        Public Function Plot3D(data As IEnumerable(Of FractionData), camera As Camera, Optional valueLabel As ValueLabels = ValueLabels.Percentage) As GraphicsData
            Dim start As New VBDouble
            Dim sweep As New VBDouble
            Dim alpha!
            Dim pt As PointF
            Dim centra As Point3D = camera.screen.GetCenter
            Dim r! = 2.0!
            Dim label$
            Dim pie As Pie
            Dim pieChart As New List(Of Surface)

            For Each x As FractionData In data
                pie = New Pie(centra, r, (start = ((+start) + (sweep = CSng(360 * x.Percentage)))) - sweep.Value, sweep, 20, 1)
                pieChart += pie.Model3D(x.Color)
                alpha = (+start) - (+sweep / 2)
                ' 在这里r/1.5是因为这些百分比的值的标签需要显示在pie的内部
                pt = (r / 1.5, alpha).ToCartesianPoint()
                pt = New PointF(pt.X + centra.X, pt.Y + centra.Y)
                label = x.GetValueLabel(valueLabel)
            Next

            Dim plot3DInternal =
                Sub(ByRef g As IGraphics, region As GraphicsRegion)
                    With pieChart.Centra
                        Dim matrix As New Matrix(.Offsets(pieChart))
                        Dim vector = camera.Rotate(matrix.Matrix)
                        Dim model2D = matrix _
                            .TranslateBuffer(camera, vector, True) _
                            .AsList

                        Call g.BufferPainting(model2D,)
                    End With
                End Sub

            Return g.GraphicsPlots(
                camera.screen, g.DefaultPadding,
                "white",
                plot3DInternal)
        End Function
    End Module
End Namespace
