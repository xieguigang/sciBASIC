#Region "Microsoft.VisualBasic::a5587c2c373946c5cc6e0e478d49713b, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Colors\ColorIndex.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class ColorIndex
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: GetLevel, IndexOf
    ' 
    '     Structure ColorRange
    ' 
    '         Properties: Level, Points
    ' 
    '         Function: GetMinDistance, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.Algorithm.base
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
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
                    Dim d# = Math.EuclideanDistance(value, New Double() { .R, .G, .B})
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
                    Function(x) Math.EuclideanDistance(
                        array,
                        New Double() {x.R, x.G, x.B}))
            End With
        End Function

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Structure
End Namespace
