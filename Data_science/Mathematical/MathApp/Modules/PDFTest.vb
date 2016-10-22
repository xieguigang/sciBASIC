Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Mathematical.BasicR
Imports Microsoft.VisualBasic.Mathematical.Distributions
Imports Microsoft.VisualBasic.Mathematical.Plots

Module PDFTest

    Public Sub betaTest()
        Dim x As New Vector(VBMathExtensions.seq(0, 1, 0.01))
        Dim s1 = Scatter.FromVector(Beta.beta(x, 0.5, 0.5), "red", xrange:=x)
        Dim s2 = Scatter.FromVector(Beta.beta(x, 5, 1), "blue", xrange:=x)
        Dim s3 = Scatter.FromVector(Beta.beta(x, 1, 3), "green", xrange:=x)
        Dim s4 = Scatter.FromVector(Beta.beta(x, 2, 2), "purple", xrange:=x)
        Dim s5 = Scatter.FromVector(Beta.beta(x, 2, 5), "orange", xrange:=x)

        Call Scatter.Plot({s1, s2, s3, s4, s5}).SaveAs("./beta_PDF.png")

        Pause()
    End Sub
End Module
