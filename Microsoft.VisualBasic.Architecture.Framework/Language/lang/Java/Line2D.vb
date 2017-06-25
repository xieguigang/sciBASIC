Imports Point2D = System.Drawing.PointF

'
' * Copyright (c) 1997, 2013, Oracle and/or its affiliates. All rights reserved.
' * ORACLE PROPRIETARY/CONFIDENTIAL. Use is subject to license terms.
' *
' *
' *
' *
' *
' *
' *
' *
' *
' *
' *
' *
' *
' *
' *
' *
' *
' *
' *
' *
' 

Namespace Language.Java


    ''' <summary>
    ''' This <code>Line2D</code> represents a line segment in {@code (x,y)}
    ''' coordinate space.  This [Class], like all of the Java 2D API, uses a
    ''' default coordinate system called <i>user space</i> in which the y-axis
    ''' values increase downward and x-axis values increase to the right.  For
    ''' more information on the user space coordinate system, see the
    ''' <a href="https://docs.oracle.com/javase/1.3/docs/guide/2d/spec/j2d-intro.fm2.html#61857">
    ''' Coordinate Systems</a> section of the Java 2D Programmer's Guide.
    ''' <p>
    ''' This class is only the abstract superclass for all objects that
    ''' store a 2D line segment.
    ''' The actual storage representation of the coordinates is left to
    ''' the subclass.
    ''' 
    ''' @author      Jim Graham
    ''' @since 1.2
    ''' </summary>
    Public MustInherit Class Line2D

        ''' <summary>
        ''' A line segment specified with float coordinates.
        ''' @since 1.2
        ''' </summary>
        <Serializable> Public Class Float
            Inherits Line2D

            ''' <summary>
            ''' The X coordinate of the start point of the line segment.
            ''' @since 1.2
            ''' @serial
            ''' </summary>
            Public x1 As Single

            ''' <summary>
            ''' The Y coordinate of the start point of the line segment.
            ''' @since 1.2
            ''' @serial
            ''' </summary>
            Public y1 As Single

            ''' <summary>
            ''' The X coordinate of the end point of the line segment.
            ''' @since 1.2
            ''' @serial
            ''' </summary>
            Public x2 As Single

            ''' <summary>
            ''' The Y coordinate of the end point of the line segment.
            ''' @since 1.2
            ''' @serial
            ''' </summary>
            Public y2 As Single

            ''' <summary>
            ''' Constructs and initializes a Line with coordinates (0, 0) &rarr; (0, 0).
            ''' @since 1.2
            ''' </summary>
            Public Sub New()
            End Sub

            ''' <summary>
            ''' Constructs and initializes a Line from the specified coordinates. </summary>
            ''' <param name="x1"> the X coordinate of the start point </param>
            ''' <param name="y1"> the Y coordinate of the start point </param>
            ''' <param name="x2"> the X coordinate of the end point </param>
            ''' <param name="y2"> the Y coordinate of the end point
            ''' @since 1.2 </param>
            Public Sub New(x1 As Single, y1 As Single, x2 As Single, y2 As Single)
                lineine(x1, y1, x2, y2)
            End Sub

            ''' <summary>
            ''' Constructs and initializes a <code>Line2D</code> from the
            ''' specified <code>Point2D</code> objects. </summary>
            ''' <param name="p1"> the start <code>Point2D</code> of this line segment </param>
            ''' <param name="p2"> the end <code>Point2D</code> of this line segment
            ''' @since 1.2 </param>
            Public Sub New(p1 As Point2D, p2 As Point2D)
                lineine(p1, p2)
            End Sub

            ''' <summary>
            ''' {@inheritDoc}
            ''' @since 1.2
            ''' </summary>
            Public Overrides ReadOnly Property x1 As Double
                Get
                    Return CDbl(x1)
                End Get
            End Property

            ''' <summary>
            ''' {@inheritDoc}
            ''' @since 1.2
            ''' </summary>
            Public Overrides ReadOnly Property y1 As Double
                Get
                    Return CDbl(y1)
                End Get
            End Property

            ''' <summary>
            ''' {@inheritDoc}
            ''' @since 1.2
            ''' </summary>
            Public Overrides ReadOnly Property p1 As Point2D
                Get
                    Return New Point2D.Float(x1, y1)
                End Get
            End Property

            ''' <summary>
            ''' {@inheritDoc}
            ''' @since 1.2
            ''' </summary>
            Public Overrides ReadOnly Property x2 As Double
                Get
                    Return CDbl(x2)
                End Get
            End Property

            ''' <summary>
            ''' {@inheritDoc}
            ''' @since 1.2
            ''' </summary>
            Public Overrides ReadOnly Property y2 As Double
                Get
                    Return CDbl(y2)
                End Get
            End Property

            ''' <summary>
            ''' {@inheritDoc}
            ''' @since 1.2
            ''' </summary>
            Public Overrides ReadOnly Property p2 As Point2D
                Get
                    Return New Point2D.Float(x2, y2)
                End Get
            End Property

            ''' <summary>
            ''' {@inheritDoc}
            ''' @since 1.2
            ''' </summary>
            Public Overrides Sub setLine(x1 As Double, y1 As Double, x2 As Double, y2 As Double)
                Me.x1 = CSng(x1)
                Me.y1 = CSng(y1)
                Me.x2 = CSng(x2)
                Me.y2 = CSng(y2)
            End Sub

            ''' <summary>
            ''' Sets the location of the end points of this <code>Line2D</code>
            ''' to the specified float coordinates. </summary>
            ''' <param name="x1"> the X coordinate of the start point </param>
            ''' <param name="y1"> the Y coordinate of the start point </param>
            ''' <param name="x2"> the X coordinate of the end point </param>
            ''' <param name="y2"> the Y coordinate of the end point
            ''' @since 1.2 </param>
            Public Overridable Sub setLine(x1 As Single, y1 As Single, x2 As Single, y2 As Single)
                Me.x1 = x1
                Me.y1 = y1
                Me.x2 = x2
                Me.y2 = y2
            End Sub

            ''' <summary>
            ''' {@inheritDoc}
            ''' @since 1.2
            ''' </summary>
            Public Overridable Property bounds2D As Rectangle2D
                Get
                    Dim x, y, w, h As Single
                    If x1 < x2 Then
                        x = x1
                        w = x2 - x1
                    Else
                        x = x2
                        w = x1 - x2
                    End If
                    If y1 < y2 Then
                        y = y1
                        h = y2 - y1
                    Else
                        y = y2
                        h = y1 - y2
                    End If
                    Return New Rectangle2D.Float(x, y, w, h)
                End Get
            End Property

            '        
            '         * JDK 1.6 serialVersionUID
            '         
            Private Const serialVersionUID As Long = 6161772511649436349L
        End Class

        ''' <summary>
        ''' A line segment specified with double coordinates.
        ''' @since 1.2
        ''' </summary>
        <Serializable> Public Class [Double]
            Inherits Line2D

            ''' <summary>
            ''' The X coordinate of the start point of the line segment.
            ''' @since 1.2
            ''' @serial
            ''' </summary>
            Public x1 As Double

            ''' <summary>
            ''' The Y coordinate of the start point of the line segment.
            ''' @since 1.2
            ''' @serial
            ''' </summary>
            Public y1 As Double

            ''' <summary>
            ''' The X coordinate of the end point of the line segment.
            ''' @since 1.2
            ''' @serial
            ''' </summary>
            Public x2 As Double

            ''' <summary>
            ''' The Y coordinate of the end point of the line segment.
            ''' @since 1.2
            ''' @serial
            ''' </summary>
            Public y2 As Double

            ''' <summary>
            ''' Constructs and initializes a Line with coordinates (0, 0) &rarr; (0, 0).
            ''' @since 1.2
            ''' </summary>
            Sub New()
            End Sub


            ''' <summary>
            ''' Constructs and initializes a <code>Line2D</code> from the
            ''' specified coordinates. </summary>
            ''' <param name="x1"> the X coordinate of the start point </param>
            ''' <param name="y1"> the Y coordinate of the start point </param>
            ''' <param name="x2"> the X coordinate of the end point </param>
            ''' <param name="y2"> the Y coordinate of the end point
            ''' @since 1.2 </param>
            Sub New(x1 As Double, y1 As Double, x2 As Double, y2 As Double)
                lineine(x1, y1, x2, y2)
            End Sub

            ''' <summary>
            ''' Constructs and initializes a <code>Line2D</code> from the
            ''' specified <code>Point2D</code> objects. </summary>
            ''' <param name="p1"> the start <code>Point2D</code> of this line segment </param>
            ''' <param name="p2"> the end <code>Point2D</code> of this line segment
            ''' @since 1.2 </param>
            Sub New(p1 As Point2D, p2 As Point2D)
                lineine(p1, p2)
            End Sub

            ''' <summary>
            ''' {@inheritDoc}
            ''' @since 1.2
            ''' </summary>
            Public Overrides ReadOnly Property x1 As Double
                Get
                    Return x1
                End Get
            End Property

            ''' <summary>
            ''' {@inheritDoc}
            ''' @since 1.2
            ''' </summary>
            Public Overrides ReadOnly Property y1 As Double
                Get
                    Return y1
                End Get
            End Property

            ''' <summary>
            ''' {@inheritDoc}
            ''' @since 1.2
            ''' </summary>
            Public Overrides ReadOnly Property p1 As Point2D
                Get
                    Return New Point2D.Double(x1, y1)
                End Get
            End Property

            ''' <summary>
            ''' {@inheritDoc}
            ''' @since 1.2
            ''' </summary>
            Public Overrides ReadOnly Property x2 As Double
                Get
                    Return x2
                End Get
            End Property

            ''' <summary>
            ''' {@inheritDoc}
            ''' @since 1.2
            ''' </summary>
            Public Overrides ReadOnly Property y2 As Double
                Get
                    Return y2
                End Get
            End Property

            ''' <summary>
            ''' {@inheritDoc}
            ''' @since 1.2
            ''' </summary>
            Public Overrides ReadOnly Property p2 As Point2D
                Get
                    Return New Point2D.Double(x2, y2)
                End Get
            End Property

            ''' <summary>
            ''' {@inheritDoc}
            ''' @since 1.2
            ''' </summary>
            Public Overrides Sub setLine(x1 As Double, y1 As Double, x2 As Double, y2 As Double)
                Me.x1 = x1
                Me.y1 = y1
                Me.x2 = x2
                Me.y2 = y2
            End Sub

            ''' <summary>
            ''' {@inheritDoc}
            ''' @since 1.2
            ''' </summary>
            Public Overridable Property bounds2D As Rectangle2D
                Get
                    Dim x, y, w, h As Double
                    If x1 < x2 Then
                        x = x1
                        w = x2 - x1
                    Else
                        x = x2
                        w = x1 - x2
                    End If
                    If y1 < y2 Then
                        y = y1
                        h = y2 - y1
                    Else
                        y = y2
                        h = y1 - y2
                    End If
                    Return New Rectangle2D.Double(x, y, w, h)
                End Get
            End Property

            '        
            '         * JDK 1.6 serialVersionUID
            '         
            Private Const serialVersionUID As Long = 7979627399746467499L
        End Class

        ''' <summary>
        ''' This is an abstract class that cannot be instantiated directly.
        ''' Type-specific implementation subclasses are available for
        ''' instantiation and provide a number of formats for storing
        ''' the information necessary to satisfy the various accessory
        ''' methods below.
        ''' </summary>
        ''' <seealso cref= java.awt.geom.Line2D.Float </seealso>
        ''' <seealso cref= java.awt.geom.Line2D.Double
        ''' @since 1.2 </seealso>
        Protected Friend Sub New()
        End Sub

        ''' <summary>
        ''' Returns the X coordinate of the start point in double precision. </summary>
        ''' <returns> the X coordinate of the start point of this
        '''         {@code Line2D} object.
        ''' @since 1.2 </returns>
        Public MustOverride ReadOnly Property x1 As Double

        ''' <summary>
        ''' Returns the Y coordinate of the start point in double precision. </summary>
        ''' <returns> the Y coordinate of the start point of this
        '''         {@code Line2D} object.
        ''' @since 1.2 </returns>
        Public MustOverride ReadOnly Property y1 As Double

        ''' <summary>
        ''' Returns the start <code>Point2D</code> of this <code>Line2D</code>. </summary>
        ''' <returns> the start <code>Point2D</code> of this <code>Line2D</code>.
        ''' @since 1.2 </returns>
        Public MustOverride ReadOnly Property p1 As Point2D

        ''' <summary>
        ''' Returns the X coordinate of the end point in double precision. </summary>
        ''' <returns> the X coordinate of the end point of this
        '''         {@code Line2D} object.
        ''' @since 1.2 </returns>
        Public MustOverride ReadOnly Property x2 As Double

        ''' <summary>
        ''' Returns the Y coordinate of the end point in double precision. </summary>
        ''' <returns> the Y coordinate of the end point of this
        '''         {@code Line2D} object.
        ''' @since 1.2 </returns>
        Public MustOverride ReadOnly Property y2 As Double

        ''' <summary>
        ''' Returns the end <code>Point2D</code> of this <code>Line2D</code>. </summary>
        ''' <returns> the end <code>Point2D</code> of this <code>Line2D</code>.
        ''' @since 1.2 </returns>
        Public MustOverride ReadOnly Property p2 As Point2D

        ''' <summary>
        ''' Sets the location of the end points of this <code>Line2D</code> to
        ''' the specified double coordinates. </summary>
        ''' <param name="x1"> the X coordinate of the start point </param>
        ''' <param name="y1"> the Y coordinate of the start point </param>
        ''' <param name="x2"> the X coordinate of the end point </param>
        ''' <param name="y2"> the Y coordinate of the end point
        ''' @since 1.2 </param>
        Public MustOverride Sub setLine(x1 As Double, y1 As Double, x2 As Double, y2 As Double)

        ''' <summary>
        ''' Sets the location of the end points of this <code>Line2D</code> to
        ''' the specified <code>Point2D</code> coordinates. </summary>
        ''' <param name="p1"> the start <code>Point2D</code> of the line segment </param>
        ''' <param name="p2"> the end <code>Point2D</code> of the line segment
        ''' @since 1.2 </param>
        Public Overridable Sub setLine(p1 As Point2D, p2 As Point2D)
            lineine(p1.X, p1.Y, p2.X, p2.Y)
        End Sub

        ''' <summary>
        ''' Sets the location of the end points of this <code>Line2D</code> to
        ''' the same as those end points of the specified <code>Line2D</code>. </summary>
        ''' <param name="l"> the specified <code>Line2D</code>
        ''' @since 1.2 </param>
        Public Overridable Property line As Line2D
            Set(l As Line2D)
                lineine(l.x1, l.y1, l.x2, l.y2)
            End Set
        End Property

        ''' <summary>
        ''' Returns an indicator of where the specified point
        ''' {@code (px,py)} lies with respect to the line segment from
        ''' {@code (x1,y1)} to {@code (x2,y2)}.
        ''' The return value can be either 1, -1, or 0 and indicates
        ''' in which direction the specified line must pivot around its
        ''' first end point, {@code (x1,y1)}, in order to point at the
        ''' specified point {@code (px,py)}.
        ''' <p>A return value of 1 indicates that the line segment must
        ''' turn in the direction that takes the positive X axis towards
        ''' the negative Y axis.  In the default coordinate system used by
        ''' Java 2D, this direction is counterclockwise.
        ''' <p>A return value of -1 indicates that the line segment must
        ''' turn in the direction that takes the positive X axis towards
        ''' the positive Y axis.  In the default coordinate system, this
        ''' direction is clockwise.
        ''' <p>A return value of 0 indicates that the point lies
        ''' exactly on the line segment.  Note that an indicator value
        ''' of 0 is rare and not useful for determining collinearity
        ''' because of floating point rounding issues.
        ''' <p>If the point is colinear with the line segment, but
        ''' not between the end points, then the value will be -1 if the point
        ''' lies "beyond {@code (x1,y1)}" or 1 if the point lies
        ''' "beyond {@code (x2,y2)}".
        ''' </summary>
        ''' <param name="x1"> the X coordinate of the start point of the
        '''           specified line segment </param>
        ''' <param name="y1"> the Y coordinate of the start point of the
        '''           specified line segment </param>
        ''' <param name="x2"> the X coordinate of the end point of the
        '''           specified line segment </param>
        ''' <param name="y2"> the Y coordinate of the end point of the
        '''           specified line segment </param>
        ''' <param name="px"> the X coordinate of the specified point to be
        '''           compared with the specified line segment </param>
        ''' <param name="py"> the Y coordinate of the specified point to be
        '''           compared with the specified line segment </param>
        ''' <returns> an integer that indicates the position of the third specified
        '''                  coordinates with respect to the line segment formed
        '''                  by the first two specified coordinates.
        ''' @since 1.2 </returns>
        Public Shared Function relativeCCW(x1 As Double, y1 As Double, x2 As Double, y2 As Double, px As Double, py As Double) As Integer
            x2 -= x1
            y2 -= y1
            px -= x1
            py -= y1
            Dim ccw As Double = px * y2 - py * x2
            If ccw = 0.0 Then
                ' The point is colinear, classify based on which side of
                ' the segment the point falls on.  We can calculate a
                ' relative value using the projection of px,py onto the
                ' segment - a negative value indicates the point projects
                ' outside of the segment in the direction of the particular
                ' endpoint used as the origin for the projection.
                ccw = px * x2 + py * y2
                If ccw > 0.0 Then
                    ' Reverse the projection to be relative to the original x2,y2
                    ' x2 and y2 are simply negated.
                    ' px and py need to have (x2 - x1) or (y2 - y1) subtracted
                    '    from them (based on the original values)
                    ' Since we really want to get a positive answer when the
                    '    point is "beyond (x2,y2)", then we want to calculate
                    '    the inverse anyway - thus we leave x2 & y2 negated.
                    px -= x2
                    py -= y2
                    ccw = px * x2 + py * y2
                    If ccw < 0.0 Then ccw = 0.0
                End If
            End If
            Return If(ccw < 0.0, -1, (If(ccw > 0.0, 1, 0)))
        End Function

        ''' <summary>
        ''' Returns an indicator of where the specified point
        ''' {@code (px,py)} lies with respect to this line segment.
        ''' See the method comments of
        ''' <seealso cref="#relativeCCW(double, double, double, double, double, double)"/>
        ''' to interpret the return value. </summary>
        ''' <param name="px"> the X coordinate of the specified point
        '''           to be compared with this <code>Line2D</code> </param>
        ''' <param name="py"> the Y coordinate of the specified point
        '''           to be compared with this <code>Line2D</code> </param>
        ''' <returns> an integer that indicates the position of the specified
        '''         coordinates with respect to this <code>Line2D</code> </returns>
        ''' <seealso cref= #relativeCCW(double, double, double, double, double, double)
        ''' @since 1.2 </seealso>
        Public Overridable Function relativeCCW(px As Double, py As Double) As Integer
            Return relativeCCW(x1, y1, x2, y2, px, py)
        End Function

        ''' <summary>
        ''' Returns an indicator of where the specified <code>Point2D</code>
        ''' lies with respect to this line segment.
        ''' See the method comments of
        ''' <seealso cref="#relativeCCW(double, double, double, double, double, double)"/>
        ''' to interpret the return value. </summary>
        ''' <param name="p"> the specified <code>Point2D</code> to be compared
        '''          with this <code>Line2D</code> </param>
        ''' <returns> an integer that indicates the position of the specified
        '''         <code>Point2D</code> with respect to this <code>Line2D</code> </returns>
        ''' <seealso cref= #relativeCCW(double, double, double, double, double, double)
        ''' @since 1.2 </seealso>
        Public Overridable Function relativeCCW(p As Point2D) As Integer
            Return relativeCCW(x1, y1, x2, y2, p.X, p.Y)
        End Function

        ''' <summary>
        ''' Tests if the line segment from {@code (x1,y1)} to
        ''' {@code (x2,y2)} intersects the line segment from {@code (x3,y3)}
        ''' to {@code (x4,y4)}.
        ''' </summary>
        ''' <param name="x1"> the X coordinate of the start point of the first
        '''           specified line segment </param>
        ''' <param name="y1"> the Y coordinate of the start point of the first
        '''           specified line segment </param>
        ''' <param name="x2"> the X coordinate of the end point of the first
        '''           specified line segment </param>
        ''' <param name="y2"> the Y coordinate of the end point of the first
        '''           specified line segment </param>
        ''' <param name="x3"> the X coordinate of the start point of the second
        '''           specified line segment </param>
        ''' <param name="y3"> the Y coordinate of the start point of the second
        '''           specified line segment </param>
        ''' <param name="x4"> the X coordinate of the end point of the second
        '''           specified line segment </param>
        ''' <param name="y4"> the Y coordinate of the end point of the second
        '''           specified line segment </param>
        ''' <returns> <code>true</code> if the first specified line segment
        '''                  and the second specified line segment intersect
        '''                  each other; <code>false</code> otherwise.
        ''' @since 1.2 </returns>
        Public Shared Function linesIntersect(x1 As Double, y1 As Double, x2 As Double, y2 As Double, x3 As Double, y3 As Double, x4 As Double, y4 As Double) As Boolean
            Return ((relativeCCW(x1, y1, x2, y2, x3, y3) * relativeCCW(x1, y1, x2, y2, x4, y4) <= 0) AndAlso (relativeCCW(x3, y3, x4, y4, x1, y1) * relativeCCW(x3, y3, x4, y4, x2, y2) <= 0))
        End Function

        ''' <summary>
        ''' Tests if the line segment from {@code (x1,y1)} to
        ''' {@code (x2,y2)} intersects this line segment.
        ''' </summary>
        ''' <param name="x1"> the X coordinate of the start point of the
        '''           specified line segment </param>
        ''' <param name="y1"> the Y coordinate of the start point of the
        '''           specified line segment </param>
        ''' <param name="x2"> the X coordinate of the end point of the
        '''           specified line segment </param>
        ''' <param name="y2"> the Y coordinate of the end point of the
        '''           specified line segment </param>
        ''' <returns> {@code <true>} if this line segment and the specified line segment
        '''                  intersect each other; <code>false</code> otherwise.
        ''' @since 1.2 </returns>
        Public Overridable Function intersectsLine(x1 As Double, y1 As Double, x2 As Double, y2 As Double) As Boolean
            Return linesIntersect(x1, y1, x2, y2, x1, y1, x2, y2)
        End Function

        ''' <summary>
        ''' Tests if the specified line segment intersects this line segment. </summary>
        ''' <param name="l"> the specified <code>Line2D</code> </param>
        ''' <returns> <code>true</code> if this line segment and the specified line
        '''                  segment intersect each other;
        '''                  <code>false</code> otherwise.
        ''' @since 1.2 </returns>
        Public Overridable Function intersectsLine(l As Line2D) As Boolean
            Return linesIntersect(l.x1, l.y1, l.x2, l.y2, x1, y1, x2, y2)
        End Function

        ''' <summary>
        ''' Returns the square of the distance from a point to a line segment.
        ''' The distance measured is the distance between the specified
        ''' point and the closest point between the specified end points.
        ''' If the specified point intersects the line segment in between the
        ''' end points, this method returns 0.0.
        ''' </summary>
        ''' <param name="x1"> the X coordinate of the start point of the
        '''           specified line segment </param>
        ''' <param name="y1"> the Y coordinate of the start point of the
        '''           specified line segment </param>
        ''' <param name="x2"> the X coordinate of the end point of the
        '''           specified line segment </param>
        ''' <param name="y2"> the Y coordinate of the end point of the
        '''           specified line segment </param>
        ''' <param name="px"> the X coordinate of the specified point being
        '''           measured against the specified line segment </param>
        ''' <param name="py"> the Y coordinate of the specified point being
        '''           measured against the specified line segment </param>
        ''' <returns> a double value that is the square of the distance from the
        '''                  specified point to the specified line segment. </returns>
        ''' <seealso cref= #ptLineDistSq(double, double, double, double, double, double)
        ''' @since 1.2 </seealso>
        Public Shared Function ptSegDistSq(x1 As Double, y1 As Double, x2 As Double, y2 As Double, px As Double, py As Double) As Double
            ' Adjust vectors relative to x1,y1
            ' x2,y2 becomes relative vector from x1,y1 to end of segment
            x2 -= x1
            y2 -= y1
            ' px,py becomes relative vector from x1,y1 to test point
            px -= x1
            py -= y1
            Dim dotprod As Double = px * x2 + py * y2
            Dim projlenSq As Double
            If dotprod <= 0.0 Then
                ' px,py is on the side of x1,y1 away from x2,y2
                ' distance to segment is length of px,py vector
                ' "length of its (clipped) projection" is now 0.0
                projlenSq = 0.0
            Else
                ' switch to backwards vectors relative to x2,y2
                ' x2,y2 are already the negative of x1,y1=>x2,y2
                ' to get px,py to be the negative of px,py=>x2,y2
                ' the dot product of two negated vectors is the same
                ' as the dot product of the two normal vectors
                px = x2 - px
                py = y2 - py
                dotprod = px * x2 + py * y2
                If dotprod <= 0.0 Then
                    ' px,py is on the side of x2,y2 away from x1,y1
                    ' distance to segment is length of (backwards) px,py vector
                    ' "length of its (clipped) projection" is now 0.0
                    projlenSq = 0.0
                Else
                    ' px,py is between x1,y1 and x2,y2
                    ' dotprod is the length of the px,py vector
                    ' projected on the x2,y2=>x1,y1 vector times the
                    ' length of the x2,y2=>x1,y1 vector
                    projlenSq = dotprod * dotprod / (x2 * x2 + y2 * y2)
                End If
            End If
            ' Distance to line is now the length of the relative point
            ' vector minus the length of its projection onto the line
            ' (which is zero if the projection falls outside the range
            '  of the line segment).
            Dim lenSq As Double = px * px + py * py - projlenSq
            If lenSq < 0 Then lenSq = 0
            Return lenSq
        End Function

        ''' <summary>
        ''' Returns the distance from a point to a line segment.
        ''' The distance measured is the distance between the specified
        ''' point and the closest point between the specified end points.
        ''' If the specified point intersects the line segment in between the
        ''' end points, this method returns 0.0.
        ''' </summary>
        ''' <param name="x1"> the X coordinate of the start point of the
        '''           specified line segment </param>
        ''' <param name="y1"> the Y coordinate of the start point of the
        '''           specified line segment </param>
        ''' <param name="x2"> the X coordinate of the end point of the
        '''           specified line segment </param>
        ''' <param name="y2"> the Y coordinate of the end point of the
        '''           specified line segment </param>
        ''' <param name="px"> the X coordinate of the specified point being
        '''           measured against the specified line segment </param>
        ''' <param name="py"> the Y coordinate of the specified point being
        '''           measured against the specified line segment </param>
        ''' <returns> a double value that is the distance from the specified point
        '''                          to the specified line segment. </returns>
        ''' <seealso cref= #ptLineDist(double, double, double, double, double, double)
        ''' @since 1.2 </seealso>
        Public Shared Function ptSegDist(x1 As Double, y1 As Double, x2 As Double, y2 As Double, px As Double, py As Double) As Double
            Return System.Math.Sqrt(ptSegDistSq(x1, y1, x2, y2, px, py))
        End Function

        ''' <summary>
        ''' Returns the square of the distance from a point to this line segment.
        ''' The distance measured is the distance between the specified
        ''' point and the closest point between the current line's end points.
        ''' If the specified point intersects the line segment in between the
        ''' end points, this method returns 0.0.
        ''' </summary>
        ''' <param name="px"> the X coordinate of the specified point being
        '''           measured against this line segment </param>
        ''' <param name="py"> the Y coordinate of the specified point being
        '''           measured against this line segment </param>
        ''' <returns> a double value that is the square of the distance from the
        '''                  specified point to the current line segment. </returns>
        ''' <seealso cref= #ptLineDistSq(double, double)
        ''' @since 1.2 </seealso>
        Public Overridable Function ptSegDistSq(px As Double, py As Double) As Double
            Return ptSegDistSq(x1, y1, x2, y2, px, py)
        End Function

        ''' <summary>
        ''' Returns the square of the distance from a <code>Point2D</code> to
        ''' this line segment.
        ''' The distance measured is the distance between the specified
        ''' point and the closest point between the current line's end points.
        ''' If the specified point intersects the line segment in between the
        ''' end points, this method returns 0.0. </summary>
        ''' <param name="pt"> the specified <code>Point2D</code> being measured against
        '''           this line segment. </param>
        ''' <returns> a double value that is the square of the distance from the
        '''                  specified <code>Point2D</code> to the current
        '''                  line segment. </returns>
        ''' <seealso cref= #ptLineDistSq(Point2D)
        ''' @since 1.2 </seealso>
        Public Overridable Function ptSegDistSq(pt As Point2D) As Double
            Return ptSegDistSq(x1, y1, x2, y2, pt.X, pt.Y)
        End Function

        ''' <summary>
        ''' Returns the distance from a point to this line segment.
        ''' The distance measured is the distance between the specified
        ''' point and the closest point between the current line's end points.
        ''' If the specified point intersects the line segment in between the
        ''' end points, this method returns 0.0.
        ''' </summary>
        ''' <param name="px"> the X coordinate of the specified point being
        '''           measured against this line segment </param>
        ''' <param name="py"> the Y coordinate of the specified point being
        '''           measured against this line segment </param>
        ''' <returns> a double value that is the distance from the specified
        '''                  point to the current line segment. </returns>
        ''' <seealso cref= #ptLineDist(double, double)
        ''' @since 1.2 </seealso>
        Public Overridable Function ptSegDist(px As Double, py As Double) As Double
            Return ptSegDist(x1, y1, x2, y2, px, py)
        End Function

        ''' <summary>
        ''' Returns the distance from a <code>Point2D</code> to this line
        ''' segment.
        ''' The distance measured is the distance between the specified
        ''' point and the closest point between the current line's end points.
        ''' If the specified point intersects the line segment in between the
        ''' end points, this method returns 0.0. </summary>
        ''' <param name="pt"> the specified <code>Point2D</code> being measured
        '''          against this line segment </param>
        ''' <returns> a double value that is the distance from the specified
        '''                          <code>Point2D</code> to the current line
        '''                          segment. </returns>
        ''' <seealso cref= #ptLineDist(Point2D)
        ''' @since 1.2 </seealso>
        Public Overridable Function ptSegDist(pt As Point2D) As Double
            Return ptSegDist(x1, y1, x2, y2, pt.X, pt.Y)
        End Function

        ''' <summary>
        ''' Returns the square of the distance from a point to a line.
        ''' The distance measured is the distance between the specified
        ''' point and the closest point on the infinitely-extended line
        ''' defined by the specified coordinates.  If the specified point
        ''' intersects the line, this method returns 0.0.
        ''' </summary>
        ''' <param name="x1"> the X coordinate of the start point of the specified line </param>
        ''' <param name="y1"> the Y coordinate of the start point of the specified line </param>
        ''' <param name="x2"> the X coordinate of the end point of the specified line </param>
        ''' <param name="y2"> the Y coordinate of the end point of the specified line </param>
        ''' <param name="px"> the X coordinate of the specified point being
        '''           measured against the specified line </param>
        ''' <param name="py"> the Y coordinate of the specified point being
        '''           measured against the specified line </param>
        ''' <returns> a double value that is the square of the distance from the
        '''                  specified point to the specified line. </returns>
        ''' <seealso cref= #ptSegDistSq(double, double, double, double, double, double)
        ''' @since 1.2 </seealso>
        Public Shared Function ptLineDistSq(x1 As Double, y1 As Double, x2 As Double, y2 As Double, px As Double, py As Double) As Double
            ' Adjust vectors relative to x1,y1
            ' x2,y2 becomes relative vector from x1,y1 to end of segment
            x2 -= x1
            y2 -= y1
            ' px,py becomes relative vector from x1,y1 to test point
            px -= x1
            py -= y1
            Dim dotprod As Double = px * x2 + py * y2
            ' dotprod is the length of the px,py vector
            ' projected on the x1,y1=>x2,y2 vector times the
            ' length of the x1,y1=>x2,y2 vector
            Dim projlenSq As Double = dotprod * dotprod / (x2 * x2 + y2 * y2)
            ' Distance to line is now the length of the relative point
            ' vector minus the length of its projection onto the line
            Dim lenSq As Double = px * px + py * py - projlenSq
            If lenSq < 0 Then lenSq = 0
            Return lenSq
        End Function

        ''' <summary>
        ''' Returns the distance from a point to a line.
        ''' The distance measured is the distance between the specified
        ''' point and the closest point on the infinitely-extended line
        ''' defined by the specified coordinates.  If the specified point
        ''' intersects the line, this method returns 0.0.
        ''' </summary>
        ''' <param name="x1"> the X coordinate of the start point of the specified line </param>
        ''' <param name="y1"> the Y coordinate of the start point of the specified line </param>
        ''' <param name="x2"> the X coordinate of the end point of the specified line </param>
        ''' <param name="y2"> the Y coordinate of the end point of the specified line </param>
        ''' <param name="px"> the X coordinate of the specified point being
        '''           measured against the specified line </param>
        ''' <param name="py"> the Y coordinate of the specified point being
        '''           measured against the specified line </param>
        ''' <returns> a double value that is the distance from the specified
        '''                   point to the specified line. </returns>
        ''' <seealso cref= #ptSegDist(double, double, double, double, double, double)
        ''' @since 1.2 </seealso>
        Public Shared Function ptLineDist(x1 As Double, y1 As Double, x2 As Double, y2 As Double, px As Double, py As Double) As Double
            Return System.Math.Sqrt(ptLineDistSq(x1, y1, x2, y2, px, py))
        End Function

        ''' <summary>
        ''' Returns the square of the distance from a point to this line.
        ''' The distance measured is the distance between the specified
        ''' point and the closest point on the infinitely-extended line
        ''' defined by this <code>Line2D</code>.  If the specified point
        ''' intersects the line, this method returns 0.0.
        ''' </summary>
        ''' <param name="px"> the X coordinate of the specified point being
        '''           measured against this line </param>
        ''' <param name="py"> the Y coordinate of the specified point being
        '''           measured against this line </param>
        ''' <returns> a double value that is the square of the distance from a
        '''                  specified point to the current line. </returns>
        ''' <seealso cref= #ptSegDistSq(double, double)
        ''' @since 1.2 </seealso>
        Public Overridable Function ptLineDistSq(px As Double, py As Double) As Double
            Return ptLineDistSq(x1, y1, x2, y2, px, py)
        End Function

        ''' <summary>
        ''' Returns the square of the distance from a specified
        ''' <code>Point2D</code> to this line.
        ''' The distance measured is the distance between the specified
        ''' point and the closest point on the infinitely-extended line
        ''' defined by this <code>Line2D</code>.  If the specified point
        ''' intersects the line, this method returns 0.0. </summary>
        ''' <param name="pt"> the specified <code>Point2D</code> being measured
        '''           against this line </param>
        ''' <returns> a double value that is the square of the distance from a
        '''                  specified <code>Point2D</code> to the current
        '''                  line. </returns>
        ''' <seealso cref= #ptSegDistSq(Point2D)
        ''' @since 1.2 </seealso>
        Public Overridable Function ptLineDistSq(pt As Point2D) As Double
            Return ptLineDistSq(x1, y1, x2, y2, pt.X, pt.Y)
        End Function

        ''' <summary>
        ''' Returns the distance from a point to this line.
        ''' The distance measured is the distance between the specified
        ''' point and the closest point on the infinitely-extended line
        ''' defined by this <code>Line2D</code>.  If the specified point
        ''' intersects the line, this method returns 0.0.
        ''' </summary>
        ''' <param name="px"> the X coordinate of the specified point being
        '''           measured against this line </param>
        ''' <param name="py"> the Y coordinate of the specified point being
        '''           measured against this line </param>
        ''' <returns> a double value that is the distance from a specified point
        '''                  to the current line. </returns>
        ''' <seealso cref= #ptSegDist(double, double)
        ''' @since 1.2 </seealso>
        Public Overridable Function ptLineDist(px As Double, py As Double) As Double
            Return ptLineDist(x1, y1, x2, y2, px, py)
        End Function

        ''' <summary>
        ''' Returns the distance from a <code>Point2D</code> to this line.
        ''' The distance measured is the distance between the specified
        ''' point and the closest point on the infinitely-extended line
        ''' defined by this <code>Line2D</code>.  If the specified point
        ''' intersects the line, this method returns 0.0. </summary>
        ''' <param name="pt"> the specified <code>Point2D</code> being measured </param>
        ''' <returns> a double value that is the distance from a specified
        '''                  <code>Point2D</code> to the current line. </returns>
        ''' <seealso cref= #ptSegDist(Point2D)
        ''' @since 1.2 </seealso>
        Public Overridable Function ptLineDist(pt As Point2D) As Double
            Return ptLineDist(x1, y1, x2, y2, pt.X, pt.Y)
        End Function

        ''' <summary>
        ''' Tests if a specified coordinate is inside the boundary of this
        ''' <code>Line2D</code>.  This method is required to implement the
        ''' <seealso cref="Shape"/> interface, but in the case of <code>Line2D</code>
        ''' objects it always returns <code>false</code> since a line contains
        ''' no area. </summary>
        ''' <param name="x"> the X coordinate of the specified point to be tested </param>
        ''' <param name="y"> the Y coordinate of the specified point to be tested </param>
        ''' <returns> <code>false</code> because a <code>Line2D</code> contains
        ''' no area.
        ''' @since 1.2 </returns>
        Public Overridable Function contains(x As Double, y As Double) As Boolean
            Return False
        End Function

        ''' <summary>
        ''' Tests if a given <code>Point2D</code> is inside the boundary of
        ''' this <code>Line2D</code>.
        ''' This method is required to implement the <seealso cref="Shape"/> interface,
        ''' but in the case of <code>Line2D</code> objects it always returns
        ''' <code>false</code> since a line contains no area. </summary>
        ''' <param name="p"> the specified <code>Point2D</code> to be tested </param>
        ''' <returns> <code>false</code> because a <code>Line2D</code> contains
        ''' no area.
        ''' @since 1.2 </returns>
        Public Overridable Function contains(p As Point2D) As Boolean
            Return False
        End Function

        ''' <summary>
        ''' {@inheritDoc}
        ''' @since 1.2
        ''' </summary>
        Public Overridable Function intersects(x As Double, y As Double, w As Double, h As Double) As Boolean
            Return intersects(New Rectangle2D.Double(x, y, w, h))
        End Function

        ''' <summary>
        ''' {@inheritDoc}
        ''' @since 1.2
        ''' </summary>
        Public Overridable Function intersects(r As Rectangle2D) As Boolean
            Return r.intersectsLine(x1, y1, x2, y2)
        End Function

        ''' <summary>
        ''' Tests if the interior of this <code>Line2D</code> entirely contains
        ''' the specified set of rectangular coordinates.
        ''' This method is required to implement the <code>Shape</code> interface,
        ''' but in the case of <code>Line2D</code> objects it always returns
        ''' false since a line contains no area. </summary>
        ''' <param name="x"> the X coordinate of the upper-left corner of the
        '''          specified rectangular area </param>
        ''' <param name="y"> the Y coordinate of the upper-left corner of the
        '''          specified rectangular area </param>
        ''' <param name="w"> the width of the specified rectangular area </param>
        ''' <param name="h"> the height of the specified rectangular area </param>
        ''' <returns> <code>false</code> because a <code>Line2D</code> contains
        ''' no area.
        ''' @since 1.2 </returns>
        Public Overridable Function contains(x As Double, y As Double, w As Double, h As Double) As Boolean
            Return False
        End Function

        ''' <summary>
        ''' Tests if the interior of this <code>Line2D</code> entirely contains
        ''' the specified <code>Rectangle2D</code>.
        ''' This method is required to implement the <code>Shape</code> interface,
        ''' but in the case of <code>Line2D</code> objects it always returns
        ''' <code>false</code> since a line contains no area. </summary>
        ''' <param name="r"> the specified <code>Rectangle2D</code> to be tested </param>
        ''' <returns> <code>false</code> because a <code>Line2D</code> contains
        ''' no area.
        ''' @since 1.2 </returns>
        Public Overridable Function contains(r As Rectangle2D) As Boolean
            Return False
        End Function

        ''' <summary>
        ''' {@inheritDoc}
        ''' @since 1.2
        ''' </summary>
        Public Overridable Property bounds As Java.awt.Rectangle
            Get
                Return bounds2D.bounds
            End Get
        End Property

        ''' <summary>
        ''' Returns an iteration object that defines the boundary of this
        ''' <code>Line2D</code>.
        ''' The iterator for this class is not multi-threaded safe,
        ''' which means that this <code>Line2D</code> class does not
        ''' guarantee that modifications to the geometry of this
        ''' <code>Line2D</code> object do not affect any iterations of that
        ''' geometry that are already in process. </summary>
        ''' <param name="at"> the specified <seealso cref="AffineTransform"/> </param>
        ''' <returns> a <seealso cref="PathIterator"/> that defines the boundary of this
        '''          <code>Line2D</code>.
        ''' @since 1.2 </returns>
        Public Overridable Function getPathIterator(at As AffineTransform) As PathIterator
            Return New LineIterator(Me, at)
        End Function

        ''' <summary>
        ''' Returns an iteration object that defines the boundary of this
        ''' flattened <code>Line2D</code>.
        ''' The iterator for this class is not multi-threaded safe,
        ''' which means that this <code>Line2D</code> class does not
        ''' guarantee that modifications to the geometry of this
        ''' <code>Line2D</code> object do not affect any iterations of that
        ''' geometry that are already in process. </summary>
        ''' <param name="at"> the specified <code>AffineTransform</code> </param>
        ''' <param name="flatness"> the maximum amount that the control points for a
        '''          given curve can vary from colinear before a subdivided
        '''          curve is replaced by a straight line connecting the
        '''          end points.  Since a <code>Line2D</code> object is
        '''          always flat, this parameter is ignored. </param>
        ''' <returns> a <code>PathIterator</code> that defines the boundary of the
        '''                  flattened <code>Line2D</code>
        ''' @since 1.2 </returns>
        Public Overridable Function getPathIterator(at As AffineTransform, flatness As Double) As PathIterator
            Return New LineIterator(Me, at)
        End Function
    End Class

End Namespace