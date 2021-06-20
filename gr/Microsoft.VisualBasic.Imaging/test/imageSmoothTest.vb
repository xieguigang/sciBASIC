Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Filters
Imports Microsoft.VisualBasic.Linq

Module imageSmoothTest

    Sub Main()
        Dim bitmap1 = "E:\mzkit\DATA\test\imzML\s042_229_continuous_large.png".LoadImage
        Dim bitmap2 = New GaussianSmooth(k:=250).Smooth(bitmap1)

        Call bitmap2.SaveAs("E:\Resources\a.html.png")

        Call GaussBlur.GaussBlur(bitmap1).SaveAs("E:\Resources\a.htm222l.png")


        Dim matrix As New Filters.Matrix(bitmap1)

        Call matrix.GetBitmap(Matrix2DFilters.Median).SaveAs("E:\Resources\a.htm222l_median.png")
        Call matrix.GetBitmap(Matrix2DFilters.Mean).SaveAs("E:\Resources\a.htm222l_mean.png")
        Call matrix.GetBitmap(Matrix2DFilters.Min).SaveAs("E:\Resources\a.htm222l_min.png")
        Call matrix.GetBitmap(Matrix2DFilters.Max).SaveAs("E:\Resources\a.htm222l_max.png")


        Call matrix.GetBitmap(Matrix2DFilters.Median).DoCall(AddressOf GaussBlur.GaussBlur).SaveAs("E:\Resources\a.htm222l_median+Gauss.png")
        Call New Matrix(GaussBlur.GaussBlur(bitmap1)).GetBitmap(Matrix2DFilters.Median).SaveAs("E:\Resources\a.htm222l_Gauss+median.png")
        Call New Matrix(GaussBlur.GaussBlur(bitmap1)).GetBitmap(Matrix2DFilters.Mean).SaveAs("E:\Resources\a.htm222l_Gauss+mean.png")
        Call New Matrix(GaussBlur.GaussBlur(bitmap1)).GetBitmap(Matrix2DFilters.Min).SaveAs("E:\Resources\a.htm222l_Gauss+min.png")
        Call New Matrix(GaussBlur.GaussBlur(bitmap1)).GetBitmap(Matrix2DFilters.Max).SaveAs("E:\Resources\a.htm222l_Gauss+max.png")
    End Sub
End Module
