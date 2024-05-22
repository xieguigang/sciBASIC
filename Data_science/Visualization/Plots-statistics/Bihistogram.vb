#Region "Microsoft.VisualBasic::6803785d2f3b8fc2e9f8f1fff515d40c, Data_science\Visualization\Plots-statistics\Bihistogram.vb"

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

    '   Total Lines: 40
    '    Code Lines: 6 (15.00%)
    ' Comment Lines: 32 (80.00%)
    '    - Xml Docs: 81.25%
    ' 
    '   Blank Lines: 2 (5.00%)
    '     File Size: 1.92 KB


    ' Module Bihistogram
    ' 
    '     Function: Plot
    ' 
    ' /********************************************************************************/

#End Region

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
        Throw New NotImplementedException
    End Function
End Module
