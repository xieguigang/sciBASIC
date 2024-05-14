#Region "Microsoft.VisualBasic::8ac7c54e4924fed834aa457292d39658, Data_science\Mathematica\Math\ANOVA\AnovaMain.vb"

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

    '   Total Lines: 60
    '    Code Lines: 30
    ' Comment Lines: 18
    '   Blank Lines: 12
    '     File Size: 2.46 KB


    '     Class AnovaMain
    ' 
    '         Sub: Main
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System

Namespace stats
    ' 
    '  Numbers for the tests are from: https://www.youtube.com/watch?v=-yQb_ZJnFXw
    '  How To Calculate and Understand Analysis of Variance (ANOVA) F Test.
    '  User: statisticsfun
    '  
    '  The distribution tables are from: http://www.socr.ucla.edu/applets.dir/f_table.html
    '  
    '  This is a simple 1 way ANOVA. 
    '  If the calculated Fscore is larger than the critial number then reject the null hypothesis.
    '  
    ' 

    Public Class AnovaMain

        Public Shared Sub Main(ParamArray strings As String())

            Dim anova As Anova = New Anova()

            ' 
            ' 		double[][] observations = new double[][] {
            ' 				{2,3,7,2,6},
            ' 				{10,8,7,5,10},
            ' 				{10,13,14,13,15},
            ' 		};
            ' 
            Dim observations = New Double()() {New Double() {2, 3, 7, 2, 6}, New Double() {10, 8, 7, 5, 10, 10, 8, 7, 5, 10}, New Double() {2, 3, 7, 2, 6}}

            Dim type = Constants.P_FIVE_PERCENT

            anova.populate_step1(observations, type)
            anova.findWithinGroupMeans_step2()
            anova.setSumOfSquaresOfGroups_step3()
            anova.setTotalSumOfSquares_step4()
            anova.setTotalSumOfSquares_step5()
            anova.divide_by_degrees_of_freedom_step6()

            Dim f_score As Double = anova.fScore_determineIt_step7()

            Dim criticalNumber = anova.criticalNumber

            Dim result = "The null hypothesis is supported! There is no especial difference in these groups. "

            If f_score > criticalNumber Then
                result = "The null hypothesis is rejected! These groups are different."
            End If
            Console.WriteLine("Groups degrees of freedom: " & anova.numenator)
            Console.WriteLine("Observations degrees of freedom: " & anova.denomenator)
            Console.WriteLine("SSW_sum_of_squares_within_groups: " & anova.SSW_sum_of_squares_within_groups)
            Console.WriteLine("SSB_sum_of_squares_between_groups: " & anova.SSB_sum_of_squares_between_groups)
            Console.WriteLine("allObservationsMean: " & anova.allObservationsMean)
            Console.WriteLine("Critical number: " & criticalNumber)
            Console.WriteLine("*** F Score: " & f_score)
            Console.WriteLine(result)
        End Sub
    End Class

End Namespace
