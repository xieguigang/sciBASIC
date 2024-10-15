#Region "Microsoft.VisualBasic::d7a18dc6a573709d3f1a0407d47fc859, gr\Microsoft.VisualBasic.Imaging\test\imageSmoothTest.vb"

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

    '   Total Lines: 48
    '    Code Lines: 33 (68.75%)
    ' Comment Lines: 0 (0.00%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 15 (31.25%)
    '     File Size: 2.05 KB


    ' Module imageSmoothTest
    ' 
    '     Sub: gaussBlurTest, Main1
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Filters
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Drawing

Module imageSmoothTest

    Sub Main1()

        Call gaussBlurTest()

        Dim bitmap1 = "E:\mzkit\DATA\test\imzML\s042_229_continuous_large.png".LoadImage
        Dim bitmap2 = New GaussianSmooth(k:=250).Smooth(bitmap1)

        Call bitmap2.SaveAs("E:\Resources\a.html.png")

        Call GaussBlur.GaussBlur(bitmap1).SaveAs("E:\Resources\a.htm222l.png")


        Dim matrix As New Filters.Matrix(bitmap1)

        Call matrix.GetSmoothBitmap(Matrix2DFilters.Median).SaveAs("E:\Resources\a.htm222l_median.png")
        Call matrix.GetSmoothBitmap(Matrix2DFilters.Mean).SaveAs("E:\Resources\a.htm222l_mean.png")
        Call matrix.GetSmoothBitmap(Matrix2DFilters.Min).SaveAs("E:\Resources\a.htm222l_min.png")
        Call matrix.GetSmoothBitmap(Matrix2DFilters.Max).SaveAs("E:\Resources\a.htm222l_max.png")


        Call matrix.GetSmoothBitmap(Matrix2DFilters.Median).DoCall(AddressOf GaussBlur.GaussBlur).SaveAs("E:\Resources\a.htm222l_median+Gauss.png")
        Call New Matrix(GaussBlur.GaussBlur(bitmap1)).GetSmoothBitmap(Matrix2DFilters.Median).SaveAs("E:\Resources\a.htm222l_Gauss+median.png")
        Call New Matrix(GaussBlur.GaussBlur(bitmap1)).GetSmoothBitmap(Matrix2DFilters.Mean).SaveAs("E:\Resources\a.htm222l_Gauss+mean.png")
        Call New Matrix(GaussBlur.GaussBlur(bitmap1)).GetSmoothBitmap(Matrix2DFilters.Min).SaveAs("E:\Resources\a.htm222l_Gauss+min.png")
        Call New Matrix(GaussBlur.GaussBlur(bitmap1)).GetSmoothBitmap(Matrix2DFilters.Max).SaveAs("E:\Resources\a.htm222l_Gauss+max.png")
    End Sub

    Sub gaussBlurTest()
        Dim large = "F:\184.9301.png".LoadImage
        Dim blur As Bitmap = New Bitmap(large)

        For i As Integer = 0 To 20
            blur = GaussBlur.GaussBlur(blur)
            Console.WriteLine(i)
        Next

        Call blur.SaveAs("F:\blur.png")

        Pause()
    End Sub
End Module
