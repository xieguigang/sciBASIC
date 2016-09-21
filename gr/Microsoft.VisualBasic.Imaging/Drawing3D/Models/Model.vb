#Region "Microsoft.VisualBasic::7941be62c61679bc67bcbe97f439f4f2, ..\visualbasic_App\Datavisualization\Microsoft.VisualBasic.Imaging\Drawing3D\Models\Model.vb"

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

Namespace Drawing3D

    ''' <summary>
    ''' 标傲世一个3D物体的模型
    ''' </summary>
    Public MustInherit Class Model : Inherits ModelData

        ''' <summary>
        ''' The <see cref="Point3D"/> data vector array length
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Length As Integer
            Get
                Return _vertices.Length
            End Get
        End Property

        Public MustOverride Sub Draw(gdi As Graphics)

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="angle"></param>
        ''' <param name="clientSize"></param>
        ''' <param name="aixs"></param>
        ''' <returns></returns>
        ''' <remarks>http://codentronix.com/2011/05/25/rotating-solid-cube-using-vb-net-and-gdi/</remarks>
        Public Function Rotate(angle As Integer, clientSize As Size, aixs As Aixs) As ModelView
            Dim t(Length - 1) As Point3D
            Dim v As Point3D
            Dim sx As Boolean = aixs.HasFlag(Aixs.X)
            Dim sy As Boolean = aixs.HasFlag(Aixs.Y)
            Dim sz As Boolean = aixs.HasFlag(Aixs.Z)
            Dim avgZ(6) As Double
            Dim order(6) As Integer

            ' Transform all the points and store them on the "t" array.
            For i As Integer = 0 To Length - 1
                v = _vertices(i)

                If sx Then v = v.RotateX(angle)
                If sy Then v = v.RotateY(angle)
                If sz Then v = v.RotateZ(angle)

                t(i) = v.Project(clientSize.Width, clientSize.Height, 256, 4)
            Next

            ' Compute the average Z value of each face.
            For i = 0 To 5
                avgZ(i) = (t(_faces(i, 0)).Z + t(_faces(i, 1)).Z + t(_faces(i, 2)).Z + t(_faces(i, 3)).Z) / 4.0
                order(i) = i
            Next

            ' Next we sort the faces in descending order based on the Z value.
            ' The objective is to draw distant faces first. This is called
            ' the PAINTERS ALGORITHM. So, the visible faces will hide the invisible ones.
            ' The sorting algorithm used is the SELECTION SORT.

            Dim iMax As Integer
            Dim tmp As Double

            For i = 0 To 4
                iMax = i
                For j = i + 1 To 5
                    If avgZ(j) > avgZ(iMax) Then
                        iMax = j
                    End If
                Next
                If iMax <> i Then
                    tmp = avgZ(i)
                    avgZ(i) = avgZ(iMax)
                    avgZ(iMax) = tmp

                    tmp = order(i)
                    order(i) = order(iMax)
                    order(iMax) = tmp
                End If
            Next

            Return New ModelView(t, _faces, _brushes, order)
        End Function
    End Class

    Public Class ModelView : Inherits ModelData

        ''' <summary>
        ''' Draw the faces using the PAINTERS ALGORITHM (distant faces first, closer faces last).
        ''' (表面的绘制的顺序)
        ''' </summary>
        Dim order() As Integer

        Sub New(vertices As Point3D(), faces As Integer(,), brushes As Brush(), orders As Integer())
            Call MyBase.New(vertices, faces, brushes)
            order = orders
        End Sub

        ''' <summary>
        ''' Draw the faces using the PAINTERS ALGORITHM (distant faces first, closer faces last).
        ''' </summary>
        ''' <param name="gdi"></param>
        Public Sub UpdateGraphics(gdi As Graphics)
            Dim points() As Point

            For Each index As Integer In order

                points = New Point() {
                    New Point(CInt(_vertices(_faces(index, 0)).X), CInt(_vertices(_faces(index, 0)).Y)),
                    New Point(CInt(_vertices(_faces(index, 1)).X), CInt(_vertices(_faces(index, 1)).Y)),
                    New Point(CInt(_vertices(_faces(index, 2)).X), CInt(_vertices(_faces(index, 2)).Y)),
                    New Point(CInt(_vertices(_faces(index, 3)).X), CInt(_vertices(_faces(index, 3)).Y))
                }
                gdi.FillPolygon(_brushes(index), points)
            Next
        End Sub
    End Class

    Public Class ModelData

        ''' <summary>
        ''' 顶点
        ''' </summary>
        Protected _vertices(8) As Point3D
        ''' <summary>
        ''' 表面
        ''' </summary>
        Protected _faces(6, 4) As Integer
        ''' <summary>
        ''' 颜色刷子的缓存
        ''' </summary>
        Protected _brushes(6) As Brush

        Sub New()
        End Sub

        Sub New(vertices As Point3D(), faces As Integer(,), brushes As Brush())
            _vertices = vertices
            _faces = faces
            _brushes = brushes
        End Sub
    End Class

    Public Enum Aixs As Byte
        X = 2
        Y = 4
        Z = 8
        All = X + Y + Z
    End Enum

    Public Class Cube : Inherits Model

        Sub New(Optional colors As Color() = Nothing)
            ' Create the cube vertices.
            _vertices = New Point3D() {
                         New Point3D(-1, 1, -1),
                         New Point3D(1, 1, -1),
                         New Point3D(1, -1, -1),
                         New Point3D(-1, -1, -1),
                         New Point3D(-1, 1, 1),
                         New Point3D(1, 1, 1),
                         New Point3D(1, -1, 1),
                         New Point3D(-1, -1, 1)}

            ' Create an array representing the 6 faces of a cube. Each face is composed by indices to the vertex array
            ' above.
            _faces = New Integer(,) {{0, 1, 2, 3}, {1, 5, 6, 2}, {5, 4, 7, 6}, {4, 0, 3, 7}, {0, 4, 5, 1}, {3, 2, 6, 7}}

            ' Define the colors of each face.
            If colors.IsNullOrEmpty Then
                colors = New Color() {Color.BlueViolet, Color.Cyan, Color.Green, Color.Yellow, Color.Violet, Color.LightSkyBlue}
            End If

            ' Create the brushes to draw each face. Brushes are used to draw filled polygons.
            For i = 0 To 5
                _brushes(i) = New SolidBrush(colors(i))
            Next
        End Sub

        Public Overrides Sub Draw(gdi As Graphics)

        End Sub
    End Class
End Namespace
