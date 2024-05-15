#Region "Microsoft.VisualBasic::32e7a38f99f1426d1796aaca71f311dd, gr\Landscape\PLY\PointCloud.vb"

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

    '   Total Lines: 18
    '    Code Lines: 13
    ' Comment Lines: 0
    '   Blank Lines: 5
    '     File Size: 589 B


    '     Class PointCloud
    ' 
    '         Properties: color, intensity, x, y, z
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Imaging.Drawing3D

Namespace Ply

    Public Class PointCloud : Implements PointF3D, IPointCloud

        Public Property x As Double Implements PointF3D.X
        Public Property y As Double Implements PointF3D.Y
        Public Property z As Double Implements PointF3D.Z
        Public Property color As String
        Public Property intensity As Double Implements IPointCloud.intensity

        Public Overrides Function ToString() As String
            Return $"[{x},{y},{z}] {intensity}"
        End Function

    End Class
End Namespace
