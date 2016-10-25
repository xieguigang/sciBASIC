Imports System.Drawing
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Mathematical.Plots

Public Module CubicSplineTest

    Sub Test()
        Dim data = "E:\GCModeller\src\runtime\visualbasic_App\Data_science\Mathematical\data\CubicSpline\duom2.txt".IterateAllLines.ToArray(Function(s) Regex.Replace(s, "\s+", " ").Trim.Split.ToArray(AddressOf Val))
        Dim points As Point() = data.ToArray(Function(c) New Point(c(0), c(1)))
        Dim result = CubicSpline.RecalcSpline(points).ToArray

        Call Scatter.Plot(result).SaveAs("./duom2_raw.png")

        Pause()
    End Sub
End Module
