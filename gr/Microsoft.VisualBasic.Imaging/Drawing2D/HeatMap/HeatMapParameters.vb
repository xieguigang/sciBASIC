Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors
Imports Microsoft.VisualBasic.Imaging.Drawing2D.Colors.Scaler
Imports Microsoft.VisualBasic.Linq



#If NET48 Then
Imports Brush = System.Drawing.Brush
#Else
Imports Brush = Microsoft.VisualBasic.Imaging.Brush
#End If

Namespace Drawing2D.HeatMap

    Public Class HeatMapParameters

        Public ReadOnly Property colorSet As String = "YlGnBu:c8"
        Public ReadOnly Property mapLevels As Integer = 30
        ''' <summary>
        ''' background color options when deal with the irregular polygon
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property defaultFill As Color = Color.Transparent
        ''' <summary>
        ''' manual controls of the heatmap value mapping range
        ''' </summary>
        ''' <returns></returns>
        Public Property ValueRange As DoubleRange

        Sub New(Optional colorSet As String = "YlGnBu:c8",
                Optional mapLevels% = 25,
                Optional defaultFill As String = "Transparent")

            _colorSet = colorSet
            _mapLevels = mapLevels
            _defaultFill = defaultFill.TranslateColor
        End Sub

        Sub New(Optional colorSet As ScalerPalette = ScalerPalette.Jet,
                Optional mapLevels% = 25,
                Optional defaultFill As String = "Transparent")

            _colorSet = colorSet.Description
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

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetMapping(values As IEnumerable(Of Double)) As ValueScaleColorProfile
            Return New ValueScaleColorProfile(values.JoinIterates(ValueRange.AsEnumerable), colorSet, mapLevels, -1)
        End Function

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