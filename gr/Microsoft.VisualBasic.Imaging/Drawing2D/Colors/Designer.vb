#Region "Microsoft.VisualBasic::55ccb9e661e6c44992394360c6fa21ce, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing2D\Colors\Designer.vb"

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
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical.Interpolation
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Drawing2D.Colors

    ''' <summary>
    ''' Generate color sequence
    ''' </summary>
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
        Public ReadOnly Property AvailableInterpolates As IReadOnlyDictionary(Of Color, Color())
        Public ReadOnly Property ColorBrewer As Dictionary(Of String, ColorBrewer)
        Public ReadOnly Property Rainbow As Color() = {
            Color.Red,
            Color.Orange,
            Color.Yellow,
            Color.Green,
            Color.Lime,
            Color.Blue,
            Color.Violet
        }

        ''' <summary>
        ''' 10 category colors for the data object cluster result
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ClusterColour As Color() = {
            Color.FromArgb(128, 200, 180),
            Color.FromArgb(135, 70, 194),
            Color.FromArgb(140, 210, 90),
            Color.FromArgb(200, 80, 147),
            Color.FromArgb(201, 169, 79),
            Color.FromArgb(112, 127, 189),
            Color.FromArgb(192, 82, 58),
            Color.FromArgb(83, 99, 60),
            Color.FromArgb(78, 45, 69),
            Color.FromArgb(202, 161, 169)
        }

        ''' <summary>
        ''' From TSF launcher on Android
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property TSF As Color() = {
            {247, 69, 58},
            {230, 28, 99},
            {156, 36, 173},
            {107, 57, 181},
            {66, 81, 181},
            {33, 150, 238},
            {8, 170, 247},
            {0, 190, 214},
            {0, 150, 132},
            {74, 174, 82},
            {132, 194, 74},
            {206, 223, 58},
            {255, 235, 58},
            {255, 190, 0},
            {255, 150, 0},
            {255, 85, 33},
            {115, 85, 66},
            {156, 158, 156},
            {99, 125, 140}
        }.RowIterator _
         .Select(Function(c)
                     Return Color.FromArgb(c(0), c(1), c(2))
                 End Function) _
         .ToArray

        Sub New()
            Dim colors As Dictionary(Of String, String()) = My.Resources _
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

        ReadOnly __allColorMapNames$() = {
            ColorMap.PatternAutumn,
            ColorMap.PatternCool,
            ColorMap.PatternGray,
            ColorMap.PatternHot,
            ColorMap.PatternJet,
            ColorMap.PatternSpring,
            ColorMap.PatternSummer,
            ColorMap.PatternWinter
        }.ToArray(AddressOf LCase)

        ''' <summary>
        ''' Google material design colors
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property MaterialPalette As Color() = {
            Color.Red, Color.Pink, Color.Black, Color.Purple, Color.DarkViolet,
            Color.Indigo, Color.Blue, Color.LightBlue, Color.Cyan, Color.Teal,
            Color.Green, Color.LightGreen, Color.Lime, Color.Yellow, Color.Orchid,
            Color.Orange, Color.DarkOrange, Color.Brown, Color.Gray, Color.CadetBlue
        }

        <Extension>
        Private Function IsColorNameList(exp$) As Boolean
            If InStr(exp, ",") > 0 Then
                If exp.IsPattern("rgb\(\d+\s*(,\s*\d+\s*)+\)") Then
                    Return False
                Else
                    Return True
                End If
            Else
                Return False
            End If
        End Function

        ''' <summary>
        ''' 对于无效的键名称，默认是返回<see cref="Office2016"/>，请注意，如果是所有的.net的颜色的话，这里面还会包含有白色，所以还需要手工去除掉白色
        ''' </summary>
        ''' <param name="term$">假若这里所输入的是一组颜色值，则必须是htmlcolor或者颜色名称，RGB表达式将不会被允许</param>
        ''' <returns></returns>
        Public Function GetColors(term$) As Color()
            If term.IsColorNameList Then
                Return term _
                    .StringSplit(",\s*") _
                    .Select(Function(c) c.TranslateColor) _
                    .ToArray
            End If
            If Array.IndexOf(__allColorMapNames, term.ToLower) > -1 Then
                Return New ColorMap(20, 255).ColorSequence(term)
            End If

            Dim key As NamedValue(Of String) =
                Drawing2D.Colors.ColorBrewer.ParseName(term)

            If ColorBrewer.ContainsKey(key.Name) Then
                Return ColorBrewer(key.Name).GetColors(key.Value)
            End If

            If term.TextEquals("material") Then
                Return MaterialPalette
            ElseIf term.TextEquals("TSF") Then
                Return TSF
            ElseIf term.TextEquals("rainbow") Then
                Return Rainbow
            ElseIf term.TextEquals("dotnet.colors") Then
                Return AllDotNetPrefixColors
            ElseIf term.TextEquals("vb.chart") Then
                Return ChartColors
            ElseIf term.TextEquals("clusters") Then
                Return ClusterColour
            End If

            ' d3.js colors
            If term.TextEquals("d3.scale.category10()") Then
                Return d3js.category10
            ElseIf term.TextEquals("d3.scale.category20()") Then
                Return d3js.category20
            ElseIf term.TextEquals("d3.scale.category20b()") Then
                Return d3js.category20b
            ElseIf term.TextEquals("d3.scale.category20c()") Then
                Return d3js.category20c
            End If

            Return OfficeColorThemes.GetAccentColors(term)
        End Function

        ''' <summary>
        ''' 这个函数是获取得到一个连续的颜色谱
        ''' </summary>
        ''' <param name="term$"></param>
        ''' <param name="n%"></param>
        ''' <param name="alpha%"></param>
        ''' <returns></returns>
        Public Function GetColors(term$, Optional n% = 256, Optional alpha% = 255) As Color()
            Return CubicSpline(GetColors(term), n, alpha)
        End Function

        ''' <summary>
        ''' ``New <see cref="SolidBrush"/>(<see cref="GetColors(String, Integer, Integer)"/>)``
        ''' </summary>
        ''' <param name="term$"></param>
        ''' <param name="n%"></param>
        ''' <param name="alpha%"></param>
        ''' <returns></returns>
        Public Function GetBrushes(term$, Optional n% = 256, Optional alpha% = 255) As SolidBrush()
            Return GetColors(term, n, alpha).ToArray(Function(c) New SolidBrush(c))
        End Function

        ''' <summary>
        ''' 相对于<see cref="GetColors"/>函数而言，这个函数是返回非连续的颜色谱，假若数量不足，会重新使用开头的起始颜色连续填充
        ''' </summary>
        ''' <param name="colors$"></param>
        ''' <param name="n%"></param>
        ''' <returns></returns>
        <Extension> Public Function FromNames(colors$(), n%) As Color()
            Return colors.Select(AddressOf ToColor).__internalFills(n)
        End Function

        <Extension>
        Private Function __internalFills(colors As IEnumerable(Of Color), n As Integer) As Color()
            Dim out As New List(Of Color)(colors)
            Dim i As Integer = Scan0

            Do While out.Count < n
                out.Add(out(i))
                i += 1
            Loop

            Return out.ToArray
        End Function

        ''' <summary>
        ''' <see cref="FromSchema"/>和<see cref="FromNames"/>适用于函数绘图之类需要区分数据系列的颜色谱的生成
        ''' </summary>
        ''' <param name="term$"></param>
        ''' <param name="n%"></param>
        ''' <returns></returns>
        Public Function FromSchema(term$, n%) As Color()
            Return GetColors(term).__internalFills(n)
        End Function

        ''' <summary>
        ''' **<see cref="ColorCube.GetColorSequence"/>**
        ''' 
        ''' Some useful color tables for images and tools to handle them.
        ''' Several color scales useful for image plots: a pleasing rainbow style color table patterned after 
        ''' that used in Matlab by Tim Hoar and also some simple color interpolation schemes between two or 
        ''' more colors. There is also a function that converts between colors and a real valued vector.
        ''' </summary>
        ''' <param name="col">A list of colors (names or hex values) to interpolate</param>
        ''' <param name="n%">Number of color levels. The setting n=64 is the orignal definition.</param>
        ''' <param name="alpha%">
        ''' The transparency of the color – 255 is opaque and 0 is transparent. This is useful for 
        ''' overlays of color and still being able to view the graphics that is covered.
        ''' </param>
        ''' <returns>
        ''' A vector giving the colors in a hexadecimal format, two extra hex digits are added for the alpha channel.
        ''' </returns>
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

        ''' <summary>
        ''' 这个函数并不会计算alpha的值
        ''' </summary>
        ''' <param name="colors"></param>
        ''' <param name="n">所期望的颜色的数量</param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function CubicSpline(colors As IEnumerable(Of Color), Optional n% = 256, Optional alpha% = 255) As Color()
            Dim source As Color() = colors.ToArray
            Dim x As New CubicSplineVector(source.Select(Function(c) CSng(c.R)))
            Dim y As New CubicSplineVector(source.Select(Function(c) CSng(c.G)))
            Dim z As New CubicSplineVector(source.Select(Function(c) CSng(c.B)))

            Call x.CalcSpline()
            Call y.CalcSpline()
            Call z.CalcSpline()

            Dim delta! = 1 / n
            Dim out As New List(Of Color)

            For f! = 0 To 1.0! Step delta!
                Dim r% = __constraint(x.GetPoint(f))
                Dim g% = __constraint(y.GetPoint(f))
                Dim b% = __constraint(z.GetPoint(f))

                out += Color.FromArgb(alpha, r, g, b)
            Next

            Return out
        End Function

        Private Function __constraint(x!) As Integer
            If x < 0! Then
                x = 0!
            ElseIf x > 255.0! Then
                x = 255.0!
            End If

            Return x
        End Function
    End Module
End Namespace
