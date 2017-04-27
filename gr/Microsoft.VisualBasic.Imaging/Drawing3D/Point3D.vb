#Region "Microsoft.VisualBasic::e659481a7a80c379bbbd859e062f42f9, ..\sciBASIC#\gr\Microsoft.VisualBasic.Imaging\Drawing3D\Point3D.vb"

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
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Imaging.Drawing3D.Math3D
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Drawing3D

    ''' <summary>
    ''' Defines the Point3D class that represents points in 3D space.
    ''' Developed by leonelmachava &lt;leonelmachava@gmail.com>
    ''' http://codentronix.com
    '''
    ''' Copyright (c) 2011 Leonel Machava
    ''' </summary>
    ''' 
    <XmlType("vertex")> Public Structure Point3D
        Implements PointF3D

        ''' <summary>
        ''' The depth of a point in the isometric plane
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Depth As Double
            Get
                ' z is weighted slightly to accommodate |_ arrangements 
                Return Me.X + Me.Y - 2 * Me.Z
            End Get
        End Property

        Public Sub New(x As Single, y As Single, Optional z As Single = 0)
            Me.X = x
            Me.Y = y
            Me.Z = z
        End Sub

        Public Sub New(p As Point)
            Call Me.New(p.X, p.Y)
        End Sub

        <XmlAttribute("x")> Public Property X As Single Implements PointF3D.X
        <XmlAttribute("y")> Public Property Y As Single Implements PointF3D.Y
        <XmlAttribute("z")> Public Property Z As Single Implements PointF3D.Z

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="angle">Degree.(度，函数里面会自动转换为三角函数所需要的弧度的)</param>
        ''' <returns></returns>
        Public Function RotateX(angle As Single) As Point3D
            Dim rad As Single, cosa As Single, sina As Single, yn As Single, zn As Single

            rad = angle * Math.PI / 180
            cosa = Math.Cos(rad)
            sina = Math.Sin(rad)
            yn = Me.Y * cosa - Me.Z * sina
            zn = Me.Y * sina + Me.Z * cosa
            Return New Point3D(Me.X, yn, zn)
        End Function

        Public Function RotateY(angle As Single) As Point3D
            Dim rad As Single, cosa As Single, sina As Single, Xn As Single, Zn As Single

            rad = angle * Math.PI / 180
            cosa = Math.Cos(rad)
            sina = Math.Sin(rad)
            Zn = Me.Z * cosa - Me.X * sina
            Xn = Me.Z * sina + Me.X * cosa

            Return New Point3D(Xn, Me.Y, Zn)
        End Function

        Public Function RotateZ(angle As Single) As Point3D
            Dim rad As Single, cosa As Single, sina As Single, Xn As Single, Yn As Single

            rad = angle * Math.PI / 180
            cosa = Math.Cos(rad)
            sina = Math.Sin(rad)
            Xn = Me.X * cosa - Me.Y * sina
            Yn = Me.X * sina + Me.Y * cosa
            Return New Point3D(Xn, Yn, Me.Z)
        End Function

        ''' <summary>
        ''' Project the 3D point to the 2D screen. By using the projection result, 
        ''' just read the property <see cref="Projection.PointXY"/>.
        ''' (将3D投影为2D，所以只需要取结果之中的<see cref="X"/>和<see cref="Y"/>就行了)
        ''' </summary>
        ''' <param name="viewWidth"></param>
        ''' <param name="viewHeight"></param>
        ''' <param name="fov">256默认值</param>
        ''' <param name="viewDistance"></param>
        ''' <returns></returns>
        Public Function Project(viewWidth%, viewHeight%, fov%, viewDistance!, Optional offset As PointF = Nothing) As Point3D
            Dim factor As Single, Xn As Single, Yn As Single

            factor = fov / (viewDistance + Me.Z)
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
        ''' <param name="viewWidth%"></param>
        ''' <param name="viewHeight%"></param>
        ''' <param name="fov%"></param>
        ''' <param name="viewDistance%">View distance to the model from the view window.</param>
        Public Shared Sub Project(ByRef x!, ByRef y!, z!, viewWidth%, viewHeight%, viewDistance%, Optional fov% = 256)
            Dim factor! = fov / (viewDistance + z)

            ' 2D point result (x, y)
            x = x * factor + viewWidth / 2
            y = y * factor + viewHeight / 2
        End Sub

        Public Shared Operator -(p3D As Point3D, offset As Point3D) As Point3D
            Return New Point3D(
                p3D.X - offset.X,
                p3D.Y - offset.Y,
                p3D.Z - offset.Z)
        End Operator

        ''' <summary>
        ''' 所有的分量是否都等于目标值？使用这个操作符可以很方便的判断点是否为空值
        ''' </summary>
        ''' <param name="p"></param>
        ''' <param name="n!"></param>
        ''' <returns></returns>
        Public Shared Operator =(p As Point3D, n!) As Boolean
            With p
                Return .X = n AndAlso .Y = n AndAlso .Z = n
            End With
        End Operator

        Public Shared Operator <>(p As Point3D, n!) As Boolean
            Return Not p = n
        End Operator

        Public Shared Widening Operator CType(pt As Point) As Point3D
            Return New Point3D(pt)
        End Operator
    End Structure
End Namespace
