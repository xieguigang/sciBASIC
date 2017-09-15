
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Driver

''' <summary>
''' ### Quantile-Quantile Plot
''' 
''' > http://www.itl.nist.gov/div898/handbook/eda/section3/qqplot.htm
''' 
''' The quantile-quantile (q-q) plot is a graphical technique for determining if two data sets come from populations with a common distribution.
''' A q-q plot Is a plot Of the quantiles Of the first data Set against the quantiles Of the second data Set. By a quantile, we mean the fraction 
''' (Or percent) Of points below the given value. That Is, the 0.3 (Or 30%) quantile Is the point at which 30% percent Of the data fall below And 
''' 70% fall above that value.
'''
''' A 45 - degree reference line Is also plotted. If the two sets come from a population With the same distribution, the points should fall 
''' approximately along this reference line. The greater the departure from this reference line, the greater the evidence for the conclusion 
''' that the two data sets have come from populations with different distributions.
'''
''' The advantages Of the q-q plot are:
'''
''' 1. The sample sizes Do Not need To be equal.
''' 2. Many distributional aspects can be simultaneously tested. For example, shifts In location, shifts In scale, changes In symmetry, And the presence 
''' Of outliers can all be detected from this plot. For example, If the two data sets come from populations whose distributions differ only by a 
''' shift In location, the points should lie along a straight line that Is displaced either up Or down from the 45-degree reference line.
''' 
''' The q-q plot Is similar To a probability plot. For a probability plot, the quantiles For one Of the data samples are replaced With the quantiles 
''' Of a theoretical distribution.
''' </summary>
Public Module QQPlot

    ''' <summary>
    ''' Purpose: Check If Two Data Sets Can Be Fit With the Same Distribution
    ''' </summary>
    ''' <param name="set1"></param>
    ''' <param name="set2"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' Definition: Quantiles for Data Set 1 Versus Quantiles of Data Set 2	
    ''' 
    ''' The q-q plot Is formed by:
    '''
    ''' + Vertical axis: Estimated quantiles from data Set 1
    ''' + Horizontal axis: Estimated quantiles from data Set 2
    '''
    ''' Both axes are In units Of their respective data sets. That Is, the actual quantile level Is Not plotted. 
    ''' For a given point On the q-q plot, we know that the quantile level Is the same For both points, but 
    ''' Not what that quantile level actually Is.
    '''
    ''' If the data sets have the same size, the q-q plot Is essentially a plot Of sorted data Set 1 against sorted data Set 2. 
    ''' If the data sets are Not Of equal size, the quantiles are usually picked To correspond To the sorted values from the 
    ''' smaller data Set And Then the quantiles For the larger data Set are interpolated.
    '''
    ''' The q-q plot Is used To answer the following questions:
    ''' 
    ''' + Do two data sets come from populations with a common distribution?
    ''' + Do two data sets have common location And scale?
    ''' + Do two data sets have similar distributional shapes?
    ''' + Do two data sets have similar tail behavior?
    ''' 
    ''' ###### Importance: Check for Common Distribution	
    ''' 
    ''' When there are two data samples, it Is often desirable to know if the assumption of a common distribution Is justified. 
    ''' If so, then location And scale estimators can pool both data sets to obtain estimates of the common location And scale. 
    ''' If two samples do differ, it Is also useful to gain some understanding of the differences. The q-q plot can provide more 
    ''' insight into the nature of the difference than analytical methods such as the chi-square And Kolmogorov-Smirnov 2-sample 
    ''' tests.
    ''' 
    ''' ###### Related Techniques	
    ''' 
    ''' + Bihistogram
    ''' + T Test
    ''' + F Test
    ''' + 2-Sample Chi-Square Test
    ''' + 2-Sample Kolmogorov-Smirnov Test
    ''' </remarks>
    Public Function Plot(set1 As IEnumerable(Of Double), set2 As IEnumerable(Of Double),
                         Optional size$ = "2700,3000",
                         Optional padding$ = g.DefaultPadding,
                         Optional bg$ = "white") As GraphicsData

    End Function
End Module
