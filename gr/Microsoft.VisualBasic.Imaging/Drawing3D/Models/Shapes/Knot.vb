#Region "Microsoft.VisualBasic::a61e815183a1237df2a8788cad9efc2f, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Models\Shapes\Knot.vb"

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

    '   Total Lines: 21
    '    Code Lines: 14 (66.67%)
    ' Comment Lines: 5 (23.81%)
    '    - Xml Docs: 80.00%
    ' 
    '   Blank Lines: 2 (9.52%)
    '     File Size: 1010 B


    '     Class Knot
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Drawing3D.Models.Isometric.Shapes

    Public Class Knot : Inherits Shape3D

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="origin">The shape model location</param>
        ''' <param name="scale#">The scale size</param>
        Public Sub New(origin As Point3D, Optional scale# = 1 / 5)
            Push((New Prism(Math3D.ORIGIN, 5, 1, 1)).paths)
            Push((New Prism(New Point3D(4, 1, 0), 1, 4, 1)).paths)
            Push((New Prism(New Point3D(4, 4, -2), 1, 1, 3)).paths)
            Push(New Path3D(New Point3D() {New Point3D(0, 0, 2), New Point3D(0, 0, 1), New Point3D(1, 0, 1), New Point3D(1, 0, 2)}))
            Push(New Path3D(New Point3D() {New Point3D(0, 0, 2), New Point3D(0, 1, 2), New Point3D(0, 1, 1), New Point3D(0, 0, 1)}))
            ScalePath3Ds(Math3D.ORIGIN, scale)
            TranslatePath3Ds(-0.1, 0.15, 0.4)
            TranslatePath3Ds(origin.X, origin.Y, origin.Z)
        End Sub
    End Class
End Namespace
