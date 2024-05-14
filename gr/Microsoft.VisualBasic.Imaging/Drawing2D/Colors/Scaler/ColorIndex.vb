#Region "Microsoft.VisualBasic::f3ed7cbe3b4ac62ecd512d166ff4a2fd, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Colors\Scaler\ColorIndex.vb"

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

    '   Total Lines: 96
    '    Code Lines: 76
    ' Comment Lines: 3
    '   Blank Lines: 17
    '     File Size: 3.27 KB


    '     Class ColorIndex
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: GetLevel, IndexOf
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.Correlations

Namespace Drawing2D.Colors.Scaler

    ''' <summary>
    ''' map colors and the numeric range
    ''' </summary>
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
            Dim colors As Color() = path.Select(AddressOf TranslateColor).ToArray
            Dim parts = path.SlideWindows(2).ToArray
            Dim levelMappings As New List(Of ColorRange)
            Dim tags$() = path.Select(Function(c) maps(c)).ToArray

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
                    Dim d# = DistanceMethods.EuclideanDistance(value, New Double() { .R, .G, .B})

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

End Namespace
