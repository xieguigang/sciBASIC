#Region "Microsoft.VisualBasic::b612bea5ab77372d66dde5ec825c93cf, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Models\Shapes\Cylinder.vb"

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

    '   Total Lines: 29
    '    Code Lines: 19
    ' Comment Lines: 3
    '   Blank Lines: 7
    '     File Size: 934 B


    '     Class Cylinder
    ' 
    '         Constructor: (+2 Overloads) Sub New
    ' 
    '     Class Pie
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Drawing3D.Models.Isometric.Shapes

    ''' <summary>
    ''' Created by fabianterhorst on 01.04.17.
    ''' </summary>
    Public Class Cylinder : Inherits Shape3D

        Public Sub New(origin As Point3D, vertices As Double, height As Double)
            Me.New(origin, 1, vertices, height)
        End Sub

        Public Sub New(origin As Point3D, radius#, vertices#, height#)
            Call MyBase.New()

            Dim circle As New Paths.Circle(origin, radius, vertices)
            Call Extrude(Me, circle, height)
        End Sub
    End Class

    Public Class Pie : Inherits Shape3D

        Public Sub New(origin As Point3D, radius#, startAngle#, sweepAngle#, vertices#, height#)
            Call MyBase.New

            Dim arc As New Paths.Arc(origin, radius, startAngle, sweepAngle, vertices)
            Call Extrude(Me, arc, height)
        End Sub
    End Class
End Namespace
