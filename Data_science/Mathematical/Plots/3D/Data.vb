#Region "Microsoft.VisualBasic::2d386f88205c395bb954ff5407187643, ..\sciBASIC#\Data_science\Mathematical\Plots\3D\Data.vb"

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
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Mathematical
Imports Microsoft.VisualBasic.Terminal

Namespace Plot3D

    ''' <summary>
    ''' Data provider
    ''' </summary>
    Public Module DataProvider

        ''' <summary>
        ''' Data provider base on the two variable.(这个函数可以同时为3D绘图或者ScatterHeatmap提供绘图数据)
        ''' </summary>
        ''' <param name="f">``z = f(x,y)``</param>
        ''' <param name="x">x取值范围</param>
        ''' <param name="y">y取值范围</param>
        ''' <param name="xsteps!"></param>
        ''' <param name="ysteps!"></param>
        ''' <returns>Populate data by x steps.(即每一次输出的一组数据的X都是相同的)</returns>
        <Extension>
        Public Iterator Function Evaluate(f As Func(Of Double, Double, Double),
                                          x As DoubleRange,
                                          y As DoubleRange,
                                          Optional xsteps! = 0.01,
                                          Optional ysteps! = 0.01,
                                          Optional parallel As Boolean = False) As IEnumerable(Of List(Of Point3D))

            Dim prog As New ProgressBar("Populates data points...", cls:=True)

            Call $"Estimates size: {(x.Length / xsteps) * (y.Length / ysteps)}...".__DEBUG_ECHO

            For xi# = x.Min To x.Max Step xsteps!

                If parallel Then
                    Dim dy As New List(Of Double)
                    Dim x0# = xi

                    For yi# = y.Min To y.Max Step ysteps!
                        dy += yi
                    Next

                    Yield LinqAPI.MakeList(Of Point3D) <= From yi As Double
                                                          In dy.AsParallel
                                                          Let z As Double = f(x0, yi)
                                                          Select pt = New Point3D With {
                                                              .X = x0,
                                                              .Y = yi,
                                                              .Z = z
                                                          }
                                                          Order By pt.Y Ascending
                Else
                    Dim out As New List(Of Point3D)

                    For yi# = y.Min To y.Max Step ysteps!
                        out += New Point3D With {
                            .X = xi#,
                            .Y = yi#,
                            .Z = f(xi, yi)
                        }
                    Next

                    Yield out
                End If

                Call prog.SetProgress(xi / x.Max * 100, $" {xi} ({x.Min}, {x.Max})")
            Next

            Call prog.Dispose()
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
                    .pen = New Pen(colors.Get(levels(Math.Round((.a.Z + .b.Z) / 2, 1)) - 1))
                }
            Next
        End Function
    End Module
End Namespace
