#Region "Microsoft.VisualBasic::94f9fecaebfd99c81915e79b2634ee36, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Colors\DesignerTerms.vb"

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

    '   Total Lines: 135
    '    Code Lines: 85
    ' Comment Lines: 25
    '   Blank Lines: 25
    '     File Size: 5.50 KB


    '     Module DesignerTerms
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Drawing2D.Colors

    ''' <summary>
    ''' Color name terms
    ''' </summary>
    Public Module DesignerTerms

        ''' <summary>
        ''' 主要是应用于命令行帮助的字符串信息
        ''' </summary>
        Public Const TermHelpInfo$ = "

Color name terms that you could used for data charting plot:

   `material`               Includes 20 typical colors that used in Google Android material design.
   `console.colors`         Includes 16 colors from Windows command terminal.
   `TSF`                    Includes 19 colors that defines in the TSFShell launcher on Android.
   `rainbow`                Includes 7 rainbow colors.
   `dotnet.colors`          All of the named colors in .NET framework.
   `scibasic.chart()`       Chartting colors in sciBASIC.NET framework.
   `scibasic.category31()`  Includes 31 colors that used for category data.
   `Clusters`               Includes 10 colors that used for category data.

All of the color name terms that in d3.js library is also available at here:

   `d3.scale.category10()`
   `d3.scale.category20()`
   `d3.scale.category20b()`
   `d3.scale.category20c()`

Color schema name terms that comes from the ColorBrewer system:

All of the color schema in ColorBrewer system have several levels, which could be use expression 
pattern such as 'schema_name:c[level]' for get colors from the color designer, examples as 
'YlGnBu:c6' will generate a color sequence which have 6 colors that comes from the ``YlGnBu`` 
pattern.

1. Sequential schemes are suited to ordered data that progress from low to high. Lightness steps 
   dominate the look of these schemes, with light colors for low data values to dark colors for 
   high data values. 

   All of the colors terms in Sequential schemes have levels from 3 to 9, schema name terms 
   includes:

   OrRd:c[3,9], PuBu:c[3,9], BuPu:c[3,9], Oranges:c[3,9], BuGn:c[3,9], YlOrBr:c[3,9]
   YlGn:c[3,9], Reds:c[3,9], RdPu:c[3,9], Greens:c[3,9], YlGnBu:c[3,9], Purples:c[3,9]
   GnBu:c[3,9], Greys:c[3,9], YlOrRd:c[3,9], PuRd:c[3,9], Blues:c[3,9], PuBuGn:c[3,9]

2. Qualitative schemes do not imply magnitude differences between legend classes, and hues are used 
   to create the primary visual differences between classes. Qualitative schemes are best suited to 
   representing nominal or categorical data. 

   The color levels in this schema range from 3 to 12, schema name terms includes:

   Set2:c[3,8], Accent:c[3,8], Set1:c[3,9], Set3:c[3,12], Dark2:c[3,8], Paired:c[3,12]
   Pastel2:c[3,8], Pastel1:c[3,9]

3. Diverging schemes put equal emphasis on mid-range critical values and extremes at both ends of 
   the data range. The critical class or break in the middle of the legend is emphasized with light 
   colors and low and high extremes are emphasized with dark colors that have contrasting hues.
   
   All of the colors terms in Sequential schemes have levels from 3 to 11, schema name terms 
   includes:

   Spectral:c[3,11], RdYlGn:c[3,11], RdBu:c[3,11], PiYG:c[3,11], PRGn:c[3,11], RdYlBu:c[3,11]
   BrBG:c[3,11], RdGy:c[3,11], PuOr:c[3,11]

You also can choose color patterns that used for gradient data visualization:

   `Autumn`  ColorMap.PatternAutumn
   `Cool`    ColorMap.PatternCool
   `Gray`    ColorMap.PatternGray
   `Hot`     ColorMap.PatternHot
   `Jet`     ColorMap.PatternJet
   `Spring`  ColorMap.PatternSpring
   `Summer`  ColorMap.PatternSummer
   `Winter`  ColorMap.PatternWinter

Also includes some color schema that used in Microsoft Office:

   `Office2016`   The default color schema that used in Office2016
   `Office2010`   The default color schema that used in Office2010
   `Slipstream`
   `Marquee`
   `Aspect`
   `Paper`
"

#Region "Color name terms"
        ''' <summary>
        ''' 20种Android MD设计中使用的颜色
        ''' </summary>
        Public Const GoogleMaterialPalette$ = "material"
        ''' <summary>
        ''' 16种Windows的命令行配色
        ''' </summary>
        Public Const ConsoleColors$ = "console.colors"
        ''' <summary>
        ''' 19中分类颜色
        ''' </summary>
        Public Const TSFShellColors$ = "TSF"
        ''' <summary>
        ''' 总共7种彩虹色
        ''' </summary>
        Public Const Rainbow$ = "rainbow"
        Public Const AllDotnetColors$ = "dotnet.colors"
        Public Const sciBASICChartColors$ = "scibasic.chart()"
        ''' <summary>
        ''' 31种分类颜色
        ''' </summary>
        Public Const sciBASICCategory31$ = "scibasic.category31()"

        ''' <summary>
        ''' 总共10种分类颜色
        ''' </summary>
        Public Const ClusterCategory10$ = Designer.Clusters
#End Region

#Region "d3.js category colors"
        Public Const d3ScaleCategory10$ = "d3.scale.category10()"
        Public Const d3ScaleCategory20$ = "d3.scale.category20()"
        Public Const d3ScaleCategory20b$ = "d3.scale.category20b()"
        Public Const d3ScaleCategory20c$ = "d3.scale.category20c()"
#End Region

#Region "Office colors"
        Public Const Office2016$ = NameOf(Office2016)
        Public Const Office2010$ = NameOf(Office2010)
        Public Const Slipstream$ = NameOf(Slipstream)
        Public Const Marquee$ = NameOf(Marquee)
        Public Const Aspect$ = NameOf(Aspect)
        Public Const Paper$ = NameOf(Paper)
#End Region
    End Module
End Namespace
