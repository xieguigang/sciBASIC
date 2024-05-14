#Region "Microsoft.VisualBasic::8c6883fafbe92abce712e721d193cfb9, gr\Microsoft.VisualBasic.Imaging\Drawing3D\Models\Shapes\Line.vb"

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

    '   Total Lines: 14
    '    Code Lines: 11
    ' Comment Lines: 0
    '   Blank Lines: 3
    '     File Size: 405 B


    '     Class Line
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: line3D
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Drawing3D.Models.Isometric.Shapes

    Public Class Line : Inherits Shape3D

        Public Sub New(a As Point3D, b As Point3D)
            Call MyBase.New
            Call Push(line3D(a, b))
        End Sub

        Private Shared Function line3D(a As Point3D, b As Point3D) As Path3D
            Return New Path3D().Push(a).Push(b)
        End Function
    End Class
End Namespace
