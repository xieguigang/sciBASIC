#Region "Microsoft.VisualBasic::b1aa91146191baad3eb2d5ef3761baf8, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Colors\Palette.vb"

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

    '   Total Lines: 104
    '    Code Lines: 31
    ' Comment Lines: 66
    '   Blank Lines: 7
    '     File Size: 3.43 KB


    '     Enum CategoryPalettes
    ' 
    '         Aspect, Clusters, Marquee, material, NA
    '         Office2010, Office2016, Paper, rainbow, Slipstream
    '         TSF
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel

Namespace Drawing2D.Colors

    ''' <summary>
    ''' color palette for category data.
    ''' </summary>
    Public Enum CategoryPalettes

        NA

        ''' <summary>
        ''' Includes 20 typical colors that used in Google Android material design.
        ''' </summary>
        material
        ''' <summary>
        ''' Includes 16 colors from Windows command terminal.
        ''' </summary>
        <Description("console.colors")> ConsoleColors
        ''' <summary>
        ''' Includes 19 colors that defines in the TSFShell launcher on Android.
        ''' </summary>
        TSF
        ''' <summary>
        ''' Includes 7 rainbow colors.
        ''' </summary>
        rainbow
        ''' <summary>
        ''' Chartting colors in sciBASIC.NET framework.
        ''' </summary>
        <Description("scibasic.chart()")> sciBasicChart
        ''' <summary>
        ''' Includes 31 colors that used for category data.
        ''' </summary>
        <Description("scibasic.category31()")> sciBasicCategory31
        ''' <summary>
        ''' Includes 10 colors that used for category data.
        ''' </summary>
        Clusters

        <Description("d3.scale.category10()")> d3ScaleCategory10
        <Description("d3.scale.category20()")> d3ScaleCategory20
        <Description("d3.scale.category20b()")> d3ScaleCategory20b
        <Description("d3.scale.category20c()")> d3ScaleCategory20c

        ''' <summary>
        ''' ColorBrewer Qualitative schemes
        ''' </summary>
        <Description("Set2:c8")> ColorBrewerSet2
        ''' <summary>
        ''' ColorBrewer Qualitative schemes
        ''' </summary>
        <Description("Accent:c8")> ColorBrewerAccent
        ''' <summary>
        ''' ColorBrewer Qualitative schemes
        ''' </summary>
        <Description("Set1:c8")> ColorBrewerSet1
        ''' <summary>
        ''' ColorBrewer Qualitative schemes
        ''' </summary>
        <Description("Set3:c8")> ColorBrewerSet3
        ''' <summary>
        ''' ColorBrewer Qualitative schemes
        ''' </summary>
        <Description("Dark2:c8")> ColorBrewerDark2
        ''' <summary>
        ''' ColorBrewer Qualitative schemes
        ''' </summary>
        <Description("Paired:c12")> ColorBrewerPaired
        ''' <summary>
        ''' ColorBrewer Qualitative schemes
        ''' </summary>
        <Description("Pastel2:c8")> ColorBrewerPastel2
        ''' <summary>
        ''' ColorBrewer Qualitative schemes
        ''' </summary>
        <Description("Pastel1:c9")> ColorBrewerPastel1

        ''' <summary>
        ''' Microsoft Office Color Accents: The default color schema that used in Office2016
        ''' </summary>
        Office2016
        ''' <summary>
        ''' Microsoft Office Color Accents: The default color schema that used in Office2010
        ''' </summary>
        Office2010
        ''' <summary>
        ''' Microsoft Office Color Accents
        ''' </summary>
        Slipstream
        ''' <summary>
        ''' Microsoft Office Color Accents
        ''' </summary>
        Marquee
        ''' <summary>
        ''' Microsoft Office Color Accents
        ''' </summary>
        Aspect
        ''' <summary>
        ''' Microsoft Office Color Accents
        ''' </summary>
        Paper
    End Enum
End Namespace
