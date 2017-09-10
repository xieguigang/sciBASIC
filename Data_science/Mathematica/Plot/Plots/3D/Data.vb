#Region "Microsoft.VisualBasic::fda911bf48e7fa08405abd7da8527567, ..\sciBASIC#\Data_science\Mathematica\Plot\Plots\3D\Data.vb"

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
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Terminal

Namespace Plot3D

    ''' <summary>
    ''' Data provider
    ''' </summary>
    Public Module DataProvider

        <Extension>
        Public Function Surface(matrix As IEnumerable(Of EntityObject)) As IEnumerable(Of (sf As Surface, c As Double()))
            Dim xData As New List(Of (pt As Point3D, C#)())

            For Each line As EntityObject In matrix
                Dim xi As Double = Val(line.ID)

                xData += LinqAPI.Exec(Of (pt As Point3D, C#)) <=
 _
                    From p As KeyValuePair(Of String, String)
                    In line.Properties
                    Let yi As Double = Val(p.Key)
                    Let z As String() = p.Value.Split(":"c)
                    Let pt As Point3D = New Point3D With {
                        .X = xi,
                        .Y = yi,
                        .Z = Val(z(0))
                    }
                    Let color As Double = Val(z(1))
                    Select (pt:=pt, C:=color)
            Next

            Return xData.Surface
        End Function

        <Extension>
        Public Iterator Function Surface(xData As IEnumerable(Of (pt As Point3D, c#)())) As IEnumerable(Of (sf As Surface, c As Double()))
            Dim previousX = xData(0)
            Dim pY0, pY1 As (pt As Point3D, C#)

            For Each xline As (pt As Point3D, C#)() In xData.Skip(1)   ' 逐行扫描每一个数据点，通过Evaluate函数所生成的数据点都是经过排序了的

                pY0 = previousX(0)
                pY1 = xline(0)

                ' ^ --->
                ' |    |
                ' <--- +
                '
                For i As Integer = 1 To xline.Length - 1
                    Dim data As (pt As Point3D, C#)() = {
                        pY0, previousX(i), xline(i), pY1
                    }
                    Dim v As Point3D() = data _
                        .ToArray(Function(d) d.pt)
                    Dim sf As New Surface With {    ' 使用一个矩形来生成一个3维表面
                        .vertices = v
                    }
                    Dim zc#() = data.ToArray(Function(d) d.C)

                    Yield (sf, zc)

                    pY0 = previousX(i)    ' 迭代到下一个表面
                    pY1 = xline(i)
                Next

                previousX = xline
            Next
        End Function

        'Public Function Grid(x As IEnumerable(Of Double), y As IEnumerable(Of Double)) As Rectangle()

        'End Function

        ''' <summary>
        ''' 生成函数计算结果的三维表面
        ''' </summary>
        ''' <param name="f"></param>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <param name="xsteps!"></param>
        ''' <param name="ysteps!"></param>
        ''' <param name="parallel"></param>
        ''' <param name="matrix"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Surface(f As Func(Of Double, Double, (Z#, c#)),
                                x As DoubleRange,
                                y As DoubleRange,
                                Optional xsteps! = 0.01,
                                Optional ysteps! = 0.01,
                                Optional parallel As Boolean = False,
                                Optional matrix As List(Of EntityObject) = Nothing) As IEnumerable(Of (sf As Surface, c As Double()))

            Dim xdatas = f.Evaluate(x, y, xsteps, ysteps, parallel, matrix).ToArray
            Return xdatas.Surface
        End Function

        <Extension>
        Public Iterator Function Evaluate(f As Func(Of Double, Double, (Z#, c#)),
                                          x As DoubleRange,
                                          y As DoubleRange,
                                          Optional xsteps! = 0.01,
                                          Optional ysteps! = 0.01,
                                          Optional parallel As Boolean = False,
                                          Optional matrix As List(Of EntityObject) = Nothing) As IEnumerable(Of (pt As Point3D, c#)())

            For Each row In f.__2DIterates(x, y, xsteps, ysteps, parallel)
                Dim out = LinqAPI.Exec(Of (pt As Point3D, C#)) <= From o
                                                                  In row
                                                                  Let pt As Point3D = New Point3D With {
                                                                      .X = o.x,
                                                                      .Y = o.y,
                                                                      .Z = o.z.Z
                                                                  }
                                                                  Select (pt, o.z.c)
                If Not matrix Is Nothing Then
                    matrix += New EntityObject With {
                        .ID = out(Scan0).pt.X,
                        .Properties = out.ToDictionary(
                            Function(yk) CStr(yk.pt.Y),
                            Function(z) z.pt.Z & ":" & z.C)
                    }
                End If

                Yield out
            Next
        End Function

        Private Function __progressProvider(total%, yLen%, ysteps#, x As DoubleRange) As Action(Of Double)
            If App.IsConsoleApp Then
                Dim tick As New ProgressProvider(total)
                Dim msg$ = $"Populates data points...(Estimates size: {tick.Target * (yLen / ysteps)}...)"
                Dim prog As New ProgressBar(msg, CLS:=True)

                Call tick.StepProgress()

                Return Sub(xi#)
                           Dim leftTime As String = tick _
                               .ETA(prog.ElapsedMilliseconds) _
                               .FormatTime

                           Call prog.SetProgress(
                                tick.StepProgress,
                                $" {xi} ({x.Min}, {x.Max}),  ETA {leftTime}")
                       End Sub
            Else
                Return Sub()
                           ' DO_NOTHING
                       End Sub
            End If
        End Function

        <Extension>
        Private Iterator Function __2DIterates(Of Tout)([in] As Func(Of Double, Double, Tout),
                                                        x As DoubleRange,
                                                        y As DoubleRange,
                                                        xsteps!, ysteps!,
                                                        parallel As Boolean) As IEnumerable(Of List(Of (x#, y#, z As Tout)))

            Dim tick As Action(Of Double) =
                __progressProvider(x.Length / xsteps, y.Length, ysteps, x)

            For xi# = x.Min To x.Max Step xsteps!

                If parallel Then
                    Dim dy As New List(Of Double)
                    Dim x0# = xi

                    For yi# = y.Min To y.Max Step ysteps!
                        dy += yi
                    Next

                    Yield LinqAPI.MakeList(Of (x#, y#, Z As Tout)) <=
 _
                        From yi As Double
                        In dy.AsParallel
                        Let z As Tout = [in](x0, yi)
                        Select pt = (x:=x0, y:=yi, z:=z)
                        Order By pt.y Ascending
                Else
                    ' 2016-12-15 
                    ' 在迭代器这里不能够用Clear， 必须要新构建一个list对象， 否则得到的所有的数据都会是第一次迭代的结果
                    Dim out As New List(Of (x#, y#, Z As Tout))

                    For yi# = y.Min To y.Max Step ysteps!
                        out += (x:=xi#, y:=yi#, Z:=[in](xi, yi))
                    Next

                    Yield out
                End If

                Call tick(xi)
            Next
        End Function

        ''' <summary>
        ''' Data provider base on the two variable.(这个函数可以同时为3D绘图或者ScatterHeatmap提供绘图数据)
        ''' </summary>
        ''' <param name="f">``z = f(x,y)``</param>
        ''' <param name="x">x取值范围</param>
        ''' <param name="y">y取值范围</param>
        ''' <param name="xsteps!"></param>
        ''' <param name="ysteps!"></param>
        ''' <param name="matrix">假若想要获取得到原始的矩阵数据，这个列表对象还需要实例化之后再传递进来</param>
        ''' <returns>Populate data by x steps.(即每一次输出的一组数据的X都是相同的)</returns>
        <Extension>
        Public Iterator Function Evaluate(f As Func(Of Double, Double, Double),
                                          x As DoubleRange,
                                          y As DoubleRange,
                                          Optional xsteps! = 0.01,
                                          Optional ysteps! = 0.01,
                                          Optional parallel As Boolean = False,
                                          Optional matrix As List(Of DataSet) = Nothing) As IEnumerable(Of (X#, y#, z#)())

            For Each row In f.__2DIterates(x, y, xsteps, ysteps, parallel)
                If Not matrix Is Nothing Then
                    matrix += New DataSet With {
                        .ID = row(Scan0).x,
                        .Properties = row _
                        .ToDictionary(
                            Function(pt) CStr(pt.y),
                            Function(pt) pt.z)
                    }
                End If

                Yield row.ToArray
            Next
        End Function

        ''' <summary>
        ''' Grid generator for function plot
        ''' </summary>
        ''' <param name="f"></param>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <param name="xsteps!"></param>
        ''' <param name="ysteps!"></param>
        ''' <param name="pen"></param>
        ''' <returns></returns>
        Public Iterator Function Grid(f As Func(Of Double, Double, Double),
                                      x As DoubleRange,
                                      y As DoubleRange,
                                      Optional xsteps! = 0.01,
                                      Optional ysteps! = 0.01,
                                      Optional pen As Pen = Nothing) As IEnumerable(Of Line3D)

            Dim a As New Point3D With {
                .X = x.Min,
                .Y = y.Min,
                .Z = f(.X, .Y)
            }
            Dim b As Point3D

            If pen Is Nothing Then
                pen = Pens.Black
            End If

            For xi# = x.Min + xsteps To x.Max Step xsteps!
                For yi# = y.Min + ysteps To y.Max Step ysteps!
                    b = New Point3D With {
                        .X = xi#,
                        .Y = yi#,
                        .Z = f(xi, yi)
                    }

                    Yield New Line3D With {
                        .a = a,
                        .b = b,
                        .pen = pen
                    }

                    a = b
                Next
            Next
        End Function

        ''' <summary>
        ''' Grid generator for function plot
        ''' </summary>
        ''' <param name="f"></param>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <param name="xsteps!"></param>
        ''' <param name="ysteps!"></param>
        ''' <param name="mapNameZ"></param>
        ''' <returns></returns>
        Public Iterator Function Grid(f As Func(Of Double, Double, Double),
                                      x As DoubleRange,
                                      y As DoubleRange,
                                      mapNameZ$,
                                      Optional xsteps! = 0.01,
                                      Optional ysteps! = 0.01) As IEnumerable(Of Line3D)
            Dim array As Line3D() = Grid(f, x, y, xsteps, ysteps).ToArray
            Dim z#() = array _
                .Select(Function(pt) Math.Round((pt.a.Z + pt.b.Z) / 2, 1)) _
                .Distinct _
                .ToArray
            Dim levels As Dictionary(Of Double, Integer) =
                z# _
                .GenerateMapping(100) _
                .SeqIterator _
                .ToDictionary(Function(o) z(o.i),
                              Function(o) o.value)
            Dim colors As Color() = New ColorMap(100 * 2, 250).ColorSequence(mapNameZ)

            For Each line As Line3D In array
                Yield New Line3D With {
                    .a = line.a,
                    .b = line.b,
                    .pen = New Pen(colors.ElementAtOrDefault(levels(Math.Round((.a.Z + .b.Z) / 2, 1)) - 1))
                }
            Next
        End Function
    End Module
End Namespace
