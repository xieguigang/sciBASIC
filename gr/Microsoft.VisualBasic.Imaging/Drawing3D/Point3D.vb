#Region "Microsoft.VisualBasic::b9e4943c916c1fc6fe0482ab80ec388b, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Point3D.vb"

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

    '   Total Lines: 363
    '    Code Lines: 207 (57.02%)
    ' Comment Lines: 101 (27.82%)
    '    - Xml Docs: 90.10%
    ' 
    '   Blank Lines: 55 (15.15%)
    '     File Size: 13.82 KB


    '     Structure Point3D
    ' 
    '         Properties: Depth, X, Y, Z
    ' 
    '         Constructor: (+7 Overloads) Sub New
    ' 
    '         Function: Add, ClampProjection, Clone, Cross, Distance
    '                   Divide, Dot, Length, Lerp, (+2 Overloads) Multiply
    '                   Normalize, Parse, Project, RotateX, RotateY
    '                   RotateYawPitch, RotateZ, Subtract, ToArray, ToPointF
    '                   ToString, Translate
    ' 
    '         Sub: Project
    ' 
    '         Operators: -, <>, =
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D
Imports std = System.Math
Imports vec = Microsoft.VisualBasic.Math.LinearAlgebra.Vector

Namespace Drawing3D

    ''' <summary>
    ''' Defines the Point3D class that represents points in 3D space with <see cref="Double"/> precise.
    ''' Developed by leonelmachava &lt;leonelmachava@gmail.com>
    ''' http://codentronix.com
    '''
    ''' Copyright (c) 2011 Leonel Machava
    ''' </summary>
    <XmlType("vertex")> Public Structure Point3D : Implements PointF3D

        ''' <summary>
        ''' The depth of a point in the isometric plane
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Depth As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                ' z is weighted slightly to accommodate |_ arrangements 
                Return Me.X + Me.Y - 2 * Me.Z
            End Get
        End Property

        <XmlAttribute("x")> Public Property X As Double Implements PointF3D.X
        <XmlAttribute("y")> Public Property Y As Double Implements PointF3D.Y
        <XmlAttribute("z")> Public Property Z As Double Implements PointF3D.Z

        Public Sub New(x!, y!, Optional z! = 0)
            Me.X = x
            Me.Y = y
            Me.Z = z
        End Sub

        Sub New(x#, y#, Optional z# = 0)
            Me.X = x
            Me.Y = y
            Me.Z = z
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(p As PointF, Optional z! = 0.0)
            Me.X = p.X
            Me.Y = p.Y
            Me.Z = z
        End Sub

        ''' <summary>
        ''' Copy 3d point data from the 3d point interface model
        ''' </summary>
        ''' <param name="p">the 3d point interface value</param>
        Sub New(p As PointF3D)
            Call Me.New(p.X, p.Y, p.Z)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub New(p As Point)
            Call Me.New(p.X, p.Y)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(xyz As Double())
            Call Me.New(xyz(0), xyz(1), xyz.ElementAtOrDefault(2))
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Sub New(xyz As Single())
            Call Me.New(xyz(0), xyz(1), xyz.ElementAtOrDefault(2))
        End Sub

        Public Overrides Function ToString() As String
            Return $"[x:{X.ToString("F2")},y:{Y.ToString("F3")},z:{Z.ToString("F3")}]"
        End Function

        Public Function Clone() As Point3D
            Return New Point3D(X, Y, Z)
        End Function

        Public Function ToArray() As Single()
            Return New Single() {CSng(X), CSng(Y), CSng(Z)}
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Dot(a As Point3D, b As Point3D) As Double
            Return a.DotProduct(b)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Cross(v1 As Point3D, v2 As Point3D) As Point3D
            Return v1.CrossProduct(v2)
        End Function

        ''' <summary>
        ''' Vector addition.
        ''' </summary>
        Public Function Add(v As Point3D) As Point3D
            Return New Point3D(X + v.X, Y + v.Y, Z + v.Z)
        End Function

        ''' <summary>
        ''' Vector subtraction (this - v).
        ''' </summary>
        Public Function Subtract(v As Point3D) As Point3D
            Return New Point3D(X - v.X, Y - v.Y, Z - v.Z)
        End Function

        ''' <summary>
        ''' Scalar multiplication.
        ''' </summary>
        Public Function Multiply(scalar As Double) As Point3D
            Return New Point3D(X * scalar, Y * scalar, Z * scalar)
        End Function

        ''' <summary>
        ''' Component-wise multiplication.
        ''' </summary>
        Public Function Multiply(v As Point3D) As Point3D
            Return New Point3D(X * v.X, Y * v.Y, Z * v.Z)
        End Function

        ''' <summary>
        ''' Component-wise division.
        ''' </summary>
        Public Function Divide(v As Point3D) As Point3D
            Return New Point3D(X / v.X, Y / v.Y, Z / v.Z)
        End Function

        ''' <summary>
        ''' Euclidean length (magnitude) of the vector.
        ''' </summary>
        Public Function Length() As Double
            Return std.Sqrt(X * X + Y * Y + Z * Z)
        End Function

        ''' <summary>
        ''' Returns the unit vector. Degenerate (zero length) vectors return the zero vector.
        ''' </summary>
        Public Function Normalize() As Point3D
            Dim length As Double = Me.Length()

            If length = 0 Then
                Return New Point3D(0, 0, 0)
            Else
                Return New Point3D(X / length, Y / length, Z / length)
            End If
        End Function

        ''' <summary>
        ''' Translation by the given offset vector (non-mutating, returns a new point).
        ''' </summary>
        Public Function Translate(v As Point3D) As Point3D
            Return New Point3D(X + v.X, Y + v.Y, Z + v.Z)
        End Function

        ''' <summary>
        ''' Euclidean distance between two points.
        ''' </summary>
        Public Shared Function Distance(a As Point3D, b As Point3D) As Double
            Return std.Sqrt((a.X - b.X) ^ 2 + (a.Y - b.Y) ^ 2 + (a.Z - b.Z) ^ 2)
        End Function

        ''' <summary>
        ''' Linear interpolation between two points.
        ''' </summary>
        Public Shared Function Lerp(a As Point3D, b As Point3D, t As Single) As Point3D
            Return a.Add(b.Subtract(a).Multiply(t))
        End Function

        ''' <summary>
        ''' Combined yaw (around Y) and pitch (around X) rotation.
        ''' </summary>
        ''' <param name="yaw">Degree.</param>
        ''' <param name="pitch">Degree.</param>
        Public Function RotateYawPitch(yaw As Single, pitch As Single) As Point3D
            ' Convert to radians
            Dim yawRads = yaw * std.PI / 180
            Dim pitchRads = pitch * std.PI / 180

            ' Step one: Rotate around X axis (pitch)
            Dim ny As Double = Y * std.Cos(pitchRads) - Z * std.Sin(pitchRads)
            Dim nz As Double = Y * std.Sin(pitchRads) + Z * std.Cos(pitchRads)

            ' Step two: Rotate around the Y axis (yaw)
            Dim nx As Double = X * std.Cos(yawRads) + nz * std.Sin(yawRads)
            nz = -X * std.Sin(yawRads) + nz * std.Cos(yawRads)

            Return New Point3D(nx, ny, nz)
        End Function

        ''' <summary>
        ''' Rotate this point around the X axis by <paramref name="angle"/> (degree).
        ''' </summary>
        Public Function RotateX(angle As Single) As Point3D
            Dim rad = angle * std.PI / 180
            Dim cosa = std.Cos(rad)
            Dim sina = std.Sin(rad)
            Dim ny = Y * cosa - Z * sina
            Dim nz = Y * sina + Z * cosa

            Return New Point3D(X, ny, nz)
        End Function

        ''' <summary>
        ''' Rotate this point around the Y axis by <paramref name="angle"/> (degree).
        ''' </summary>
        Public Function RotateY(angle As Single) As Point3D
            Dim rad = angle * std.PI / 180
            Dim cosa = std.Cos(rad)
            Dim sina = std.Sin(rad)
            Dim nx = Z * cosa - X * sina
            Dim nz = Z * sina + X * cosa

            Return New Point3D(nx, Y, nz)
        End Function

        ''' <summary>
        ''' Rotate this point around the Z axis by <paramref name="angle"/> (degree).
        ''' </summary>
        Public Function RotateZ(angle As Single) As Point3D
            Dim rad = angle * std.PI / 180
            Dim cosa = std.Cos(rad)
            Dim sina = std.Sin(rad)
            Dim nx = X * cosa - Y * sina
            Dim ny = X * sina + Y * cosa

            Return New Point3D(nx, ny, Z)
        End Function

        ''' <summary>
        ''' Project the 3D point to the 2D screen. By using the projection result, 
        ''' just read the property <see cref="PointF.X"/> and <see cref="PointF.Y"/>.
        ''' (将3D投影为2D，所以只需要取结果之中的<see cref="X"/>和<see cref="Y"/>就行了)
        ''' </summary>
        ''' <param name="viewWidth"></param>
        ''' <param name="viewHeight"></param>
        ''' <param name="fov">256默认值</param>
        ''' <param name="viewDistance"></param>
        ''' <returns></returns>
        Public Function Project(viewWidth%, viewHeight%, fov%, viewDistance!, Optional offset As PointF = Nothing) As Point3D
            ' 1. 计算深度
            Dim depth As Single = viewDistance + Me.Z

            ' Behind / on the camera plane: avoid division by zero and NaN/Infinity
            ' leaking into the draw pipeline by collapsing the perspective factor to 0
            ' (the point effectively projects onto the screen centre).
            Dim factor As Single
            Dim Xn As Single
            Dim Yn As Single

            If depth <= 0 Then
                factor = 0
            Else
                factor = fov / depth
            End If

            Xn = Me.X * factor + viewWidth / 2 + offset.X
            Yn = Me.Y * factor + viewHeight / 2 + offset.Y

            Return New Point3D(Xn, Yn, Me.Z)
        End Function

        ''' <summary>
        ''' Project the 3D point to the 2D screen. 
        ''' </summary>
        ''' <param name="x!"></param>
        ''' <param name="y!"></param>
        ''' <param name="z!">Using for the painter algorithm.</param>
        ''' <param name="viewWidth"></param>
        ''' <param name="viewHeight"></param>
        ''' <param name="fov">球度，当这个参数值非常大的时候，则产生的3D图像为isometrix类型的对称等边图形</param>
        ''' <param name="viewDistance">View distance to the model from the view window.</param>
        Public Shared Sub Project(ByRef x!, ByRef y!, z!, viewWidth%, viewHeight%, viewDistance%, Optional fov% = 256)
            Dim factor! = fov / (viewDistance + z)

            ' 2D point result (x, y)
            x = x * factor + viewWidth / 2
            y = y * factor + viewHeight / 2
        End Sub

        ''' <summary>
        ''' Clamp a projected coordinate to the valid screen range, replacing
        ''' positive/negative infinity and NaN with the corresponding edge.
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function ClampProjection(value As Double, edge As Double) As Double
            If value > Integer.MaxValue OrElse Single.IsPositiveInfinity(CSng(value)) Then
                Return edge
            ElseIf value < Integer.MinValue OrElse Single.IsNegativeInfinity(CSng(value)) Then
                Return 0
            ElseIf Single.IsNaN(CSng(value)) Then
                Return edge
            End If

            Return value
        End Function

        ''' <summary>
        ''' Convert this 3D point into a 2D GDI+ point, clamping out-of-range values.
        ''' </summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function ToPointF(Optional rect As Size = Nothing) As PointF
            Dim width As Double = If(rect = Nothing, 0, rect.Width)
            Dim height As Double = If(rect = Nothing, 0, rect.Height)

            Return New PointF(CSng(ClampProjection(X, width)), CSng(ClampProjection(Y, height)))
        End Function

        Public Shared Function Parse(data As String) As Point3D
            Dim xyz As String() = data.Matches("[-]?\d+(\.\d+)?").ToArray
            Dim p As Double() = xyz.Select(AddressOf Double.Parse).ToArray

            If p.Length = 0 Then
                Return Nothing
            Else
                Return New Point3D(p)
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator -(p3D As Point3D, offset As Point3D) As Point3D
            Return New Point3D(
                x:=p3D.X - offset.X,
                y:=p3D.Y - offset.Y,
                z:=p3D.Z - offset.Z
            )
        End Operator

        ''' <summary>
        ''' 所有的分量是否都等于目标值？使用这个操作符可以很方便的判断点是否为空值
        ''' </summary>
        ''' <param name="p"></param>
        ''' <param name="n!"></param>
        ''' <returns></returns>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator =(p As Point3D, n!) As Boolean
            With p
                Return .X = n AndAlso .Y = n AndAlso .Z = n
            End With
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Operator <>(p As Point3D, n!) As Boolean
            Return Not p = n
        End Operator

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Widening Operator CType(pt As Point) As Point3D
            Return New Point3D(pt)
        End Operator

        Public Shared Widening Operator CType(expr As String) As Point3D
            With CType(expr, vec)
                Return New Point3D(.Item(0), .Item(1), .ElementAtOrDefault(2))
            End With
        End Operator
    End Structure
End Namespace
