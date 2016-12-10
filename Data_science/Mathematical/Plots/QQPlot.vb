#Region "Microsoft.VisualBasic::1727d80867edb35e81e5c3e1b685b91f, ..\sciBASIC#\Data_science\Mathematical\Plots\QQPlot.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.TagData
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Mathematical.Quantile

''' <summary>
''' Q-Q plot(Quantile-Quantile Plot)
''' </summary>
Public Module QQPlot

    ''' <summary>
    ''' [2016-9-30 Currently this function is not working]
    ''' 
    ''' The quantile-quantile (q-q) plot is a graphical technique for determining if two data sets 
    ''' come from populations with a common distribution.
    ''' 
    ''' A q-q plot Is a plot Of the quantiles Of the first data Set against the quantiles Of the 
    ''' second data Set. By a quantile, we mean the fraction (Or percent) Of points below the given 
    ''' value. That Is, the 0.3 (Or 30%) quantile Is the point at which 30% percent Of the data 
    ''' fall below And 70% fall above that value.
    '''
    ''' A 45-degree reference line Is also plotted. If the two sets come from a population with the 
    ''' same distribution, the points should fall approximately along this reference line. The 
    ''' greater the departure from this reference line, the greater the evidence for the conclusion 
    ''' that the two data sets have come from populations with different distributions.
    '''
    ''' The advantages Of the q-q plot are:
    '''
    ''' + The sample sizes Do Not need To be equal.
    ''' + Many distributional aspects can be simultaneously tested. For example, shifts In location, 
    '''   shifts In scale, changes In symmetry, And the presence Of outliers can all be detected from 
    '''   this plot. For example, If the two data sets come from populations whose distributions 
    '''   differ only by a shift In location, the points should lie along a straight line that Is 
    '''   displaced either up Or down from the 45-degree reference line.
    ''' 
    ''' The q-q plot Is similar To a probability plot. For a probability plot, the quantiles For 
    ''' one Of the data samples are replaced With the quantiles Of a theoretical distribution.
    ''' </summary>
    ''' <param name="x"></param>
    ''' <param name="y"></param>
    ''' <param name="size"></param>
    ''' <param name="margin"></param>
    ''' <param name="bg$"></param>
    ''' <param name="ptSize!"></param>
    ''' <returns></returns>
    Public Function Plot(x#(), y#(),
                         Optional size As Size = Nothing,
                         Optional margin As Size = Nothing,
                         Optional bg$ = "white",
                         Optional xcol$ = "black",
                         Optional ycol$ = "black",
                         Optional ptSize! = 35,
                         Optional lv% = 100000,
                         Optional epsilon# = epsilon,
                         Optional compact_size% = 1000) As Bitmap

        Dim data As Double() = x.Join(y).Distinct.ToArray
        Dim maps As Integer() = data.GenerateMapping(lv)
        Dim lvs = data.SeqIterator _
            .ToDictionary(Function(n) maps(n.i),
                          Function(n) n.value)
        Dim q#() = {
            0.05, 0.1, 0.15, 0.2, 0.25, 0.3, 0.35, 0.4, 0.45, 0.5,
            0.55, 0.6, 0.65, 0.7, 0.75, 0.8, 0.85, 0.9, 0.95, 1
        }
        Dim quantiles As Dictionary(Of Double, Double()) =
            lvs.Keys _
            .SelectByQuantile(Function(n) CLng(n), q#, epsilon#, compact_size%) _
            .ToDictionary(Function(o) o.Tag,
                          Function(o) o.value.ToArray(
                          Function(i) lvs(i)))

        Dim screen As Func(Of Double, Double(), Double()) =
            Function(qr, m) LinqAPI.Exec(Of Double) <=
 _
                From n As Double
                In quantiles(qr#)
                Select m.Where(Function(o) o = n)

        ' {quantile, numbers}
        Dim Xlv As Dictionary(Of Double, Integer) = q#.ToDictionary(
            Function(n) n,
            Function(n) screen(n, x#).Length)
        Dim Ylv As Dictionary(Of Double, Integer) = q#.ToDictionary(
            Function(n) n,
            Function(n) screen(n, y#).Length)
        Dim xlMax% = Xlv.Values.Max,
            ylMax% = Ylv.Values.Max

        If size.IsEmpty Then
            size = New Size(3000, 3000)
        End If

        Return GraphicsPlots(size, margin, bg,
            Sub(ByRef g, region)
                ' canvas width/height
                Dim cw! = region.Size.Width% - region.Margin.Width% * 2  ' x轴的长度为y向量的高
                Dim ch! = region.Size.Height - region.Margin.Height * 2  ' y轴的长度为x向量的高
                Dim Xbottom! = region.Size.Height - region.Margin.Height
                Dim Ybottom! = region.Size.Width - region.Margin.Width
                Dim yh As Func(Of Integer, Single) = Function(n) Ybottom - cw! * (n / ylMax)  ' x 轴为y的高
                Dim xh As Func(Of Integer, Single) = Function(n) Xbottom - ch! * (n / xlMax)  ' y 轴为x的高
                Dim dx! = cw! / q#.Length
                Dim dy! = ch! / q#.Length
                Dim left! = margin.Width
                Dim top! = Xbottom
                Dim bx As New SolidBrush(xcol.ToColor)
                Dim by As New SolidBrush(ycol.ToColor)

                For Each ql# In q#
                    Dim xl! = xh(Xlv(ql#))  ' y 轴的高度位置
                    Dim yl! = yh(Ylv(ql#))  ' x 轴的高度位置

                    Call g.FillPie(bx, left, xl, ptSize, ptSize, 0, 360)
                    Call g.FillPie(by, top, yl, ptSize, ptSize, 0, 360)

                    top -= dy
                    left += dx
                Next
            End Sub)
    End Function
End Module
