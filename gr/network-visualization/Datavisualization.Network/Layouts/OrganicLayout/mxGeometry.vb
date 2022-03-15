#Region "Microsoft.VisualBasic::c815be91669f34a09384e5ac0c9ffb18, sciBASIC#\gr\network-visualization\Datavisualization.Network\Layouts\OrganicLayout\mxGeometry.vb"

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

    '   Total Lines: 225
    '    Code Lines: 87
    ' Comment Lines: 99
    '   Blank Lines: 39
    '     File Size: 9.37 KB


    '     Class mxGeometry
    ' 
    '         Properties: AlternateBounds, Offset, Points, Relative, SourcePoint
    '                     TargetPoint
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '         Function: clone, getTerminalPoint, setTerminalPoint
    ' 
    '         Sub: swap, translate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.LayoutModel

Namespace Layouts


    ''' <summary>
    ''' Represents the geometry of a cell. For vertices, the geometry consists
    ''' of the x- and y-location, as well as the width and height. For edges,
    ''' the geometry either defines the source- and target-terminal, or it
    ''' defines the respective terminal points.
    ''' 
    ''' For edges, if the geometry is relative (default), then the x-coordinate
    ''' is used to describe the distance from the center of the edge from -1 to 1
    ''' with 0 being the center of the edge and the default value, and the
    ''' y-coordinate is used to describe the absolute, orthogonal distance in
    ''' pixels from that point. In addition, the offset is used as an absolute
    ''' offset vector from the resulting point. 
    ''' </summary>
    Public Class mxGeometry
        Inherits mxRectangle

        ''' 
        Private Const serialVersionUID As Long = 2649828026610336589L

        ''' <summary>
        ''' Global switch to translate the points in translate. Default is true.
        ''' </summary>
        Public Shared TRANSLATE_CONTROL_POINTS As Boolean = True

        ''' <summary>
        ''' Constructs a new geometry at (0, 0) with the width and height set to 0.
        ''' </summary>
        Public Sub New()
            Me.New(0, 0, 0, 0)
        End Sub

        ''' <summary>
        ''' Constructs a geometry using the given parameters.
        ''' </summary>
        ''' <param name="x"> X-coordinate of the new geometry. </param>
        ''' <param name="y"> Y-coordinate of the new geometry. </param>
        ''' <param name="width"> Width of the new geometry. </param>
        ''' <param name="height"> Height of the new geometry. </param>
        Public Sub New(ByVal x As Double, ByVal y As Double, ByVal width As Double, ByVal height As Double)
            MyBase.New(x, y, width, height)
        End Sub

        ''' <summary>
        ''' Stores alternate values for x, y, width and height in a rectangle.
        ''' Default is null.
        ''' </summary>
        Public Overridable Property AlternateBounds As mxRectangle

        ''' <summary>
        ''' Defines the source- and target-point of the edge. This is used if the
        ''' corresponding edge does not have a source vertex. Otherwise it is
        ''' ignored. Default is null.
        ''' </summary>
        Public Overridable Property SourcePoint As mxPoint

        ''' <summary>
        ''' Defines the source- and target-point of the edge. This is used if the
        ''' corresponding edge does not have a source vertex. Otherwise it is
        ''' ignored. Default is null.
        ''' </summary>
        Public Overridable Property TargetPoint As mxPoint

        ''' <summary>
        ''' List of mxPoints which specifies the control points along the edge.
        ''' These points are the intermediate points on the edge, for the endpoints
        ''' use targetPoint and sourcePoint or set the terminals of the edge to
        ''' a non-null value. Default is null.
        ''' </summary>
        Public Overridable Property Points As IList(Of mxPoint)

        ''' <summary>
        ''' Holds the offset of the label for edges. This is the absolute vector
        ''' between the center of the edge and the top, left point of the label.
        ''' Default is null.
        ''' </summary>
        Public Overridable Property Offset As mxPoint

        ''' <summary>
        ''' Specifies if the coordinates in the geometry are to be interpreted as
        ''' relative coordinates. Default is false. This is used to mark a geometry
        ''' with an x- and y-coordinate that is used to describe an edge label
        ''' position, or a relative location with respect to a parent cell's
        ''' width and height.
        ''' </summary>
        Public Overridable Property Relative As Boolean = False

        ''' <summary>
        ''' Swaps the x, y, width and height with the values stored in
        ''' alternateBounds and puts the previous values into alternateBounds as
        ''' a rectangle. This operation is carried-out in-place, that is, using the
        ''' existing geometry instance. If this operation is called during a graph
        ''' model transactional change, then the geometry should be cloned before
        ''' calling this method and setting the geometry of the cell using
        ''' mxGraphModel.setGeometry.
        ''' </summary>
        Public Overridable Sub swap()
            If alternateBounds IsNot Nothing Then
                Dim old As New mxRectangle(X, Y, Width, Height)

                X = alternateBounds.X
                Y = alternateBounds.Y
                Width = alternateBounds.Width
                Height = alternateBounds.Height

                alternateBounds = old
            End If
        End Sub

        ''' <summary>
        ''' Returns the point representing the source or target point of this edge.
        ''' This is only used if the edge has no source or target vertex.
        ''' </summary>
        ''' <param name="isSource"> Boolean that specifies if the source or target point
        ''' should be returned. </param>
        ''' <returns> Returns the source or target point. </returns>
        Public Overridable Function getTerminalPoint(ByVal isSource As Boolean) As mxPoint
            Return If(isSource, sourcePoint, targetPoint)
        End Function

        ''' <summary>
        ''' Sets the sourcePoint or targetPoint to the given point and returns the
        ''' new point.
        ''' </summary>
        ''' <param name="point"> Point to be used as the new source or target point. </param>
        ''' <param name="isSource"> Boolean that specifies if the source or target point
        ''' should be set. </param>
        ''' <returns> Returns the new point. </returns>
        Public Overridable Function setTerminalPoint(ByVal point As mxPoint, ByVal isSource As Boolean) As mxPoint
            If isSource Then
                sourcePoint = point
            Else
                targetPoint = point
            End If

            Return point
        End Function

        ''' <summary>
        ''' Translates the geometry by the specified amount. That is, x and y of the
        ''' geometry, the sourcePoint, targetPoint and all elements of points are
        ''' translated by the given amount. X and y are only translated if the
        ''' geometry is not relative. If TRANSLATE_CONTROL_POINTS is false, then
        ''' are not modified by this function.
        ''' </summary>
        ''' <param name="dx"> Integer that specifies the x-coordinate of the translation. </param>
        ''' <param name="dy"> Integer that specifies the y-coordinate of the translation. </param>
        Public Overridable Sub translate(ByVal dx As Double, ByVal dy As Double)
            ' Translates the geometry
            If Not relative Then
                X += dx
                Y += dy
            End If

            ' Translates the source point
            If sourcePoint IsNot Nothing Then
                sourcePoint.X = sourcePoint.X + dx
                sourcePoint.Y = sourcePoint.Y + dy
            End If

            ' Translates the target point
            If targetPoint IsNot Nothing Then
                targetPoint.X = targetPoint.X + dx
                targetPoint.Y = targetPoint.Y + dy
            End If

            ' Translate the control points
            If TRANSLATE_CONTROL_POINTS AndAlso points IsNot Nothing Then
                Dim count As Integer = points.Count

                For i As Integer = 0 To count - 1
                    Dim pt As mxPoint = points(i)

                    pt.X = pt.X + dx
                    pt.Y = pt.Y + dy
                Next
            End If
        End Sub

        ''' <summary>
        ''' Returns a clone of the cell.
        ''' </summary>
        Public Overrides Function clone() As Object
            Dim ___clone As mxGeometry = CType(MyBase.Clone(), mxGeometry)

            ___clone.X = X
            ___clone.Y = Y
            ___clone.Width = Width
            ___clone.Height = Height
            ___clone.relative = relative

            Dim pts As IList(Of mxPoint) = points

            If pts IsNot Nothing Then
                ___clone.points = New List(Of mxPoint)(pts.Count)

                For i As Integer = 0 To pts.Count - 1
                    ___clone.points.Add(CType(pts(i).Clone(), mxPoint))
                Next
            End If

            Dim tp As mxPoint = targetPoint

            If tp IsNot Nothing Then ___clone.targetPoint = CType(tp.Clone(), mxPoint)

            Dim sp As mxPoint = sourcePoint

            If sp IsNot Nothing Then sourcePoint = CType(sp.Clone(), mxPoint)

            Dim [off] As mxPoint = offset

            If [off] IsNot Nothing Then ___clone.offset = CType([off].Clone(), mxPoint)

            Dim alt As mxRectangle = alternateBounds

            If alt IsNot Nothing Then alternateBounds = CType(alt.Clone(), mxRectangle)

            Return ___clone
        End Function
    End Class
End Namespace
