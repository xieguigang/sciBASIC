Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Filters

Module imageSmoothTest

    Sub Main()
        Dim bitmap1 = "E:\mzkit\DATA\test\imzML\s042_229_continuous_large.png".LoadImage
        Dim bitmap2 = New GaussianSmooth(k:=250).Smooth(bitmap1)

        Call bitmap2.SaveAs("E:\Resources\a.html.png")

        Call GaussBlur.GaussBlur(bitmap1).SaveAs("E:\Resources\a.htm222l.png")
    End Sub
End Module
