#Region "Microsoft.VisualBasic::44f3498ebf109f53e3e8753839c42588, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Isometric.vb"

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

    '   Total Lines: 464
    '    Code Lines: 347
    ' Comment Lines: 54
    '   Blank Lines: 63
    '     File Size: 19.31 KB


    '     Class IsometricEngine
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: FindItemForPosition, GetEnumerator, IEnumerable_GetEnumerator, IntersectionWith, (+2 Overloads) IsPointInPoly
    '                   sortPaths, TranslatePoint
    ' 
    '         Sub: (+3 Overloads) Add, AddPath, Clear, Draw, Measure
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Device
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models.Isometric
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq
Imports stdNum = System.Math

Namespace Drawing3D

    ''' <summary>
    ''' Isometric 3d graphics painter
    ''' </summary>
    Public Class IsometricEngine : Implements IEnumerable(Of Surface)

        Dim transformation As Double()()
        Dim originX, originY As Double

        ''' <summary>
        ''' The 3D models
        ''' </summary>
        Dim models As New List(Of Model2D)

        ReadOnly lightAngle As Point3D
        ReadOnly colorDifference As Double
        ReadOnly lightColor As Color
        ReadOnly angle, scale As Double

        Public Sub New()
            Me.angle = stdNum.PI / 6
            Me.scale = 70
            Me.transformation = {
                ({Me.scale * stdNum.Cos(Me.angle), Me.scale * stdNum.Sin(Me.angle)}),
                ({Me.scale * stdNum.Cos(stdNum.PI - Me.angle), Me.scale * stdNum.Sin(stdNum.PI - Me.angle)})
            }
            Me.lightAngle = New Point3D(2, -1, 3).Normalize()
            Me.colorDifference = 0.2
            Me.lightColor = Color.FromArgb(255, 255, 255)
        End Sub

        ''' <summary>
        ''' + X rides along the angle extended from the origin
        ''' + Y rides perpendicular to this angle (in isometric view: PI - angle)
        ''' + Z affects the y coordinate of the drawn point
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function TranslatePoint(point As Point3D) As Point3D
            Return New Point3D(
                Me.originX + point.X * Me.transformation(0)(0) + point.Y * Me.transformation(1)(0),
                Me.originY - point.X * Me.transformation(0)(1) - point.Y * Me.transformation(1)(1) - (point.Z * Me.scale))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Add(path As Path3D, color As Color)
            AddPath(path, color)
        End Sub

        Public Sub Add(paths As Path3D(), color As Color)
            For Each path As Path3D In paths
                Add(path, color)
            Next
        End Sub

        Public Sub Add(shape As Shape3D, color As Color)
            ' Fetch paths ordered by distance to prevent overlaps 
            Dim paths As Path3D() = shape.OrderedPath3Ds()

            For Each path As Path3D In paths
                Call AddPath(path, color)
            Next
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub Clear()
            Call models.Clear()
        End Sub

        ''' <summary>
        ''' 这个函数会同时对曲面进行光照处理
        ''' </summary>
        ''' <param name="path"></param>
        ''' <param name="color"></param>
        Private Sub AddPath(path As Path3D, color As Color)
            color = path.Lighting(lightAngle, color, colorDifference, lightColor)
            models.Add(New Model2D(path, color))
        End Sub

        ''' <summary>
        ''' The 3D projection and model matrix transformation.
        ''' (在绘图前面需要调用这个方法进行图形合成)
        ''' </summary>
        ''' <param name="width"></param>
        ''' <param name="height"></param>
        ''' <param name="sort"></param>
        Public Sub Measure(width As Integer, height As Integer, sort As Boolean)
            Me.originX = width \ 2
            Me.originY = height * 0.9

            For Each model As Model2D In models

                model.TransformedPoints = New Point3D(model.path.Points.Count - 1) {}

                ' Todo: test if .reset is not needed and rewind is enough
                If Not model.DrawPath Is Nothing Then
                    model.DrawPath.Rewind()
                End If

                Dim i As Integer = 0
                Dim point As Point3D

                For i% = 0 To model.path.Points.Count - 1
                    point = model.path.Points(i)
                    model.TransformedPoints(i) = TranslatePoint(point)
                Next

                Dim length As Integer = model.TransformedPoints.Length

                Call model.DrawPath.MoveTo(CSng(model.TransformedPoints(0).X), CSng(model.TransformedPoints(0).Y))

                i = 1
                Do While i < length
                    model.DrawPath.LineTo(CSng(model.TransformedPoints(i).X), CSng(model.TransformedPoints(i).Y))
                    i += 1
                Loop

                model.DrawPath.CloseAllFigures()
            Next

            If sort Then
                Me.models = sortPaths()
            End If
        End Sub

        Private Function sortPaths() As IList(Of Model2D)
            Dim sortedItems As New List(Of Model2D)
            Dim observer As New Point3D(-10, -10, 20)
            Dim length As Integer = models.Count
            Dim drawBefore As New List(Of IList(Of Integer))(length)

            For i As Integer = 0 To length - 1
                drawBefore.Insert(i, New List(Of Integer))
            Next

            Dim itemA As Model2D
            Dim itemB As Model2D

            For i As Integer = 0 To length - 1
                itemA = models(i)
                For j As Integer = 0 To i - 1
                    itemB = models(j)
                    If IntersectionWith(itemA.TransformedPoints, itemB.TransformedPoints) Then
                        Dim cmpPath As Integer = itemA.path.CloserThan(itemB.path, observer)
                        If cmpPath < 0 Then
                            drawBefore(i).Add(j)
                        ElseIf cmpPath > 0 Then
                            drawBefore(j).Add(i)
                        End If
                    End If
                Next
            Next

            Dim drawThisTurn As Integer = 1
            Dim currItem As Model2D
            Dim integers As List(Of Integer)

            Do While drawThisTurn = 1
                drawThisTurn = 0
                For i As Integer = 0 To length - 1
                    currItem = models(i)
                    integers = drawBefore(i)
                    If currItem.drawn = 0 Then
                        Dim canDraw As Integer = 1
                        Dim j As Integer = 0
                        Dim lengthIntegers As Integer = integers.Count
                        Do While j < lengthIntegers
                            If models(integers(j)).drawn = 0 Then
                                canDraw = 0
                                Exit Do
                            End If
                            j += 1
                        Loop
                        If canDraw = 1 Then
                            Dim item As New Model2D(currItem)
                            sortedItems.Add(item)
                            currItem.drawn = 1
                            models(i) = currItem
                            drawThisTurn = 1
                        End If
                    End If
                Next
            Loop

            For i As Integer = 0 To length - 1
                currItem = models(i)
                If currItem.drawn = 0 Then
                    sortedItems.Add(New Model2D(currItem))
                End If
            Next
            Return sortedItems
        End Function

        ''' <summary>
        ''' Draw the 3D graphics onto the canvas device object.(进行三维图形绘图操作)
        ''' </summary>
        ''' <param name="canvas"></param>
        Public Sub Draw(ByRef canvas As IGraphics)
            ' 进行图形合成
            With canvas.Size
                Call Me.Measure(.Width, .Height, True)
            End With

            For Each model2D As Model2D In models
                If model2D.isDot Then
                    Throw New NotImplementedException
                ElseIf model2D.isLine Then
                    canvas.DrawLine(New Pen(model2D.baseColor), model2D.TransformedPoints(0).PointXY(canvas.Size), model2D.TransformedPoints(1).PointXY(canvas.Size))
                Else
                    With model2D
                        Call canvas.FillPath(.Paint, .DrawPath.Path)
                        ' 对于线条而言，直接使用DrawPath来进行绘制
                        Call canvas.DrawPath(New Pen(.Paint), .DrawPath.Path)
                    End With
                End If
            Next
        End Sub

        'Todo: use android.grphics region object to check if point is inside region
        'Todo: use path.op to check if the path intersects with another path
        Public Function FindItemForPosition(position As Point3D) As Model2D
            'Todo: reverse sorting for click detection, because hidden object is getting drawed first und will be returned as the first as well
            'Items are already sorted for depth sort so break should not be a problem here
            For Each m2 As Model2D In Me.models
                If m2.TransformedPoints Is Nothing Then
                    Continue For
                End If

                Dim items As New List(Of Point3D)
                Dim top As Point3D = Nothing, bottom As Point3D = Nothing, left As Point3D = Nothing, right As Point3D = Nothing

                For Each point As Point3D In m2.TransformedPoints
                    If top = 0! OrElse point.Y > top.Y Then
                        If top = 0! Then
                            top = New Point3D(point.X, point.Y)
                        Else
                            top.Y = point.Y
                            top.X = point.X
                        End If
                    End If
                    If bottom = 0! OrElse point.Y < bottom.Y Then
                        If bottom = 0! Then
                            bottom = New Point3D(point.X, point.Y)
                        Else
                            bottom.Y = point.Y
                            bottom.X = point.X
                        End If
                    End If
                    If left = 0! OrElse point.X < left.X Then
                        If left = 0! Then
                            left = New Point3D(point.X, point.Y)
                        Else
                            left.X = point.X
                            left.Y = point.Y
                        End If
                    End If
                    If right = 0! OrElse point.X > right.X Then
                        If right = 0! Then
                            right = New Point3D(point.X, point.Y)
                        Else
                            right.X = point.X
                            right.Y = point.Y
                        End If
                    End If
                Next

                items.Add(left)
                items.Add(top)
                items.Add(right)
                items.Add(bottom)

                'search for equal points that are above or below for left and right or left and right for bottom and top
                For Each point As Point3D In m2.TransformedPoints
                    If point.X = left.X Then
                        If point.Y <> left.Y Then items.Add(point)
                    End If
                    If point.X = right.X Then
                        If point.Y <> right.Y Then items.Add(point)
                    End If
                    If point.Y = top.Y Then
                        If point.Y <> top.Y Then items.Add(point)
                    End If
                    If point.Y = bottom.Y Then
                        If point.Y <> bottom.Y Then items.Add(point)
                    End If
                Next

                If IsPointInPoly(items, position.X, position.Y) Then
                    Return m2
                End If
            Next

            Return Nothing
        End Function

        ''' <summary>
        ''' 点对象是否处于多边形对象<paramref name="poly"/>之中
        ''' </summary>
        ''' <param name="poly"></param>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        Private Function IsPointInPoly(poly As IList(Of Point3D), x As Double, y As Double) As Boolean
            Dim c As Boolean = False
            Dim i As i32 = -1
            Dim l As Integer = poly.Count
            Dim j As Integer = l - 1

            Do While ++i < l - 1
                If ((poly(i).Y <= y AndAlso y < poly(j).Y) OrElse
                    (poly(j).Y <= y AndAlso y < poly(i).Y)) AndAlso
                    (x < (poly(j).X - poly(i).X) * (y - poly(i).Y) / (poly(j).Y - poly(i).Y) + poly(i).X) Then

                    c = Not c
                End If

                j = i
            Loop

            Return c
        End Function

        Private Function IsPointInPoly(poly As Point3D(), x As Double, y As Double) As Boolean
            Dim c As Boolean = False
            Dim i As i32 = -1
            Dim l As Integer = poly.Length
            Dim j As Integer = l - 1

            Do While ++i < l - 1
                If ((poly(i).Y <= y AndAlso y < poly(j).Y) OrElse
                    (poly(j).Y <= y AndAlso y < poly(i).Y)) AndAlso
                    (x < (poly(j).X - poly(i).X) * (y - poly(i).Y) / (poly(j).Y - poly(i).Y) + poly(i).X) Then

                    c = Not c
                End If

                j = i
            Loop
            Return c
        End Function

        ''' <summary>
        ''' 判断两个多边形是否具有相交的部分
        ''' </summary>
        ''' <param name="pointsA"></param>
        ''' <param name="pointsB"></param>
        ''' <returns></returns>
        Private Function IntersectionWith(pointsA As Point3D(), pointsB As Point3D()) As Boolean
            Dim i As Integer, j As Integer, lengthA As Integer = pointsA.Length, lengthB As Integer = pointsB.Length, lengthPolyA As Integer, lengthPolyB As Integer
            Dim AminX As Double = pointsA(0).X
            Dim AminY As Double = pointsA(0).Y
            Dim AmaxX As Double = AminX
            Dim AmaxY As Double = AminY
            Dim BminX As Double = pointsB(0).X
            Dim BminY As Double = pointsB(0).Y
            Dim BmaxX As Double = BminX
            Dim BmaxY As Double = BminY

            Dim point As Point3D

            For i = 0 To lengthA - 1
                point = pointsA(i)
                AminX = stdNum.Min(AminX, point.X)
                AminY = stdNum.Min(AminY, point.Y)
                AmaxX = stdNum.Max(AmaxX, point.X)
                AmaxY = stdNum.Max(AmaxY, point.Y)
            Next
            For i = 0 To lengthB - 1
                point = pointsB(i)
                BminX = stdNum.Min(BminX, point.X)
                BminY = stdNum.Min(BminY, point.Y)
                BmaxX = stdNum.Max(BmaxX, point.X)
                BmaxY = stdNum.Max(BmaxY, point.Y)
            Next

            If ((AminX <= BminX AndAlso BminX <= AmaxX) OrElse (BminX <= AminX AndAlso AminX <= BmaxX)) AndAlso ((AminY <= BminY AndAlso BminY <= AmaxY) OrElse (BminY <= AminY AndAlso AminY <= BmaxY)) Then
                ' now let's be more specific
                Dim polyA As Point3D() = {pointsA(0)}.JoinIterates(pointsA).ToArray
                Dim polyB As Point3D() = {pointsB(0)}.JoinIterates(pointsB).ToArray

                ' see if edges cross, or one contained in the other
                lengthPolyA = polyA.Length
                lengthPolyB = polyB.Length

                Dim deltaAX As Double() = New Double(lengthPolyA - 1) {}
                Dim deltaAY As Double() = New Double(lengthPolyA - 1) {}
                Dim deltaBX As Double() = New Double(lengthPolyB - 1) {}
                Dim deltaBY As Double() = New Double(lengthPolyB - 1) {}

                Dim rA As Double() = New Double(lengthPolyA - 1) {}
                Dim rB As Double() = New Double(lengthPolyB - 1) {}

                For i = 0 To lengthPolyA - 2
                    point = polyA(i)
                    deltaAX(i) = polyA(i + 1).X - point.X
                    deltaAY(i) = polyA(i + 1).Y - point.Y
                    'equation written as deltaY.x - deltaX.y + r = 0
                    rA(i) = deltaAX(i) * point.Y - deltaAY(i) * point.X
                Next

                For i = 0 To lengthPolyB - 2
                    point = polyB(i)
                    deltaBX(i) = polyB(i + 1).X - point.X
                    deltaBY(i) = polyB(i + 1).Y - point.Y
                    rB(i) = deltaBX(i) * point.Y - deltaBY(i) * point.X
                Next

                For i = 0 To lengthPolyA - 2
                    For j = 0 To lengthPolyB - 2
                        If deltaAX(i) * deltaBY(j) <> deltaAY(i) * deltaBX(j) Then
                            'case when vectors are colinear, or one polygon included in the other, is covered after
                            'two segments cross each other if and only if the points of the first are on each side of the line defined by the second and vice-versa
                            If (deltaAY(i) * polyB(j).X - deltaAX(i) * polyB(j).Y + rA(i)) * (deltaAY(i) * polyB(j + 1).X - deltaAX(i) * polyB(j + 1).Y + rA(i)) < -0.000000001 AndAlso
                                (deltaBY(j) * polyA(i).X - deltaBX(j) * polyA(i).Y + rB(j)) * (deltaBY(j) * polyA(i + 1).X - deltaBX(j) * polyA(i + 1).Y + rB(j)) < -0.000000001 Then
                                Return True
                            End If
                        End If
                    Next j
                Next

                For i = 0 To lengthPolyA - 2
                    point = polyA(i)
                    If IsPointInPoly(polyB, point.X, point.Y) Then
                        Return True
                    End If
                Next
                For i = 0 To lengthPolyB - 2
                    point = polyB(i)
                    If IsPointInPoly(polyA, point.X, point.Y) Then
                        Return True
                    End If
                Next

                Return False
            Else
                Return False
            End If
        End Function

        Public Iterator Function GetEnumerator() As IEnumerator(Of Surface) Implements IEnumerable(Of Surface).GetEnumerator
            For Each model As Model2D In models
                Yield New Surface With {
                    .vertices = model.path _
                        .Points _
                        .ToArray,
                    .brush = model.Paint
                }
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Class
End Namespace
