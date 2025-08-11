Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging.BitmapImage

Namespace CCL

    ''' <summary>
    ''' Connected Component Labeling
    ''' </summary>
    Public Class CCLabeling

        Private _board As Integer(,)
        Private _input As BitmapBuffer
        Private _width As Integer
        Private _height As Integer

        Public Shared Function Process(input As BitmapBuffer) As IDictionary(Of Integer, BitmapBuffer)
            Dim ccl As New CCLabeling With {
                ._input = input,
                ._width = input.Width,
                ._height = input.Height,
                ._board = New Integer(._width - 1, ._height - 1) {}
            }
            Dim patterns As Dictionary(Of Integer, List(Of Pixel)) = ccl.Find()
            Dim images = New Dictionary(Of Integer, BitmapBuffer)()

            For Each pattern In patterns
                Call images.Add(pattern.Key, ccl.CreateBitmap(pattern.Value))
            Next

            Return images
        End Function

        Protected Overridable Function CheckIsBackGround(currentPixel As Pixel) As Boolean
            Return currentPixel.color.A = 255 AndAlso
                currentPixel.color.R = 255 AndAlso
                currentPixel.color.G = 255 AndAlso
                currentPixel.color.B = 255
        End Function

        Private Function Find() As Dictionary(Of Integer, List(Of Pixel))
            Dim labelCount = 1
            Dim allLabels = New Dictionary(Of Integer, Label)()

            For i = 0 To _height - 1
                For j = 0 To _width - 1
                    Dim currentPixel As Pixel = New Pixel(New Point(j, i), _input.GetPixel(j, i))

                    If CheckIsBackGround(currentPixel) Then
                        Continue For
                    End If

                    Dim neighboringLabels = GetNeighboringLabels(currentPixel)
                    Dim currentLabel As Integer

                    If Not neighboringLabels.Any() Then
                        currentLabel = labelCount
                        allLabels.Add(currentLabel, New Label(currentLabel))
                        labelCount += 1
                    Else
                        currentLabel = neighboringLabels.Min(Function(n) allLabels(n).GetRoot().Name)
                        Dim root As Label = allLabels(currentLabel).GetRoot()

                        For Each neighbor In neighboringLabels
                            If root.Name <> allLabels(neighbor).GetRoot().Name Then
                                allLabels(neighbor).Join(allLabels(currentLabel))
                            End If
                        Next
                    End If

                    _board(j, i) = currentLabel
                Next
            Next


            Dim patterns = AggregatePatterns(allLabels)

            Return patterns
        End Function

        Private Function GetNeighboringLabels(pix As Pixel) As IEnumerable(Of Integer)
            Dim neighboringLabels = New List(Of Integer)()

            Dim i = pix.Position.Y - 1

            While i <= pix.Position.Y + 2 AndAlso i < _height - 1
                Dim j = pix.Position.X - 1

                While j <= pix.Position.X + 2 AndAlso j < _width - 1
                    If i > -1 AndAlso j > -1 AndAlso _board(j, i) <> 0 Then
                        neighboringLabels.Add(_board(j, i))
                    End If

                    j += 1
                End While

                i += 1
            End While

            Return neighboringLabels
        End Function

        Private Function AggregatePatterns(allLabels As Dictionary(Of Integer, Label)) As Dictionary(Of Integer, List(Of Pixel))
            Dim patterns = New Dictionary(Of Integer, List(Of Pixel))()

            For i = 0 To _height - 1
                For j = 0 To _width - 1
                    Dim patternNumber = _board(j, i)

                    If patternNumber <> 0 Then
                        patternNumber = allLabels(patternNumber).GetRoot().Name

                        If Not patterns.ContainsKey(patternNumber) Then
                            patterns(patternNumber) = New List(Of Pixel)()
                        End If

                        patterns(patternNumber).Add(New Pixel(New Point(j, i), Color.Black))
                    End If
                Next
            Next

            Return patterns
        End Function

        Private Function CreateBitmap(pattern As List(Of Pixel)) As BitmapBuffer
            Dim minX = pattern.Min(Function(p) p.Position.X)
            Dim maxX = pattern.Max(Function(p) p.Position.X)

            Dim minY = pattern.Min(Function(p) p.Position.Y)
            Dim maxY = pattern.Max(Function(p) p.Position.Y)

            Dim width = maxX + 1 - minX
            Dim height = maxY + 1 - minY

            Dim bmp As BitmapBuffer = BitmapBuffer.White(width, height)

            For Each pix In pattern
                ' shift position by minX and minY
                bmp.SetPixel(pix.Position.X - minX, pix.Position.Y - minY, pix.color)
            Next

            Return bmp
        End Function
    End Class
End Namespace
