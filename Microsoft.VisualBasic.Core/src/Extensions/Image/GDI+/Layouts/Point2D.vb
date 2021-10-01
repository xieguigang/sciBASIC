#Region "Microsoft.VisualBasic::4c6e4bcaf43cd53cf190230a393b776e, Microsoft.VisualBasic.Core\src\Extensions\Image\GDI+\Layouts\Point2D.vb"

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

    '     Class Point2D
    ' 
    '         Properties: Point, PointF, X, Y
    ' 
    '         Constructor: (+5 Overloads) Sub New
    '         Function: Clone, (+3 Overloads) DistanceTo, (+2 Overloads) Equals, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.Correlations
Imports stdNum = System.Math

Namespace Imaging.LayoutModel

    ''' <summary>
    ''' Implements a 2-dimensional point with <see cref="Double"/> precision coordinates.
    ''' </summary>
    ''' <remarks>
    ''' 这个数据对象主要是用来储存作图的原始数据
    ''' 因为如果作图的原始数据值很高的话，往往会超过single的上限，所以会需要使用double
    ''' 类型的数据进行存储，在经过绘图函数的scale之后，就可以转换为pointf对象进行作图了
    ''' </remarks>
    <Serializable> Public Class Point2D : Implements ICloneable

        ''' <summary>
        ''' Returns the x-coordinate of the point.
        ''' </summary>
        ''' <returns> Returns the x-coordinate. </returns>
        Public Property X As Double

        ''' <summary>
        ''' Returns the x-coordinate of the point.
        ''' </summary>
        ''' <returns> Returns the x-coordinate. </returns>
        Public Property Y As Double

        Default Public Overridable Property Axis(a As String) As Double
            Get
                Select Case UCase(a)
                    Case NameOf(X)
                        Return X
                    Case NameOf(Y)
                        Return Y
                    Case Else
                        Throw New NotImplementedException(a)
                End Select
            End Get
            Set(value As Double)
                Select Case UCase(a)
                    Case NameOf(X)
                        X = value
                    Case NameOf(Y)
                        Y = value
                    Case Else
                        Throw New NotImplementedException(a)
                End Select
            End Set
        End Property

        ''' <summary>
        ''' Returns the coordinates as a new point.
        ''' </summary>
        ''' <returns> Returns a new point for the location. </returns>
        Public Overridable ReadOnly Property Point As Point
            Get
                Return New Point(CInt(Fix(stdNum.Round(X))), CInt(Fix(stdNum.Round(Y))))
            End Get
        End Property

        Public ReadOnly Property PointF As PointF
            Get
                Return New PointF(X, Y)
            End Get
        End Property

        ''' <summary>
        ''' Constructs a new point at (0, 0).
        ''' </summary>
        Public Sub New()
            Me.New(0, 0)
        End Sub

        ''' <summary>
        ''' Constructs a new point at the location of the given point.
        ''' </summary>
        ''' <param name="point"> Point that specifies the location. </param>
        Public Sub New(point As Point)
            Me.New(point.X, point.Y)
        End Sub

        <DebuggerStepThrough>
        Sub New(point As PointF)
            Call Me.New(point.X, point.Y)
        End Sub

        ''' <summary>
        ''' Constructs a new point at the location of the given point.
        ''' </summary>
        ''' <param name="point"> Point that specifies the location. </param>
        Public Sub New(point As Point2D)
            Me.New(point.X, point.Y)
        End Sub

        ''' <summary>
        ''' Constructs a new point at (x, y).
        ''' </summary>
        ''' <param name="x"> X-coordinate of the point to be created. </param>
        ''' <param name="y"> Y-coordinate of the point to be created. </param>
        ''' 
        <DebuggerStepThrough>
        Public Sub New(x As Double, y As Double)
            Me.X = x
            Me.Y = y
        End Sub

        ''' 
        ''' <summary>
        ''' Returns true if the given object equals this rectangle.
        ''' </summary>
        Public Overrides Function Equals(obj As Object) As Boolean
            If TypeOf obj Is Point2D Then
                Dim pt As Point2D = CType(obj, Point2D)

                Return pt.X = X AndAlso pt.Y = Y
            End If

            Return False
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overloads Shared Function Equals(a As Point2D, b As Point2D) As Boolean
            Return stdNum.Abs(a.X - b.X) <= 0.001 AndAlso stdNum.Abs(a.Y - b.Y) <= 0.001
        End Function

        ''' <summary>
        ''' Returns a new instance of the same point.
        ''' </summary>
        Public Overridable Function Clone() As Object Implements ICloneable.Clone
            Return New Point2D(X, Y)
        End Function

        Public Function DistanceTo(b As Point) As Double
            Return {X, Y}.EuclideanDistance(New Double() {b.X, b.Y})
        End Function

        Public Function DistanceTo(b As PointF) As Double
            Return {X, Y}.EuclideanDistance(New Double() {b.X, b.Y})
        End Function

        Public Function DistanceTo(b As Point2D) As Double
            Return {X, Y}.EuclideanDistance({b.X, b.Y})
        End Function

        ''' <summary>
        ''' Returns a <code>String</code> that represents the value
        ''' of this <code>mxPoint</code>. </summary>
        ''' <returns> a string representation of this <code>mxPoint</code>. </returns>
        Public Overrides Function ToString() As String
            Return Me.GetType().Name & "[" & X & ", " & Y & "]"
        End Function

        Public Shared Narrowing Operator CType(p As Point2D) As PointF
            Return New PointF(p.X, p.Y)
        End Operator
    End Class
End Namespace
