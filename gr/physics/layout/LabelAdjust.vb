Imports randf = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

' 
' Copyright 2008-2010 Gephi
' Authors : Mathieu Jacomy
' Website : http://www.gephi.org
' 
' This file is part of Gephi.
' 
' DO NOT ALTER OR REMOVE COPYRIGHT NOTICES OR THIS HEADER.
' 
' Copyright 2011 Gephi Consortium. All rights reserved.
' 
' The contents of this file are subject to the terms of either the GNU
' General Public License Version 3 only ("GPL") or the Common
' Development and Distribution License("CDDL") (collectively, the
' "License"). You may not use this file except in compliance with the
' License. You can obtain a copy of the License at
' http://gephi.org/about/legal/license-notice/
' or /cddl-1.0.txt and /gpl-3.0.txt. See the License for the
' specific language governing permissions and limitations under the
' License.  When distributing the software, include this License Header
' Notice in each file and include the License files at
' /cddl-1.0.txt and /gpl-3.0.txt. If applicable, add the following below the
' License Header, with the fields enclosed by brackets [] replaced by
' your own identifying information:
' "Portions Copyrighted [year] [name of copyright owner]"
' 
' If you wish your version of this file to be governed by only the CDDL
' or only the GPL Version 3, indicate your decision by adding
' "[Contributor] elects to include this software in this distribution
' under the [CDDL or GPL Version 3] license." If you do not indicate a
' single choice of license, a recipient has the option to distribute
' your version of this file under either the CDDL, the GPL Version 3 or
' to extend the choice of license to its licensees as provided above.
' However, if you add GPL Version 3 code and therefore, elected the GPL
' Version 3 license, then the option applies only if the new code is
' made subject to such option by the copyright holder.
' 
' Contributor(s):
' 
' Portions Copyrighted 2011 Gephi Consortium.
' 

