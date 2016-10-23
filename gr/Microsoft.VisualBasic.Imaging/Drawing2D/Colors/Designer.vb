#Region "Microsoft.VisualBasic::cec4f1ce41344e17c0b8b65c3d8ded7c, ..\visualbasic_App\gr\Microsoft.VisualBasic.Imaging\Drawing2D\Colors\Designer.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region


Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Linq
Imports System.Text.RegularExpressions
Imports System.Text

Namespace Drawing2D.Colors

    Public Module Designer

        '''<summary>
        ''' {  
        '''  &quot;Color [PapayaWhip]&quot;: [
        '''    {
        '''      &quot;knownColor&quot;: 93,
        '''      &quot;name&quot;: null,
        '''      &quot;state&quot;: 1,
        '''      &quot;value&quot;: 0
        '''    },
        '''    {
        '''      &quot;knownColor&quot;: 119,
        '''      &quot;name&quot;: null,
        '''      &quot;state&quot;: 1,
        '''      &quot;value&quot;: 0
        '''    },
        '''    {
        '''      &quot;knownColor&quot;: 30,
        '''      &quot;name&quot;: null,
        '''      &quot;state&quot;: 1,
        '''      &quot;value&quot;: 0
        '''    },
        '''    {
        '''      &quot;knownColor&quot;: 165,
        '''      &quot;name&quot;: null,
        '''      &quot;state&quot;: 1,
        '''      &quot;value&quot;: 0
        '''    },
        '''    {
        '''      &quot;knownColor&quot;: 81,
        '''      &quot;name&quot;: null,
        '''      &quot;state&quot;: 1,
        '''      &quot;value&quot;: [rest of string was truncated]&quot;;.
        '''</summary>
        Public ReadOnly AvailableInterpolates As IReadOnlyDictionary(Of Color, Color())
        Public ReadOnly ColorBrewer As Dictionary(Of String, ColorBrewer)

        Sub New()
            Dim colors = My.Resources _
                .designer_colors _
                .LoadObject(Of Dictionary(Of String, String()))
            Dim valids As New Dictionary(Of Color, Color())

            For Each x In colors
                valids(ColorTranslator.FromHtml(x.Key)) =
                    x.Value.ToArray(AddressOf ColorTranslator.FromHtml)
            Next

            AvailableInterpolates = valids

            Dim ns = Regex.Matches(My.Resources.colorbrewer, """\d+""") _
                .ToArray(Function(m) m.Trim(""""c))
            Dim sb As New StringBuilder(My.Resources.colorbrewer)

            For Each n In ns.Distinct
                Call sb.Replace($"""{n}""", $"""c{n}""")
            Next

            ColorBrewer = sb.ToString _
                .LoadObject(Of Dictionary(Of String, ColorBrewer))
        End Sub

        ''' <summary>
        ''' Some useful color tables for images and tools to handle them.
        ''' Several color scales useful for image plots: a pleasing rainbow style color table patterned after 
        ''' that used in Matlab by Tim Hoar and also some simple color interpolation schemes between two or 
        ''' more colors. There is also a function that converts between colors and a real valued vector.
        ''' </summary>
        ''' <param name="col">A list of colors (names or hex values) to interpolate</param>
        ''' <param name="n%">Number of color levels. The setting n=64 is the orignal definition.</param>
        ''' <param name="alpha%">The transparency of the color – 255 is opaque and 0 is transparent. This is useful for overlays of color and still being able to view the graphics that is covered.</param>
        ''' <returns>A vector giving the colors in a hexadecimal format, two extra hex digits are added for the alpha channel.</returns>
        Public Function Colors(col As Color(), Optional n% = 256, Optional alpha% = 255) As Color()
            Dim out As New List(Of Color)
            Dim steps! = n / col.Length
            Dim previous As Value(Of Color) = col.First

            For Each c As Color In col.Skip(1)
                out += ColorCube.GetColorSequence(
                    source:=previous,
                    target:=previous = c,
                    increment:=steps!,
                    alpha:=alpha%)
            Next

            Return out
        End Function
    End Module
End Namespace
