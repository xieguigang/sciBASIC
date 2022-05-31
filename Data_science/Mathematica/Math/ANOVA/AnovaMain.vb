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
