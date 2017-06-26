#Region "Microsoft.VisualBasic::032babc153868f93e7754cd1d3c128e6, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\Extensions\Image\GDI+\LayoutedObject.vb"

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
Imports Microsoft.VisualBasic.Language

Namespace Imaging.LayoutModel

    ''' <summary>
    ''' Any typed object with a location layout value
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Interface ILayoutedObject(Of T)
        Inherits Value(Of T).IValueOf

        Property Location As PointF
    End Interface

    ''' <summary>
    ''' Implements a 2-dimensional point with <see cref="Double"/> precision coordinates.
    ''' </summary>
    <Serializable>
    Public Class mxPoint : Implements ICloneable

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
        Public Sub New(ByVal point As Point)
            Me.New(point.X, point.Y)
        End Sub

        ''' <summary>
        ''' Constructs a new point at the location of the given point.
        ''' </summary>
        ''' <param name="point"> Point that specifies the location. </param>
        Public Sub New(ByVal point As mxPoint)
            Me.New(point.X, point.Y)
        End Sub

        ''' <summary>
        ''' Constructs a new point at (x, y).
        ''' </summary>
        ''' <param name="x"> X-coordinate of the point to be created. </param>
        ''' <param name="y"> Y-coordinate of the point to be created. </param>
        Public Sub New(ByVal x As Double, ByVal y As Double)
            x = x
            y = y
        End Sub

        ''' <summary>
        ''' Returns the x-coordinate of the point.
        ''' </summary>
        ''' <returns> Returns the x-coordinate. </returns>
        Public Overridable Property X As Double

        ''' <summary>
        ''' Returns the x-coordinate of the point.
        ''' </summary>
        ''' <returns> Returns the x-coordinate. </returns>
        Public Overridable Property Y As Double

        ''' <summary>
        ''' Returns the coordinates as a new point.
        ''' </summary>
        ''' <returns> Returns a new point for the location. </returns>
        Public Overridable ReadOnly Property Point As Point
            Get
                Return New Point(CInt(Fix(Math.Round(X))), CInt(Fix(Math.Round(Y))))
            End Get
        End Property

        ''' 
        ''' <summary>
        ''' Returns true if the given object equals this rectangle.
        ''' </summary>
        Public Overrides Function Equals(ByVal obj As Object) As Boolean
            If TypeOf obj Is mxPoint Then
                Dim pt As mxPoint = CType(obj, mxPoint)

                Return pt.X = X AndAlso pt.Y = Y
            End If

            Return False
        End Function

        ''' <summary>
        ''' Returns a new instance of the same point.
        ''' </summary>
        Public Overridable Function Clone() As Object Implements ICloneable.Clone
            Return New mxPoint(X, Y)
        End Function

        ''' <summary>
        ''' Returns a <code>String</code> that represents the value
        ''' of this <code>mxPoint</code>. </summary>
        ''' <returns> a string representation of this <code>mxPoint</code>. </returns>
        Public Overrides Function ToString() As String
            Return Me.GetType().Name & "[" & X & ", " & Y & "]"
        End Function
    End Class
End Namespace
