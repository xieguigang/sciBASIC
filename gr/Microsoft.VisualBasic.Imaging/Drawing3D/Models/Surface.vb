#Region "Microsoft.VisualBasic::dba279924f628a15492cb4af1f5a9cf7, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing3D\Models\Surface.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Drawing
Imports System.Drawing.Drawing2D
Imports Microsoft.VisualBasic.Linq

Namespace Drawing3D

    Public Class Surface
        Implements IEnumerable(Of Point3D)
        Implements I3DModel

        ''' <summary>
        ''' 请注意，在这里面的点都是有先后顺序分别的
        ''' </summary>
        Public vertices() As Point3D
        Public brush As Brush

        Dim path As Point()

        Public Sub Allocation()
            path = New Point(vertices.Length - 1) {}
        End Sub

        Public Sub Draw(ByRef canvas As Graphics, camera As Camera) Implements I3DModel.Draw
            SyncLock path
                Dim polygon As New GraphicsPath

                For Each pt As SeqValue(Of Point3D) In camera.Project(vertices).SeqIterator
                    path(pt.i) = pt.obj.PointXY(camera.screen)
                Next

                Dim a As Point = path(0)
                Dim b As Point

                For i As Integer = 1 To path.Length - 1
                    b = path(i)
                    Call polygon.AddLine(a, b)
                    a = b
                Next

                Call polygon.AddLine(a, path(0))
                Call polygon.CloseFigure()
#If DEBUG Then
                Call canvas.DrawPath(Pens.Black, polygon)
#End If
                Call canvas.FillPath(brush, polygon)
            End SyncLock
        End Sub

        Public Iterator Function GetEnumerator() As IEnumerator(Of Point3D) Implements IEnumerable(Of Point3D).GetEnumerator
            For Each pt As Point3D In vertices
                Yield pt
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function

        Public Function Copy(data As IEnumerable(Of Point3D)) As I3DModel Implements I3DModel.Copy
            Dim model As New Surface With {
                .brush = brush,
                .vertices = data.ToArray
            }
            Call model.Allocation()

            Return model
        End Function
    End Class
End Namespace