Namespace layout

    ''' <summary>
    ''' the layout data model
    ''' </summary>
    Public Class Node : Implements Layout2D

        Public Property X As Double Implements Layout2D.X
        Public Property Y As Double Implements Layout2D.Y
        Public Property LayoutData As ForceVectorNodeLayoutData
        Public Property size As Double
        Public Property fixed As Boolean

    End Class

    ''' <summary>
    ''' @author Mathieu Jacomy
    ''' </summary>
    Public Class LabelAdjust

        'Settings
        Private speedField As Double = 1
        Private adjustBySizeField As Boolean = True
        Private radiusScale As Single = 1.1F
        'Graph size
        Private xmin As Single
        Private xmax As Single
        Private ymin As Single
        Private ymax As Single

        Dim Converged As Boolean

        Public Sub resetPropertiesValues()
            speedField = 1
            radiusScale = 1.1F
            adjustBySizeField = True
        End Sub

        Public Sub initAlgo()
            Converged = False
        End Sub

        Public Sub goAlgo(nodes As Node())
            'Reset Layout Data
            For Each n As Node In nodes
                If n.LayoutData Is Nothing OrElse Not (TypeOf n.LayoutData Is LabelAdjustLayoutData) Then
                    n.LayoutData = New LabelAdjustLayoutData()
                End If
                Dim layoutData As LabelAdjustLayoutData = n.LayoutData
                layoutData.freeze = 0
                layoutData.dx = 0
                layoutData.dy = 0
            Next

            ' Get xmin, xmax, ymin, ymax
            xmin = Single.MaxValue
            xmax = Single.Epsilon
            ymin = Single.MaxValue
            ymax = Single.Epsilon

            Dim correctNodes As New List(Of Node)()
            For Each n As Node In nodes
                Dim x As Single = n.X()
                Dim y As Single = n.Y()
                Dim t As gephi.graph.api.TextProperties = n.TextProperties
                Dim w As Single = t.Width
                Dim h As Single = t.Height
                Dim radius As Single = n.size() / 2.0F

                If w > 0 AndAlso h > 0 Then
                    ' Get the rectangle occupied by the node (size + label)
                    Dim nxmin = std.Min(x - w / 2, x - radius)
                    Dim nxmax = std.Max(x + w / 2, x + radius)
                    Dim nymin = std.Min(y - h / 2, y - radius)
                    Dim nymax = std.Max(y + h / 2, y + radius)

                    ' Update global boundaries
                    xmin = std.Min(xmin, nxmin)
                    xmax = std.Max(xmax, nxmax)
                    ymin = std.Min(ymin, nymin)
                    ymax = std.Max(ymax, nymax)

                    correctNodes.Add(n)
                End If
            Next

            If correctNodes.Count = 0 OrElse xmin = xmax OrElse ymin = ymax Then
                Return
            End If

            Dim timeStamp As Long = 1
            Dim someCollision = False

            'Add all nodes in the quadtree
            Dim quadTree As QuadTree = New QuadTree(Me, correctNodes.Count, (xmax - xmin) / (ymax - ymin))
            For Each n As Node In correctNodes
                quadTree.add(n)
            Next

            'Compute repulsion - with neighbours in the 8 quadnodes around the node
            For Each n As Node In correctNodes
                timeStamp += 1
                Dim layoutData As LabelAdjustLayoutData = n.LayoutData
                Dim quad = quadTree.getQuadNode(layoutData.labelAdjustQuadNode)

                'Repulse with adjacent quad - but only one per pair of nodes, timestamp is guaranteeing that
                For Each neighbour As Node In quadTree.getAdjacentNodes(quad.row, quad.col)
                    Dim neighborLayoutData As LabelAdjustLayoutData = neighbour.LayoutData
                    If neighbour IsNot n AndAlso neighborLayoutData.freeze < timeStamp Then
                        Dim collision = Me.repulse(n, neighbour)
                        someCollision = someCollision OrElse collision
                    End If
                    neighborLayoutData.freeze = timeStamp 'Use the existing freeze float variable to set timestamp
                Next
            Next

            If Not someCollision Then
                Converged = True
            Else
                ' apply forces
                For Each n As Node In correctNodes
                    Dim layoutData As LabelAdjustLayoutData = n.LayoutData
                    If Not n.Fixed Then
                        layoutData.dx *= speedField
                        layoutData.dy *= speedField
                        Dim x As Single = n.X() + layoutData.dx
                        Dim y As Single = n.Y() + layoutData.dy

                        n.X = x
                        n.Y = y
                    End If
                Next
            End If
        End Sub

        Private Function repulse(n1 As Node, n2 As Node) As Boolean
            Dim collision = False
            Dim n1x As Single = n1.x()
            Dim n1y As Single = n1.y()
            Dim n2x As Single = n2.x()
            Dim n2y As Single = n2.y()
            Dim t1 As gephi.graph.api.TextProperties = n1.TextProperties
            Dim t2 As gephi.graph.api.TextProperties = n2.TextProperties
            Dim n1w As Single = t1.Width
            Dim n2w As Single = t2.Width
            Dim n1h As Single = t1.Height
            Dim n2h As Single = t2.Height
            Dim n2Data As LabelAdjustLayoutData = n2.LayoutData

            Dim n1xmin = n1x - 0.5 * n1w
            Dim n2xmin = n2x - 0.5 * n2w
            Dim n1ymin = n1y - 0.5 * n1h
            Dim n2ymin = n2y - 0.5 * n2h
            Dim n1xmax = n1x + 0.5 * n1w
            Dim n2xmax = n2x + 0.5 * n2w
            Dim n1ymax = n1y + 0.5 * n1h
            Dim n2ymax = n2y + 0.5 * n2h

            'Sphere repulsion
            If adjustBySizeField Then
                Dim xDist As Double = n2x - n1x
                Dim yDist As Double = n2y - n1y
                Dim dist = std.Sqrt(xDist * xDist + yDist * yDist)
                Dim sphereCollision As Boolean = dist < radiusScale * (n1.size() + n2.size())
                If sphereCollision Then
                    Dim f As Double = 0.1 * n1.size() / dist
                    If dist > 0 Then
                        n2Data.dx = CSng(n2Data.dx + xDist / dist * f)
                        n2Data.dy = CSng(n2Data.dy + yDist / dist * f)
                    Else
                        n2Data.dx = CSng(n2Data.dx + 0.01 * (0.5 - randf.NextDouble))
                        n2Data.dy = CSng(n2Data.dy + 0.01 * (0.5 - randf.NextDouble))
                    End If
                    collision = True
                End If
            End If

            Dim upDifferential = n1ymax - n2ymin
            Dim downDifferential = n2ymax - n1ymin
            Dim labelCollisionXleft = n2xmax - n1xmin
            Dim labelCollisionXright = n1xmax - n2xmin

            If upDifferential > 0 AndAlso downDifferential > 0 Then ' Potential collision
                If labelCollisionXleft > 0 AndAlso labelCollisionXright > 0 Then ' Collision
                    If upDifferential > downDifferential Then
                        ' N1 pushes N2 up
                        n2Data.dy = CSng(n2Data.dy - 0.02 * n1h * (0.8 + 0.4 * randf.NextDouble))
                        collision = True
                    Else
                        ' N1 pushes N2 down
                        n2Data.dy = CSng(n2Data.dy + 0.02 * n1h * (0.8 + 0.4 * randf.NextDouble))
                        collision = True
                    End If
                    If labelCollisionXleft > labelCollisionXright Then
                        ' N1 pushes N2 right
                        n2Data.dx = CSng(n2Data.dx + 0.01 * (n1h * 2) * (0.8 + 0.4 * randf.NextDouble))
                        collision = True
                    Else
                        ' N1 pushes N2 left
                        n2Data.dx = CSng(n2Data.dx - 0.01 * (n1h * 2) * (0.8 + 0.4 * randf.NextDouble))
                        collision = True
                    End If
                End If
            End If

            Return collision
        End Function

        Public Overridable Property Speed As Double?
            Get
                Return speedField
            End Get
            Set(value As Double?)
                speedField = value.Value
            End Set
        End Property


        Public Overridable Property AdjustBySize As Boolean?
            Get
                Return adjustBySizeField
            End Get
            Set(value As Boolean?)
                adjustBySizeField = value.Value
            End Set
        End Property


        Private Class QuadNode

            Friend ReadOnly index As Integer
            Friend ReadOnly row As Integer
            Friend ReadOnly col As Integer
            Friend ReadOnly nodesField As IList(Of Node)

            Public Sub New(index As Integer, row As Integer, col As Integer)
                Me.index = index
                Me.row = row
                Me.col = col
                nodesField = New List(Of Node)()
            End Sub

            Public Overridable ReadOnly Property Nodes As IList(Of Node)
                Get
                    Return nodesField
                End Get
            End Property

            Public Overridable Sub add(n As Node)
                nodesField.Add(n)
            End Sub
        End Class

        Private Class QuadTree
            Private ReadOnly outerInstance As LabelAdjust


            Friend ReadOnly quads As QuadNode()
            Friend ReadOnly COLUMNS As Integer
            Friend ReadOnly ROWS As Integer

            Public Sub New(outerInstance As LabelAdjust, numberNodes As Integer, aspectRatio As Single)
                Me.outerInstance = outerInstance
                If aspectRatio > 0 Then
                    COLUMNS = CInt(std.Ceiling(numberNodes / 50.0F))
                    ROWS = CInt(std.Ceiling(COLUMNS / aspectRatio))
                Else
                    ROWS = CInt(std.Ceiling(numberNodes / 50.0F))
                    COLUMNS = CInt(std.Ceiling(ROWS / aspectRatio))
                End If
                quads = New QuadNode(COLUMNS * ROWS - 1) {}
                For row = 0 To ROWS - 1
                    For col = 0 To COLUMNS - 1
                        quads(row * COLUMNS + col) = New QuadNode(row * COLUMNS + col, row, col)
                    Next
                Next
            End Sub

            Public Overridable Sub add(node As Node)
                Dim x As Single = node.x()
                Dim y As Single = node.y()
                Dim t As gephi.graph.api.TextProperties = node.TextProperties
                Dim w As Single = t.Width
                Dim h As Single = t.Height
                Dim radius As Single = node.size()

                ' Get the rectangle occupied by the node (size + label)
                Dim nxmin = std.Min(x - w / 2, x - radius)
                Dim nxmax = std.Max(x + w / 2, x + radius)
                Dim nymin = std.Min(y - h / 2, y - radius)
                Dim nymax = std.Max(y + h / 2, y + radius)

                ' Get the rectangle as boxes
                Dim minXbox As Integer = std.Floor((COLUMNS - 1) * (nxmin - outerInstance.xmin) / (outerInstance.xmax - outerInstance.xmin))
                Dim maxXbox As Integer = std.Floor((COLUMNS - 1) * (nxmax - outerInstance.xmin) / (outerInstance.xmax - outerInstance.xmin))
                Dim minYbox As Integer = std.Floor((ROWS - 1) * ((outerInstance.ymax - outerInstance.ymin - (nymax - outerInstance.ymin)) / (outerInstance.ymax - outerInstance.ymin)))
                Dim maxYbox As Integer = std.Floor((ROWS - 1) * ((outerInstance.ymax - outerInstance.ymin - (nymin - outerInstance.ymin)) / (outerInstance.ymax - outerInstance.ymin)))
                Dim col = minXbox

                While col <= maxXbox AndAlso col < COLUMNS AndAlso col >= 0
                    Dim row = minYbox

                    While row <= maxYbox AndAlso row < ROWS AndAlso row >= 0
                        quads(CInt(row * COLUMNS + col)).add(node)
                        row += 1
                    End While

                    col += 1
                End While

                'Get the node center
                Dim centerX As Integer = std.Floor((COLUMNS - 1) * (x - outerInstance.xmin) / (outerInstance.xmax - outerInstance.xmin))
                Dim centerY As Integer = std.Floor((ROWS - 1) * ((outerInstance.ymax - outerInstance.ymin - (y - outerInstance.ymin)) / (outerInstance.ymax - outerInstance.ymin)))
                Dim layoutData As LabelAdjustLayoutData = node.LayoutData
                layoutData.labelAdjustQuadNode = quads(centerY * COLUMNS + centerX).index
            End Sub

            Public Overridable Function [get](row As Integer, col As Integer) As IList(Of Node)
                Return quads(row * ROWS + col).Nodes
            End Function

            Public Overridable Function getAdjacentNodes(row As Integer, col As Integer) As IList(Of Node)
                If quads.Length = 1 Then
                    Return quads(0).Nodes
                End If

                Dim adjNodes As New List(Of Node)()
                Dim left = std.Max(0, col - 1)
                Dim top = std.Max(0, row - 1)
                Dim right = std.Min(COLUMNS - 1, col + 1)
                Dim bottom = std.Min(ROWS - 1, row + 1)
                For i = left To right
                    For j = top To bottom
                        CType(adjNodes, List(Of Node)).AddRange(quads(j * COLUMNS + i).Nodes)
                    Next
                Next
                Return adjNodes
            End Function

            Public Overridable Function getQuadNode(index As Integer) As QuadNode
                Return quads(index)
            End Function
        End Class
    End Class

End Namespace
