#Region "Microsoft.VisualBasic::77611a0edb6226754cb43329f630e336, Data_science\Mathematica\Math\Math\Spline\Extensions.vb"

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

    '   Total Lines: 103
    '    Code Lines: 66 (64.08%)
    ' Comment Lines: 22 (21.36%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 15 (14.56%)
    '     File Size: 3.01 KB


    '     Enum Splines
    ' 
    '         B_Spline, Bezier, CatmullRomSpline, CentripetalCatmullRomSpline, CubicSpline
    ' 
    '  
    ' 
    ' 
    ' 
    '     Module Extensions
    ' 
    '         Function: CubicSpline, ParseSplineValue, Range
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq

Namespace Interpolation

    ''' <summary>
    ''' 线条插值算法类型
    ''' </summary>
    Public Enum Splines As Byte
        ''' <summary>
        ''' 无插值操作
        ''' </summary>
        None = 0
        ''' <summary>
        ''' 二次插值
        ''' </summary>
        B_Spline
        ''' <summary>
        ''' 贝塞尔曲线插值
        ''' </summary>
        Bezier
        CatmullRomSpline
        CentripetalCatmullRomSpline
        ''' <summary>
        ''' 三次插值处理
        ''' </summary>
        CubicSpline
    End Enum

    <HideModuleName>
    Public Module Extensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function CubicSpline(points As IEnumerable(Of PointF), Optional expected# = 100) As PointF()
            Return Interpolation.CubicSpline.RecalcSpline(points, expected).ToArray
        End Function

        ReadOnly splineValues As Dictionary(Of String, Splines) = Enums(Of Splines).ToDictionary(Function(a) a.Description.ToLower)

        Public Function ParseSplineValue(describ As String) As Splines
            With LCase(describ).Trim
                If .DoCall(AddressOf splineValues.ContainsKey) Then
                    Return .DoCall(Function(key) splineValues(key))
                Else
                    Return Splines.None
                End If
            End With
        End Function

        ''' <summary>
        ''' Computes the range of a strided array.
        ''' </summary>
        ''' <param name="N">number of indexed elements</param>
        ''' <param name="x">input array</param>
        ''' <param name="stride">stride length</param>
        ''' <returns></returns>
        Public Function Range(N As Integer, x As Double(), stride As Integer) As Double
            Dim max As Double
            Dim min As Double
            Dim ix As Integer
            Dim v As Double

            If N <= 0 Then
                Return Double.NaN
            End If

            If N = 1 OrElse stride = 0 Then
                If x(0).IsNaNImaginary Then
                    Return Double.NaN
                End If
                Return 0.0
            End If

            If stride < 0 Then
                ix = (1 - N) * stride
            Else
                ix = 0
            End If

            min = x(ix)
            max = min

            For i As Integer = 1 To N - 1
                ix += stride
                v = x(ix)

                If v.IsNaNImaginary Then
                    Return v
                End If

                If v < min Then
                    min = v
                ElseIf v > max Then
                    max = v
                End If
            Next

            Return max - min
        End Function
    End Module
End Namespace
