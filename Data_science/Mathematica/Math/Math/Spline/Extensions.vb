#Region "Microsoft.VisualBasic::78232cc60eb0c5bc9e4e57b660ba0516, Data_science\Mathematica\Math\Math\Spline\Extensions.vb"

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
    '         Function: CubicSpline, ParseSplineValue
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
    End Module
End Namespace
