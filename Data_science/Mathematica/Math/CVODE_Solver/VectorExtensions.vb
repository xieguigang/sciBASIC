#Region "Microsoft.VisualBasic::ccbca4c62e414a74b409eb0213f103b7, Data_science\Mathematica\Math\CVODE_Solver\VectorExtensions.vb"

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

    '   Total Lines: 68
    '    Code Lines: 38 (55.88%)
    ' Comment Lines: 21 (30.88%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 9 (13.24%)
    '     File Size: 2.57 KB


    ' Module VectorExtensions
    ' 
    '     Function: WRMSNorm, WRMSNormSquare
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports std = System.Math

''' <summary>
''' CVODE求解器专用的<see cref="Vector"/>扩展方法模块
''' </summary>
''' <remarks>
''' 这些方法为SUNDIALS CVODE求解器所特有的加权范数计算，
''' 不属于通用的基础数学向量API，因此以扩展方法的形式保留在CVODE项目之内
''' </remarks>
<HideModuleName>
Public Module VectorExtensions

    ''' <summary>
    ''' 加权RMS范数（Weighted Root-Mean-Square Norm）：
    ''' ``||v||wrms = sqrt(sum((w_i * v_i)^2) / n)``
    ''' </summary>
    ''' <param name="v">参与范数计算的向量</param>
    ''' <param name="weights">权重向量（通常为误差权重向量ewt），长度必须与<paramref name="v"/>相同</param>
    ''' <returns>加权RMS范数值</returns>
    <Extension>
    Public Function WRMSNorm(v As Vector, weights As Vector) As Double
        If weights Is Nothing Then
            Throw New ArgumentNullException(NameOf(weights))
        ElseIf v.Length <> weights.Length Then
            Throw New ArgumentException("向量长度不匹配")
        End If

        Dim y As Double() = v.Array
        Dim w As Double() = weights.Array
        Dim sum As Double = 0.0

        For i As Integer = 0 To y.Length - 1
            Dim temp As Double = w(i) * y(i)
            sum += temp * temp
        Next

        Return std.Sqrt(sum / y.Length)
    End Function

    ''' <summary>
    ''' 加权RMS范数的平方（Weighted Root-Mean-Square Norm Square）：
    ''' ``||v||wrms^2 = sum((w_i * v_i)^2) / n``
    ''' </summary>
    ''' <param name="v">参与范数计算的向量</param>
    ''' <param name="weights">权重向量（通常为误差权重向量ewt），长度必须与<paramref name="v"/>相同</param>
    ''' <returns>加权RMS范数的平方值</returns>
    <Extension>
    Public Function WRMSNormSquare(v As Vector, weights As Vector) As Double
        If weights Is Nothing Then
            Throw New ArgumentNullException(NameOf(weights))
        ElseIf v.Length <> weights.Length Then
            Throw New ArgumentException("向量长度不匹配")
        End If

        Dim y As Double() = v.Array
        Dim w As Double() = weights.Array
        Dim sum As Double = 0.0

        For i As Integer = 0 To y.Length - 1
            Dim temp As Double = w(i) * y(i)
            sum += temp * temp
        Next

        Return sum / y.Length
    End Function
End Module
