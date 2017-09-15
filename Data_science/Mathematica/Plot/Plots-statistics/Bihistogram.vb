
Imports Microsoft.VisualBasic.Imaging.Driver

''' <summary>
''' ## Bihistogram
''' 
''' > http://www.itl.nist.gov/div898/handbook/eda/section3/bihistog.htm
''' 
''' The bihistogram is an EDA tool for assessing whether a before-versus-after engineering modification has caused a change in
''' 
''' + location;
''' + variation; Or
''' + distribution.
''' 
''' It Is a graphical alternative To the two-sample t-test. The bihistogram can be more powerful than the t-test In that all Of 
''' the distributional features (location, scale, skewness, outliers) are evident on a single plot. It Is also based on the 
''' common And well-understood histogram.
''' </summary>
Public Module Bihistogram

    ''' <summary>
    ''' The bihistogram can provide answers to the following questions:
    ''' 
    ''' + Is a (2-level) factor significant?
    ''' + Does a(2 - level) factor have an effect?
    ''' + Does the location change between the 2 subgroups?
    ''' + Does the variation change between the 2 subgroups?
    ''' + Does the distributional shape change between subgroups?
    ''' + Are there any outliers?
    ''' 
    ''' The bihistogram is an important EDA tool for determining if a factor "has an effect". Since the bihistogram provides insight into 
    ''' the validity of three (location, variation, and distribution) out of the four (missing only randomness) underlying assumptions in 
    ''' a measurement process, it is an especially valuable tool. Because of the dual (above/below) nature of the plot, the bihistogram is 
    ''' restricted to assessing factors that have only two levels. However, this is very common in the before-versus-after character of 
    ''' many scientific and engineering experiments.
    ''' </summary>
    ''' <returns></returns>
    Public Function Plot() As GraphicsData

    End Function
End Module
