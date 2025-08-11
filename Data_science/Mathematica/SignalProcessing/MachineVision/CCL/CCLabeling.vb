Imports System.Drawing
Imports Microsoft.VisualBasic.Imaging
Imports Microsoft.VisualBasic.Imaging.BitmapImage
Imports Microsoft.VisualBasic.Imaging.Math2D

Namespace CCL

    ''' <summary>
    ''' Connected Component Labeling
    ''' </summary>
    Public Class CCLabeling

        Private _board As Integer(,)
        Private _input As BitmapBuffer
        Private _width As Integer
        Private _height As Integer

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="input"></param>
        ''' <param name="background">
        ''' the background color usually be white color
        ''' </param>
        ''' <param name="tolerance"></param>
        ''' <returns></returns>
        Public Shared Function Process(input As BitmapBuffer,
                                       Optional background As Color? = Nothing,
                                       Optional tolerance As Integer = 3) As IDictionary(Of Integer, Polygon2D)

            Dim ccl As New CCLabeling With {
                ._input = input,
                ._width = input.Width,
                ._height = input.Height,
                ._board = New Integer(._width - 1, ._height - 1) {}
            }

            If background Is Nothing Then
                background = Color.White
            End If

            Dim patterns As Dictionary(Of Integer, List(Of Point)) = ccl.Find(background, tolerance)
            Dim images = New Dictionary(Of Integer, Polygon2D)()

            For Each pattern In patterns
                With pattern.Value
                    ' get x and y axis data from the point collection
                    ' for contruct a 2d polygon shape
                    ' as the label target
                    Call images.Add(pattern.Key, New Polygon2D(.X, .Y))
                End With
            Next

            Return images
        End Function

        Private Function Find(background As Color, tolerance As Integer) As Dictionary(Of Integer, List(Of Point))
            Dim labelCount = 1
            Dim allLabels = New Dictionary(Of Integer, Label)()

            For i = 0 To _height - 1
                For j = 0 To _width - 1
                    Dim color = _input.GetPixel(j, i)

                    If color.Equals(background, tolerance:=tolerance) Then
                        Continue For
                    End If

                    Dim currentPixel As New Point(j, i)
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

        Private Function GetNeighboringLabels(pix As Point) As IEnumerable(Of Integer)
            Dim neighboringLabels = New List(Of Integer)()

            Dim i = pix.Y - 1

            While i <= pix.Y + 2 AndAlso i < _height - 1
                Dim j = pix.X - 1

                While j <= pix.X + 2 AndAlso j < _width - 1
                    If i > -1 AndAlso j > -1 AndAlso _board(j, i) <> 0 Then
                        neighboringLabels.Add(_board(j, i))
                    End If

                    j += 1
                End While

                i += 1
            End While

            Return neighboringLabels
        End Function

        Private Function AggregatePatterns(allLabels As Dictionary(Of Integer, Label)) As Dictionary(Of Integer, List(Of Point))
            Dim patterns = New Dictionary(Of Integer, List(Of Point))()

            For i = 0 To _height - 1
                For j = 0 To _width - 1
                    Dim patternNumber = _board(j, i)

                    If patternNumber <> 0 Then
                        patternNumber = allLabels(patternNumber).GetRoot().Name

                        If Not patterns.ContainsKey(patternNumber) Then
                            patterns(patternNumber) = New List(Of Point)()
                        End If

                        patterns(patternNumber).Add(New Point(j, i))
                    End If
                Next
            Next

            Return patterns
        End Function
    End Class
End Namespace
