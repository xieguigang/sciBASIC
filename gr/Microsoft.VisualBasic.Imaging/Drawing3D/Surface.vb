#Region "Microsoft.VisualBasic::e1b238c9e896b1b0e4a315ffcf4699d9, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Surface.vb"

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

    '   Total Lines: 86
    '    Code Lines: 60 (69.77%)
    ' Comment Lines: 12 (13.95%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 14 (16.28%)
    '     File Size: 2.88 KB


    '     Structure Surface
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Copy, GetEnumerator, IEnumerable_GetEnumerator
    ' 
    '         Sub: Draw
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Models
Imports Microsoft.VisualBasic.Linq

Namespace Drawing3D

    ''' <summary>
    ''' Object model that using for the 3D graphics.
    ''' (进行实际3D绘图操作的对象模型，这个对象实际上就是相当于Path3D??)
    ''' </summary>
    Public Structure Surface
        Implements IEnumerable(Of Point3D)
        Implements I3DModel

        ''' <summary>
        ''' Vertix in this list have the necessary element orders
        ''' for construct a correct closed figure.
        ''' (请注意，在这里面的点都是有先后顺序分别的)
        ''' </summary>
        Public vertices() As Point3D
        ''' <summary>
        ''' Drawing texture material of this surface.
        ''' </summary>
        Public brush As Brush

        Sub New(v As Point3D(), b As Brush)
            brush = b
            vertices = v
        End Sub

        Public Sub Draw(ByRef canvas As IGraphics, camera As Camera) Implements I3DModel.Draw
            Dim path = New PointF(vertices.Length - 1) {}
            Dim polygon As New GraphicsPath

            For Each pt As SeqValue(Of Point3D) In camera.Project(vertices).SeqIterator
                path(pt.i) = pt.value.PointXY(camera.screen)
            Next

            Dim a As PointF = path(0)
            Dim b As PointF

            For i As Integer = 1 To path.Length - 1
                b = path(i)
                polygon.AddLine(a, b)
                a = b
            Next

            Call polygon.AddLine(a, path(0))
            Call polygon.CloseFigure()

            Try
#If DEBUG Then
                Call canvas.DrawPath(Pens.Black, polygon)
#End If
                Call canvas.FillPath(brush, polygon)
            Catch ex As Exception
#If DEBUG Then
                Call ex.PrintException
#End If
            End Try
        End Sub

        Public Iterator Function GetEnumerator() As IEnumerator(Of Point3D) Implements IEnumerable(Of Point3D).GetEnumerator
            For Each pt As Point3D In vertices
                Yield pt
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Copy(data As IEnumerable(Of Point3D)) As I3DModel Implements I3DModel.Copy
            Dim model As New Surface With {
                .brush = brush,
                .vertices = data.ToArray
            }

            Return model
        End Function
    End Structure
End Namespace
