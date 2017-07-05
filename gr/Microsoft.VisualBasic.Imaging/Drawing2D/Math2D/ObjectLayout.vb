Imports System.Drawing
Imports Microsoft.VisualBasic.MIME.Markup.HTML.CSS

Namespace Drawing2D.Math2D

    Public Enum Regions As Byte
        ALL = 0
        TopLeft = 2
        TopRight = 4
        BottomLeft = 8
        BottomRight = 16
    End Enum

    Public Class ObjectLayout

        Public Structure LayoutRegion

            Dim region As Rectangle
            Dim placed As List(Of RectangleF)

            Public Overrides Function ToString() As String
                Return $"{placed.Count} objects was placed in region [{region.ToString}]"
            End Function

            Public Shared Function Create(place As Regions, center As PointF, region As GraphicsRegion) As LayoutRegion
                Dim rect As Rectangle

                Select Case place
                    Case Regions.BottomLeft

                    Case Regions.BottomRight

                    Case Regions.TopLeft

                    Case Regions.TopRight

                End Select

                Return New LayoutRegion With {
                    .placed = New List(Of RectangleF),
                    .region = rect
                }
            End Function
        End Structure

        ''' <summary>
        ''' 已经被占用的位置区域
        ''' </summary>
        Dim occupied As New Dictionary(Of Regions, LayoutRegion)

        ReadOnly center As PointF
        ReadOnly region As GraphicsRegion

        ''' <summary>
        ''' 画布的大小
        ''' </summary>
        ''' <param name="canvasSize"></param>
        Sub New(canvasSize As Size, margin As Padding)
            region = New GraphicsRegion(canvasSize, margin)
            center = region.Size.GetCenter + New Point(margin.Left, margin.Top)

            occupied(Regions.BottomLeft) = LayoutRegion.Create(Regions.BottomLeft, center, region)
            occupied(Regions.BottomRight) = LayoutRegion.Create(Regions.BottomRight, center, region)
            occupied(Regions.TopLeft) = LayoutRegion.Create(Regions.TopLeft, center, region)
            occupied(Regions.TopRight) = LayoutRegion.Create(Regions.TopRight, center, region)
        End Sub

        Public Function InRegion(target As PointF) As Regions
            If target.X < center.X Then
                ' 左边
                If target.Y < center.Y Then
                    Return Regions.TopLeft
                Else
                    Return Regions.BottomLeft
                End If
            Else
                ' 右边
                If target.Y < center.Y Then
                    Return Regions.TopRight
                Else
                    Return Regions.BottomRight
                End If
            End If
        End Function

        ''' <summary>
        ''' 根据对象的密度来决定位置
        ''' </summary>
        ''' <param name="region"></param>
        ''' <returns></returns>
        Public Function GetOffset(region As Regions) As Point
            Dim placed = occupied(region)

            ' 获取滑窗操作之中，密度最小的区域的坐标

        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="target">layout所围绕的目标对象</param>
        ''' <param name="size">所需要放置的对象的大小</param>
        ''' <returns></returns>
        Public Function Calculate(target As RectangleF, size As SizeF) As PointF
            Dim region As Regions = InRegion(target.Location)

            occupied(region).placed.Add(target)

            ' 因为在开始的时候会将target也防止到canvas之上，
            ' 所以这个init应该定义于添加操作的后面
            Dim init As Point = GetOffset(region)

        End Function
    End Class
End Namespace