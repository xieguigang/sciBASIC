#Region "Microsoft.VisualBasic::0a6c0f30e7585934df21c302afd3243c, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing2D\Colors\ColorIndex.vb"

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
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.ComponentModel.DataStructures
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Drawing2D.Colors

    Public Class ColorIndex

        Dim colors As Color()
        Dim levels As ColorRange()
        Dim upbound As NamedValue(Of Color)

        Sub New(path$(), levels%)
            Call Me.New(
                path.ToDictionary(Function(x) x, Function(x) x),
                levels)
        End Sub

        Sub New(maps As Dictionary(Of String, String), levels%)
            Dim path$() = maps.Keys.ToArray
            Dim colors As Color() = path.ToArray(AddressOf TranslateColor)
            Dim parts = path.SlideWindows(2).ToArray
            Dim levelMappings As New List(Of ColorRange)
            Dim tags$() = path.ToArray(Function(c) maps(c))

            With Me
                .colors = colors.CubicSpline(levels)

                For Each part In parts.SeqIterator
                    Dim start = IndexOf((+part).First)
                    Dim ends = IndexOf((+part).Last)

                    colors = New Color(ends - start - 1) {}
                    Array.ConstrainedCopy(.colors, start, colors, Scan0, colors.Length)
                    levelMappings += New ColorRange With {
                        .Level = tags(part),
                        .Points = colors
                    }
                Next

                .levels = levelMappings
                .upbound = New NamedValue(Of Color) With {
                    .Name = maps(path.Last),
                    .Value = path.Last.TranslateColor
                }
            End With
        End Sub

        Public Function IndexOf(color$) As Integer
            Dim value#()
            Dim minD# = Integer.MaxValue
            Dim minIndex%

            With color.TranslateColor
                value = { .R, .G, .B}
            End With

            For i As Integer = 0 To colors.Length - 1
                With colors(i)
                    Dim d# = Mathematical.EuclideanDistance(value, New Double() { .R, .G, .B})
                    If d <= minD Then
                        minD = d
                        minIndex = i
                    End If
                End With
            Next

            Return minIndex
        End Function

        Public Function GetLevel(color$) As String
            Dim value As Color = color.TranslateColor
            Dim mind_orders = From x As ColorRange
                              In levels
                              Select d = x.GetMinDistance(value),
                                  x
                              Order By d Ascending
            Dim level = mind_orders.First
            Dim up# = EuclideanDistance(upbound.Value, value)

            If level.d >= up Then
                Return upbound.Name
            Else
                Return level.x.Level
            End If
        End Function
    End Class

    Public Structure ColorRange : Implements INamedValue

        Public Property Level$ Implements INamedValue.Key
        Public Property Points As Color()

        ''' <summary>
        ''' 返回和最近的一个颜色点的距离值
        ''' </summary>
        ''' <param name="color"></param>
        ''' <returns></returns>
        Public Function GetMinDistance(color As Color) As Double
            With color
                Dim array As Double() = { .R, .G, .B}
                Return Points.Min(
                    Function(x) Mathematical.EuclideanDistance(
                        array,
                        New Double() {x.R, x.G, x.B}))
            End With
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure
End Namespace
