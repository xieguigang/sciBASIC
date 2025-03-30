Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors

#If NET48 Then
Imports Brush = System.Drawing.Brush
#Else
Imports Brush = Microsoft.VisualBasic.Imaging.Brush
#End If

Namespace Drawing2D.HeatMap

    Public Class HeatMapParameters

        Public ReadOnly Property colorSet As String = "YlGnBu:c8"
        Public ReadOnly Property mapLevels As Integer = 30
        Public ReadOnly Property defaultFill As Color = Color.Transparent

        Sub New(Optional colorSet As String = "YlGnBu:c8",
                Optional mapLevels% = 25,
                Optional defaultFill As String = "Transparent")

            _colorSet = colorSet
            _mapLevels = mapLevels
            _defaultFill = defaultFill.TranslateColor
        End Sub

        Sub New(background As Color,
                Optional colorSet As String = "YlGnBu:c8",
                Optional mapLevels% = 25)

            _colorSet = colorSet
            _mapLevels = mapLevels
            _defaultFill = background
        End Sub

        Public Function GetColors() As Color()
            Return Designer.GetColors(colorSet, mapLevels)
        End Function

        Public Function GetBrushes() As Brush()
            Return Designer.GetBrushes(colorSet, mapLevels)
        End Function

        Public Overrides Function ToString() As String
            Return $"{colorSet}[interpolate={mapLevels}], background={defaultFill.ToHtmlColor}"
        End Function

    End Class
End Namespace