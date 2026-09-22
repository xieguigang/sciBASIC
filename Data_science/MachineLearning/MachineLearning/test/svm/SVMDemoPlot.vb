#Region "Microsoft.VisualBasic::631bfa4d52ef6e6110e25d8d714d824a, Data_science\MachineLearning\MachineLearning\test\svm\SVMDemoPlot.vb"

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

    '   Total Lines: 533
    '    Code Lines: 322 (60.41%)
    ' Comment Lines: 112 (21.01%)
    '    - Xml Docs: 72.32%
    ' 
    '   Blank Lines: 99 (18.57%)
    '     File Size: 23.24 KB


    '     Module SVMDemoPlot
    ' 
    '         Function: buildPalette, classColor, className, dataBounds, dataToPixel
    '                   lighten, pixelToDatum
    ' 
    '         Sub: drawBoundary, drawLayout, drawSamples, drawSupportVectors, fillRegions
    '              PlotResult
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' /********************************************************************************/
'
'   GDI+ renderer of the SVM demo test.
'
'   Draws the trained support vector machine model into a PNG image so that the
'   classification behaviour can be visually inspected:
'
'     - decision regions : the whole plot area is predicted on a down sampled grid
'                          (stride = 4 pixels) and each grid cell is painted by a
'                          lightened class color, which renders the background
'                          heat map of the classification result
'     - decision boundary: the outline where two neighbouring grid cells disagree
'     - samples          : the training samples, colored by their real label
'     - support vectors  : highlighted by a dark ring, these are the samples which
'                          actually define the separating surface of the model
'
'   The module is a pure presentation layer and only depends on the public API of
'   the SVM namespace together with System.Drawing (GDI+).
'
' /********************************************************************************/

Imports System
Imports System.Collections.Generic
Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Drawing.Imaging
Imports System.IO
Imports System.Linq
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.MachineLearning.SVM

