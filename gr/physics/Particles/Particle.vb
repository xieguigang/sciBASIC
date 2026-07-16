#Region "Microsoft.VisualBasic::a1856fc2cfc008d0765dbad57eb84f0d, gr\physics\Particles\Particle.vb"

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
    '    Code Lines: 35 (77.78%)
    ' Comment Lines: 3 (6.67%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 7 (15.56%)
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
            Return PredictedPosition.x
        End Get
        Set(value As Double)
            If Not PredictedPosition Is Nothing Then
                PredictedPosition.x = value
            End If
        End Set
    End Property

    Public Property Y As Double Implements Layout2D.Y
        Get
            Return PredictedPosition.y
        End Get
        Set(value As Double)
            If Not PredictedPosition Is Nothing Then
                PredictedPosition.y = value
            End If
        End Set
    End Property

    Public Position As Vector2
    Public Velocity As Vector2
    Public Index As Integer
    Public PredictedPosition As Vector2

    ''' <summary>
    ''' Density, Near Density
    ''' </summary>
    Public Density As Vector2

    Sub New(i As Integer, box As Size)
        Index = i
        Position = Vector2.Random(box)
        Velocity = Vector2.Random(New SizeF(10, 10))
        PredictedPosition = Vector2.Zero
        Density = Vector2.Zero
    End Sub

End Class
