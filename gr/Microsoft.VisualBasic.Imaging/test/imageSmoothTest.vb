#Region "Microsoft.VisualBasic::40eb0a3e75193f9df17e69ca5b36f66a, gr\Microsoft.VisualBasic.Imaging\test\imageSmoothTest.vb"

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

    ' Module imageSmoothTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

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

