#Region "Microsoft.VisualBasic::ac2e876116c3af1b434e3869cf3d1a1a, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Colors\ColorBrewer.vb"

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

    '   Total Lines: 819
    '    Code Lines: 531 (64.84%)
    ' Comment Lines: 199 (24.30%)
    '    - Xml Docs: 96.98%
    ' 
    '   Blank Lines: 89 (10.87%)
    '     File Size: 29.95 KB


    '     Structure ColorBrewer
    ' 
    '         Properties: Accent, Blues, BrBG, BuGn, BuPu
    '                     c10, c11, c12, c3, c4
    '                     c5, c6, c7, c8, c9
    '                     Dark2, GnBu, Greens, Greys, Oranges
    '                     OrRd, Paired, Pastel1, Pastel2, PiYG
    '                     PRGn, PuBu, PuBuGn, PuOr, PuRd
    '                     Purples, RdBu, RdGy, RdPu, RdYlBu
    '                     RdYlGn, Reds, Set1, Set2, Set3
    '                     Spectral, type, YlGn, YlGnBu, YlOrBr
    '                     YlOrRd
    ' 
    '         Function: GetColorHtml, GetColors, GetMaxColors, ParseName, ToString
    '         Class DivergingSchemes
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    '         Class QualitativeSchemes
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    '         Class SequentialSchemes
    ' 
    '             Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Drawing2D.Colors

    ''' <summary>
    ''' Colorbrewer is a nice application for creating color schemes.
    ''' (color data source: https://github.com/xieguigang/sciBASIC/blob/e9ebaf2670ffca4eab35338cb08d84f8ec3d54c8/gr/Colors/colorbrewer/colorbrewer.json)
    ''' </summary>
    ''' <remarks>
    ''' 1. Sequential schemes are suited to ordered data that progress from low to high. Lightness steps dominate the 
    ''' look of these schemes, with light colors for low data values to dark colors for high data values. 
    ''' 
    ''' 2. Diverging schemes put equal emphasis on mid-range critical values and extremes at both ends of the data range. 
    ''' The critical class or break in the middle of the legend is emphasized with light colors and low and high extremes 
    ''' are emphasized with dark colors that have contrasting hues. diverging
    ''' Diverging schemes are most effective when the class break in the middle of the sequence, or the lightest middle 
    ''' color, is meaningfully related to the mapped data. Use the break or class emphasized by a hue and lightness change 
    ''' to represent a critical value in the data such as the mean, median, or zero. Colors increase in darkness to 
    ''' represent differences in both directions from this meaningful mid-range value in the data.
    ''' 
    ''' NOTE: Although we have designed the diverging schemes to be symmetrical, you may need to customize schemes by 
    ''' moving the critical break/class closer to one end of the sequence to suit your map data. For example, a map of 
    ''' population change might have two classes of population loss and five classes of growth, requiring a scheme 
    ''' with only two colors on one side of a zero-change break and five on the other. Choose a scheme with ten-colors 
    ''' and omit three colors from the loss side of the scheme.
    '''
    ''' 3. Qualitative schemes do not imply magnitude differences between legend classes, and hues are used to create the 
    ''' primary visual differences between classes. Qualitative schemes are best suited to representing nominal or 
    ''' categorical data. 
    ''' 
    ''' Most of the qualitative schemes rely on differences in hue with only subtle lightness differences between colors. 
    ''' You may pick a subset of colors from a legend with more classes if you are not pleased with the subsets. 
    ''' For example, you could pick four colors from a seven-color legend. Two exceptions to the use of consistent lightness:
    ''' 
    ''' +  paired scheme
    '''    Paired Scheme: This scheme presents a series of lightness pairs for each hue (e.g. light green and dark green). 
    '''    Use this when you have categories that should be visually related, though they are not explicitly ordered. 
    '''    For example, 'forest' and 'woodland' would be suitably represented with dark and light green.
    ''' +  accent scheme
    '''    Accent Scheme: Use to accent small areas or important classes with colors that are more saturated/darker/lighter 
    '''    than others in the scheme - found at the bottom of the 'Accents' legends. Beware of emphasizing unimportant 
    '''    classes when you use qualitative schemes.
    ''' </remarks>
    Public Structure ColorBrewer

        Public Property c3 As String()
        Public Property c4 As String()
        Public Property c5 As String()
        Public Property c6 As String()
        Public Property c7 As String()
        Public Property c8 As String()
        Public Property c9 As String()
        Public Property c10 As String()
        Public Property c11 As String()
        Public Property c12 As String()

        Public Property type As String

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetColors(name As String) As Color()
            Return GetColorHtml(name).Select(Function(si) si.ToColor).ToArray
        End Function

        Private Function GetColorHtml(name As String) As String()
            If name Is Nothing Then
                Return GetMaxColors()
            End If

            Select Case LCase(name)
                Case NameOf(c3) : Return c3
                Case NameOf(c4) : Return c4
                Case NameOf(c5) : Return c5
                Case NameOf(c6) : Return c6
                Case NameOf(c7) : Return c7
                Case NameOf(c8) : Return c8
                Case NameOf(c9) : Return c9
                Case NameOf(c10) : Return c10
                Case NameOf(c11) : Return c11
                Case NameOf(c12) : Return c12
                Case Else
                    Return GetMaxColors()
            End Select
        End Function

        Private Function GetMaxColors() As String()
            If Not c12.IsNullOrEmpty Then
                Return c12
            ElseIf Not c11.IsNullOrEmpty Then
                Return c11
            ElseIf Not c10.IsNullOrEmpty Then
                Return c10
            ElseIf Not c9.IsNullOrEmpty Then
                Return c9
            ElseIf Not c8.IsNullOrEmpty Then
                Return c8
            ElseIf Not c7.IsNullOrEmpty Then
                Return c7
            ElseIf Not c6.IsNullOrEmpty Then
                Return c6
            ElseIf Not c5.IsNullOrEmpty Then
                Return c5
            ElseIf Not c4.IsNullOrEmpty Then
                Return c4
            Else
                Return c3
            End If
        End Function

        ''' <summary>
        ''' example: ``Accent:c6``
        ''' </summary>
        ''' <param name="term$"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function ParseName(term$) As NamedValue(Of String)
            Return term.GetTagValue(":")
        End Function

#Region "div"

        ''' <summary>
        ''' Diverging schemes put equal emphasis on mid-range critical values and extremes at both ends of the data range. 
        ''' The critical class or break in the middle of the legend is emphasized with light colors and low and high extremes 
        ''' are emphasized with dark colors that have contrasting hues.
        ''' </summary>
        Public NotInheritable Class DivergingSchemes
            Public Const Spectral3$ = "Spectral:c3"
            Public Const Spectral4$ = "Spectral:c4"
            Public Const Spectral5$ = "Spectral:c5"
            Public Const Spectral6$ = "Spectral:c6"
            Public Const Spectral7$ = "Spectral:c7"
            Public Const Spectral8$ = "Spectral:c8"
            Public Const Spectral9$ = "Spectral:c9"
            Public Const Spectral10$ = "Spectral:c10"
            Public Const Spectral11$ = "Spectral:c11"

            Public Const RdYlGn3$ = "RdYlGn:c3"
            Public Const RdYlGn4$ = "RdYlGn:c4"
            Public Const RdYlGn5$ = "RdYlGn:c5"
            Public Const RdYlGn6$ = "RdYlGn:c6"
            Public Const RdYlGn7$ = "RdYlGn:c7"
            Public Const RdYlGn8$ = "RdYlGn:c8"
            Public Const RdYlGn9$ = "RdYlGn:c9"
            Public Const RdYlGn10$ = "RdYlGn:c10"
            Public Const RdYlGn11$ = "RdYlGn:c11"

            Public Const RdBu3$ = "RdBu:c3"
            Public Const RdBu4$ = "RdBu:c4"
            Public Const RdBu5$ = "RdBu:c5"
            Public Const RdBu6$ = "RdBu:c6"
            Public Const RdBu7$ = "RdBu:c7"
            Public Const RdBu8$ = "RdBu:c8"
            Public Const RdBu9$ = "RdBu:c9"
            Public Const RdBu10$ = "RdBu:c10"
            Public Const RdBu11$ = "RdBu:c11"

            Public Const PiYG3$ = "PiYG:c3"
            Public Const PiYG4$ = "PiYG:c4"
            Public Const PiYG5$ = "PiYG:c5"
            Public Const PiYG6$ = "PiYG:c6"
            Public Const PiYG7$ = "PiYG:c7"
            Public Const PiYG8$ = "PiYG:c8"
            Public Const PiYG9$ = "PiYG:c9"
            Public Const PiYG10$ = "PiYG:c10"
            Public Const PiYG11$ = "PiYG:c11"

            Public Const PRGn3$ = "PRGn:c3"
            Public Const PRGn4$ = "PRGn:c4"
            Public Const PRGn5$ = "PRGn:c5"
            Public Const PRGn6$ = "PRGn:c6"
            Public Const PRGn7$ = "PRGn:c7"
            Public Const PRGn8$ = "PRGn:c8"
            Public Const PRGn9$ = "PRGn:c9"
            Public Const PRGn10$ = "PRGn:c10"
            Public Const PRGn11$ = "PRGn:c11"

            Public Const RdYlBu3$ = "RdYlBu:c3"
            Public Const RdYlBu4$ = "RdYlBu:c4"
            Public Const RdYlBu5$ = "RdYlBu:c5"
            Public Const RdYlBu6$ = "RdYlBu:c6"
            Public Const RdYlBu7$ = "RdYlBu:c7"
            Public Const RdYlBu8$ = "RdYlBu:c8"
            Public Const RdYlBu9$ = "RdYlBu:c9"
            Public Const RdYlBu10$ = "RdYlBu:c10"
            Public Const RdYlBu11$ = "RdYlBu:c11"

            Public Const BrBG3$ = "BrBG:c3"
            Public Const BrBG4$ = "BrBG:c4"
            Public Const BrBG5$ = "BrBG:c5"
            Public Const BrBG6$ = "BrBG:c6"
            Public Const BrBG7$ = "BrBG:c7"
            Public Const BrBG8$ = "BrBG:c8"
            Public Const BrBG9$ = "BrBG:c9"
            Public Const BrBG10$ = "BrBG:c10"
            Public Const BrBG11$ = "BrBG:c11"

            Public Const RdGy3$ = "RdGy:c3"
            Public Const RdGy4$ = "RdGy:c4"
            Public Const RdGy5$ = "RdGy:c5"
            Public Const RdGy6$ = "RdGy:c6"
            Public Const RdGy7$ = "RdGy:c7"
            Public Const RdGy8$ = "RdGy:c8"
            Public Const RdGy9$ = "RdGy:c9"
            Public Const RdGy10$ = "RdGy:c10"
            Public Const RdGy11$ = "RdGy:c11"

            Public Const PuOr3$ = "PuOr:c3"
            Public Const PuOr4$ = "PuOr:c4"
            Public Const PuOr5$ = "PuOr:c5"
            Public Const PuOr6$ = "PuOr:c6"
            Public Const PuOr7$ = "PuOr:c7"
            Public Const PuOr8$ = "PuOr:c8"
            Public Const PuOr9$ = "PuOr:c9"
            Public Const PuOr10$ = "PuOr:c10"
            Public Const PuOr11$ = "PuOr:c11"

            Private Sub New()
            End Sub
        End Class

        ''' <summary>
        ''' div
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Spectral As Color()
            Get
                Return Designer.GetColors("Spectral:c11")
            End Get
        End Property

        ''' <summary>
        ''' div
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property RdYlGn As Color()
            Get
                Return Designer.GetColors("RdYlGn:c11")
            End Get
        End Property

        ''' <summary>
        ''' div
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property RdBu As Color()
            Get
                Return Designer.GetColors("RdBu:c11")
            End Get
        End Property

        ''' <summary>
        ''' div
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property PiYG As Color()
            Get
                Return Designer.GetColors("PiYG:c11")
            End Get
        End Property

        ''' <summary>
        ''' div
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property PRGn As Color()
            Get
                Return Designer.GetColors("PRGn:c11")
            End Get
        End Property

        ''' <summary>
        ''' div
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property RdYlBu As Color()
            Get
                Return Designer.GetColors("RdYlBu:c11")
            End Get
        End Property

        ''' <summary>
        ''' div
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property BrBG As Color()
            Get
                Return Designer.GetColors("BrBG:c11")
            End Get
        End Property

        ''' <summary>
        ''' div
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property RdGy As Color()
            Get
                Return Designer.GetColors("RdGy:c11")
            End Get
        End Property

        ''' <summary>
        ''' div
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property PuOr As Color()
            Get
                Return Designer.GetColors("PuOr:c11")
            End Get
        End Property
