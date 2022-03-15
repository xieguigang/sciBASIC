#Region "Microsoft.VisualBasic::45fc5ad21ef13e7b44fcdae02ca19a5e, sciBASIC#\Data_science\MachineLearning\SVMDemo\MainWindow.xaml.vb"

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

    '   Total Lines: 175
    '    Code Lines: 148
    ' Comment Lines: 3
    '   Blank Lines: 24
    '     File Size: 7.70 KB


    '     Class MainWindow
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: samplePoint
    ' 
    '         Sub: addDataPoint, addPoints, classCB_SelectionChanged, classify, classifyB_Click
    '              clearB_Click, plot_MouseLeftButtonDown, plot_MouseRightButtonDown
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Collections.Generic
Imports System.Linq
Imports System.Threading
Imports System.Windows
Imports System.Windows.Controls
Imports System.Windows.Input
Imports System.Windows.Media
Imports System.Windows.Media.Imaging
Imports System.Windows.Shapes
Imports Microsoft.VisualBasic.MachineLearning.SVM

Namespace SVMDemo
    ''' <summary>
    ''' Interaction logic for MainWindow.xaml
    ''' </summary>
    Public Partial Class MainWindow
        Inherits Window

        Private Const CLUSTER_SIZE As Integer = 10
        Private Const CLUSTER_STDDEV As Integer = 8
        Private Const SCALE As Integer = 2
        Private _classifyThread As Thread
        Private _data As List(Of DataPoint) = New List(Of DataPoint)()
        Private _colors As Color()
        Private _rand As Random = New Random()

        Public Sub New()
            Me.InitializeComponent()
            _colors = New Color(Me.classCB.Items.Count - 1) {}
            Dim lighten = Color.FromRgb(120, 120, 120)

            For i = 0 To _colors.Length - 1
                _colors(i) = TryCast(TryCast(Me.classCB.Items(CInt(i)), ComboBoxItem).Foreground, SolidColorBrush).Color + lighten
            Next
        End Sub

        Private Sub classify(ByVal args As Object)
            Dim train As Problem = New Problem With {
                .X = _data.[Select](Function(o) New Node() {New Node(1, o.Position.X), New Node(2, o.Position.Y)}).ToArray(),
                .Y = _data.[Select](Function(o) o.Label).ToArray(),
                .MaxIndex = 2
            }
            Dim param As Parameter = TryCast(args, Parameter)
            Dim transform = RangeTransform.Compute(train)
            train = transform.Scale(train)
            Dim model = Training.Train(train, param)
            Dim width = CInt(Me.plot.ActualWidth)
            Dim height = CInt(Me.plot.ActualHeight)
            Dim pixels = New Byte(width * height * 3 - 1) {}
            Dim cWidth = (width >> SCALE) + 1
            Dim cHeight = (height >> SCALE) + 1
            Dim labels = New Integer(cHeight - 1, cWidth - 1) {}
            Dim r = 0, i = 0

            While r < cHeight
                Dim c = 0

                While c < cWidth
                    Dim rr = r << SCALE
                    Dim cc = c << SCALE
                    Dim datum As Node() = New Node() {New Node(1, cc), New Node(2, rr)}
                    datum = transform.Transform(datum)
                    labels(r, c) = CInt(model.Predict(datum))
                    Me.classifyPB.Dispatcher.Invoke(Function()
                                                        Me.classifyPB.Value = i * 100 / (cHeight * cWidth)
                                                        Return Me.classifyPB.Value
                                                    End Function)
                    c += 1
                    i += 1
                End While

                r += 1
            End While

            Dim format = PixelFormats.Rgb24
            i = 0
            r = 0

            While r < height

                For c = 0 To width - 1
                    Dim label = labels(r >> SCALE, c >> SCALE)
                    Dim color = _colors(label)
                    pixels(Math.Min(Interlocked.Increment(i), i - 1)) = color.R
                    pixels(Math.Min(Interlocked.Increment(i), i - 1)) = color.G
                    pixels(Math.Min(Interlocked.Increment(i), i - 1)) = color.B
                Next

                r += 1
            End While

            Me.plot.Dispatcher.Invoke(Sub()
                                          Dim brush As ImageBrush = New ImageBrush(BitmapSource.Create(width, height, 96, 96, format, Nothing, pixels, width * 3))
                                          brush.Stretch = Stretch.None
                                          brush.AlignmentX = 0
                                          brush.AlignmentY = 0
                                          Me.plot.Background = brush
                                      End Sub)
            Me.classifyPB.Dispatcher.Invoke(Function()
                                                Me.classifyPB.Value = 0
                                                Return 0
                                            End Function)
        End Sub

        Private Sub classCB_SelectionChanged(ByVal sender As Object, ByVal e As SelectionChangedEventArgs)
            Me.classCB.Foreground = TryCast(Me.classCB.SelectedItem, System.Windows.Controls.ComboBoxItem).Foreground
        End Sub

        Private Sub plot_MouseLeftButtonDown(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
            Me.addPoints(e.GetPosition(Me.plot), Me.classCB.SelectedIndex)
        End Sub

        Private Sub classifyB_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            If _data.Count = 0 Then Return
            Dim param As Parameter = New Parameter()
            param.Gamma = .5
            param.SvmType = CType(Me.svmTypeCB.SelectedIndex, SvmType)
            param.KernelType = CType(Me.kernelTypeCB.SelectedIndex, KernelType)
            _classifyThread = New Thread(New ParameterizedThreadStart(AddressOf classify))
            _classifyThread.Start(param)
        End Sub

        Private Sub plot_MouseRightButtonDown(ByVal sender As Object, ByVal e As MouseButtonEventArgs)
            Dim label As Integer = (Me.classCB.SelectedIndex + 3) Mod Me.classCB.Items.Count
            Me.addPoints(e.GetPosition(Me.plot), label)
        End Sub

        Private Sub addDataPoint(ByVal position As Point, ByVal label As Integer)
            Dim datum As DataPoint = New DataPoint With {
                .Position = position,
                .Label = label
            }
            _data.Add(datum)
            Dim ellipse As Ellipse = New Ellipse()
            ellipse.Fill = TryCast(Me.classCB.Items(CInt(label)), ComboBoxItem).Foreground
            ellipse.Width = 8
            ellipse.Height = 8
            ellipse.Stroke = New SolidColorBrush(Colors.Black)
            ellipse.StrokeThickness = 1
            Canvas.SetLeft(ellipse, datum.Position.X - 4)
            Canvas.SetTop(ellipse, datum.Position.Y - 4)
            Me.plot.Children.Add(ellipse)
        End Sub

        Private Sub addPoints(ByVal center As Point, ByVal label As Integer)
            addDataPoint(center, label)

            If Me.placementCB.SelectedIndex = 1 Then
                For i = 0 To CLUSTER_SIZE - 1
                    Dim sample As Point

                    Do
                        sample = samplePoint(center, CLUSTER_STDDEV)
                    Loop While sample.X < 0 OrElse sample.X >= Me.plot.ActualWidth OrElse sample.Y < 0 OrElse sample.Y >= Me.plot.ActualHeight

                    addDataPoint(sample, label)
                Next
            End If
        End Sub

        Private Sub clearB_Click(ByVal sender As Object, ByVal e As RoutedEventArgs)
            _data.Clear()
            Me.plot.Background = New SolidColorBrush(Colors.White)
            Me.plot.Children.Clear()
        End Sub

        Private Function samplePoint(ByVal center As Point, ByVal standardDeviation As Double) As Point
            Dim theta As Double = 2 * Math.PI * _rand.NextDouble()
            Dim rho As Double = Math.Sqrt(-2 * Math.Log(1 - _rand.NextDouble()))
            Dim scale = standardDeviation * rho
            Return New Point(center.X + scale * Math.Cos(theta), center.Y + scale * Math.Sin(theta))
        End Function
    End Class
End Namespace
