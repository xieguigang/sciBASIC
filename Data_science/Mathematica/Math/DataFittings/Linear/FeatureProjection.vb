#Region "Microsoft.VisualBasic::4727dd3905c17d0657064742b099e5c0, Data_science\Mathematica\Math\DataFittings\Linear\FeatureProjection.vb"

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

    '   Total Lines: 37
    '    Code Lines: 25 (67.57%)
    ' Comment Lines: 6 (16.22%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 6 (16.22%)
    '     File Size: 1.31 KB


    ' Module FeatureProjection
    ' 
    '     Function: (+3 Overloads) Project
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math
Imports Microsoft.VisualBasic.Math.LinearAlgebra

Public Module FeatureProjection

    ''' <summary>
    ''' 利用多项式线性拟合将不等长的向量投影为指定维度的向量
    ''' </summary>
    ''' <param name="points"></param>
    ''' <param name="dimension%"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Project(points As IEnumerable(Of PointF), dimension%) As Vector
        With points.ToArray
            Return (.X.ToArray, .Y.ToArray).Project(dimension)
        End With
    End Function

    <Extension>
    Public Function Project(points As (x As Double(), y As Double()), dimension%) As Vector
        Dim fit = LeastSquares.PolyFit(points.x, points.y, poly_n:=dimension)
        Dim projection As Vector = DirectCast(fit.Polynomial, Polynomial).Factors

        Return projection
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Project(vector As Vector, dimension%) As Vector
        Dim x As Double() = vector.Sequence.Cast(Of Double).ToArray
        Dim y As Double() = vector.ToArray

        Return (x, y).Project(dimension)
    End Function
End Module
