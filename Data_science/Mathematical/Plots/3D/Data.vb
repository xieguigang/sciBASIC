#Region "Microsoft.VisualBasic::630566348564724e099bb4b03374261d, ..\sciBASIC#\Data_science\Mathematical\Plots\3D\Data.vb"

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

Namespace Plot3D

    Public Module Data

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="f"></param>
        ''' <param name="x">x取值范围</param>
        ''' <param name="y">y取值范围</param>
        ''' <param name="xsteps!"></param>
        ''' <param name="ysteps!"></param>
        ''' <returns></returns>
        <Extension>
        Public Function Evaluate(f As Func(Of Double, Double, Double),
                                 x As DoubleRange,
                                 y As DoubleRange,
                                 Optional xsteps! = 0.01,
                                 Optional ysteps! = 0.01) As Point3D()
            Dim out As New List(Of Point3D)

            For xi# = x.Min To x.Max Step xsteps!
                For yi# = y.Min To y.Max Step ysteps!
                    out += New Point3D With {
                        .X = xi#,
                        .Y = yi#,
                        .Z = f(xi, yi)
                    }
                Next
            Next

            Return out
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
                              Function(o) o.obj)
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
