#Region "Microsoft.VisualBasic::24fe77940a2e97203e4ee57610c1000c, Data_science\Mathematica\Math\DataFittings\Evaluation.vb"

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

    ' Class Evaluation
    ' 
    '     Properties: R_square, RMSE, SSE, SSR
    ' 
    '     Function: Calculate, ToString
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports stdNum = System.Math

''' <summary>
''' Data fitting result evaluation.
''' </summary>
Public Class Evaluation

    Public Property SSR As Double
    Public Property SSE As Double
    Public Property RMSE As Double

    ''' <summary>
    ''' 确定系数，系数是0~1之间的数，是数理上判定拟合优度的一个量
    ''' </summary>
    ''' <returns></returns>
    Public ReadOnly Property R_square As Double
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Get
            If (SSR + SSE).IsNaNImaginary Then
                ' Fix bugs for x / Inf = 0
                Return 0
            Else
                Return 1 - (SSE / (SSR + SSE))
            End If
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return "R^2 = " & R_square
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="X">The training data input</param>
    ''' <param name="Y">The actual Y value output for training</param>
    ''' <param name="fx">A function for generate Y fit prediction result from the given <paramref name="X"/> input.</param>
    ''' <returns></returns>
    Public Shared Function Calculate(X As Vector(), Y As Double(), fx As Func(Of Vector, Double), Optional parallel As Boolean = True) As Evaluation
        Dim length As Integer = Y.Length
        Dim mean_y As Double = Y.Sum / length
        Dim pCalc = X _
            .Populate(parallel) _
            .Select(Function(xi As Vector, i As Integer)
                        Dim yi = fx(xi)
                        ' 计算回归平方和
                        Dim SSR = (yi - mean_y) ^ 2
                        ' 残差平方和
                        Dim SSE = (yi - Y(i)) ^ 2

                        Return (SSR:=SSR, SSE:=SSE)
                    End Function) _
            .ToArray

        Dim result As New Evaluation

        ' 计算回归平方和
        result.SSR = pCalc.Select(Function(t) t.SSR).Sum
        ' 残差平方和
        result.SSE = pCalc.Select(Function(t) t.SSE).Sum
        result.RMSE = stdNum.Sqrt(result.SSE / CDbl(length))

        Return result
    End Function
End Class
