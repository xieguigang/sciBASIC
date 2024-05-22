#Region "Microsoft.VisualBasic::e924ad3b5077799ccb8c858fd143ccf4, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Models\Paths\Rectangle.vb"

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

    '   Total Lines: 16
    '    Code Lines: 11 (68.75%)
    ' Comment Lines: 3 (18.75%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 2 (12.50%)
    '     File Size: 575 B


    '     Class Rectangle
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Drawing3D.Models.Isometric.Paths

    ''' <summary>
    ''' Created by fabianterhorst on 01.04.17.
    ''' </summary>
    Public Class Rectangle : Inherits Path3D

        Public Sub New(origin As Point3D, width As Integer, height As Integer)
            MyBase.New()
            Push(origin)
            Push(New Point3D(origin.X + width, origin.Y, origin.Z))
            Push(New Point3D(origin.X + width, origin.Y + height, origin.Z))
            Push(New Point3D(origin.X, origin.Y + height, origin.Z))
        End Sub
    End Class
End Namespace
