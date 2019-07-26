#Region "Microsoft.VisualBasic::ac9d30dc9167c97f8dc53d80086bdca5, Data_science\Darwinism\NonlinearGrid\TopologyInference\Models\Correlation.vb"

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

' Class Correlation
' 
'     Properties: B, BC
' 
'     Function: Clone, Evaluate, ToString
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization

''' <summary>
''' A linear correlation system
''' </summary>
Public Class Correlation : Implements ICloneable(Of Correlation)

    ''' <summary>
    ''' B is a vector that related with X
    ''' </summary>
    ''' <returns></returns>
    Public Property B As Vector
    Public Property BC As Double

    ' ReadOnly sigmoid As New BipolarSigmoid

    ''' <summary>
    ''' sigmoid(C + B*X)
    ''' </summary>
    ''' <param name="X"></param>
    ''' <returns></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Evaluate(X As Vector) As Double
        ' 通过sigmoid函数强制约束输出到[-1, 1]这个区间内
        ' 因为实际的X值可能会比较大，这样子累加之后的相关系数会非常高
        ' 故而会导致相应的系统变量的结果值很容易的就出现无穷大的结果值
        ' 添加了BipolarSigmoid函数的约束之后可以避免出现这种情况
        ' Return BC + sigmoid.Function((B * X).Sum)

        Dim c As Vector = BC + B * X
        Return c.Sum
    End Function

    Public Overrides Function ToString() As String
        Return B.ToString
    End Function

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Function Clone() As Correlation Implements ICloneable(Of Correlation).Clone
        Return New Correlation With {
            .B = New Vector(B.AsEnumerable),
            .BC = BC
        }
    End Function
End Class
