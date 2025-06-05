#Region "Microsoft.VisualBasic::fa7674632427c41d378c602890b57de5, gr\Microsoft.VisualBasic.Imaging\Drawing2D\HeatMap\HeatMapParameters.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 110
    '    Code Lines: 74 (67.27%)
    ' Comment Lines: 16 (14.55%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 20 (18.18%)
    '     File Size: 4.09 KB


    '     Class HeatMapBrushes
    ' 
    '         Properties: brushes, defaultFill, valueRange
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: GetMapping
    ' 
    '     Class HeatMapParameters
    ' 
    '         Properties: colorSet, defaultFill, mapLevels, valueRange
    ' 
    '         Constructor: (+3 Overloads) Sub New
    '         Function: CreateBrushParameters, GetBrushes, GetColors, GetMapping, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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

    Public Class HeatMapBrushes

        Public ReadOnly Property brushes As IReadOnlyCollection(Of SolidBrush)
        ''' <summary>
        ''' background color options when deal with the irregular polygon
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property defaultFill As String = NameOf(Color.Transparent)
        ''' <summary>
        ''' manual controls of the heatmap value mapping range
        ''' </summary>
        ''' <returns></returns>
        Public Property valueRange As DoubleRange

        Sub New(args As HeatMapParameters)
            brushes = args.GetBrushes
            defaultFill = args.defaultFill
            valueRange = args.valueRange
        End Sub

        Sub New(brushes As IEnumerable(Of SolidBrush), defaultFill$)
            Me.brushes = brushes.SafeQuery.ToArray
            Me.defaultFill = defaultFill
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetMapping(values As IEnumerable(Of Double)) As ValueScaleColorProfile
            Return New ValueScaleColorProfile(values.JoinIterates(valueRange.AsEnumerable), brushes.Select(Function(b) b.Color), -1)
        End Function
    End Class

    Public Class HeatMapParameters

        Public ReadOnly Property colorSet As String = "YlGnBu:c8"
        Public ReadOnly Property mapLevels As Integer = 30
        ''' <summary>
        ''' background color options when deal with the irregular polygon
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property defaultFill As String = NameOf(Color.Transparent)
        ''' <summary>
        ''' manual controls of the heatmap value mapping range
        ''' </summary>
        ''' <returns></returns>
        Public Property valueRange As DoubleRange

        Sub New(Optional colorSet As String = "YlGnBu:c8",
                Optional mapLevels% = 25,
                Optional defaultFill As String = "Transparent")

            _colorSet = colorSet
            _mapLevels = mapLevels
            _defaultFill = defaultFill
        End Sub

        Sub New(Optional colorSet As ScalerPalette = ScalerPalette.Jet,
                Optional mapLevels% = 25,
                Optional defaultFill As String = "Transparent")

            _colorSet = colorSet.Description
            _mapLevels = mapLevels
            _defaultFill = defaultFill
        End Sub

        Sub New(background As Color,
                Optional colorSet As String = "YlGnBu:c8",
                Optional mapLevels% = 25)

            _colorSet = colorSet
            _mapLevels = mapLevels
            _defaultFill = background.ToHtmlColor(allowTransparent:=True)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetMapping(values As IEnumerable(Of Double)) As ValueScaleColorProfile
            Return New ValueScaleColorProfile(values.JoinIterates(valueRange.AsEnumerable), colorSet, mapLevels, -1)
        End Function

        Public Function GetColors() As Color()
            Return Designer.GetColors(colorSet, mapLevels)
        End Function

        Public Function GetBrushes() As SolidBrush()
            Return Designer.GetBrushes(colorSet, mapLevels)
        End Function

        Public Function CreateBrushParameters() As HeatMapBrushes
            Return New HeatMapBrushes(Me)
        End Function

        Public Overrides Function ToString() As String
            Return $"{colorSet}[interpolate={mapLevels}], background={defaultFill}"
        End Function
    End Class
End Namespace
