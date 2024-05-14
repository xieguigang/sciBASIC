#Region "Microsoft.VisualBasic::333db1696fff70faa1e9f73f66b01303, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Math3D\Vector3D.vb"

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

    '   Total Lines: 158
    '    Code Lines: 111
    ' Comment Lines: 20
    '   Blank Lines: 27
    '     File Size: 5.95 KB


    '     Structure Vector3D
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: GetEnumerator, IEnumerable_GetEnumerator, PointXY, Project, RotateX
    '                   RotateY, RotateZ
    '         Operators: -
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

Namespace Drawing3D.Math3D

    Public Structure Vector3D : Implements IEnumerable(Of Point3D)

        Public X As Vector
        Public Y As Vector
        Public Z As Vector

        Default Public ReadOnly Property Surface(v As Integer()) As Point3D()
            Get
                Dim out As Point3D() = New Point3D(v.Length - 1) {}

                For k As Integer = 0 To v.Length - 1
                    Dim i = v(k)
                    out(k) = New Point3D(X(i), Y(i), Z(i))
                Next

                Return out
            End Get
        End Property

        ''' <summary>
        ''' 函数返回来的点集合之中的元素顺序和向量之中的数值的顺序是一致的
        ''' </summary>
        ''' <param name="screen"></param>
        ''' <returns></returns>
        Public Function PointXY(screen As Size) As IEnumerable(Of Point)
            Dim out As New List(Of Point)
            Dim width! = screen.Width, height! = screen.Height

            For i As Integer = 0 To Me.X.Count - 1
                Dim x# = Me.X(i)
                Dim y# = Me.Y(i)

                If x > Integer.MaxValue OrElse Single.IsPositiveInfinity(x) Then
                    x = width
                ElseIf x < Integer.MinValue OrElse Single.IsNegativeInfinity(x) Then
                    x = 0
                ElseIf Single.IsNaN(x) Then
                    x = width
                End If

                If y > Integer.MaxValue OrElse Single.IsPositiveInfinity(y) Then
                    y = height
                ElseIf y < Integer.MinValue OrElse Single.IsNegativeInfinity(y) Then
                    y = 0
                ElseIf Single.IsNaN(y) Then
                    y = height
                End If

                out += New Point(x, y)
            Next

            Return out
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(x As Vector, y As Vector, z As Vector)
            With Me
                .X = x
                .Y = y
                .Z = z
            End With
        End Sub

        Sub New(points As IEnumerable(Of Point3D))
            With points.ToArray
                Me.X = .Select(Function(p) CDbl(p.X)).ToArray
                Me.Y = .Select(Function(p) CDbl(p.Y)).ToArray
                Me.Z = .Select(Function(p) CDbl(p.Z)).ToArray
            End With
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="angle">Degree.(度，函数里面会自动转换为三角函数所需要的弧度的)</param>
        ''' <returns></returns>
        Public Function RotateX(angle As Single) As Vector3D
            Dim rad As Single, cosa As Single, sina As Single, yn As Vector, zn As Vector

            rad = angle * std.PI / 180
            cosa = std.Cos(rad)
            sina = std.Sin(rad)
            yn = Me.Y * cosa - Me.Z * sina
            zn = Me.Y * sina + Me.Z * cosa
            Return New Vector3D(New Vector(Me.X), yn, zn)
        End Function

        Public Function RotateY(angle As Single) As Vector3D
            Dim rad As Single, cosa As Single, sina As Single, Xn As Vector, Zn As Vector

            rad = angle * std.PI / 180
            cosa = std.Cos(rad)
            sina = std.Sin(rad)
            Zn = Me.Z * cosa - Me.X * sina
            Xn = Me.Z * sina + Me.X * cosa

            Return New Vector3D(Xn, New Vector(Me.Y), Zn)
        End Function

        Public Function RotateZ(angle As Single) As Vector3D
            Dim rad As Single, cosa As Single, sina As Single, Xn As Vector, Yn As Vector

            rad = angle * std.PI / 180
            cosa = std.Cos(rad)
            sina = std.Sin(rad)
            Xn = Me.X * cosa - Me.Y * sina
            Yn = Me.X * sina + Me.Y * cosa
            Return New Vector3D(Xn, Yn, New Vector(Me.Z))
        End Function

        ''' <summary>
        ''' Project the 3D point to the 2D screen. By using the projection result, 
        ''' just read the property <see cref="PointXY"/>.
        ''' (将3D投影为2D，所以只需要取结果之中的<see cref="X"/>和<see cref="Y"/>就行了)
        ''' </summary>
        ''' <param name="viewWidth"></param>
        ''' <param name="viewHeight"></param>
        ''' <param name="fov">256默认值</param>
        ''' <param name="viewDistance"></param>
        ''' <returns></returns>
        Public Function Project(viewWidth%, viewHeight%, fov%, viewDistance!, Optional offset As PointF = Nothing) As Vector3D
            Dim factor As Vector, Xn As Vector, Yn As Vector

            factor = fov / (viewDistance + Me.Z)
            Xn = Me.X * factor + viewWidth / 2 + offset.X
            Yn = Me.Y * factor + viewHeight / 2 + offset.Y

            Return New Vector3D(Xn, Yn, New Vector(Me.Z))
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator -(p3D As Vector3D, offset As Point3D) As Vector3D
            Return New Vector3D With {
                .X = p3D.X - offset.X,
                .Y = p3D.Y - offset.Y,
                .Z = p3D.Z - offset.Z
            }
        End Operator

        Public Iterator Function GetEnumerator() As IEnumerator(Of Point3D) Implements IEnumerable(Of Point3D).GetEnumerator
            For i As Integer = 0 To X.Length - 1
                Yield New Point3D(X(i), Y(i), Z(i))
            Next
        End Function

        Private Iterator Function IEnumerable_GetEnumerator() As IEnumerator Implements IEnumerable.GetEnumerator
            Yield GetEnumerator()
        End Function
    End Structure
End Namespace