Namespace SVMDemo

    ''' <summary>
    ''' Renders the classification result of the SVM demo test as a PNG image.
    ''' </summary>
    Public Module SVMDemoPlot

        ''' <summary>
        ''' The size of the rendered image.
        ''' </summary>
        Public Const WIDTH As Integer = 920
        Public Const HEIGHT As Integer = 700

        ''' <summary>
        ''' The margin between the plot area and the image border.
        ''' </summary>
        Public Const PAD As Integer = 70

        ''' <summary>
        ''' The size of one decision region grid cell, in pixels. Larger value means
        ''' a coarser grid and a faster rendering, at the cost of a rougher boundary.
        ''' </summary>
        Public Const STRIDE As Integer = 4

        ''' <summary>
        ''' The ratio of the class color which is mixed into the white background
        ''' for painting the decision regions.
        ''' </summary>
        Const REGION_RATIO As Double = 0.35

        ''' <summary>
        ''' The default color palette used for rendering the sample classes. Do not
        ''' rename this field to <c>PALETTE</c>: visual basic is case insensitive and
        ''' the identifier would collide with the local <c>palette</c> variables.
        ''' </summary>
        Private ReadOnly PALETTE_COLORS As Color() = {
            Color.FromArgb(214, 69, 65),
            Color.FromArgb(70, 130, 180),
            Color.FromArgb(60, 179, 113),
            Color.FromArgb(255, 140, 0)
        }

        ''' <summary>
        ''' Renders the decision regions of the given model together with the training
        ''' samples and the support vectors, and then saves the result as a PNG image.
        ''' </summary>
        ''' <param name="problem">The raw (not scaled) training problem.</param>
        ''' <param name="model">The trained model which was created from the scaled problem.</param>
        ''' <param name="transform">The range transform which was used for scaling the training data.</param>
        ''' <param name="file">The PNG file which will be generated.</param>
        ''' <param name="summary">Optional text lines which will be printed onto the image.</param>
        Public Sub PlotResult(problem As Problem,
                              model As Model,
                              transform As IRangeTransform,
                              file As String,
                              Optional summary As String() = Nothing)

            If problem Is Nothing OrElse problem.count = 0 Then
                Throw New InvalidOperationException("the training problem is empty, nothing can be rendered!")
            End If
            If model Is Nothing Then
                Throw New ArgumentNullException(NameOf(model), "a trained svm model is required for rendering the decision regions!")
            End If
            If transform Is Nothing Then
                Throw New ArgumentNullException(NameOf(transform), "the range transform which was used for training the model is required!")
            End If

            Dim palette As Dictionary(Of Integer, Color) = buildPalette(problem)
            Dim bounds As Double() = dataBounds(problem)

            Dim left As Integer = PAD
            Dim top As Integer = PAD
            Dim right As Integer = WIDTH - PAD
            Dim bottom As Integer = HEIGHT - PAD
            Dim plotWidth As Integer = right - left
            Dim plotHeight As Integer = bottom - top

            ' 1) evaluate the decision regions on a down sampled grid. this reduces
            '    the number of the prediction calls from WIDTH*HEIGHT to about
            '    (WIDTH/STRIDE)*(HEIGHT/STRIDE)
            Dim cols As Integer = plotWidth \ STRIDE + 1
            Dim rows As Integer = plotHeight \ STRIDE + 1
            Dim labels(rows - 1, cols - 1) As Integer

            For r As Integer = 0 To rows - 1

                For c As Integer = 0 To cols - 1
                    Dim px As Integer = left + c * STRIDE
                    Dim py As Integer = top + r * STRIDE
                    ' the grid point is mapped back into the data space, and then it
                    ' must be scaled by the very same transform which was used for
                    ' training the model
                    Dim datum As Node() = transform.Transform(pixelToDatum(px, py, bounds, left, top, plotWidth, plotHeight))

                    labels(r, c) = model.Predict(datum).class
                Next
            Next

            Dim dir As String = Path.GetDirectoryName(file)

            If Not String.IsNullOrEmpty(dir) AndAlso Not Directory.Exists(dir) Then
                Call Directory.CreateDirectory(dir)
            End If

            ' 2) paint the image
            Using bmp As New Bitmap(WIDTH, HEIGHT, PixelFormat.Format32bppArgb)

                Using g As Graphics = Graphics.FromImage(bmp)
                    g.Clear(Color.White)
                End Using

                Call fillRegions(bmp, labels, palette, cols, rows, left, top, plotWidth, plotHeight)

                Using g As Graphics = Graphics.FromImage(bmp)
                    g.SmoothingMode = SmoothingMode.HighQuality

                    Call drawBoundary(g, labels, cols, rows, left, top)
                    Call drawSamples(g, problem, palette, bounds, left, top, plotWidth, plotHeight)
                    Call drawSupportVectors(g, problem, model, bounds, left, top, plotWidth, plotHeight)
                    Call drawLayout(g, problem, palette, summary, bounds, left, top, right, bottom)
                End Using

                bmp.Save(file, ImageFormat.Png)
            End Using
        End Sub

        ''' <summary>
        ''' Fill the plot area of the bitmap with the decision region colors. The pixels
        ''' are written directly into the bitmap buffer, which is much faster than
        ''' issuing one <see cref="Graphics.FillRectangle(Brush, Rectangle)"/> call per
        ''' grid cell.
        ''' </summary>
        Private Sub fillRegions(bmp As Bitmap,
                                labels(,) As Integer,
                                palette As Dictionary(Of Integer, Color),
                                cols As Integer,
                                rows As Integer,
                                left As Integer,
                                top As Integer,
                                plotWidth As Integer,
                                plotHeight As Integer)

            Dim area As New Rectangle(0, 0, bmp.Width, bmp.Height)
            Dim data As BitmapData = bmp.LockBits(area, ImageLockMode.WriteOnly, PixelFormat.Format32bppArgb)

            Try
                Dim buffer As Byte() = New Byte(data.Stride * bmp.Height - 1) {}

                Call Marshal.Copy(data.Scan0, buffer, 0, buffer.Length)

                For y As Integer = top To top + plotHeight - 1
                    Dim r As Integer = Math.Min((y - top) \ STRIDE, rows - 1)
                    Dim colorCache As New Dictionary(Of Integer, Byte())()
                    Dim rowOffset As Integer = y * data.Stride

                    For x As Integer = left To left + plotWidth - 1
                        Dim c As Integer = Math.Min((x - left) \ STRIDE, cols - 1)
                        Dim cls As Integer = labels(r, c)
                        Dim bgr As Byte() = Nothing

                        If Not colorCache.TryGetValue(cls, bgr) Then
                            Dim color As Color = lighten(classColor(palette, cls), REGION_RATIO)
                            bgr = New Byte() {color.B, color.G, color.R, CByte(255)}
                            colorCache(cls) = bgr
                        End If

                        Dim offset As Integer = rowOffset + x * 4

                        buffer(offset) = bgr(0)
                        buffer(offset + 1) = bgr(1)
                        buffer(offset + 2) = bgr(2)
                        buffer(offset + 3) = bgr(3)
                    Next
                Next

                Call Marshal.Copy(buffer, 0, data.Scan0, buffer.Length)
            Finally
                Call bmp.UnlockBits(data)
            End Try
        End Sub

        ''' <summary>
        ''' Draw the outline of the decision boundary: wherever two neighbouring grid
        ''' cells are assigned with different classes, their shared edge belongs to
        ''' the separating surface of the model.
        ''' </summary>
        Private Sub drawBoundary(g As Graphics,
                                 labels(,) As Integer,
                                 cols As Integer,
                                 rows As Integer,
                                 left As Integer,
                                 top As Integer)

            Using pen As New Pen(Color.FromArgb(120, Color.Black), 1.4F)

                For r As Integer = 0 To rows - 1

                    For c As Integer = 0 To cols - 1
                        Dim x As Integer = left + c * STRIDE
                        Dim y As Integer = top + r * STRIDE

                        If c + 1 < cols AndAlso labels(r, c + 1) <> labels(r, c) Then
                            g.DrawLine(pen, x + STRIDE, y, x + STRIDE, y + STRIDE)
                        End If
                        If r + 1 < rows AndAlso labels(r + 1, c) <> labels(r, c) Then
                            g.DrawLine(pen, x, y + STRIDE, x + STRIDE, y + STRIDE)
                        End If
                    Next
                Next
            End Using
        End Sub

        ''' <summary>
        ''' Draw the training samples, each of them is colored by its real class label.
        ''' </summary>
        Private Sub drawSamples(g As Graphics,
                                problem As Problem,
                                palette As Dictionary(Of Integer, Color),
                                bounds As Double(),
                                left As Integer,
                                top As Integer,
                                plotWidth As Integer,
                                plotHeight As Integer)

            For Each cls As Integer In palette.Keys.OrderBy(Function(a) a)

                Using brush As New SolidBrush(palette(cls))
                    Using pen As New Pen(Color.FromArgb(200, 40, 40, 40), 1.0F)

                        For i As Integer = 0 To problem.count - 1

                            If CInt(problem.Y(i).factor) <> cls Then
                                Continue For
                            End If

                            Dim p As PointF = dataToPixel(problem.X(i), bounds, left, top, plotWidth, plotHeight)

                            g.FillEllipse(brush, p.X - 4, p.Y - 4, 8, 8)
                            g.DrawEllipse(pen, p.X - 4, p.Y - 4, 8, 8)
                        Next
                    End Using
                End Using
            Next
        End Sub

        ''' <summary>
        ''' Highlight the support vectors of the model. A support vector is a training
        ''' sample which is located on (or inside) the margin of the separating surface.
        ''' </summary>
        Private Sub drawSupportVectors(g As Graphics,
                                       problem As Problem,
                                       model As Model,
                                       bounds As Double(),
                                       left As Integer,
                                       top As Integer,
                                       plotWidth As Integer,
                                       plotHeight As Integer)

            If model.supportVectorIndices Is Nothing Then
                Return
            End If

            Using pen As New Pen(Color.FromArgb(230, 25, 25, 25), 1.6F)

                For Each index As Integer In model.supportVectorIndices
                    ' the support vector indices are 1 based indices of the training set
                    Dim i As Integer = index - 1

                    If i < 0 OrElse i >= problem.count Then
                        Continue For
                    End If

                    Dim p As PointF = dataToPixel(problem.X(i), bounds, left, top, plotWidth, plotHeight)

                    g.DrawEllipse(pen, p.X - 6.5F, p.Y - 6.5F, 13, 13)
                Next
            End Using
        End Sub

        ''' <summary>
        ''' Draw the plot frame, the axis labels, the title, the legend and the
        ''' optional summary text.
        ''' </summary>
        Private Sub drawLayout(g As Graphics,
                               problem As Problem,
                               palette As Dictionary(Of Integer, Color),
                               summary As String(),
                               bounds As Double(),
                               left As Integer,
                               top As Integer,
                               right As Integer,
                               bottom As Integer)

            g.DrawRectangle(Pens.Gray, left, top, right - left, bottom - top)

            ' the numeric range of both feature dimensions
            Using font As New Font("Consolas", 9)
                Dim maxValue As String = bounds(1).ToString("G4")
                Dim maxY As String = bounds(3).ToString("G4")

                g.DrawString(bounds(0).ToString("G4"), font, Brushes.DimGray, left - 14, bottom + 5)
                g.DrawString(maxValue, font, Brushes.DimGray, right - g.MeasureString(maxValue, font).Width, bottom + 5)
                g.DrawString(bounds(2).ToString("G4"), font, Brushes.DimGray, 6, bottom - 14)
                g.DrawString(maxY, font, Brushes.DimGray, 6, top)
            End Using

            Using font As New Font("Consolas", 10)

                If summary IsNot Nothing AndAlso summary.Length > 0 Then

                    For i As Integer = 0 To summary.Length - 1
                        g.DrawString(summary(i), font, Brushes.Black, left - 14, bottom + 24 + i * 17)
                    Next
                End If
            End Using

            Using font As New Font("Consolas", 13, FontStyle.Bold)
                g.DrawString("SVM classification: C-SVC / RBF kernel (gamma = 0.5, C = 1)",
                             font, Brushes.Black, left - 10, 14)
            End Using

            ' class legend, it is placed at the top right corner of the plot area
            Dim classes As Integer() = palette.Keys.OrderBy(Function(a) a).ToArray
            Dim boxWidth As Integer = 210
            Dim boxHeight As Integer = classes.Length * 22 + 12
            Dim boxX As Integer = right - boxWidth - 12
            Dim boxY As Integer = top + 12

            Using brush As New SolidBrush(Color.FromArgb(225, Color.White))
                g.FillRectangle(brush, boxX, boxY, boxWidth, boxHeight)
            End Using

            g.DrawRectangle(Pens.Gray, boxX, boxY, boxWidth, boxHeight)

            Using font As New Font("Consolas", 9.5)
                Dim y As Integer = boxY + 6

                For Each cls As Integer In classes

                    Using brush As New SolidBrush(palette(cls))
                        g.FillEllipse(brush, boxX + 10, y + 3, 10, 10)
                    End Using

                    g.DrawEllipse(Pens.Black, boxX + 10, y + 3, 10, 10)
                    g.DrawString($"class {className(problem, cls)}", font, Brushes.Black, boxX + 28, y)
                    y += 22
                Next
            End Using
        End Sub

        ''' <summary>
        ''' Build the class color palette from the distinct class labels of the given
        ''' problem. The class label values are the <c>factor</c> values of the label
        ''' encoder, hence they usually start from zero.
        ''' </summary>
        Private Function buildPalette(problem As Problem) As Dictionary(Of Integer, Color)
            Dim classes As New List(Of Integer)

            For i As Integer = 0 To problem.count - 1
                Dim cls As Integer = CInt(problem.Y(i).factor)

                If Not classes.Contains(cls) Then
                    classes.Add(cls)
                End If
            Next

            classes.Sort()

            Dim palette As New Dictionary(Of Integer, Color)

            For i As Integer = 0 To classes.Count - 1
                palette(classes(i)) = PALETTE_COLORS(i Mod PALETTE_COLORS.Length)
            Next

            Return palette
        End Function

        ''' <summary>
        ''' Gets the color of the given class, a gray color will be returned for an
        ''' unknown class value as a fallback.
        ''' </summary>
        Private Function classColor(palette As Dictionary(Of Integer, Color), cls As Integer) As Color
            Dim color As Color

            If palette.TryGetValue(cls, color) Then
                Return color
            End If

            Return Color.Gray
        End Function

        ''' <summary>
        ''' Mix the given color into white, the <paramref name="ratio"/> value controls
        ''' how much of the source color is kept.
        ''' </summary>
        Private Function lighten(color As Color, ratio As Double) As Color
            Return Color.FromArgb(
                CInt(color.R * ratio + 255 * (1 - ratio)),
                CInt(color.G * ratio + 255 * (1 - ratio)),
                CInt(color.B * ratio + 255 * (1 - ratio)))
        End Function

        ''' <summary>
        ''' Gets the name of the given class label value.
        ''' </summary>
        Private Function className(problem As Problem, cls As Integer) As String

            For Each label In problem.Y

                If CInt(label.factor) = cls Then
                    Return label.name
                End If
            Next

            Return cls.ToString
        End Function

        ''' <summary>
        ''' Calculate the value range of each feature dimension of the given problem,
        ''' a small amount of padding is added so that the samples will not be drawn
        ''' exactly on the border of the plot area.
        ''' </summary>
        ''' <returns>An array in order of <c>{minX, maxX, minY, maxY}</c>.</returns>
        Private Function dataBounds(problem As Problem) As Double()
            Dim minX As Double = Double.MaxValue
            Dim maxX As Double = Double.MinValue
            Dim minY As Double = Double.MaxValue
            Dim maxY As Double = Double.MinValue

            For i As Integer = 0 To problem.count - 1

                For Each node As Node In problem.X(i)

                    Select Case node.index
                        Case 1
                            minX = Math.Min(minX, node.value)
                            maxX = Math.Max(maxX, node.value)
                        Case 2
                            minY = Math.Min(minY, node.value)
                            maxY = Math.Max(maxY, node.value)
                    End Select
                Next
            Next

            If minX > maxX OrElse minY > maxY Then
                Throw New InvalidOperationException("the given problem does not contain any two dimensional feature data!")
            End If

            Dim padX As Double = Math.Max((maxX - minX) * 0.08, 1)
            Dim padY As Double = Math.Max((maxY - minY) * 0.08, 1)

            Return {minX - padX, maxX + padX, minY - padY, maxY + padY}
        End Function

        ''' <summary>
        ''' Maps a pixel inside the plot area back into the data space.
        ''' </summary>
        Private Function pixelToDatum(px As Integer,
                                      py As Integer,
                                      bounds As Double(),
                                      left As Integer,
                                      top As Integer,
                                      plotWidth As Integer,
                                      plotHeight As Integer) As Node()

            Dim x As Double = bounds(0) + (px - left) / plotWidth * (bounds(1) - bounds(0))
            Dim y As Double = bounds(3) - (py - top) / plotHeight * (bounds(3) - bounds(2))

            ' the node index is 1 based for the libsvm style feature vector
            Return New Node() {New Node(1, x), New Node(2, y)}
        End Function

        ''' <summary>
        ''' Maps a datum of the data space onto a pixel of the plot area.
        ''' </summary>
        Private Function dataToPixel(node As Node(),
                                     bounds As Double(),
                                     left As Integer,
                                     top As Integer,
                                     plotWidth As Integer,
                                     plotHeight As Integer) As PointF

            Dim x As Double = bounds(0)
            Dim y As Double = bounds(2)

            For Each n As Node In node

                Select Case n.index
                    Case 1
                        x = n.value
                    Case 2
                        y = n.value
                End Select
            Next

            Return New PointF(
                CSng(left + (x - bounds(0)) / (bounds(1) - bounds(0)) * plotWidth),
                CSng(top + (bounds(3) - y) / (bounds(3) - bounds(2)) * plotHeight))
        End Function

    End Module
End Namespace
