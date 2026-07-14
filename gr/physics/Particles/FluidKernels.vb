#Region "Microsoft.VisualBasic::ced47e48a30a0d5fdf82a96f9f2c1744, gr\physics\Particles\FluidKernels.vb"

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
    ' along with this program.  If not, see <http://www.gnu.org/licenses/>.
    
    
    
    ' /********************************************************************************/

    ' Summaries:


    ' Class FluidKernels
    ' 
    '     Constructor: (+1 Overloads) Sub New
    ' 
    '     Function: DerivativeSpikyPow2, DerivativeSpikyPow3, SmoothingKernelPoly6
    '               SpikyKernelPow2, SpikyKernelPow3
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Math

''' <summary>
''' SPH smoothing kernels used by <see cref="FluidEngine"/>.
''' The scaling factors depend only on the smoothing radius, so they are
''' precomputed once in the constructor.
''' </summary>
Public Class FluidKernels

    ReadOnly Poly6ScalingFactor As Single
    ReadOnly SpikyPow3ScalingFactor As Single
    ReadOnly SpikyPow2ScalingFactor As Single
    ReadOnly SpikyPow3DerivativeScalingFactor As Single
    ReadOnly SpikyPow2DerivativeScalingFactor As Single

    Sub New(smoothingRadius As Single)
        Poly6ScalingFactor = 4 / (PI * Pow(smoothingRadius, 8))
        SpikyPow3ScalingFactor = 10 / (PI * Pow(smoothingRadius, 5))
        SpikyPow2ScalingFactor = 6 / (PI * Pow(smoothingRadius, 4))
        SpikyPow3DerivativeScalingFactor = 30 / (Pow(smoothingRadius, 5) * PI)
        SpikyPow2DerivativeScalingFactor = 12 / (Pow(smoothingRadius, 4) * PI)
    End Sub

    Private Function SmoothingKernelPoly6(dst As Single, radius As Single) As Single
        If dst < radius Then
            Dim v As Single = radius * radius - dst * dst
            Return v * v * v * Poly6ScalingFactor
        End If
        Return 0
    End Function

    Private Function SpikyKernelPow3(dst As Single, radius As Single) As Single
        If dst < radius Then
            Dim v As Single = radius - dst
            Return v * v * v * SpikyPow3ScalingFactor
        End If
        Return 0
    End Function

    Private Function SpikyKernelPow2(dst As Single, radius As Single) As Single
        If dst < radius Then
            Dim v As Single = radius - dst
            Return v * v * SpikyPow2ScalingFactor
        End If
        Return 0
    End Function

    Private Function DerivativeSpikyPow3(dst As Single, radius As Single) As Single
        If dst <= radius Then
            Dim v As Single = radius - dst
            Return -v * v * SpikyPow3DerivativeScalingFactor
        End If
        Return 0
    End Function

    Private Function DerivativeSpikyPow2(dst As Single, radius As Single) As Single
        If dst <= radius Then
            Dim v As Single = radius - dst
            Return -v * SpikyPow2DerivativeScalingFactor
        End If
        Return 0
    End Function
End Class
