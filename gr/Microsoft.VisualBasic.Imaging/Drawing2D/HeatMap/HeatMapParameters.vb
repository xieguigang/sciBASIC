Imports System.Drawing

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

        Public Overrides Function ToString() As String
            Return $"{colorSet}[interpolate={mapLevels}], background={defaultFill.ToHtmlColor}"
        End Function

    End Class
End Namespace