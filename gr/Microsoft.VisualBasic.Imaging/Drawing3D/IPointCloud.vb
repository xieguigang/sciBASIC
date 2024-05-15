#Region "Microsoft.VisualBasic::05f642cac8c5ea00e983402090afcfed, gr\Microsoft.VisualBasic.Imaging\Drawing3D\IPointCloud.vb"

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

    '   Total Lines: 11
    '    Code Lines: 5
    ' Comment Lines: 3
    '   Blank Lines: 3
    '     File Size: 291 B


    '     Interface IPointCloud
    ' 
    '         Properties: intensity
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Drawing3D

    ''' <summary>
    ''' A 3D spatial heatmap point, 3d point [x,y,z] combine with the color scaler(or heatmap) data
    ''' </summary>
    Public Interface IPointCloud : Inherits PointF3D

        Property intensity As Double

    End Interface
End Namespace
