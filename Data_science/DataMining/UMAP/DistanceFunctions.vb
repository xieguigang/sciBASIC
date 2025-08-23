#Region "Microsoft.VisualBasic::e8c3c5134f82399f8b9c0717d5fa9ac8, Data_science\DataMining\UMAP\DistanceFunctions.vb"

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

'   Total Lines: 102
'    Code Lines: 57 (55.88%)
' Comment Lines: 32 (31.37%)
'    - Xml Docs: 87.50%
' 
'   Blank Lines: 13 (12.75%)
'     File Size: 3.75 KB


' Enum DistanceFunction
' 
'     Cosine, Euclidean, NormalizedCosine, SpectralCosine, TanimotoFingerprint
' 
'  
' 
' 
' 
' Class DistanceFunctions
' 
'     Function: Cosine, CosineForNormalizedVectors, Euclidean, GetFunction, JaccardSimilarity
'               SpectralSimilarity
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.Correlations
Imports std = System.Math

Public Enum DistanceFunction
    Cosine
    NormalizedCosine
    SpectralCosine
    TanimotoFingerprint
    Euclidean
    Pearson
    AbsPearson
End Enum

Public NotInheritable Class DistanceFunctions

    Public Shared Function GetFunction(method As DistanceFunction) As DistanceCalculation
        Select Case method
            Case DistanceFunction.Cosine : Return AddressOf Cosine
            Case DistanceFunction.NormalizedCosine : Return AddressOf CosineForNormalizedVectors
            Case DistanceFunction.SpectralCosine : Return AddressOf SpectralSimilarity
            Case DistanceFunction.TanimotoFingerprint : Return AddressOf JaccardSimilarity
            Case DistanceFunction.Euclidean : Return AddressOf Euclidean
            Case DistanceFunction.Pearson : Return AddressOf PearsonCor
            Case DistanceFunction.AbsPearson : Return AddressOf PearsonAbs
            Case Else
                Return AddressOf Cosine
        End Select
    End Function

    Public Shared Function PearsonCor(lhs As Double(), rhs As Double()) As Double
        Dim cor As Double = Correlations.GetPearson(lhs, rhs)

        If Double.IsNaN(cor) Then
            Return 1
        ElseIf Double.IsInfinity(cor) Then
            Return 0
        Else
            ' pearson distance is defined as 1 - r ^ 2
            Return 1 - cor ^ 2
        End If
    End Function

    Public Shared Function PearsonAbs(lhs As Double(), rhs As Double()) As Double
        Dim cor As Double = Correlations.GetPearson(lhs, rhs)

        If Double.IsNaN(cor) Then
            Return 1
        ElseIf Double.IsInfinity(cor) Then
            Return 0
        Else
            ' pearson distance is defined as 1 - |r|
            Return 1 - std.Abs(cor)
        End If
    End Function

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

    ''' <summary>
    ''' usually be tanimoto method for compares two fingerprint data
    ''' </summary>
    ''' <param name="lhs"></param>
    ''' <param name="rhs"></param>
    ''' <returns></returns>
    ''' <remarks>
    ''' zero as missing, other non-zero data as fingerprint 1
    ''' </remarks>
    Public Shared Function JaccardSimilarity(lhs As Double(), rhs As Double()) As Double
        ' 计算交集（共同为1的位数）和并集（任意一个为1的位数）
        Dim intersection As Integer = 0
        Dim union As Integer = 0

        For i As Integer = 0 To lhs.Length - 1
            Dim bit1 As Boolean = lhs(i) > 0
            Dim bit2 As Boolean = rhs(i) > 0

            If bit1 AndAlso bit2 Then
                intersection += 1
            End If

            If bit1 OrElse bit2 Then
                union += 1
            End If
        Next

        ' 处理全0情况（避免除以零）
        If union = 0 Then
            Return 1.0
        End If

        ' 计算Tanimoto系数
        Return 1 - CDbl(intersection) / CDbl(union)
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Shared Function Euclidean(lhs As Double(), rhs As Double()) As Double
        ' TODO: Replace with netcore3 MathF class when the framework is available
        Return std.Sqrt(SIMD.Euclidean(lhs, rhs))
    End Function
End Class
