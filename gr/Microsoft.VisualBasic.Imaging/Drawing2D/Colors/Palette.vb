Imports System.ComponentModel

Namespace Drawing2D.Colors

    Public Enum Palettes

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
        ''' All of the named colors in .NET framework.
        ''' </summary>
        <Description("dotnet.colors")> DotNetColors
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
        ''' ColorBrewer Sequential schemes
        ''' </summary>
        <Description("OrRd:c9")> OrRd
        ''' <summary>
        ''' ColorBrewer Sequential schemes
        ''' </summary>
        <Description("PuBu:c9")> PuBu
        ''' <summary>
        ''' ColorBrewer Sequential schemes
        ''' </summary>
        <Description("BuPu:c9")> BuPu
        ''' <summary>
        ''' ColorBrewer Sequential schemes
        ''' </summary>
        <Description("Oranges:c9")> Oranges
        ''' <summary>
        ''' ColorBrewer Sequential schemes
        ''' </summary>
        <Description("BuGn:c9")> BuGn
        ''' <summary>
        ''' ColorBrewer Sequential schemes
        ''' </summary>
        <Description("YlOrBr:c9")> YlOrBr
        ''' <summary>
        ''' ColorBrewer Sequential schemes
        ''' </summary>
        <Description("YlGn:c9")> YlGn
        ''' <summary>
        ''' ColorBrewer Sequential schemes
        ''' </summary>
        <Description("Reds:c9")> Reds
        ''' <summary>
        ''' ColorBrewer Sequential schemes
        ''' </summary>
        <Description("RdPu:c9")> RdPu
        ''' <summary>
        ''' ColorBrewer Sequential schemes
        ''' </summary>
        <Description("Greens:c9")> Greens
        ''' <summary>
        ''' ColorBrewer Sequential schemes
        ''' </summary>
        <Description("YlGnBu:c9")> YlGnBu
        ''' <summary>
        ''' ColorBrewer Sequential schemes
        ''' </summary>
        <Description("Purples:c9")> Purples
        ''' <summary>
        ''' ColorBrewer Sequential schemes
        ''' </summary>
        <Description("GnBu:c9")> GnBu
        ''' <summary>
        ''' ColorBrewer Sequential schemes
        ''' </summary>
        <Description("Greys:c9")> Greys
        ''' <summary>
        ''' ColorBrewer Sequential schemes
        ''' </summary>
        <Description("YlOrRd:c9")> YlOrRd
        ''' <summary>
        ''' ColorBrewer Sequential schemes
        ''' </summary>
        <Description("PuRd:c9")> PuRd
        ''' <summary>
        ''' ColorBrewer Sequential schemes
        ''' </summary>
        <Description("Blues:c9")> Blues
        ''' <summary>
        ''' ColorBrewer Sequential schemes
        ''' </summary>
        <Description("PuBuGn:c9")> PuBuGn

        ''' <summary>
        ''' ColorBrewer Qualitative schemes
        ''' </summary>
        <Description("Set2:c8")> Set2
        ''' <summary>
        ''' ColorBrewer Qualitative schemes
        ''' </summary>
        <Description("Accent:c8")> Accent
        ''' <summary>
        ''' ColorBrewer Qualitative schemes
        ''' </summary>
        <Description("Set1:c8")> Set1
        ''' <summary>
        ''' ColorBrewer Qualitative schemes
        ''' </summary>
        <Description("Set3:c8")> Set3
        ''' <summary>
        ''' ColorBrewer Qualitative schemes
        ''' </summary>
        <Description("Dark2:c8")> Dark2
        ''' <summary>
        ''' ColorBrewer Qualitative schemes
        ''' </summary>
        <Description("Paired:c12")> Paired
        ''' <summary>
        ''' ColorBrewer Qualitative schemes
        ''' </summary>
        <Description("Pastel2:c8")> Pastel2
        ''' <summary>
        ''' ColorBrewer Qualitative schemes
        ''' </summary>
        <Description("Pastel1:c9")> Pastel1

        ''' <summary>
        ''' ColorBrewer Diverging schemes
        ''' </summary>
        <Description("Spectral:c11")> Spectral
        ''' <summary>
        ''' ColorBrewer Diverging schemes
        ''' </summary>
        <Description("RdYlGn:c11")> RdYlGn
        ''' <summary>
        ''' ColorBrewer Diverging schemes
        ''' </summary>
        <Description("RdBu:c11")> RdBu
        ''' <summary>
        ''' ColorBrewer Diverging schemes
        ''' </summary>
        <Description("PiYG:c11")> PiYG
        ''' <summary>
        ''' ColorBrewer Diverging schemes
        ''' </summary>
        <Description("PRGn:c11")> PRGn
        ''' <summary>
        ''' ColorBrewer Diverging schemes
        ''' </summary>
        <Description("RdYlBu:c11")> RdYlBu
        ''' <summary>
        ''' ColorBrewer Diverging schemes
        ''' </summary>
        <Description("BrBG:c11")> BrBG
        ''' <summary>
        ''' ColorBrewer Diverging schemes
        ''' </summary>
        <Description("RdGy:c11")> RdGy
        ''' <summary>
        ''' ColorBrewer Diverging schemes
        ''' </summary>
        <Description("PuOr:c11")> PuOr

        ''' <summary>
        ''' <see cref="ColorMap.PatternAutumn"/>
        ''' </summary>
        Autumn
        ''' <summary>
        ''' <see cref="ColorMap.PatternCool"/>
        ''' </summary>
        Cool
        ''' <summary>
        ''' <see cref="ColorMap.PatternGray"/>
        ''' </summary>
        Gray
        ''' <summary>
        ''' <see cref="ColorMap.PatternHot"/>
        ''' </summary>
        Hot
        ''' <summary>
        ''' <see cref="ColorMap.PatternJet"/>
        ''' </summary>
        Jet
        ''' <summary>
        ''' <see cref="ColorMap.PatternSpring"/>
        ''' </summary>
        Spring
        ''' <summary>
        ''' <see cref="ColorMap.PatternSummer"/>
        ''' </summary>
        Summer
        ''' <summary>
        ''' <see cref="ColorMap.PatternWinter"/>
        ''' </summary>
        Winter

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