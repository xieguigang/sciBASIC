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

        Private Sub New()
        End Sub

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
            Dim labelCount As Integer = 1
            Dim allLabels As New Dictionary(Of Integer, Label)()

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
                        Dim rootLabels = neighboringLabels.Select(Function(l) allLabels(l).GetRoot()).ToList()
                        currentLabel = rootLabels.Min(Function(r) r.Name)
                        Dim currentRoot As Label = allLabels(currentLabel).GetRoot()

                        ' 合并所有其他根标签到当前根标签
                        For Each root In rootLabels
                            If root.Name <> currentRoot.Name Then
                                root.Join(currentRoot)
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
            Dim x = pix.X
            Dim y = pix.Y

            ' 检查左上 (x-1, y-1)
            If x > 0 AndAlso y > 0 Then
                Dim label = _board(x - 1, y - 1)
                If label <> 0 Then neighboringLabels.Add(label)
            End If

            ' 检查上 (x, y-1)
            If y > 0 Then
                Dim label = _board(x, y - 1)
                If label <> 0 Then neighboringLabels.Add(label)
            End If

            ' 检查右上 (x+1, y-1)
            If x < _width - 1 AndAlso y > 0 Then
                Dim label = _board(x + 1, y - 1)
                If label <> 0 Then neighboringLabels.Add(label)
            End If

            ' 检查左 (x-1, y)
            If x > 0 Then
                Dim label = _board(x - 1, y)
                If label <> 0 Then neighboringLabels.Add(label)
            End If

            Return neighboringLabels
        End Function

        Private Function AggregatePatterns(allLabels As Dictionary(Of Integer, Label)) As Dictionary(Of Integer, List(Of Point))
            Dim patterns = New Dictionary(Of Integer, List(Of Point))()

            For i = 0 To _height - 1
                For j = 0 To _width - 1
                    Dim patternNumber = _board(j, i)

                    If patternNumber <> 0 Then
                        ' 获取根标签
                        Dim rootLabel = allLabels(patternNumber).GetRoot()
                        Dim rootName = rootLabel.Name

                        If Not patterns.ContainsKey(rootName) Then
                            patterns(rootName) = New List(Of Point)()
                        End If

                        patterns(rootName).Add(New Point(j, i))
                    End If
                Next
            Next

            Return patterns
        End Function
    End Class
End Namespace
