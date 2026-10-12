#Region "Microsoft.VisualBasic::bad39784353aeb961d839c95e8c59873, gr\physics\Particles\FluidKernels.vb"

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

    '   Total Lines: 63
    '    Code Lines: 50 (79.37%)
    ' Comment Lines: 5 (7.94%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (12.70%)
    '     File Size: 2.29 KB


    ' Class FluidKernels
    ' 
    '     Constructor: (+1 Overloads) Sub New
    '     Function: DerivativeSpikyPow2, DerivativeSpikyPow3, SmoothingKernelPoly6, SpikyKernelPow2, SpikyKernelPow3
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Math

''' <summary>
''' SPH smoothing kernels shared by the 2D solver <see cref="FluidEngine"/> and
''' the 3D solver <see cref="FluidEngine3D"/>.
''' 
''' The five kernel *functions* are strictly identical in two and in three
''' dimensions -- what differs is only the five normalization scaling factors,
''' which is why this single type carries a <see cref="Dimension"/> switch
''' instead of being duplicated into a 2D and a 3D flavour.
''' 
''' IMPORTANT: the normalization constant is WHERE THE DIMENSION ACTUALLY
''' LIVES. It is not possible to emulate a 2D simulation with the 3D code by
''' simply pinning the z coordinate to zero: the kernels are normalized against
''' <c>∫W dV = 1</c>, and the integration measure is <c>2πr·dr</c> in 2D but
''' <c>4πr²·dr</c> in 3D. The resulting constants do not merely differ by a
''' factor, they differ by a whole power of <c>h</c> (i.e. by their physical
''' dimension: 1/area vs 1/volume). Feeding 3D constants into a 2D solver makes
''' the density estimate -- and therefore every pressure term -- systematically
''' wrong.
''' 
''' The exact constants, obtained from <c>∫ (h-r)^n · 2πr dr</c> (2D) and
''' <c>∫ (h-r)^n · 4πr² dr</c> (3D) over <c>r ∈ [0,h]</c>:
''' 
''' + Poly6            (h²-r²)³ : 2D ∫ = πh⁸/4    → 4   / (PI * h^8)
'''                              3D ∫ = 64πh⁹/315 → 315 / (64 * PI * h^9)
''' + SpikyPow3        (h-r)³   : 2D ∫ = πh⁵/10   → 10  / (PI * h^5)
'''                              3D ∫ = πh⁶/15    → 15  / (PI * h^6)
''' + SpikyPow2        (h-r)²   : 2D ∫ = πh⁴/6    → 6   / (PI * h^4)
'''                              3D ∫ = 2πh⁵/15   → 15  / (2 * PI * h^5)
''' + d/dr SpikyPow3 -3(h-r)²   : 2D → 30  / (PI * h^5)
'''                              3D → 45  / (PI * h^6)
''' + d/dr SpikyPow2 -2(h-r)    : 2D → 12  / (PI * h^4)
'''                              3D → 15  / (PI * h^5)
''' 
''' NOTE: the SpikyPow2 pair of the 3D solver was previously normalized with the
''' 2D-derived constants (15/(PI*h^5) and 30/(PI*h^5)), which are exactly 2x too
''' large in three dimensions. That factor of two made the pressure term of
''' <see cref="FluidEngine3D"/> inconsistent with the near-pressure term and is
''' one of the reasons why the original 3D solver looked unstable.
''' 
''' The scaling factors depend only on the smoothing radius (and the dimension),
''' so they are precomputed once in the constructor.
''' </summary>
Public Class FluidKernels

    ''' <summary>
    ''' the spatial dimension these kernels are normalized for. 2 = area
    ''' measure (2D solver), 3 = volume measure (3D solver).
    ''' </summary>
    Public ReadOnly Property Dimension As Integer

    ReadOnly Poly6ScalingFactor As Single
    ReadOnly SpikyPow3ScalingFactor As Single
    ReadOnly SpikyPow2ScalingFactor As Single
    ReadOnly SpikyPow3DerivativeScalingFactor As Single
    ReadOnly SpikyPow2DerivativeScalingFactor As Single

    ''' <summary>the self contribution W(0) of the density kernel (h-r)^2</summary>
    ReadOnly SelfDensity As Single
    ''' <summary>the self contribution W(0) of the near density kernel (h-r)^3</summary>
    ReadOnly SelfNearDensity As Single

    ''' <summary>
    ''' create a kernel bundle normalized for the given spatial dimension.
    ''' </summary>
    ''' <param name="smoothingRadius">the SPH smoothing radius h</param>
    ''' <param name="dimension">
    ''' 2 or 3. this parameter is mandatory on purpose: making it optional would
    ''' silently hand 3D constants to a 2D caller (and vice versa) whenever it
    ''' is omitted, which is a physics bug rather than a compile error.
    ''' </param>
    Sub New(smoothingRadius As Single, dimension As Integer)
        If dimension <> 2 AndAlso dimension <> 3 Then
            Throw New ArgumentOutOfRangeException(
                NameOf(dimension), dimension,
                "SPH kernels can only be normalized for dimension 2 or 3.")
        End If

        Me.Dimension = dimension

        If dimension = 2 Then
            Poly6ScalingFactor = 4 / (PI * Pow(smoothingRadius, 8))
            SpikyPow3ScalingFactor = 10 / (PI * Pow(smoothingRadius, 5))
            SpikyPow2ScalingFactor = 6 / (PI * Pow(smoothingRadius, 4))
            SpikyPow3DerivativeScalingFactor = 30 / (Pow(smoothingRadius, 5) * PI)
            SpikyPow2DerivativeScalingFactor = 12 / (Pow(smoothingRadius, 4) * PI)
        Else
            Poly6ScalingFactor = 315 / (64 * PI * Pow(smoothingRadius, 9))
            SpikyPow3ScalingFactor = 15 / (PI * Pow(smoothingRadius, 6))
            SpikyPow2ScalingFactor = 15 / (2 * PI * Pow(smoothingRadius, 5))
            SpikyPow3DerivativeScalingFactor = 45 / (PI * Pow(smoothingRadius, 6))
            SpikyPow2DerivativeScalingFactor = 15 / (PI * Pow(smoothingRadius, 5))
        End If

        SelfDensity = SpikyKernelPow2(0, smoothingRadius)
        SelfNearDensity = SpikyKernelPow3(0, smoothingRadius)
    End Sub

    ''' <summary>a kernel bundle normalized for a two dimensional simulation.</summary>
    Public Shared Function For2D(smoothingRadius As Single) As FluidKernels
        Return New FluidKernels(smoothingRadius, 2)
    End Function

    ''' <summary>a kernel bundle normalized for a three dimensional simulation.</summary>
    Public Shared Function For3D(smoothingRadius As Single) As FluidKernels
        Return New FluidKernels(smoothingRadius, 3)
    End Function

    ' the precomputed scaling factors are exposed so that downstream parameter
    ' bundles (e.g. SphParams3D) can source the constants from here instead of
    ' repeating the same literals a second time.

    ''' <summary>the normalization constant of <see cref="SmoothingKernelPoly6"/></summary>
    Public ReadOnly Property Poly6Factor As Single
        Get
            Return Poly6ScalingFactor
        End Get
    End Property

    ''' <summary>the normalization constant of <see cref="SpikyKernelPow3"/></summary>
    Public ReadOnly Property SpikyPow3Factor As Single
        Get
            Return SpikyPow3ScalingFactor
        End Get
    End Property

    ''' <summary>the normalization constant of <see cref="SpikyKernelPow2"/></summary>
    Public ReadOnly Property SpikyPow2Factor As Single
        Get
            Return SpikyPow2ScalingFactor
        End Get
    End Property

    ''' <summary>the normalization constant of <see cref="DerivativeSpikyPow3"/></summary>
    Public ReadOnly Property GradSpikyPow3Factor As Single
        Get
            Return SpikyPow3DerivativeScalingFactor
        End Get
    End Property

    ''' <summary>the normalization constant of <see cref="DerivativeSpikyPow2"/></summary>
    Public ReadOnly Property GradSpikyPow2Factor As Single
        Get
            Return SpikyPow2DerivativeScalingFactor
        End Get
    End Property

    ''' <summary>
    ''' The self density contribution W(0) of the (h-r)^2 density kernel: a
    ''' particle always contributes its own kernel value to its density sum.
    ''' </summary>
    ''' <returns>W_spiky2(0); 2D: 6/(PI*h^2), 3D: 15*h^2/(2*PI*h^5)</returns>
    Public Function SelfDensityKernel() As Single
        Return SelfDensity
    End Function

    ''' <summary>
    ''' The self density contribution W(0) of the (h-r)^3 near density kernel.
    ''' </summary>
    ''' <returns>W_spiky3(0); 2D: 10/(PI*h^2), 3D: 15*h^3/(PI*h^6)</returns>
    Public Function SelfNearDensityKernel() As Single
        Return SelfNearDensity
    End Function

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
