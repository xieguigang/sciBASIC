#Region "Microsoft.VisualBasic::f39527e8cdcd59e1a16565fe71e72b13, Data_science\Mathematica\Math\Math.Statistics\HypothesisTesting\MantelTest\corr.vb"

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

    '   Total Lines: 100
    '    Code Lines: 37 (37.00%)
    ' Comment Lines: 53 (53.00%)
    '    - Xml Docs: 94.34%
    ' 
    '   Blank Lines: 10 (10.00%)
    '     File Size: 5.96 KB


    '     Module corr
    ' 
    ' 
    '         Delegate Function
    ' 
    '             Function: evalInternal, GetCorrelations, Pearson
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Hypothesis.Mantel

    Public Module corr

        ''' <summary>
        ''' An abstract delegate function for measure the correlation between two given numeric vector
        ''' </summary>
        ''' <param name="x">a numeric vector</param>
        ''' <param name="y">another numeric vector, these two vector should be equals on dimension size!</param>
        ''' <returns></returns>
        Public Delegate Function corr(x As Double(), y As Double()) As (cor As Double, pvalue As Double)

        Private Function evalInternal(d As SeqValue(Of Double()), mat As Double()(), cor As corr) As (i As Integer, cor As Double(), pval As Double())
            Dim vec As New List(Of Double)
            Dim vec2 As New List(Of Double)
            Dim x As Double() = d.value
            Dim r As (cor As Double, pvalue As Double)

            For Each y As Double() In mat
                r = cor(x, y)
                vec += r.cor
                vec2 += r.pvalue
            Next

            Return (d.i, vec.ToArray, vec2.ToArray)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="mat"></param>
        ''' <param name="cor"></param>
        ''' <returns></returns>
        <Extension>
        Public Function GetCorrelations(mat As Double()(), cor As corr) As (cor As Double()(), pvalue As Double()())
            Dim evalData = mat _
                .SeqIterator _
                .AsParallel _
                .Select(Function(d) evalInternal(d, mat, cor)) _
                .OrderBy(Function(d) d.i) _
                .ToArray
            Dim corr As Double()() = evalData.Select(Function(i) i.Item2).ToArray
            Dim pvalue As Double()() = evalData.Select(Function(i) i.Item3).ToArray

            Return (corr, pvalue)
        End Function

        ''' <summary>
        ''' A wrapper to the <see cref="Correlations.GetPearson"/> base method.
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' The Pearson correlation coefficient, often denoted by r, is a measure of the linear relationship between 
        ''' two continuous variables. It was developed by Karl Pearson from a related idea introduced by Francis Galton 
        ''' in the 1880s. The coefficient is calculated using the covariance of the two variables divided by the 
        ''' product of their standard deviations.
        ''' 
        ''' Here's a breakdown of the key aspects of the Pearson correlation:
        ''' 
        ''' **Range**: The value of the Pearson correlation coefficient ranges from -1 to 1.
        ''' - **1** indicates a perfect positive linear relationship: as one variable increases, the other variable also increases in a perfectly linear manner.
        ''' - **-1** indicates a perfect negative linear relationship: as one variable increases, the other variable decreases in a perfectly linear manner.
        ''' - **0** indicates no linear relationship between the variables.
        ''' **Symmetry**: The Pearson correlation is symmetric, meaning that the correlation of X with Y is the same as the correlation of Y with X.
        ''' **Scale Independence**: The Pearson correlation is not affected by changes in the scale of the variables. For example, if you multiply all values 
        '''   of one variable by a constant, the correlation coefficient remains the same.
        ''' **Unit-Free**: The correlation coefficient is a unit-free measure, which means that it does not have any units of measurement attached to it.
        ''' **Sensitivity to Outliers**: The Pearson correlation coefficient can be sensitive to outliers, which are extreme values that differ significantly 
        '''   from other observations. Outliers can have a disproportionate effect on the calculation of the correlation coefficient.
        ''' **Assumptions**:
        ''' - The relationship between the variables is linear.
        ''' - Both variables are continuous and normally distributed.
        ''' - The data is homoscedastic, meaning that the variance of the residuals (differences between observed and predicted values) is constant across all 
        '''   levels of the independent variable.
        ''' **Interpretation**:
        ''' - A correlation close to 1 or -1 suggests a strong linear relationship.
        ''' - A correlation close to 0 suggests a weak linear relationship.
        ''' - The sign of the correlation coefficient indicates the direction of the relationship: positive means that as one variable increases, the other tends 
        '''   to increase, and negative means that as one variable increases, the other tends to decrease.
        ''' **Limitations**:
        ''' - It only measures linear relationships and may not capture non-linear relationships effectively.
        ''' - It does not imply causation; a high correlation between two variables does not mean that one causes the other.
        ''' - It can be misleading in the presence of outliers or when the data does not meet the assumptions of normality and homoscedasticity.
        ''' The Pearson correlation coefficient is a useful statistical tool for assessing the strength and direction of the linear relationship between two 
        ''' continuous variables, but it should be used with an understanding of its assumptions and limitations.
        ''' </remarks>
        Public Function Pearson(x As Double(), y As Double()) As (cor#, pvalue#)
            Dim pvalue As Double
            Dim corVal = Correlations.GetPearson(x, y, prob:=pvalue, throwMaxIterError:=False)

            Return (corVal, pvalue)
        End Function
    End Module
End Namespace
