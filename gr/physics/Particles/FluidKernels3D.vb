#Region "Microsoft.VisualBasic::6309de1146c4c290a56c96ea904ae862, gr\physics\Particles\FluidKernels3D.vb"

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

    '   Total Lines: 74
    '    Code Lines: 50 (67.57%)
    ' Comment Lines: 16 (21.62%)
    '    - Xml Docs: 81.25%
    ' 
    '   Blank Lines: 8 (10.81%)
    '     File Size: 2.78 KB


    ' Class FluidKernels3D
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: DerivativeSpikyPow2, DerivativeSpikyPow3, SmoothingKernelPoly6, SpikyKernelPow2, SpikyKernelPow3
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Math

''' <summary>
''' SPH smoothing kernels used by the 3D fluid engine <see cref="FluidEngine3D"/>.
''' 
''' The method signatures mirror the 2D <see cref="FluidKernels"/>, but the
''' normalization scaling factors use the correct three dimensional constants
''' so that the kernels integrate to unity over a 3D sphere of radius h:
''' 
''' + Poly6:            315 / (64 * PI * h^9)
''' + SpikyPow3:        15  / (PI * h^6)
''' + SpikyPow2:        15  / (PI * h^5)
''' + d/dr SpikyPow3:   45  / (PI * h^6)
''' + d/dr SpikyPow2:   30  / (PI * h^5)
''' 
''' The scaling factors depend only on the smoothing radius, so they are
''' precomputed once in the constructor.
''' </summary>
Public Class FluidKernels3D

    ReadOnly Poly6ScalingFactor As Single
    ReadOnly SpikyPow3ScalingFactor As Single
    ReadOnly SpikyPow2ScalingFactor As Single
    ReadOnly SpikyPow3DerivativeScalingFactor As Single
    ReadOnly SpikyPow2DerivativeScalingFactor As Single

    Sub New(smoothingRadius As Single)
        Poly6ScalingFactor = 315 / (64 * PI * Pow(smoothingRadius, 9))
        SpikyPow3ScalingFactor = 15 / (PI * Pow(smoothingRadius, 6))
        SpikyPow2ScalingFactor = 15 / (PI * Pow(smoothingRadius, 5))
        SpikyPow3DerivativeScalingFactor = 45 / (PI * Pow(smoothingRadius, 6))
        SpikyPow2DerivativeScalingFactor = 30 / (PI * Pow(smoothingRadius, 5))
    End Sub

    Public Function SmoothingKernelPoly6(dst As Single, radius As Single) As Single
        If dst < radius Then
            Dim v As Single = radius * radius - dst * dst
            Return v * v * v * Poly6ScalingFactor
        End If
        Return 0
    End Function

    Public Function SpikyKernelPow3(dst As Single, radius As Single) As Single
        If dst < radius Then
            Dim v As Single = radius - dst
            Return v * v * v * SpikyPow3ScalingFactor
        End If
        Return 0
    End Function

    Public Function SpikyKernelPow2(dst As Single, radius As Single) As Single
        If dst < radius Then
            Dim v As Single = radius - dst
            Return v * v * SpikyPow2ScalingFactor
        End If
        Return 0
    End Function

    Public Function DerivativeSpikyPow3(dst As Single, radius As Single) As Single
        If dst <= radius Then
            Dim v As Single = radius - dst
            Return -v * v * SpikyPow3DerivativeScalingFactor
        End If
        Return 0
    End Function

    Public Function DerivativeSpikyPow2(dst As Single, radius As Single) As Single
        If dst <= radius Then
            Dim v As Single = radius - dst
            Return -v * SpikyPow2DerivativeScalingFactor
        End If
        Return 0
    End Function
End Class
