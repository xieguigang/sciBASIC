Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports std = System.Math

Namespace HoughCircles

    Public Module Algorithm

        Public Function CircleHough(baseImage As BitmapBuffer, radius As Single) As Result()
            ' the circle result
            Dim pick As Result() = Nothing
            Dim grayImg = CreateNegative(baseImage)
            Dim binarMap = DetectEdge(grayImg)

            If String.IsNullOrEmpty(radius) Then
                Dim houghMap = CreateHoughSpace(binarMap)

                pick = GetHighestVotes(houghMap)
            Else
                Dim r = Short.Parse(radius)
                Dim houghMap = CreateHoughSpace(binarMap, r)

                pick = GetHighestVotes(houghMap, r)
            End If

            Return pick
        End Function

        Public Function CircleVector(baseImage As BitmapBuffer, radius As Single) As Result()
            Dim pick As Result() = Nothing
            Dim grayImg = CreateNegative(baseImage)
            Dim binarMap = DetectEdge(grayImg)

            If String.IsNullOrEmpty(radius) Then
                Dim vSpace = CreateVectorVoteSpace(binarMap, grayImg)

                pick = GetHighestVotes(vSpace, 10)
            Else
                Dim r = Short.Parse(radius)
                Dim vSpace = CreateVectorVoteSpace(binarMap, r)

                pick = GetHighestVotes(vSpace, r, 0)
            End If

            Return pick
        End Function

        Public Function SquareVector(baseImage As BitmapBuffer) As Result()
            Dim grayImg = CreateNegative(baseImage)
            Dim binarMap = DetectEdge(grayImg)
            Dim vSpace = CreateVectorVoteSpace(binarMap, grayImg, Table.Square)
            Dim pick = GetHighestVotes(vSpace, 4)

            Return pick
        End Function

        Private Function CreateVectorVoteSpace(binarMap As Boolean(,), radius As Integer) As Short(,)
            Dim height = binarMap.GetLength(0)
            Dim width = binarMap.GetLength(1)
            Dim result = New Short(height - 1, width - 1) {}
            For y As Integer = 0 To height - 1
                For x As Integer = 0 To width - 1
                    If binarMap(y, x) Then
                        If y + radius < height Then
                            result(y + radius, x) += 1
                        End If
                        If x + radius < width Then
                            result(y, x + radius) += 1
                        End If
                        If y - radius >= 0 Then
                            result(y - radius, x) += 1
                        End If
                        If x - radius >= 0 Then
                            result(y, x - radius) += 1
                        End If
                    End If
                Next
            Next

            Return result
        End Function

        Private Function CreateVectorVoteSpace(BinarEdgeMap As Boolean(,), grayImg As Short(,)) As Short(,,)
            Dim binarHeight = BinarEdgeMap.GetLength(0)
            Dim binarWidth = BinarEdgeMap.GetLength(1)

            Dim radius = If(binarHeight < binarWidth, binarHeight, binarWidth)

            Dim resultCube = New Short(radius - 1, binarHeight - 1, binarWidth - 1) {}
            For r = 1 To radius - 1
                For Y As Integer = 0 To binarHeight - 1
                    For X As Integer = 0 To binarWidth - 1
                        If BinarEdgeMap(Y, X) Then
                            Dim angle = GetGradAngle(grayImg, X, Y)

                            Dim x0 = CInt(r * std.Cos(angle))
                            Dim y0 = CInt(r * std.Sin(angle))

                            If X + x0 >= 0 AndAlso Y + y0 >= 0 AndAlso X + x0 < binarWidth AndAlso Y + y0 < binarHeight Then
                                resultCube(r, Y + y0, X + x0) += 1
                            End If

                            x0 = CInt(r * std.Cos(angle + std.PI))
                            y0 = CInt(r * std.Sin(angle + std.PI))
                            If X + x0 >= 0 AndAlso Y + y0 >= 0 AndAlso X + x0 < binarWidth AndAlso Y + y0 < binarHeight Then
                                resultCube(r, Y + y0, X + x0) += 1
                            End If

                            'if (y + r < binarHeight) { resultCube[r, y + r, x]++; };
                            'if (x + r < binarWidth) { resultCube[r, y, x + r]++; };
                            'if (y - r >= 0) { resultCube[r, y - r, x]++; };
                            'if (x - r >= 0) { resultCube[r, y, x - r]++; };
                        End If
                    Next
                Next
            Next
            Return resultCube
        End Function

        ''' <summary>
        ''' the circle detection result
        ''' </summary>
        Public Class Result
            Public x As Short
            Public y As Short
            Public r As Short
            Public value As Short
        End Class

        Private Function GetHighestVotes(accCube As Short(,,), Optional trashold As Integer = 254) As Result()
            Dim radius = accCube.GetLength(0)
            Dim height = accCube.GetLength(1)
            Dim width = accCube.GetLength(2)

            Dim ResultList = New List(Of Result)()
            For r As Short = 1 To radius - 1
                Dim result As Result = New Result()
                For y As Short = 0 To height - 1
                    For x As Short = 0 To width - 1
                        If accCube(r, y, x) > trashold AndAlso result.value < accCube(r, y, x) Then
                            result.x = x
                            result.y = y
                            result.r = r
                            result.value = accCube(r, y, x)
                        End If
                    Next
                Next

                If result.value <> 0 Then
                    ResultList.Add(result)
                End If
            Next
            Return ResultList.ToArray()
        End Function

        Private Function GetHighestVotes(accMatrix As Short(,), radius As Short, Optional trashold As Integer = 254) As Result()
            Dim height = accMatrix.GetLength(0)
            Dim width = accMatrix.GetLength(1)

            Dim results = New List(Of Result)()
            For y As Short = 0 To height - 1
                For x As Short = 0 To width - 1
                    If accMatrix(y, x) > trashold Then ' && result.value < accMatrix[y, x])
                        Dim result As Result = New Result()
                        result.x = x
                        result.y = y
                        result.r = radius
                        result.value = accMatrix(y, x)
                        results.Add(result)
                    End If
                Next
            Next
            Return results.ToArray()
        End Function

        Private Function CreateHoughSpace(BinarEdgeMap As Boolean(,), radius As Integer) As Short(,)
            Dim binarHeight = BinarEdgeMap.GetLength(0)
            Dim binarWidth = BinarEdgeMap.GetLength(1)

            Dim resultMatrix = New Short(binarHeight - 1, binarWidth - 1) {}
            For Y As Integer = 0 To binarHeight - 1
                For X As Integer = 0 To binarWidth - 1
                    If BinarEdgeMap(Y, X) Then
                        UpdateHoughMatrix(resultMatrix, X, Y, radius)
                    End If
                Next
            Next
            Return resultMatrix
        End Function

        Private Function CreateHoughSpace(BinarEdgeMap As Boolean(,)) As Short(,,)
            Dim binarHeight = BinarEdgeMap.GetLength(0)
            Dim binarWidth = BinarEdgeMap.GetLength(1)

            Dim radius = If(binarHeight < binarWidth, binarHeight, binarWidth)

            Dim resultCube = New Short(radius - 1, binarHeight - 1, binarWidth - 1) {}
            For Y As Integer = 0 To binarHeight - 1
                For X As Integer = 0 To binarWidth - 1
                    If BinarEdgeMap(Y, X) Then
                        UpdateHoughMatrix(resultCube, X, Y, radius)
                    End If
                Next
            Next
            Return resultCube
        End Function

        Private Sub UpdateHoughMatrix(ByRef matrix As Short(,), x As Integer, y As Integer, radius As Integer)
            For teta = 0 To 359
                Dim a = CInt(x + radius * std.Cos(teta))
                Dim b = CInt(y + radius * std.Sin(teta))

                If a < 0 OrElse b < 0 OrElse b >= matrix.GetLength(0) OrElse a >= matrix.GetLength(1) Then
                    Continue For
                End If

                matrix(b, a) += 1
            Next
        End Sub

        Private Sub UpdateHoughMatrix(ByRef cube As Short(,,), x As Integer, y As Integer, maxRadius As Integer)
            For radius = 1 To maxRadius - 1
                For teta = 0 To 359
                    Dim a = CInt(x + radius * std.Cos(teta))
                    Dim b = CInt(y + radius * std.Sin(teta))

                    If a < 0 OrElse b < 0 OrElse a >= cube.GetLength(0) OrElse b >= cube.GetLength(1) Then
                        Continue For
                    End If

                    cube(radius, b, a) += 1
                Next
            Next
        End Sub

        Private Function CreateBinarMap(negativeImage As BitmapBuffer, thrashold As Integer) As Boolean(,)
            Dim binarMap = New Boolean(negativeImage.Height - 1, negativeImage.Width - 1) {}

            For Y As Integer = 0 To negativeImage.Height - 1
                For X As Integer = 0 To negativeImage.Width - 1
                    Dim color = negativeImage.GetPixel(X, Y)
                    If (color.R + color.G + color.B) / 3 > thrashold Then
                        binarMap(Y, X) = True
                    End If
                Next
            Next

            Return binarMap
        End Function

        Private Function CreateImgFromMap(map As Boolean(,)) As BitmapBuffer
            Dim height = map.GetLength(0)
            Dim width = map.GetLength(1)
            Dim img As New BitmapBuffer(width, height)

            For Y As Integer = 0 To height - 1
                For X As Integer = 0 To width - 1
                    If map(Y, X) Then
                        img.SetPixel(X, Y, Color.White)
                    Else
                        img.SetPixel(X, Y, Color.Black)
                    End If
                Next
            Next
            Return img
        End Function

        Private Function CreateNegative(img As BitmapBuffer) As Short(,)
            Dim grayMatrix = ToGrayMatrix(img)

            Dim height = grayMatrix.GetLength(0)
            Dim width = grayMatrix.GetLength(1)
            Dim result = New Short(height - 1, width - 1) {}
            For Y As Integer = 0 To height - 1
                For X As Integer = 0 To width - 1
                    result(Y, X) = CShort(255 - grayMatrix(Y, X))
                Next
            Next

            Return result
        End Function

        Private Function DetectEdge(binarImg As Short(,)) As Boolean(,)
            Dim height = binarImg.GetLength(0)
            Dim width = binarImg.GetLength(1)
            Dim bb = New Boolean(height - 1, width - 1) {}

            Dim gx = New Integer(,) {
  {-1, 0, 1},
  {-2, 0, 2},
                  {-1, 0, 1}}

            Dim gy = New Integer(,) {
    {1, 2, 1},
    {0, 0, 0},
                {-1, -2, -1}}

            Dim limit = 128 * 128

            Dim newX = 0, newY = 0, c = 0
            For Y As Integer = 1 To height - 1 - 1
                For X As Integer = 1 To width - 1 - 1

                    newX = 0
                    newY = 0
                    c = 0

                    For hw = -1 To 1
                        For ww = -1 To 1
                            c = binarImg(Y + hw, X + ww)
                            newX += gx(hw + 1, ww + 1) * c
                            newY += gy(hw + 1, ww + 1) * c
                        Next
                    Next
                    If newX * newX + newY * newY > limit Then
                        bb(Y, X) = True
                    Else
                        bb(Y, X) = False
                    End If
                Next
            Next
            Return bb
        End Function

        Private Function GetGradAngle(grayImg As Short(,), x0 As Integer, y0 As Integer) As Double
            Dim defX = grayImg(y0, x0 + 1) - grayImg(y0, x0)

            Return std.Atan2(grayImg(y0 + 1, x0) - grayImg(y0, x0), defX)
        End Function

        Private Function ToGrayMatrix(img As BitmapBuffer) As Short(,)
            Dim grayMatrix = New Short(img.Height - 1, img.Width - 1) {}
            For y As Integer = 0 To img.Height - 1
                For x As Integer = 0 To img.Width - 1
                    Dim color = img.GetPixel(x, y)
                    grayMatrix(y, x) = CShort((color.R + color.G + color.B) / 3)
                Next
            Next

            Return grayMatrix
        End Function

        Private Function CreateVectorVoteSpace(BinarEdgeMap As Boolean(,), grayImg As Short(,), table As TableRow()) As Short(,,)
            Dim binarHeight = BinarEdgeMap.GetLength(0)
            Dim binarWidth = BinarEdgeMap.GetLength(1)

            Dim factor = If(binarHeight < binarWidth, binarHeight, binarWidth)

            Dim resultCube = New Short(factor - 1, binarHeight - 1, binarWidth - 1) {}
            For f = 1 To factor - 1
                For Y As Integer = 0 To binarHeight - 1
                    For X As Integer = 0 To binarWidth - 1
                        If BinarEdgeMap(Y, X) Then
                            Dim angle = GetGradAngle(grayImg, X, Y)

                            Dim row = GetTableRow(table, angle)

                            If row Is Nothing Then
                                Continue For
                            End If

                            Dim x0 = CInt(f * row.Length * std.Cos(angle))
                            Dim y0 = CInt(f * row.Length * std.Sin(angle))

                            If X + x0 >= 0 AndAlso Y + y0 >= 0 AndAlso X + x0 < binarWidth AndAlso Y + y0 < binarHeight Then
                                resultCube(f, Y + y0, X + x0) += 1
                            End If

                            x0 = CInt(f * row.Length * std.Cos(angle + std.PI))
                            y0 = CInt(f * row.Length * std.Sin(angle + std.PI))
                            If X + x0 >= 0 AndAlso Y + y0 >= 0 AndAlso X + x0 < binarWidth AndAlso Y + y0 < binarHeight Then
                                resultCube(f, Y + y0, X + x0) += 1
                            End If

                            'if (y + r < binarHeight) { resultCube[r, y + r, x]++; };
                            'if (x + r < binarWidth) { resultCube[r, y, x + r]++; };
                            'if (y - r >= 0) { resultCube[r, y - r, x]++; };
                            'if (x - r >= 0) { resultCube[r, y, x - r]++; };
                        End If
                    Next
                Next
            Next
            Return resultCube
        End Function

        Public Function GetTableRow(rows As TableRow(), angle As Double) As TableRow
            For i = 0 To rows.Count - 1
                If rows(i).Angle = angle Then
                    Return rows(i)
                End If
            Next

            Return Nothing
        End Function
    End Module
End Namespace
