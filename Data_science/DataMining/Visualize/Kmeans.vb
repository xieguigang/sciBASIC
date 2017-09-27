#Region "Microsoft.VisualBasic::80fc21a29455d209471dd84d3d0d73e6, ..\sciBASIC#\Data_science\DataMining\Visualize\Kmeans.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
' 
' Copyright (c) 2016 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Imaging.Drawing2D
Imports Microsoft.VisualBasic.Imaging.Drawing3D
Imports Microsoft.VisualBasic.Imaging.Driver

Public Module Kmeans

    Public Function Scatter2D()

    End Function

    <Extension>
    Public Function Scatter3D(points As IEnumerable(Of Point3D),
                              Optional size$ = "1600,1200",
                              Optional bg$ = "white",
                              Optional padding$ = g.DefaultPadding) As GraphicsData

    End Function
End Module

