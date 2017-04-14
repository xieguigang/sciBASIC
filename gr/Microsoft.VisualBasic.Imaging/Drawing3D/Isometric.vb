Imports Microsoft.VisualBasic
Imports System
Imports System.Collections.Generic

Namespace Drawing3D

    Public Class Isometric

        Private ReadOnly angle, scale As Double

        Private transformation As Double()()

        Private originX, originY As Double

        Private items As IList(Of Item) = New List(Of Item)

        Private ReadOnly lightAngle As Vector

        Private ReadOnly colorDifference As Double

        Private ReadOnly lightColor As Color

        Public Sub New()
            Me.angle = Math.PI / 6
            Me.scale = 70
            Me.transformation = New Double() {{Me.scale * Math.Cos(Me.angle), Me.scale * Math.Sin(Me.angle)}, {Me.scale * Math.Cos(Math.PI - Me.angle), Me.scale * Math.Sin(Math.PI - Me.angle)}}
            Dim lightPosition As New Vector(2, -1, 3)
            Me.lightAngle = lightPosition.normalize()
            Me.colorDifference = 0.2
            Me.lightColor = New Color(255, 255, 255)

        End Sub

        ''' <summary>
        ''' X rides along the angle extended from the origin
        ''' Y rides perpendicular to this angle (in isometric view: PI - angle)
        ''' Z affects the y coordinate of the drawn point
        ''' </summary>
        Public Overridable Function translatePoint(ByVal ___point As Point) As Point
            Return New Point(Me.originX + ___point.x * Me.transformation(0)(0) + ___point.y * Me.transformation(1)(0), Me.originY - ___point.x * Me.transformation(0)(1) - ___point.y * Me.transformation(1)(1) - (___point.z * Me.scale))
        End Function

        Public Overridable Sub add(ByVal ___path As Path, ByVal color As Color)
            addPath(___path, color)
        End Sub

        Public Overridable Sub add(ByVal paths As Path(), ByVal color As Color)
            For Each ___path As Path In paths
                add(___path, color)
            Next ___path
        End Sub

        Public Overridable Sub add(ByVal ___shape As Shape, ByVal color As Color)
            ' Fetch paths ordered by distance to prevent overlaps 
            Dim paths As Path() = ___shape.orderedPaths()

            For Each ___path As Path In paths
                addPath(___path, color)
            Next ___path
        End Sub

        Public Overridable Sub clear()
            items.Clear()
        End Sub

        Private Sub addPath(ByVal ___path As Path, ByVal color As Color)
            Me.items.Add(New Item(___path, transformColor(___path, color)))
        End Sub

        Private Function transformColor(ByVal ___path As Path, ByVal color As Color) As Color
            Dim p1 As Point = ___path.points(1)
            Dim p2 As Point = ___path.points(0)
            Dim i As Double = p2.x - p1.x
            Dim j As Double = p2.y - p1.y
            Dim k As Double = p2.z - p1.z
            p1 = ___path.points(2)
            p2 = ___path.points(1)
            Dim i2 As Double = p2.x - p1.x
            Dim j2 As Double = p2.y - p1.y
            Dim k2 As Double = p2.z - p1.z
            Dim i3 As Double = j * k2 - j2 * k
            Dim j3 As Double = -1 * (i * k2 - i2 * k)
            Dim k3 As Double = i * j2 - i2 * j
            Dim magnitude As Double = Math.Sqrt(i3 * i3 + j3 * j3 + k3 * k3)
            i = If(magnitude = 0, 0, i3 / magnitude)
            j = If(magnitude = 0, 0, j3 / magnitude)
            k = If(magnitude = 0, 0, k3 / magnitude)
            Dim brightness As Double = i * lightAngle.i + j * lightAngle.j + k * lightAngle.k
            Return color.lighten(brightness * Me.colorDifference, Me.lightColor)
        End Function

        Public Overridable Sub measure(ByVal width As Integer, ByVal height As Integer, ByVal sort As Boolean)
            Me.originX = width \ 2
            Me.originY = height * 0.9

            For Each item As Item In items

                item.transformedPoints = New Point(item.path.points.Length - 1) {}

                If Not item.drawPath.Empty Then item.drawPath.rewind() 'Todo: test if .reset is not needed and rewind is enough

                Dim ___point As Point
                For i As Integer = 0 To item.path.points.Length - 1
                    ___point = item.path.points(i)
                    item.transformedPoints(i) = translatePoint(___point)
                Next i

                item.drawPath.moveTo(CSng(item.transformedPoints(0).x), CSng(item.transformedPoints(0).y))

                Dim i As Integer = 1
                Dim length As Integer = item.transformedPoints.Length
                Do While i < length
                    item.drawPath.lineTo(CSng(item.transformedPoints(i).x), CSng(item.transformedPoints(i).y))
                    i += 1
                Loop

                item.drawPath.close()
            Next item

            If sort Then Me.items = sortPaths()
        End Sub

        Private Function sortPaths() As IList(Of Item)
            Dim sortedItems As New List(Of Item)
            Dim observer As New Point(-10, -10, 20)
            Dim length As Integer = items.Count
            Dim drawBefore As IList(Of IList(Of Integer?)) = New List(Of IList(Of Integer?))(length)
            For i As Integer = 0 To length - 1
                drawBefore.Insert(i, New List(Of Integer?))
            Next i
            Dim itemA As Item
            Dim itemB As Item
            For i As Integer = 0 To length - 1
                itemA = items(i)
                For j As Integer = 0 To i - 1
                    itemB = items(j)
                    If hasIntersection(itemA.transformedPoints, itemB.transformedPoints) Then
                        Dim cmpPath As Integer = itemA.path.closerThan(itemB.path, observer)
                        If cmpPath < 0 Then
                            drawBefore(i).Add(j)
                        ElseIf cmpPath > 0 Then
                            drawBefore(j).Add(i)
                        End If
                    End If
                Next j
            Next i
            Dim drawThisTurn As Integer = 1
            Dim currItem As Item
            Dim integers As IList(Of Integer?)
            Do While drawThisTurn = 1
                drawThisTurn = 0
                For i As Integer = 0 To length - 1
                    currItem = items(i)
                    integers = drawBefore(i)
                    If currItem.drawn = 0 Then
                        Dim canDraw As Integer = 1
                        Dim j As Integer = 0
                        Dim lengthIntegers As Integer = integers.Count
                        Do While j < lengthIntegers
                            If items(integers(j)).drawn = 0 Then
                                canDraw = 0
                                Exit Do
                            End If
                            j += 1
                        Loop
                        If canDraw = 1 Then
                            Dim item As New Item(currItem)
                            sortedItems.Add(item)
                            currItem.drawn = 1
                            items(i) = currItem
                            drawThisTurn = 1
                        End If
                    End If
                Next i
            Loop

            For i As Integer = 0 To length - 1
                currItem = items(i)
                If currItem.drawn = 0 Then sortedItems.Add(New Item(currItem))
            Next i
            Return sortedItems
        End Function

        Public Overridable Sub draw(ByVal canvas As android.graphics.Canvas)
            For Each item As Item In items
                '            this.ctx.globalAlpha = color.a;
                '            this.ctx.fillStyle = this.ctx.strokeStyle = color.toHex();
                '            this.ctx.stroke();
                '            this.ctx.fill();
                '            this.ctx.restore();
                canvas.drawPath(item.drawPath, item.paint)
            Next item
        End Sub

        'Todo: use android.grphics region object to check if point is inside region
        'Todo: use path.op to check if the path intersects with another path
        'JAVA TO VB CONVERTER TODO TASK: Most Java annotations will not have direct .NET equivalent attributes:
        Public Overridable Function findItemForPosition(ByVal position As Point) As Item
            'Todo: reverse sorting for click detection, because hidden object is getting drawed first und will be returned as the first as well
            'Items are already sorted for depth sort so break should not be a problem here
            For Each item As Item In Me.items
                If item.transformedPoints Is Nothing Then Continue For
                Dim items As IList(Of Point) = New List(Of Point)
                Dim top As Point = Nothing, bottom As Point = Nothing, left As Point = Nothing, right As Point = Nothing
                For Each ___point As Point In item.transformedPoints
                    If top Is Nothing OrElse ___point.y > top.y Then
                        If top Is Nothing Then
                            top = New Point(___point.x, ___point.y)
                        Else
                            top.y = ___point.y
                            top.x = ___point.x
                        End If
                    End If
                    If bottom Is Nothing OrElse ___point.y < bottom.y Then
                        If bottom Is Nothing Then
                            bottom = New Point(___point.x, ___point.y)
                        Else
                            bottom.y = ___point.y
                            bottom.x = ___point.x
                        End If
                    End If
                    If left Is Nothing OrElse ___point.x < left.x Then
                        If left Is Nothing Then
                            left = New Point(___point.x, ___point.y)
                        Else
                            left.x = ___point.x
                            left.y = ___point.y
                        End If
                    End If
                    If right Is Nothing OrElse ___point.x > right.x Then
                        If right Is Nothing Then
                            right = New Point(___point.x, ___point.y)
                        Else
                            right.x = ___point.x
                            right.y = ___point.y
                        End If
                    End If
                Next ___point

                items.Add(left)
                items.Add(top)
                items.Add(right)
                items.Add(bottom)

                'search for equal points that are above or below for left and right or left and right for bottom and top
                For Each ___point As Point In item.transformedPoints
                    If ___point.x = left.x Then
                        If ___point.y <> left.y Then items.Add(___point)
                    End If
                    If ___point.x = right.x Then
                        If ___point.y <> right.y Then items.Add(___point)
                    End If
                    If ___point.y = top.y Then
                        If ___point.y <> top.y Then items.Add(___point)
                    End If
                    If ___point.y = bottom.y Then
                        If ___point.y <> bottom.y Then items.Add(___point)
                    End If
                Next ___point

                If isPointInPoly(items, position.x, position.y) Then Return item
            Next item
            Return Nothing
        End Function

        Friend Class Item
            Friend path As Path
            Friend baseColor As Color
            Friend paint As android.graphics.Paint
            Friend drawn As Integer
            Friend transformedPoints As Point()
            Friend drawPath As android.graphics.Path

            Friend Sub New(ByVal item As Item)
                transformedPoints = item.transformedPoints
                drawPath = item.drawPath
                drawn = item.drawn
                Me.paint = item.paint
                Me.path = item.path
                Me.baseColor = item.baseColor
            End Sub

            Friend Sub New(ByVal ___path As Path, ByVal baseColor As Color)
                drawPath = New android.graphics.Path
                drawn = 0
                Me.paint = New android.graphics.Paint(android.graphics.Paint.ANTI_ALIAS_FLAG)
                Me.paint.Style = android.graphics.Paint.Style.FILL_AND_STROKE
                Me.paint.StrokeWidth = 1
                Me.path = ___path
                Me.baseColor = baseColor
                Me.paint.Color = android.graphics.Color.argb(CInt(Fix(baseColor.a)), CInt(Fix(baseColor.r)), CInt(Fix(baseColor.g)), CInt(Fix(baseColor.b)))
            End Sub
        End Class

        Private Function isPointInPoly(ByVal poly As IList(Of Point), ByVal x As Double, ByVal y As Double) As Boolean
            Dim c As Boolean = False
            Dim i As Integer = -1
            Dim l As Integer = poly.Count
            Dim j As Integer = l - 1
            'JAVA TO VB CONVERTER TODO TASK: Assignments within expressions are not supported in VB
            Do While i += 1 < l
				If ((poly(i).y <= y AndAlso y < poly(j).y) OrElse (poly(j).y <= y AndAlso y < poly(i).y)) AndAlso (x < (poly(j).x - poly(i).x) * (y - poly(i).y) / (poly(j).y - poly(i).y) + poly(i).x) Then c = Not c
                j = i
            Loop
            Return c
        End Function

        Private Function isPointInPoly(ByVal poly As Point(), ByVal x As Double, ByVal y As Double) As Boolean
            Dim c As Boolean = False
            Dim i As Integer = -1
            Dim l As Integer = poly.Length
            Dim j As Integer = l - 1
            'JAVA TO VB CONVERTER TODO TASK: Assignments within expressions are not supported in VB
            Do While i += 1 < l
				If ((poly(i).y <= y AndAlso y < poly(j).y) OrElse (poly(j).y <= y AndAlso y < poly(i).y)) AndAlso (x < (poly(j).x - poly(i).x) * (y - poly(i).y) / (poly(j).y - poly(i).y) + poly(i).x) Then c = Not c
                j = i
            Loop
            Return c
        End Function

        Private Function hasIntersection(ByVal pointsA As Point(), ByVal pointsB As Point()) As Boolean
            Dim i As Integer, j As Integer, lengthA As Integer = pointsA.Length, lengthB As Integer = pointsB.Length, lengthPolyA As Integer, lengthPolyB As Integer
            Dim AminX As Double = pointsA(0).x
            Dim AminY As Double = pointsA(0).y
            Dim AmaxX As Double = AminX
            Dim AmaxY As Double = AminY
            Dim BminX As Double = pointsB(0).x
            Dim BminY As Double = pointsB(0).y
            Dim BmaxX As Double = BminX
            Dim BmaxY As Double = BminY

            Dim ___point As Point

            For i = 0 To lengthA - 1
                ___point = pointsA(i)
                AminX = Math.Min(AminX, ___point.x)
                AminY = Math.Min(AminY, ___point.y)
                AmaxX = Math.Max(AmaxX, ___point.x)
                AmaxY = Math.Max(AmaxY, ___point.y)
            Next i
            For i = 0 To lengthB - 1
                ___point = pointsB(i)
                BminX = Math.Min(BminX, ___point.x)
                BminY = Math.Min(BminY, ___point.y)
                BmaxX = Math.Max(BmaxX, ___point.x)
                BmaxY = Math.Max(BmaxY, ___point.y)
            Next i

            If ((AminX <= BminX AndAlso BminX <= AmaxX) OrElse (BminX <= AminX AndAlso AminX <= BmaxX)) AndAlso ((AminY <= BminY AndAlso BminY <= AmaxY) OrElse (BminY <= AminY AndAlso AminY <= BmaxY)) Then
                ' now let's be more specific
                Dim polyA As Point() = Path.add(pointsA(0), pointsA)
                Dim polyB As Point() = Path.add(pointsB(0), pointsB)

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
                    ___point = polyA(i)
                    deltaAX(i) = polyA(i + 1).x - ___point.x
                    deltaAY(i) = polyA(i + 1).y - ___point.y
                    'equation written as deltaY.x - deltaX.y + r = 0
                    rA(i) = deltaAX(i) * ___point.y - deltaAY(i) * ___point.x
                Next i

                For i = 0 To lengthPolyB - 2
                    ___point = polyB(i)
                    deltaBX(i) = polyB(i + 1).x - ___point.x
                    deltaBY(i) = polyB(i + 1).y - ___point.y
                    rB(i) = deltaBX(i) * ___point.y - deltaBY(i) * ___point.x
                Next i

                For i = 0 To lengthPolyA - 2
                    For j = 0 To lengthPolyB - 2
                        If deltaAX(i) * deltaBY(j) <> deltaAY(i) * deltaBX(j) Then
                            'case when vectors are colinear, or one polygon included in the other, is covered after
                            'two segments cross each other if and only if the points of the first are on each side of the line defined by the second and vice-versa
                            If (deltaAY(i) * polyB(j).x - deltaAX(i) * polyB(j).y + rA(i)) * (deltaAY(i) * polyB(j + 1).x - deltaAX(i) * polyB(j + 1).y + rA(i)) < -0.000000001 AndAlso (deltaBY(j) * polyA(i).x - deltaBX(j) * polyA(i).y + rB(j)) * (deltaBY(j) * polyA(i + 1).x - deltaBX(j) * polyA(i + 1).y + rB(j)) < -0.000000001 Then Return True
                        End If
                    Next j
                Next i

                For i = 0 To lengthPolyA - 2
                    ___point = polyA(i)
                    If isPointInPoly(polyB, ___point.x, ___point.y) Then Return True
                Next i
                For i = 0 To lengthPolyB - 2
                    ___point = polyB(i)
                    If isPointInPoly(polyA, ___point.x, ___point.y) Then Return True
                Next i

                Return False
            Else
                Return False
            End If
        End Function
    End Class

End Namespace