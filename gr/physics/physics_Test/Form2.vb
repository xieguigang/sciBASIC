#Region "Microsoft.VisualBasic::3534aed12b57d6a04a055eb72dbc4fef, gr\physics\physics_Test\Form2.vb"

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

    '   Total Lines: 48
    '    Code Lines: 35 (72.92%)
    ' Comment Lines: 6 (12.50%)
    '    - Xml Docs: 66.67%
    ' 
    '   Blank Lines: 7 (14.58%)
    '     File Size: 1.54 KB


    '     Class Form2
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Sub: pictureBox1_Click, pictureBox1_SizeChanged, Reset, timer1_Tick
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Windows.Forms
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Imaging.Physics.Boids
Imports Microsoft.VisualBasic.Parallel

Namespace Boids.Viewer

    ''' <summary>
    ''' Graphics Simulations
    ''' 
    ''' https://github.com/swharden/Csharp-Data-Visualization/tree/main/dev/old/drawing/boids
    ''' </summary>
    Partial Public Class Form2
        Inherits Form
        Private field As Field
        Public Sub New()
            InitializeComponent()
            Reset()

            Size = New Size(2000, 1200)
        End Sub

        Private Sub pictureBox1_SizeChanged(sender As Object, e As EventArgs) Handles pictureBox1.SizeChanged
            Reset()
        End Sub
        Private Sub pictureBox1_Click(sender As Object, e As EventArgs)
            Reset()
        End Sub
        Private Sub Reset()
            VectorTask.n_threads = 12
            field = New Field(pictureBox1.Width, pictureBox1.Height, 15000)
        End Sub

        Dim counter As New PerformanceCounter

        Private Sub timer1_Tick(sender As Object, e As EventArgs) Handles timer1.Tick
            counter.Set()
            field.Advance()
            counter.Mark("physics simulation")
            ' pictureBox1.Image?.Dispose()
            pictureBox1.Image = RenderField(field)
            counter.Mark("gdi+ rendering")
            Text = counter.TaskSequentialCounterReport
        End Sub

    End Class
End Namespace
