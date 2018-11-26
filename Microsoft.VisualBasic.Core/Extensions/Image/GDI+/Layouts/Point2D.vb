Imports System.Drawing
Imports sys = System.Math

Namespace Imaging.LayoutModel

    ''' <summary>
    ''' Implements a 2-dimensional point with <see cref="Double"/> precision coordinates.
    ''' </summary>
    <Serializable> Public Class Point2D : Implements ICloneable

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
        Public Sub New(x As Double, y As Double)
            Me.X = x
            Me.Y = y
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
                Return New Point(CInt(Fix(sys.Round(X))), CInt(Fix(sys.Round(Y))))
            End Get
        End Property

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

        ''' <summary>
        ''' Returns a new instance of the same point.
        ''' </summary>
        Public Overridable Function Clone() As Object Implements ICloneable.Clone
            Return New Point2D(X, Y)
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