#Region "Microsoft.VisualBasic::f1fa4d1df7f156b0fe243748914e3ac8, Data_science\Mathematica\Math\Math.Statistics\Example\Program.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module ExampleMonteCarlo
    ' 
    '     Sub: Main, MonteCarlo
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Math.Statistics

Public Module ExampleMonteCarlo

    Sub Main()
        Call MonteCarlo()
        Call Pause()
    End Sub

    Public Sub MonteCarlo()
        ' this Is a very trivial example of creating a monte carlo using a
        ' standard normal distribution
        Dim sn As New Distributions.MethodOfMoments.Normal()

        ' output now contains 10000 random normally distributed values.
        Dim output As Vector = sn.GetInvCDF(rand(10000))

        ' to evaluate the mean And standard deviation of the output
        ' you can use Basic Product Moment Stats
        Dim BPM As New MomentFunctions.BasicProductMoments(output)

        Call println("Mean: %s", BPM.Mean())
        Call println("StDev: %s", BPM.StDev())
        Call println("Sample Size: %s", BPM.SampleSize())
        Call println("Minimum: %s", BPM.Min())
        Call println("Maximum: %s", BPM.Max())
    End Sub
End Module
