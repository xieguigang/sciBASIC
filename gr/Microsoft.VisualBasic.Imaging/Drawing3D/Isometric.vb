Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.Drawing3D.IsoMetric
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace Drawing3D

    Public Class IsometricEngine

        Private ReadOnly angle, scale As Double

        Private transformation As Double()()

        Private originX, originY As Double

        Dim models As New List(Of Model2D)

        Private ReadOnly lightAngle As Point3D

        Private ReadOnly colorDifference As Double

        Private ReadOnly lightColor As Color

        Public Sub New()
            Me.angle = Math.PI / 6
            Me.scale = 70
            Me.transformation = {
                ({Me.scale * Math.Cos(Me.angle), Me.scale * Math.Sin(Me.angle)}),
                ({Me.scale * Math.Cos(Math.PI - Me.angle), Me.scale * Math.Sin(Math.PI - Me.angle)})
            }
            Dim lightPosition As New Point3D(2, -1, 3)
            Me.lightAngle = lightPosition.Normalize()
            Me.colorDifference = 0.2
            Me.lightColor = Color.FromArgb(255, 255, 255)
        End Sub

        ''' <summary>
        ''' X rides along the angle extended from the origin
        ''' Y rides perpendicular to this angle (in isometric view: PI - angle)
        ''' Z affects the y coordinate of the drawn point
        ''' </summary>
        Public Function translatePoint(___point As Point3D) As Point3D
            Return New Point3D(
                Me.originX + ___point.X * Me.transformation(0)(0) + ___point.Y * Me.transformation(1)(0),
                Me.originY - ___point.X * Me.transformation(0)(1) - ___point.Y * Me.transformation(1)(1) - (___point.Z * Me.scale))
        End Function

        Public Sub add(___path As Path3D, color As HSLColor)
            addPath(___path, color)
        End Sub

        Public Sub add(paths As Path3D(), color As HSLColor)
            For Each ___path As Path3D In paths
                add(___path, color)
            Next ___path
        End Sub

        Public Sub add(___shape As Shape3D, color As HSLColor)
            ' Fetch paths ordered by distance to prevent overlaps 
            Dim paths As Path3D() = ___shape.orderedPath3Ds()

            For Each ___path As Path3D In paths
                addPath(___path, color)
            Next ___path
        End Sub

        Public Sub clear()
            models.Clear()
        End Sub

        Private Sub addPath(___path As Path3D, color As HSLColor)
            Me.models.Add(New Model2D(___path, transformColor(___path, color)))
        End Sub

        Private Function transformColor(___path As Path3D, color As HSLColor) As Color
            Dim p1 As Point3D = ___path.Points(1)
            Dim p2 As Point3D = ___path.Points(0)
            Dim i As Double = p2.X - p1.X
            Dim j As Double = p2.Y - p1.Y
            Dim k As Double = p2.Z - p1.Z
            p1 = ___path.Points(2)
            p2 = ___path.Points(1)
            Dim i2 As Double = p2.X - p1.X
            Dim j2 As Double = p2.Y - p1.Y
            Dim k2 As Double = p2.Z - p1.Z
            Dim i3 As Double = j * k2 - j2 * k
            Dim j3 As Double = -1 * (i * k2 - i2 * k)
            Dim k3 As Double = i * j2 - i2 * j
            Dim magnitude As Double = Math.Sqrt(i3 * i3 + j3 * j3 + k3 * k3)
            i = If(magnitude = 0, 0, i3 / magnitude)
            j = If(magnitude = 0, 0, j3 / magnitude)
            k = If(magnitude = 0, 0, k3 / magnitude)
            Dim brightness As Double = i * lightAngle.X + j * lightAngle.Y + k * lightAngle.Z
            Return color.Lighten(brightness * Me.colorDifference, Me.lightColor)
        End Function

        Public Sub measure(width As Integer, height As Integer, sort As Boolean)
            Me.originX = width \ 2
            Me.originY = height * 0.9

            For Each item As Model2D In models

                item.transformedPoints = New Point3D(item.path.Points.Count - 1) {}

                If Not item.drawPath Is Nothing Then
                    item.drawPath.Rewind() 'Todo: test if .reset is not needed and rewind is enough
                End If
                Dim i As Integer = 0
                Dim ___point As Point3D
                For i% = 0 To item.path.Points.Count - 1
                    ___point = item.path.Points(i)
                    item.transformedPoints(i) = translatePoint(___point)
                Next

                Dim length As Integer = item.transformedPoints.Length

                Call item.drawPath.MoveTo(CSng(item.transformedPoints(0).X), CSng(item.transformedPoints(0).Y))

                i = 1
                Do While i < length
                    item.drawPath.LineTo(CSng(item.transformedPoints(i).X), CSng(item.transformedPoints(i).Y))
                    i += 1
                Loop

                item.drawPath.CloseAllFigures()
            Next item

            If sort Then Me.models = sortPaths()
        End Sub

        Private Function sortPaths() As IList(Of Model2D)
            Dim sortedItems As New List(Of Model2D)
            Dim observer As New Point3D(-10, -10, 20)
            Dim length As Integer = models.Count
            Dim drawBefore As New List(Of IList(Of Integer))(length)
            For i As Integer = 0 To length - 1
                drawBefore.Insert(i, New List(Of Integer))
            Next i
            Dim itemA As Model2D
            Dim itemB As Model2D
            For i As Integer = 0 To length - 1
                itemA = models(i)
                For j As Integer = 0 To i - 1
                    itemB = models(j)
                    If hasIntersection(itemA.transformedPoints, itemB.transformedPoints) Then
                        Dim cmpPath As Integer = itemA.path.CloserThan(itemB.path, observer)
                        If cmpPath < 0 Then
                            drawBefore(i).Add(j)
                        ElseIf cmpPath > 0 Then
                            drawBefore(j).Add(i)
                        End If
                    End If
                Next j
            Next i
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
                Next i
            Loop

            For i As Integer = 0 To length - 1
                currItem = models(i)
                If currItem.drawn = 0 Then sortedItems.Add(New Model2D(currItem))
            Next i
            Return sortedItems
        End Function

        ''' <summary>
        ''' 进行三维图形绘图操作
        ''' </summary>
        ''' <param name="canvas"></param>
        Public Sub Draw(ByRef canvas As IGraphics)
            For Each model2D As Model2D In models
                '            this.ctx.globalAlpha = color.a;
                '            this.ctx.fillStyle = this.ctx.strokeStyle = color.toHex();
                '            this.ctx.stroke();
                '            this.ctx.fill();
                '            this.ctx.restore();
                With model2D
                    Call canvas.DrawPath(.paint, .drawPath.Path)
                End With
            Next
        End Sub

        'Todo: use android.grphics region object to check if point is inside region
        'Todo: use path.op to check if the path intersects with another path
        Public Function findItemForPosition(position As Point3D) As Model2D
            'Todo: reverse sorting for click detection, because hidden object is getting drawed first und will be returned as the first as well
            'Items are already sorted for depth sort so break should not be a problem here
            For Each item As Model2D In Me.models
                If item.transformedPoints Is Nothing Then Continue For
                Dim items As IList(Of Point3D) = New List(Of Point3D)
                Dim top As Point3D = Nothing, bottom As Point3D = Nothing, left As Point3D = Nothing, right As Point3D = Nothing
                For Each ___point As Point3D In item.transformedPoints
                    If top = 0! OrElse ___point.Y > top.Y Then
                        If top = 0! Then
                            top = New Point3D(___point.X, ___point.Y)
                        Else
                            top.Y = ___point.Y
                            top.X = ___point.X
                        End If
                    End If
                    If bottom = 0! OrElse ___point.Y < bottom.Y Then
                        If bottom = 0! Then
                            bottom = New Point3D(___point.X, ___point.Y)
                        Else
                            bottom.Y = ___point.Y
                            bottom.X = ___point.X
                        End If
                    End If
                    If left = 0! OrElse ___point.X < left.X Then
                        If left = 0! Then
                            left = New Point3D(___point.X, ___point.Y)
                        Else
                            left.X = ___point.X
                            left.Y = ___point.Y
                        End If
                    End If
                    If right = 0! OrElse ___point.X > right.X Then
                        If right = 0! Then
                            right = New Point3D(___point.X, ___point.Y)
                        Else
                            right.X = ___point.X
                            right.Y = ___point.Y
                        End If
                    End If
                Next ___point

                items.Add(left)
                items.Add(top)
                items.Add(right)
                items.Add(bottom)

                'search for equal points that are above or below for left and right or left and right for bottom and top
                For Each ___point As Point3D In item.transformedPoints
                    If ___point.X = left.X Then
                        If ___point.Y <> left.Y Then items.Add(___point)
                    End If
                    If ___point.X = right.X Then
                        If ___point.Y <> right.Y Then items.Add(___point)
                    End If
                    If ___point.Y = top.Y Then
                        If ___point.Y <> top.Y Then items.Add(___point)
                    End If
                    If ___point.Y = bottom.Y Then
                        If ___point.Y <> bottom.Y Then items.Add(___point)
                    End If
                Next ___point

                If isPointInPoly(items, position.X, position.Y) Then Return item
            Next item
            Return Nothing
        End Function

        Private Function isPointInPoly(poly As IList(Of Point3D), x As Double, y As Double) As Boolean
            Dim c As Boolean = False
            Dim i As int = 0
            Dim l As Integer = poly.Count
            Dim j As Integer = l - 1

            Do While ++i < l
                If ((poly(i).Y <= y AndAlso y < poly(j).Y) OrElse (poly(j).Y <= y AndAlso y < poly(i).Y)) AndAlso (x < (poly(j).X - poly(i).X) * (y - poly(i).Y) / (poly(j).Y - poly(i).Y) + poly(i).X) Then
                    c = Not c
                End If
                j = i
            Loop

            Return c
        End Function

        Private Function isPointInPoly(poly As Point3D(), x As Double, y As Double) As Boolean
            Dim c As Boolean = False
            Dim i As int = 0
            Dim l As Integer = poly.Length
            Dim j As Integer = l - 1

            Do While ++i < l
                If ((poly(i).Y <= y AndAlso y < poly(j).Y) OrElse (poly(j).Y <= y AndAlso y < poly(i).Y)) AndAlso (x < (poly(j).X - poly(i).X) * (y - poly(i).Y) / (poly(j).Y - poly(i).Y) + poly(i).X) Then
                    c = Not c
                End If
                j = i
            Loop
            Return c
        End Function

        Private Function hasIntersection(pointsA As Point3D(), pointsB As Point3D()) As Boolean
            Dim i As Integer, j As Integer, lengthA As Integer = pointsA.Length, lengthB As Integer = pointsB.Length, lengthPolyA As Integer, lengthPolyB As Integer
            Dim AminX As Double = pointsA(0).X
            Dim AminY As Double = pointsA(0).Y
            Dim AmaxX As Double = AminX
            Dim AmaxY As Double = AminY
            Dim BminX As Double = pointsB(0).X
            Dim BminY As Double = pointsB(0).Y
            Dim BmaxX As Double = BminX
            Dim BmaxY As Double = BminY

            Dim ___point As Point3D

            For i = 0 To lengthA - 1
                ___point = pointsA(i)
                AminX = Math.Min(AminX, ___point.X)
                AminY = Math.Min(AminY, ___point.Y)
                AmaxX = Math.Max(AmaxX, ___point.X)
                AmaxY = Math.Max(AmaxY, ___point.Y)
            Next i
            For i = 0 To lengthB - 1
                ___point = pointsB(i)
                BminX = Math.Min(BminX, ___point.X)
                BminY = Math.Min(BminY, ___point.Y)
                BmaxX = Math.Max(BmaxX, ___point.X)
                BmaxY = Math.Max(BmaxY, ___point.Y)
            Next i

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
                    ___point = polyA(i)
                    deltaAX(i) = polyA(i + 1).X - ___point.X
                    deltaAY(i) = polyA(i + 1).Y - ___point.Y
                    'equation written as deltaY.x - deltaX.y + r = 0
                    rA(i) = deltaAX(i) * ___point.Y - deltaAY(i) * ___point.X
                Next i

                For i = 0 To lengthPolyB - 2
                    ___point = polyB(i)
                    deltaBX(i) = polyB(i + 1).X - ___point.X
                    deltaBY(i) = polyB(i + 1).Y - ___point.Y
                    rB(i) = deltaBX(i) * ___point.Y - deltaBY(i) * ___point.X
                Next i

                For i = 0 To lengthPolyA - 2
                    For j = 0 To lengthPolyB - 2
                        If deltaAX(i) * deltaBY(j) <> deltaAY(i) * deltaBX(j) Then
                            'case when vectors are colinear, or one polygon included in the other, is covered after
                            'two segments cross each other if and only if the points of the first are on each side of the line defined by the second and vice-versa
                            If (deltaAY(i) * polyB(j).X - deltaAX(i) * polyB(j).Y + rA(i)) * (deltaAY(i) * polyB(j + 1).X - deltaAX(i) * polyB(j + 1).Y + rA(i)) < -0.000000001 AndAlso (deltaBY(j) * polyA(i).X - deltaBX(j) * polyA(i).Y + rB(j)) * (deltaBY(j) * polyA(i + 1).X - deltaBX(j) * polyA(i + 1).Y + rB(j)) < -0.000000001 Then Return True
                        End If
                    Next j
                Next i

                For i = 0 To lengthPolyA - 2
                    ___point = polyA(i)
                    If isPointInPoly(polyB, ___point.X, ___point.Y) Then Return True
                Next i
                For i = 0 To lengthPolyB - 2
                    ___point = polyB(i)
                    If isPointInPoly(polyA, ___point.X, ___point.Y) Then Return True
                Next i

                Return False
            Else
                Return False
            End If
        End Function
    End Class

End Namespace