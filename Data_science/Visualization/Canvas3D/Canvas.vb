#Region "Microsoft.VisualBasic::b5635777a177f9de3ca9b830c9e06f13, sciBASIC#\Data_science\Visualization\Canvas3D\Canvas.vb"

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

    '   Total Lines: 93
    '    Code Lines: 40
    ' Comment Lines: 32
    '   Blank Lines: 21
    '     File Size: 4.05 KB


    '     Class Canvas
    ' 
    '         Properties: Camera
    ' 
    '         Sub: __init, Canvas_Load, InitializeComponent
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Data.ChartPlots.Drawing3D.Device
Imports Microsoft.VisualBasic.Imaging.Drawing3D

Namespace Plot3D.Device

    Public Class Canvas : Inherits GDIDevice

        Friend WithEvents TrackBar1 As TrackBar

        Public ReadOnly Property Camera As Camera
            Get
                Return MyBase._camera
            End Get
        End Property

        Protected Overrides Sub __init()

            'Dim a As New Point3D(0, 0, 0)
            'Dim b As New Point3D(10, 0, 0)
            'Dim c As New Point3D(10, 10, 0)
            'Dim d As New Point3D(0, 10, 0)

            ''  data = Evaluate(Function(x, y) x * y, New DoubleRange(-1, 10), New DoubleRange(-10, 10))
            'models += New Surface With {.brush = Brushes.Red, .vertices = {a, b, c, d}}
            'models += New Shape2D With {.focus = a, .shape = New Circle(10, Color.Red)}
            'models += New Shape2D With {.focus = b, .shape = New Circle(10, Color.Red)}
            'models += New Shape2D With {.focus = c, .shape = New Circle(10, Color.Red)}

            'models += New Drawing3D.Line3D With {.a = a, .b = c, .pen = Pens.Yellow}


            'a = New Point3D(0, 20, 10)
            'b = New Point3D(50, 50, 0)
            'c = New Point3D(0, 0, -10)

            'models += New Surface With {.brush = Brushes.Blue, .vertices = {a, b, c}}

            'Call DirectCast(models.Last, Surface).Allocation()
            'Call DirectCast(models.First, Surface).Allocation()

            '   Dim x As New DoubleRange(-5, 5)
            '  Dim y As New DoubleRange(-5, 5)

            '  models += Grid(Function(xx, yy) stdNum.Sin(xx ^ 2) * stdNum.Cos(yy ^ 2), x, y, ColorMap.PatternJet, 0.05, 0.05).Select(Function(l) DirectCast(l, I3DModel))
            '  models += Grid(Function(xx, yy) xx * yy, x, y, 0.05, 0.05, Pens.Green).Select(Function(l) DirectCast(l, I3DModel))

            'models += New Line3D With {.a = New Point3D(30, 0, 0), .b = New Point3D(-30, 0, 0), .pen = Pens.Red}
            'models += New Line3D With {.a = New Point3D(0, 30, 0), .b = New Point3D(0, -30, 0), .pen = Pens.Blue}
            'models += New Line3D With {.a = New Point3D(0, 0, 30), .b = New Point3D(0, 0, -30), .pen = Pens.DarkViolet}
            'models += New Cube(25)

            '  Call Run()

            '  Call SetAutoRotate(New Point3D(1, 0, 0))
        End Sub

        Private Sub InitializeComponent()
            Me.TrackBar1 = New System.Windows.Forms.TrackBar()
            CType(Me.TrackBar1, System.ComponentModel.ISupportInitialize).BeginInit()
            Me.SuspendLayout()
            '
            'TrackBar1
            '
            Me.TrackBar1.Anchor = CType((System.Windows.Forms.AnchorStyles.Bottom Or System.Windows.Forms.AnchorStyles.Right), System.Windows.Forms.AnchorStyles)
            Me.TrackBar1.BackColor = System.Drawing.Color.FromArgb(CType(CType(20, Byte), Integer), CType(CType(20, Byte), Integer), CType(CType(20, Byte), Integer))
            Me.TrackBar1.Location = New System.Drawing.Point(486, 470)
            Me.TrackBar1.Maximum = 100
            Me.TrackBar1.Minimum = -100
            Me.TrackBar1.Name = "TrackBar1"
            Me.TrackBar1.Size = New System.Drawing.Size(259, 45)
            Me.TrackBar1.TabIndex = 0
            '
            'Canvas
            '
            Me.Controls.Add(Me.TrackBar1)
            Me.DoubleBuffered = True
            Me.Name = "Canvas"
            Me.Size = New System.Drawing.Size(765, 529)
            CType(Me.TrackBar1, System.ComponentModel.ISupportInitialize).EndInit()
            Me.ResumeLayout(False)
            Me.PerformLayout()

        End Sub

        Private Sub Canvas_Load(sender As Object, e As EventArgs) Handles Me.Load
            Call MyBase.GDIDevice_Load(Nothing, Nothing)
            Call MyBase.Run()

            DisableScreenResize = True
            BackgroundImageLayout = ImageLayout.Zoom
        End Sub
    End Class
End Namespace
