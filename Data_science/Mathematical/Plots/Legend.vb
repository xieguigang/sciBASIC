Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.MarkupLanguage.CSS
Imports Microsoft.VisualBasic.Serialization.JSON

Public Class Legend

    Public Property style As LegendStyles
    Public Property title As String
    Public Property color As String
    ''' <summary>
    ''' CSS expression, which can be parsing by <see cref="CSSFont"/> 
    ''' </summary>
    ''' <returns></returns>
    Public Property fontstyle As String

    Public Function GetFont() As Font
        Return CSSFont.TryParse(fontstyle).GDIObject
    End Function

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class

Public Enum LegendStyles
    Rectangle
    Circle
    SolidLine
    DashLine
    Diamond
    Triangle
    Hexagon
End Enum

Public Module LegendPlotExtensions

    ''' <summary>
    ''' 函数返回最大的那个rectange的大小
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="pos"></param>
    ''' <param name="l"></param>
    ''' <returns></returns>
    <Extension>
    Public Function DrawLegend(ByRef g As Graphics, pos As Point, graphicsSize As SizeF, l As Legend) As SizeF
        Dim font As Font = l.GetFont
        Dim fSize As SizeF = g.MeasureString(l.title, font)

        Select Case l.style
            Case LegendStyles.Circle
            Case LegendStyles.DashLine
            Case LegendStyles.Diamond
            Case LegendStyles.Hexagon
            Case LegendStyles.Rectangle
            Case LegendStyles.SolidLine
            Case LegendStyles.Triangle
            Case Else

        End Select

        If fSize.Height > graphicsSize.Height Then
            Return fSize
        Else
            Return graphicsSize
        End If
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="g"></param>
    ''' <param name="topLeft"></param>
    ''' <param name="ls"></param>
    ''' <param name="graphicSize">单个legend图形的绘图区域的大小</param>
    ''' <param name="d"></param>
    <Extension>
    Public Sub DrawLegends(ByRef g As Graphics,
                           topLeft As Point,
                           ls As IEnumerable(Of Legend),
                           Optional graphicSize As SizeF = Nothing,
                           Optional d As Integer = 10)

        If graphicSize.IsEmpty Then
            graphicSize = New SizeF(120, 45)
        End If

        For Each l As Legend In ls
            topLeft = New Point(
                topLeft.X,
                g.DrawLegend(
                topLeft,
                graphicSize, l).Height + d + topLeft.Y)
        Next
    End Sub
End Module