#End Region

#Region "qual"

        ''' <summary>
        ''' Qualitative schemes do not imply magnitude differences between legend classes, and hues are used to create the 
        ''' primary visual differences between classes. Qualitative schemes are best suited to representing nominal or 
        ''' categorical data. 
        ''' </summary>
        Public NotInheritable Class QualitativeSchemes

            Public Const Set2_3$ = "Set2:c3"
            Public Const Set2_4$ = "Set2:c4"
            Public Const Set2_5$ = "Set2:c5"
            Public Const Set2_6$ = "Set2:c6"
            Public Const Set2_7$ = "Set2:c7"
            Public Const Set2_8$ = "Set2:c8"

            Public Const Accent3$ = "Accent:c3"
            Public Const Accent4$ = "Accent:c4"
            Public Const Accent5$ = "Accent:c5"
            Public Const Accent6$ = "Accent:c6"
            Public Const Accent7$ = "Accent:c7"
            Public Const Accent8$ = "Accent:c8"

            Public Const Set1_3$ = "Set1:c3"
            Public Const Set1_4$ = "Set1:c4"
            Public Const Set1_5$ = "Set1:c5"
            Public Const Set1_6$ = "Set1:c6"
            Public Const Set1_7$ = "Set1:c7"
            Public Const Set1_8$ = "Set1:c8"
            Public Const Set1_9$ = "Set1:c9"

            Public Const Set3_3$ = "Set3:c3"
            Public Const Set3_4$ = "Set3:c4"
            Public Const Set3_5$ = "Set3:c5"
            Public Const Set3_6$ = "Set3:c6"
            Public Const Set3_7$ = "Set3:c7"
            Public Const Set3_8$ = "Set3:c8"
            Public Const Set3_9$ = "Set3:c9"
            Public Const Set3_10$ = "Set3:c10"
            Public Const Set3_11$ = "Set3:c11"
            Public Const Set3_12$ = "Set3:c12"

            Public Const Dark2_3$ = "Dark2:c3"
            Public Const Dark2_4$ = "Dark2:c4"
            Public Const Dark2_5$ = "Dark2:c5"
            Public Const Dark2_6$ = "Dark2:c6"
            Public Const Dark2_7$ = "Dark2:c7"
            Public Const Dark2_8$ = "Dark2:c8"

            Public Const Paired3$ = "Paired:c3"
            Public Const Paired4$ = "Paired:c4"
            Public Const Paired5$ = "Paired:c5"
            Public Const Paired6$ = "Paired:c6"
            Public Const Paired7$ = "Paired:c7"
            Public Const Paired8$ = "Paired:c8"
            Public Const Paired9$ = "Paired:c9"
            Public Const Paired10$ = "Paired:c10"
            Public Const Paired11$ = "Paired:c11"
            Public Const Paired12$ = "Paired:c12"

            Public Const Pastel2_3$ = "Pastel2:c3"
            Public Const Pastel2_4$ = "Pastel2:c4"
            Public Const Pastel2_5$ = "Pastel2:c5"
            Public Const Pastel2_6$ = "Pastel2:c6"
            Public Const Pastel2_7$ = "Pastel2:c7"
            Public Const Pastel2_8$ = "Pastel2:c8"

            Public Const Pastel1_3$ = "Pastel1:c3"
            Public Const Pastel1_4$ = "Pastel1:c4"
            Public Const Pastel1_5$ = "Pastel1:c5"
            Public Const Pastel1_6$ = "Pastel1:c6"
            Public Const Pastel1_7$ = "Pastel1:c7"
            Public Const Pastel1_8$ = "Pastel1:c8"
            Public Const Pastel1_9$ = "Pastel1:c9"

            Private Sub New()
            End Sub
        End Class

        ''' <summary>
        ''' qual
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Set2 As Color()
            Get
                Return Designer.GetColors("Set2:c8")
            End Get
        End Property

        ''' <summary>
        ''' qual
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Accent As Color()
            Get
                Return Designer.GetColors("Accent:c8")
            End Get
        End Property

        ''' <summary>
        ''' qual
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Set1 As Color()
            Get
                Return Designer.GetColors("Set1:c9")
            End Get
        End Property

        ''' <summary>
        ''' qual
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Set3 As Color()
            Get
                Return Designer.GetColors("Set3:c12")
            End Get
        End Property

        ''' <summary>
        ''' qual
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Dark2 As Color()
            Get
                Return Designer.GetColors("Dark2:c8")
            End Get
        End Property

        ''' <summary>
        ''' qual
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Paired As Color()
            Get
                Return Designer.GetColors("Paired:c12")
            End Get
        End Property

        ''' <summary>
        ''' qual
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Pastel2 As Color()
            Get
                Return Designer.GetColors("Pastel2:c8")
            End Get
        End Property

        ''' <summary>
        ''' qual
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Pastel1 As Color()
            Get
                Return Designer.GetColors("Pastel1:c9")
            End Get
        End Property
#End Region

#Region "seq"

        ''' <summary>
        ''' Sequential schemes are suited to ordered data that progress from low to high. Lightness steps dominate the 
        ''' look of these schemes, with light colors for low data values to dark colors for high data values. 
        ''' </summary>
        Public NotInheritable Class SequentialSchemes

            Public Const OrRd3$ = "OrRd:c3"
            Public Const OrRd4$ = "OrRd:c4"
            Public Const OrRd5$ = "OrRd:c5"
            Public Const OrRd6$ = "OrRd:c6"
            Public Const OrRd7$ = "OrRd:c7"
            Public Const OrRd8$ = "OrRd:c8"
            Public Const OrRd9$ = "OrRd:c9"

            Public Const PuBu3$ = "PuBu:c3"
            Public Const PuBu4$ = "PuBu:c4"
            Public Const PuBu5$ = "PuBu:c5"
            Public Const PuBu6$ = "PuBu:c6"
            Public Const PuBu7$ = "PuBu:c7"
            Public Const PuBu8$ = "PuBu:c8"
            Public Const PuBu9$ = "PuBu:c9"

            Public Const BuPu3$ = "BuPu:c3"
            Public Const BuPu4$ = "BuPu:c4"
            Public Const BuPu5$ = "BuPu:c5"
            Public Const BuPu6$ = "BuPu:c6"
            Public Const BuPu7$ = "BuPu:c7"
            Public Const BuPu8$ = "BuPu:c8"
            Public Const BuPu9$ = "BuPu:c9"

            Public Const Oranges3$ = "Oranges:c3"
            Public Const Oranges4$ = "Oranges:c4"
            Public Const Oranges5$ = "Oranges:c5"
            Public Const Oranges6$ = "Oranges:c6"
            Public Const Oranges7$ = "Oranges:c7"
            Public Const Oranges8$ = "Oranges:c8"
            Public Const Oranges9$ = "Oranges:c9"

            Public Const BuGn3$ = "BuGn:c3"
            Public Const BuGn4$ = "BuGn:c4"
            Public Const BuGn5$ = "BuGn:c5"
            Public Const BuGn6$ = "BuGn:c6"
            Public Const BuGn7$ = "BuGn:c7"
            Public Const BuGn8$ = "BuGn:c8"
            Public Const BuGn9$ = "BuGn:c9"

            Public Const YlOrBr3$ = "YlOrBr:c3"
            Public Const YlOrBr4$ = "YlOrBr:c4"
            Public Const YlOrBr5$ = "YlOrBr:c5"
            Public Const YlOrBr6$ = "YlOrBr:c6"
            Public Const YlOrBr7$ = "YlOrBr:c7"
            Public Const YlOrBr8$ = "YlOrBr:c8"
            Public Const YlOrBr9$ = "YlOrBr:c9"

            Public Const YlGn3$ = "YlGn:c3"
            Public Const YlGn4$ = "YlGn:c4"
            Public Const YlGn5$ = "YlGn:c5"
            Public Const YlGn6$ = "YlGn:c6"
            Public Const YlGn7$ = "YlGn:c7"
            Public Const YlGn8$ = "YlGn:c8"
            Public Const YlGn9$ = "YlGn:c9"

            Public Const Reds3$ = "Reds:c3"
            Public Const Reds4$ = "Reds:c4"
            Public Const Reds5$ = "Reds:c5"
            Public Const Reds6$ = "Reds:c6"
            Public Const Reds7$ = "Reds:c7"
            Public Const Reds8$ = "Reds:c8"
            Public Const Reds9$ = "Reds:c9"

            Public Const RdPu3$ = "RdPu:c3"
            Public Const RdPu4$ = "RdPu:c4"
            Public Const RdPu5$ = "RdPu:c5"
            Public Const RdPu6$ = "RdPu:c6"
            Public Const RdPu7$ = "RdPu:c7"
            Public Const RdPu8$ = "RdPu:c8"
            Public Const RdPu9$ = "RdPu:c9"

            Public Const Greens3$ = "Greens:c3"
            Public Const Greens4$ = "Greens:c4"
            Public Const Greens5$ = "Greens:c5"
            Public Const Greens6$ = "Greens:c6"
            Public Const Greens7$ = "Greens:c7"
            Public Const Greens8$ = "Greens:c8"
            Public Const Greens9$ = "Greens:c9"

            Public Const YlGnBu3$ = "YlGnBu:c3"
            Public Const YlGnBu4$ = "YlGnBu:c4"
            Public Const YlGnBu5$ = "YlGnBu:c5"
            Public Const YlGnBu6$ = "YlGnBu:c6"
            Public Const YlGnBu7$ = "YlGnBu:c7"
            Public Const YlGnBu8$ = "YlGnBu:c8"
            Public Const YlGnBu9$ = "YlGnBu:c9"

            Public Const Purples3$ = "Purples:c3"
            Public Const Purples4$ = "Purples:c4"
            Public Const Purples5$ = "Purples:c5"
            Public Const Purples6$ = "Purples:c6"
            Public Const Purples7$ = "Purples:c7"
            Public Const Purples8$ = "Purples:c8"
            Public Const Purples9$ = "Purples:c9"

            Public Const GnBu3$ = "GnBu:c3"
            Public Const GnBu4$ = "GnBu:c4"
            Public Const GnBu5$ = "GnBu:c5"
            Public Const GnBu6$ = "GnBu:c6"
            Public Const GnBu7$ = "GnBu:c7"
            Public Const GnBu8$ = "GnBu:c8"
            Public Const GnBu9$ = "GnBu:c9"

            Public Const Greys3$ = "Greys:c3"
            Public Const Greys4$ = "Greys:c4"
            Public Const Greys5$ = "Greys:c5"
            Public Const Greys6$ = "Greys:c6"
            Public Const Greys7$ = "Greys:c7"
            Public Const Greys8$ = "Greys:c8"
            Public Const Greys9$ = "Greys:c9"

            Public Const YlOrRd3$ = "YlOrRd:c3"
            Public Const YlOrRd4$ = "YlOrRd:c4"
            Public Const YlOrRd5$ = "YlOrRd:c5"
            Public Const YlOrRd6$ = "YlOrRd:c6"
            Public Const YlOrRd7$ = "YlOrRd:c7"
            Public Const YlOrRd8$ = "YlOrRd:c8"

            Public Const PuRd3$ = "PuRd:c3"
            Public Const PuRd4$ = "PuRd:c4"
            Public Const PuRd5$ = "PuRd:c5"
            Public Const PuRd6$ = "PuRd:c6"
            Public Const PuRd7$ = "PuRd:c7"
            Public Const PuRd8$ = "PuRd:c8"
            Public Const PuRd9$ = "PuRd:c9"

            Public Const Blues3$ = "Blues:c3"
            Public Const Blues4$ = "Blues:c4"
            Public Const Blues5$ = "Blues:c5"
            Public Const Blues6$ = "Blues:c6"
            Public Const Blues7$ = "Blues:c7"
            Public Const Blues8$ = "Blues:c8"
            Public Const Blues9$ = "Blues:c9"

            Public Const PuBuGn3$ = "PuBuGn:c3"
            Public Const PuBuGn4$ = "PuBuGn:c4"
            Public Const PuBuGn5$ = "PuBuGn:c5"
            Public Const PuBuGn6$ = "PuBuGn:c6"
            Public Const PuBuGn7$ = "PuBuGn:c7"
            Public Const PuBuGn8$ = "PuBuGn:c8"
            Public Const PuBuGn9$ = "PuBuGn:c9"

            Private Sub New()
            End Sub
        End Class

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property OrRd As Color()
            Get
                Return Designer.GetColors("OrRd:c9")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property PuBu As Color()
            Get
                Return Designer.GetColors("PuBu:c9")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property BuPu As Color()
            Get
                Return Designer.GetColors("BuPu:c9")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Oranges As Color()
            Get
                Return Designer.GetColors("Oranges:c9")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property BuGn As Color()
            Get
                Return Designer.GetColors("BuGn:c9")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property YlOrBr As Color()
            Get
                Return Designer.GetColors("YlOrBr:c9")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property YlGn As Color()
            Get
                Return Designer.GetColors("YlGn:c9")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Reds As Color()
            Get
                Return Designer.GetColors("Reds:c9")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property RdPu As Color()
            Get
                Return Designer.GetColors("RdPu:c9")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Greens As Color()
            Get
                Return Designer.GetColors("Greens:c9")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property YlGnBu As Color()
            Get
                Return Designer.GetColors("YlGnBu:c9")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Purples As Color()
            Get
                Return Designer.GetColors("Purples:c9")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property GnBu As Color()
            Get
                Return Designer.GetColors("GnBu:c9")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Greys As Color()
            Get
                Return Designer.GetColors("Greys:c9")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property YlOrRd As Color()
            Get
                Return Designer.GetColors("YlOrRd:c8")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property PuRd As Color()
            Get
                Return Designer.GetColors("PuRd:c9")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property Blues As Color()
            Get
                Return Designer.GetColors("Blues:c9")
            End Get
        End Property

        ''' <summary>
        ''' seq
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property PuBuGn As Color()
            Get
                Return Designer.GetColors("PuBuGn:c9")
            End Get
        End Property
#End Region

    End Structure
End Namespace
