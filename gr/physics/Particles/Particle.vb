#Region "Microsoft.VisualBasic::60911abb1f4ed47a5547e28b771fb6ee, gr\physics\Particles\Particle.vb"

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

    '   Total Lines: 45
    '    Code Lines: 35
    ' Comment Lines: 3
    '   Blank Lines: 7
    '     File Size: 1.19 KB


    ' Class Particle
    ' 
    '     Properties: X, Y
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing

Public Class Particle : Implements Layout2D

    Public Property X As Double Implements Layout2D.X
        Get
            Return predictedPosition.x
        End Get
        Set(value As Double)
            If Not predictedPosition Is Nothing Then
                predictedPosition.x = value
            End If
        End Set
    End Property

    Public Property Y As Double Implements Layout2D.Y
        Get
            Return predictedPosition.y
        End Get
        Set(value As Double)
            If Not predictedPosition Is Nothing Then
                predictedPosition.y = value
            End If
        End Set
    End Property

    Public position As Vector2
    Public velocity As Vector2
    Public index As Integer
    Public predictedPosition As Vector2

    ''' <summary>
    ''' Density, Near Density
    ''' </summary>
    Public density As Vector2

    Sub New(i As Integer, box As Size)
        index = i
        position = Vector2.random(box)
        velocity = Vector2.random(New SizeF(10, 10))
        predictedPosition = Vector2.zero
        density = Vector2.zero
    End Sub

End Class
