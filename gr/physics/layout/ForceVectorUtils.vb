#Region "Microsoft.VisualBasic::ade76905815fb7ba8cb1986b4e7d592b, gr\physics\layout\ForceVectorUtils.vb"

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

    '   Total Lines: 222
    '    Code Lines: 129
    ' Comment Lines: 45
    '   Blank Lines: 48
    '     File Size: 8.90 KB


    '     Class ForceVectorUtils
    ' 
    '         Function: attraction, distance, repulsion
    ' 
    '         Sub: fcBiAttractor, fcBiAttractor_noCollide, fcBiFlatAttractor, fcBiRepulsor, fcBiRepulsor_noCollide
    '              fcBiRepulsor_y, fcUniAttractor, fcUniRepulsor
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Math
Imports std = System.Math

'
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
    Public Class ForceVectorUtils

        Public Shared Function distance(n1 As Layout2D, n2 As Layout2D) As Single
            Return CSng(Hypot(n1.X() - n2.X(), n1.Y() - n2.Y()))
        End Function

        Public Shared Sub fcBiRepulsor(N1 As Node, N2 As Node, c As Double)
            Dim xDist As Double = N1.X() - N2.X() ' distance en x entre les deux noeuds
            Dim yDist As Double = N1.Y() - N2.Y()
            Dim dist As Double = CSng(std.Sqrt(xDist * xDist + yDist * yDist)) ' distance tout court

            If dist > 0 Then
                Dim f = repulsion(c, dist)

                Dim N1L As ForceVectorNodeLayoutData = N1.LayoutData
                Dim N2L As ForceVectorNodeLayoutData = N2.LayoutData

                N1L.dx += xDist / dist * f
                N1L.dy += yDist / dist * f

                N2L.dx -= xDist / dist * f
                N2L.dy -= yDist / dist * f
            End If
        End Sub

        Public Shared Sub fcBiRepulsor_y(N1 As Node, N2 As Node, c As Double, verticalization As Double)
            Dim xDist As Double = N1.X() - N2.X() ' distance en x entre les deux noeuds
            Dim yDist As Double = N1.Y() - N2.Y()
            Dim dist As Double = CSng(std.Sqrt(xDist * xDist + yDist * yDist)) ' distance tout court

            If dist > 0 Then
                Dim f = repulsion(c, dist)

                Dim N1L As ForceVectorNodeLayoutData = N1.LayoutData
                Dim N2L As ForceVectorNodeLayoutData = N2.LayoutData

                N1L.dx += xDist / dist * f
                N1L.dy += verticalization * yDist / dist * f

                N2L.dx -= xDist / dist * f
                N2L.dy -= verticalization * yDist / dist * f
            End If
        End Sub

        Public Shared Sub fcBiRepulsor_noCollide(N1 As Node, N2 As Node, c As Double)
            Dim xDist As Double = N1.X() - N2.X() ' distance en x entre les deux noeuds
            Dim yDist As Double = N1.Y() - N2.Y()
            Dim dist As Double = std.Sqrt(xDist * xDist + yDist * yDist) - N1.size() - N2.size() ' distance (from the border of each node)

            If dist > 0 Then
                Dim f = repulsion(c, dist)

                Dim N1L As ForceVectorNodeLayoutData = N1.LayoutData
                Dim N2L As ForceVectorNodeLayoutData = N2.LayoutData

                N1L.dx += xDist / dist * f
                N1L.dy += yDist / dist * f

                N2L.dx -= xDist / dist * f
                N2L.dy -= yDist / dist * f
            ElseIf dist <> 0 Then
                Dim f = -c 'flat repulsion

                Dim N1L As ForceVectorNodeLayoutData = N1.LayoutData
                Dim N2L As ForceVectorNodeLayoutData = N2.LayoutData

                N1L.dx += xDist / dist * f
                N1L.dy += yDist / dist * f

                N2L.dx -= xDist / dist * f
                N2L.dy -= yDist / dist * f
            End If
        End Sub

        Public Shared Sub fcUniRepulsor(N1 As Node, N2 As Node, c As Double)
            Dim xDist As Double = N1.X() - N2.X() ' distance en x entre les deux noeuds
            Dim yDist As Double = N1.Y() - N2.Y()
            Dim dist As Double = CSng(std.Sqrt(xDist * xDist + yDist * yDist)) ' distance tout court

            If dist > 0 Then
                Dim f = repulsion(c, dist)

                Dim N2L As ForceVectorNodeLayoutData = N2.LayoutData

                N2L.dx -= xDist / dist * f
                N2L.dy -= yDist / dist * f
            End If
        End Sub

        Public Shared Sub fcBiAttractor(N1 As Node, N2 As Node, c As Double)
            Dim xDist As Double = N1.X() - N2.X() ' distance en x entre les deux noeuds
            Dim yDist As Double = N1.Y() - N2.Y()
            Dim dist As Double = CSng(std.Sqrt(xDist * xDist + yDist * yDist)) ' distance tout court

            If dist > 0 Then
                Dim f = attraction(c, dist)

                Dim N1L As ForceVectorNodeLayoutData = N1.LayoutData
                Dim N2L As ForceVectorNodeLayoutData = N2.LayoutData

                N1L.dx += xDist / dist * f
                N1L.dy += yDist / dist * f

                N2L.dx -= xDist / dist * f
                N2L.dy -= yDist / dist * f
            End If
        End Sub

        Public Shared Sub fcBiAttractor_noCollide(N1 As Node, N2 As Node, c As Double)
            Dim xDist As Double = N1.X() - N2.X() ' distance en x entre les deux noeuds
            Dim yDist As Double = N1.Y() - N2.Y()
            Dim dist As Double = std.Sqrt(xDist * xDist + yDist * yDist) - N1.size() - N2.size() ' distance (from the border of each node)

            If dist > 0 Then
                Dim f = attraction(c, dist)

                Dim N1L As ForceVectorNodeLayoutData = N1.LayoutData
                Dim N2L As ForceVectorNodeLayoutData = N2.LayoutData

                N1L.dx += xDist / dist * f
                N1L.dy += yDist / dist * f

                N2L.dx -= xDist / dist * f
                N2L.dy -= yDist / dist * f
            End If
        End Sub

        Public Shared Sub fcBiFlatAttractor(N1 As Node, N2 As Node, c As Double)
            Dim xDist As Double = N1.X() - N2.X() ' distance en x entre les deux noeuds
            Dim yDist As Double = N1.Y() - N2.Y()
            Dim dist As Double = CSng(std.Sqrt(xDist * xDist + yDist * yDist)) ' distance tout court

            If dist > 0 Then
                Dim f = -c

                Dim N1L As ForceVectorNodeLayoutData = N1.LayoutData
                Dim N2L As ForceVectorNodeLayoutData = N2.LayoutData

                N1L.dx += xDist / dist * f
                N1L.dy += yDist / dist * f

                N2L.dx -= xDist / dist * f
                N2L.dy -= yDist / dist * f
            End If
        End Sub

        Public Shared Sub fcUniAttractor(N1 As Node, N2 As Node, c As Single)
            Dim xDist As Double = N1.X() - N2.X() ' distance en x entre les deux noeuds
            Dim yDist As Double = N1.Y() - N2.Y()
            Dim dist As Double = CSng(std.Sqrt(xDist * xDist + yDist * yDist)) ' distance tout court

            If dist > 0 Then
                Dim f = attraction(c, dist)

                Dim N2L As ForceVectorNodeLayoutData = N2.LayoutData

                N2L.dx -= xDist / dist * f
                N2L.dy -= yDist / dist * f
            End If
        End Sub

        Protected Friend Shared Function attraction(c As Double, dist As Double) As Double
            Return 0.01 * -c * dist
        End Function

        Protected Friend Shared Function repulsion(c As Double, dist As Double) As Double
            Return 0.001 * c / dist
        End Function
    End Class

End Namespace
