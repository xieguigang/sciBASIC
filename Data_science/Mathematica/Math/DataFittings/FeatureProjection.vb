#Region "Microsoft.VisualBasic::773f243cc62d6d51ba47c796f0812f87, Data_science\Mathematica\Math\DataFittings\FeatureProjection.vb"

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

    ' Module FeatureProjection
    ' 
    '     Function: (+2 Overloads) Project
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
            Dim fit = LeastSquares.PolyFit(.X, .Y, poly_n:=dimension)
            Dim projection As Vector = fit.Polynomial.Factors

            Return projection
        End With
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function Project(vector As Vector, dimension%) As Vector
        Return vector _
            .Select(Function(d, i) New PointF(i, d)) _
            .Project(dimension)
    End Function
End Module

