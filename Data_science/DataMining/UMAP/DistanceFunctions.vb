#Region "Microsoft.VisualBasic::4655d992c7a58446e397085a2173acc4, Data_science\DataMining\UMAP\DistanceFunctions.vb"

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

    '   Total Lines: 46
    '    Code Lines: 21
    ' Comment Lines: 20
    '   Blank Lines: 5
    '     File Size: 1.85 KB


    ' Class DistanceFunctions
    ' 
    '     Function: Cosine, CosineForNormalizedVectors, Euclidean, SpectralSimilarity
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports stdNum = System.Math
Imports Microsoft.VisualBasic.Math

Public NotInheritable Class DistanceFunctions

    ''' <summary>
    ''' this function will do data normalization and then evaluated the cosine similarity
    ''' </summary>
    ''' <param name="lhs"></param>
    ''' <param name="rhs"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function Cosine(lhs As Double(), rhs As Double()) As Double
        Return 1 - SIMD.DotProduct(lhs, rhs) / (SIMD.Magnitude(lhs) * SIMD.Magnitude(rhs))
    End Function

    ''' <summary>
    ''' use this function if the input vector <paramref name="lhs"/> and <paramref name="rhs"/> 
    ''' has been normalized to range [0,1]
    ''' </summary>
    ''' <param name="lhs"></param>
    ''' <param name="rhs"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function CosineForNormalizedVectors(lhs As Double(), rhs As Double()) As Double
        Return 1 - SIMD.DotProduct(lhs, rhs)
    End Function

    ''' <summary>
    ''' this function could be give the un-normalized vector data
    ''' </summary>
    ''' <param name="lhs"></param>
    ''' <param name="rhs"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function SpectralSimilarity(lhs As Double(), rhs As Double()) As Double
        Return 1 - SSM_SIMD(lhs, rhs)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function Euclidean(lhs As Double(), rhs As Double()) As Double
        ' TODO: Replace with netcore3 MathF class when the framework is available
        Return stdNum.Sqrt(SIMD.Euclidean(lhs, rhs))
    End Function
End Class
