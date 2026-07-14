#Region "Microsoft.VisualBasic::22d529a3b06becec315d4644ed6f410a, gr\physics\layout\LabelAdjust.vb"

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
    ' along with this program.  If not, see <http://www.gnu.org/licenses/>.
    
    
    
    ' /********************************************************************************/

    ' Code Statistics:

    '   Total Lines: 424
    '    Code Lines: 284 (66.98%)
    ' Comment Lines: 79 (18.63%)
    '    - Xml Docs: 25.32%
    ' 
    '   Blank Lines: 61 (14.39%)
    '     File Size: 17.16 KB


    '     Class LabelAdjust
    ' 
    '         Properties: AdjustBySize, Speed
    ' 
    '         Function: Repulse
    ' 
    '         Sub: Iterate, ResetPropertiesValues, Solve
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
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
    ''' @author Mathieu Jacomy
    ''' </summary>
    Public Class LabelAdjust

        'Settings
        Private RadiusScale As Single = 1.1F
        'Graph size
        Private XMin As Single
        Private XMax As Single
        Private YMin As Single
        Private YMax As Single

        Dim Converged As Boolean

        Public MaxIterations As Integer = 1000
        Public Canvas As SizeF

        Public Sub ResetPropertiesValues()
            Speed = 1
            RadiusScale = 1.1F
            AdjustBySize = True
        End Sub

        Public Sub Solve(nodes As Node(), labels As Dictionary(Of Node, TextProperties))
            Dim layoutData As LabelAdjustLayoutData

            'Reset Layout Data
            For Each n As Node In nodes
                If n.LayoutData Is Nothing OrElse Not (TypeOf n.LayoutData Is LabelAdjustLayoutData) Then
                    n.LayoutData = New LabelAdjustLayoutData()
                End If
                layoutData = n.LayoutData
                layoutData.Freeze = 0
                layoutData.Dx = 0
                layoutData.Dy = 0
            Next

            Converged = False

            Dim i As Integer = 0

            Do While Not Converged
                Call Iterate(nodes, labels)

                If i > MaxIterations Then
                    Exit Do
                Else
                    i += 1
                End If
            Loop
        End Sub

        ''' <summary>
        ''' one iteration
        ''' </summary>
        ''' <param name="nodes"></param>
        ''' <param name="labels"></param>
        Private Sub Iterate(nodes As Node(), labels As Dictionary(Of Node, TextProperties))
            ' Get xmin, xmax, ymin, ymax
            XMin = Single.MaxValue
            XMax = Single.Epsilon
            YMin = Single.MaxValue
            YMax = Single.Epsilon

            Dim correctNodes As New List(Of Node)()
            For Each n As Node In nodes
                Dim x As Single = n.X()
                Dim y As Single = n.Y()
                Dim t As TextProperties = labels(n)
                Dim w As Single = t.Width
                Dim h As Single = t.Height
                Dim radius As Single = n.Size / 2.0F

                If w > 0 AndAlso h > 0 Then
                    ' Get the rectangle occupied by the node (size + label)
                    Dim nxmin = std.Min(x - w / 2, x - radius)
                    Dim nxmax = std.Max(x + w / 2, x + radius)
                    Dim nymin = std.Min(y - h / 2, y - radius)
                    Dim nymax = std.Max(y + h / 2, y + radius)

                    ' Update global boundaries
                    XMin = std.Min(XMin, nxmin)
                    XMax = std.Max(XMax, nxmax)
                    YMin = std.Min(YMin, nymin)
                    YMax = std.Max(YMax, nymax)

                    correctNodes.Add(n)
                End If
            Next

            If correctNodes.Count = 0 OrElse XMin = XMax OrElse YMin = YMax Then
                Converged = True
                Return
            End If

            Dim timeStamp As Long = 1
            Dim someCollision = False

            'Add all nodes in the quadtree
            Dim quadTree As New QuadTree(correctNodes.Count, (XMax - XMin) / (YMax - YMin)) With {
                .XMax = Me.XMax,
                .XMin = Me.XMin,
                .YMax = Me.YMax,
                .YMin = Me.YMin
            }

            For Each n As Node In correctNodes
                quadTree.Add(n, labels)
            Next

            'Compute repulsion - with neighbours in the 8 quadnodes around the node
            For Each n As Node In correctNodes
                timeStamp += 1
                Dim layoutData As LabelAdjustLayoutData = n.LayoutData
                Dim quad = quadTree.GetQuadNode(layoutData.LabelAdjustQuadNode)

                'Repulse with adjacent quad - but only one per pair of nodes, timestamp is guaranteeing that
                For Each neighbour As Node In quadTree.GetAdjacentNodes(quad.row, quad.col)
                    Dim neighborLayoutData As LabelAdjustLayoutData = neighbour.LayoutData
                    If neighbour IsNot n AndAlso neighborLayoutData.Freeze < timeStamp Then
                        Dim collision = Me.Repulse(n, neighbour, labels)
                        someCollision = someCollision OrElse collision
                    End If
                    neighborLayoutData.Freeze = timeStamp 'Use the existing freeze float variable to set timestamp
                Next
            Next

            If Not someCollision Then
                Converged = True
            Else
                ' apply forces
                For Each n As Node In correctNodes
                    Dim layoutData As LabelAdjustLayoutData = n.LayoutData
                    If Not n.Fixed Then
                        layoutData.Dx *= Speed
                        layoutData.Dy *= Speed
                        Dim x As Single = n.X() + layoutData.Dx
                        Dim y As Single = n.Y() + layoutData.Dy

                        If x < 0 Then x = 0
                        If y < 0 Then y = 0
                        If x + labels(n).Width > Canvas.Width Then
                            x = Canvas.Width - labels(n).Width
                        End If
                        If y + labels(n).Height > Canvas.Height Then
                            y = Canvas.Height - labels(n).Height
                        End If

                        n.X = x
                        n.Y = y
                    End If
                Next
            End If
        End Sub

        Private Function Repulse(n1 As Node, n2 As Node, labels As Dictionary(Of Node, TextProperties)) As Boolean
            Dim collision = False
            Dim n1x As Single = n1.X()
            Dim n1y As Single = n1.Y()
            Dim n2x As Single = n2.X()
            Dim n2y As Single = n2.Y()
            Dim t1 As TextProperties = labels(n1)
            Dim t2 As TextProperties = labels(n2)
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
            If AdjustBySize Then
                Dim xDist As Double = n2x - n1x
                Dim yDist As Double = n2y - n1y
                Dim dist = std.Sqrt(xDist * xDist + yDist * yDist)
                Dim sphereCollision As Boolean = dist < RadiusScale * (n1.Size + n2.Size)
                If sphereCollision Then
                    Dim f As Double = 0.1 * n1.Size / dist
                    If dist > 0 Then
                        n2Data.Dx = CSng(n2Data.Dx + xDist / dist * f)
                        n2Data.Dy = CSng(n2Data.Dy + yDist / dist * f)
                    Else
                        n2Data.Dx = CSng(n2Data.Dx + 0.01 * (0.5 - randf.NextDouble))
                        n2Data.Dy = CSng(n2Data.Dy + 0.01 * (0.5 - randf.NextDouble))
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
                        n2Data.Dy = CSng(n2Data.Dy - 0.02 * n1h * (0.8 + 0.4 * randf.NextDouble))
                        collision = True
                    Else
                        ' N1 pushes N2 down
                        n2Data.Dy = CSng(n2Data.Dy + 0.02 * n1h * (0.8 + 0.4 * randf.NextDouble))
                        collision = True
                    End If
                    If labelCollisionXleft > labelCollisionXright Then
                        ' N1 pushes N2 right
                        n2Data.Dx = CSng(n2Data.Dx + 0.01 * (n1h * 2) * (0.8 + 0.4 * randf.NextDouble))
                        collision = True
                    Else
                        ' N1 pushes N2 left
                        n2Data.Dx = CSng(n2Data.Dx - 0.01 * (n1h * 2) * (0.8 + 0.4 * randf.NextDouble))
                        collision = True
                    End If
                End If
            End If

            Return collision
        End Function

        Public Overridable Property Speed As Double = 1

        Public Overridable Property AdjustBySize As Boolean = True

    End Class

End Namespace
