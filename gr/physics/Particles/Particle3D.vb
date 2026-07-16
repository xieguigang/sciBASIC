#Region "Microsoft.VisualBasic::gr\physics\Particles\Particle3D.vb"

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

#End Region

''' <summary>
''' A single water particle inside the 3D SPH fluid simulation. This is the
''' three dimensional counterpart of the 2D <see cref="Particle"/>: all of the
''' spatial state (position, velocity, predicted position) is stored as a
''' <see cref="Vector3"/>.
''' </summary>
Public Class Particle3D

    Public Position As Vector3
    Public Velocity As Vector3
    Public PredictedPosition As Vector3
    Public Index As Integer

    ''' <summary>
    ''' Density, Near Density. only the <see cref="Vector3.x"/> and
    ''' <see cref="Vector3.y"/> component is used, the <see cref="Vector3.z"/>
    ''' component is unused (kept for a compact storage).
    ''' </summary>
    Public Density As Vector3

    Sub New(i As Integer, box As Vector3)
        Index = i
        Position = Vector3.Random(box)
        Velocity = Vector3.Zero
        PredictedPosition = Position.Clone()
        Density = Vector3.Zero
    End Sub

End Class
