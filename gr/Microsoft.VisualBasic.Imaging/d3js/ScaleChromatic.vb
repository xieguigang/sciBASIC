Imports System.Drawing

Namespace d3js

    ''' <summary>
    ''' ###### d3-scale-chromatic
    ''' 
    ''' > https://github.com/d3/d3-scale-chromatic
    ''' 
    ''' This module provides sequential, diverging and categorical color schemes derived from Cynthia A. Brewer’s ColorBrewer. 
    ''' Since ColorBrewer publishes only discrete color schemes, the sequential and diverging scales are interpolated using 
    ''' uniform B-splines. These schemes and interpolators are designed to work with d3-scale’s d3.scaleOrdinal and 
    ''' d3.scaleSequential. 
    ''' 
    ''' For example, to create a categorical color scale using the Accent color scheme:
    '''
    ''' ```vbnet
    ''' Dim color = d3js.scaleOrdinal(d3js.schemeAccent)
    ''' ```
    '''
    ''' To create a diverging color scale using the PiYG color scheme:
    ''' 
    ''' ```vbnet
    ''' Dim color = d3js.scaleSequential(d3js.interpolatePiYG)
    ''' ```
    ''' </summary>
    Public Module ScaleChromatic

        ''' <summary>
        ''' ``# d3.scale.category10()``
        ''' Constructs a new ordinal scale with a range of ten categorical colors:
        ''' </summary>
        ''' <returns></returns>
        Public Function category10() As Color()
            Return {
                "#1f77b4",
                "#ff7f0e",
                "#2ca02c",
                "#d62728",
                "#9467bd",
                "#8c564b",
                "#e377c2",
                "#7f7f7f",
                "#bcbd22",
                "#17becf"
            }.Select(Function(c) c.TranslateColor) _
             .ToArray
        End Function

        ''' <summary>
        ''' ``# d3.scale.category20()``
        ''' 
        ''' Constructs a new ordinal scale with a range of twenty categorical colors:
        ''' </summary>
        ''' <returns></returns>
        Public Function category20() As Color()
            Return {
                "#1f77b4",
                "#aec7e8",
                "#ff7f0e",
                "#ffbb78",
                "#2ca02c",
                "#98df8a",
                "#d62728",
                "#ff9896",
                "#9467bd",
                "#c5b0d5",
                "#8c564b",
                "#c49c94",
                "#e377c2",
                "#f7b6d2",
                "#7f7f7f",
                "#c7c7c7",
                "#bcbd22",
                "#dbdb8d",
                "#17becf",
                "#9edae5"
            }.Select(AddressOf TranslateColor) _
             .ToArray
        End Function

        ''' <summary>
        ''' ``# d3.scale.category20b()``
        ''' 
        ''' Constructs a new ordinal scale with a range of twenty categorical colors:
        ''' </summary>
        ''' <returns></returns>
        Public Function category20b() As Color()
            Return {
                "#393b79",
                "#5254a3",
                "#6b6ecf",
                "#9c9ede",
                "#637939",
                "#8ca252",
                "#b5cf6b",
                "#cedb9c",
                "#8c6d31",
                "#bd9e39",
                "#e7ba52",
                "#e7cb94",
                "#843c39",
                "#ad494a",
                "#d6616b",
                "#e7969c",
                "#7b4173",
                "#a55194",
                "#ce6dbd",
                "#de9ed6"
            }.Select(AddressOf TranslateColor) _
             .ToArray
        End Function

        ''' <summary>
        ''' ``# d3.scale.category20c()``
        ''' 
        ''' Constructs a new ordinal scale with a range of twenty categorical colors:
        ''' </summary>
        ''' <returns></returns>
        Public Function category20c()  As  Color ()
            Return {
                "#3182bd",
                "#6baed6",
                "#9ecae1",
                "#c6dbef",
                "#e6550d",
                "#fd8d3c",
                "#fdae6b",
                "#fdd0a2",
                "#31a354",
                "#74c476",
                "#a1d99b",
                "#c7e9c0",
                "#756bb1",
                "#9e9ac8",
                "#bcbddc",
                "#dadaeb",
                "#636363",
                "#969696",
                "#bdbdbd",
                "#d9d9d9"
            }.Select(AddressOf TranslateColor) _
             .ToArray
        End Function
    End Module
End Namespace