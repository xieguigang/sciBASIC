﻿#Region "Microsoft.VisualBasic::f60294dc8326710cf9531a1ac4813de9, G:/GCModeller/src/runtime/sciBASIC#/Data_science/Mathematica/Math/Math//Algebra/Vector/Extensions.vb"

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

    '   Total Lines: 122
    '    Code Lines: 71
    ' Comment Lines: 35
    '   Blank Lines: 16
    '     File Size: 4.82 KB


    '     Module Extensions
    ' 
    '         Function: AsVector, Point2D, (+2 Overloads) Points, (+2 Overloads) rand, Take
    '                   Top
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Language.Default
Imports Microsoft.VisualBasic.Language.Vectorization
Imports Microsoft.VisualBasic.Linq
Imports randf2 = Microsoft.VisualBasic.Math.RandomExtensions

Namespace LinearAlgebra

    <HideModuleName>
    Public Module Extensions

        ''' <summary>
        ''' Populate points from two <see cref="Vector"/>.
        ''' </summary>
        ''' <param name="x"></param>
        ''' <param name="y"></param>
        ''' <returns></returns>
        Public Iterator Function Points(x As Vector, y As Vector) As IEnumerable(Of PointF)
            If x.Length <> y.Length Then
                Throw New ArgumentException($"size of vector x({x.Length}) should be equals to the size of vector y({y.Length})!")
            End If

            For i As Integer = 0 To x.Length - 1
                Yield New PointF(x(i), y(i))
            Next
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function Points(point As (x As Vector, y As Vector)) As IEnumerable(Of PointF)
            Return Points(point.x, point.y)
        End Function

        ''' <summary>
        ''' iterates a set of point from the given vector tuple
        ''' </summary>
        ''' <param name="polygon">
        ''' two vector should be in size equals
        ''' </param>
        ''' <returns>
        ''' a set of the point data in float data type
        ''' </returns>
        <Extension>
        Public Function Point2D(polygon As (X As Vector, Y As Vector)) As IEnumerable(Of PointF)
            With polygon
                Return Points(.X, .Y)
            End With
        End Function

        <Extension> Public Function Top(vector As Vector, n%) As BooleanVector
            Dim desc = vector.OrderByDescending(Function(x) x).Take(n).AsList
            Dim bools As New BooleanVector(vector.Select(Function(x) desc.IndexOf(x) > -1))

            Return bools
        End Function

        ''' <summary>
        ''' Simplified (numPy) take method: 1) axis is always 0, 2) first argument is always a vector
        ''' </summary>
        ''' <param name="v">List of values</param>
        ''' <param name="indices">List of indices</param>
        ''' <returns>Vector containing elements from vector 1 at the indicies in vector 2</returns>
        <Extension>
        Public Function Take(v As Vector, indices As IEnumerable(Of Integer)) As Vector
            Return New Vector(v.Takes(indices.ToArray))
        End Function

        ReadOnly normalRange As New [Default](Of DoubleRange)(New DoubleRange(0, 1))

        ''' <summary>
        ''' 默认返回目标长度的``[0-1]``之间的随机数向量
        ''' </summary>
        ''' <param name="size">the length of the result vector, or n lements</param>
        ''' <returns></returns>
        ''' <remarks>
        ''' this method can be affected by the <see cref="randf2.SetSeed(Integer)"/> method.
        ''' </remarks>
        Public Function rand(size%, Optional range As DoubleRange = Nothing) As Vector
            Dim list As Double() = New Double(size - 1) {}

            SyncLock randf2.seeds
                If range Is Nothing Then
                    For i As Integer = 0 To size - 1
                        list(i) = randf2.seeds.NextDouble
                    Next
                Else
                    Dim d = range.Length

                    For i As Integer = 0 To size - 1
                        list(i) = randf2.seeds.NextDouble * d + range.Min
                    Next
                End If
            End SyncLock

            Return New Vector(list)
        End Function

        ''' <summary>
        ''' 获取一个指定长度的由目标区间范围内的随机数所构成的向量
        ''' </summary>
        ''' <param name="range"></param>
        ''' <param name="size%">函数所返回的向量的长度</param>
        ''' <returns></returns>
        <Extension>
        Public Function rand(range As DoubleRange, size%) As Vector
            Return rand(size) _
                .RangeTransform([to]:=range) _
                .DoCall(Function(v)
                            Return New Vector(v)
                        End Function)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function AsVector(range As DoubleRange, Optional counts% = 1000) As Vector
            Return New Vector(range.Enumerate(counts))
        End Function
    End Module
End Namespace
