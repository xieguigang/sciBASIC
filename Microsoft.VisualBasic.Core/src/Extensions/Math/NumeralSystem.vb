#Region "Microsoft.VisualBasic::a485a7ddf0aaed44e8cd3f637d59ce52, Microsoft.VisualBasic.Core\src\Extensions\Math\NumeralSystem.vb"

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
    '    Code Lines: 70 (57.38%)
    ' Comment Lines: 34 (27.87%)
    '    - Xml Docs: 88.24%
    ' 
    '   Blank Lines: 18 (14.75%)
    '     File Size: 4.88 KB


    '     Module NumeralSystem
    ' 
    '         Function: FindNthRoot, TranslateDecimal, (+2 Overloads) Ulp
    '         Structure DoubleUnion
    ' 
    ' 
    ' 
    '         Structure SingleUnion
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports System.Runtime.InteropServices
Imports std = System.Math

Namespace Math

    Public Module NumeralSystem

        <StructLayout(LayoutKind.Explicit)>
        Public Structure DoubleUnion
            <FieldOffset(0)>
            Public DoubleValue As Double
            <FieldOffset(0)>
            Public LongValue As Long
        End Structure

        ''' <summary>
        ''' Unit in the Last Place, ulp
        ''' </summary>
        ''' <param name="value"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' Math.ulp 是一个用于获取一个数的最小精度单位（Unit in the Last Place，ulp）的方法。
        ''' 这个方法在浮点数运算中非常有用，特别是在需要考虑数值精度和误差分析的场景。
        ''' </remarks>
        Public Function Ulp(value As Double) As Double
            Dim union As DoubleUnion
            union.DoubleValue = value
            Dim nextValue As Double = BitConverter.Int64BitsToDouble(union.LongValue + 1)
            Return nextValue - value
        End Function

#If NET8_0_OR_GREATER Then
        ' 定义一个联合体，用于浮点数和整数的转换
        <StructLayout(LayoutKind.Explicit)>
        Public Structure SingleUnion
            <FieldOffset(0)>
            Public SingleValue As Single
            <FieldOffset(0)>
            Public IntegerValue As Integer
        End Structure

        ' 计算单精度浮点数的ulp值
        Public Function Ulp(f As Single) As Single
            Dim union As SingleUnion
            union.SingleValue = f

            ' 如果输入的是非规格化数，则ulp值为最小正非规格化数
            If union.IntegerValue And &H7F800000 = 0 Then
                Return CSng(BitConverter.Int32BitsToSingle(1))
            End If

            ' 计算ulp值
            Dim ulp32 As Integer = If(union.IntegerValue And &H7FFFFFFF = &H7F7FFFFF, union.IntegerValue - &H7F7FFFFF, union.IntegerValue + 1) - union.IntegerValue
            Return CSng(BitConverter.Int32BitsToSingle(ulp32))
        End Function
#End If

        ''' <summary>
        ''' A helper function for translate decimal number to the number of another kind of custom number system
        ''' </summary>
        ''' <param name="d"></param>
        ''' <param name="alphabets"></param>
        ''' <returns></returns>
        ''' <remarks>
        ''' 将十进制数转换到另外的一个数进制
        ''' </remarks>
        <Extension>
        Public Function TranslateDecimal(d%, alphabets As Char()) As String
            Dim r = d Mod alphabets.Length
            Dim result$

            If (d - r = 0) Then
                result = alphabets(r)
            Else
                result = ((d - r) \ alphabets.Length).TranslateDecimal(alphabets) & alphabets(r)
            End If

            Return result
        End Function

        ''' <summary>
        ''' Method which finds root of specific degree of number.
        ''' </summary>
        ''' <param name="number">Source number.</param>
        ''' <param name="degree">Degree of root.</param>
        ''' <param name="precision">Precision with which the calculations are performed. value should be in range (0,1).</param>
        ''' <returns>Root of number.</returns>
        ''' <exception cref="ArgumentOutOfRangeException">Thrown when values of degree or precision are out of range.</exception>
        ''' <exception cref="ArgumentException">Thrown when root's degree is even for calculation with negative numbers.</exception>
        ''' <remarks>
        ''' 开n次方
        ''' </remarks>
        Public Function FindNthRoot(number As Double, degree As Integer, precision As Double) As Double
            If degree < 0 Then
                Throw New ArgumentOutOfRangeException($"{degree} is out of range.")
            End If

            If precision <= 0 OrElse precision >= 1 Then
                Throw New ArgumentOutOfRangeException($"{precision} is out of range.")
            End If

            If number < 0 AndAlso degree Mod 2 = 0 Then
                Throw New ArgumentException("Root's degree cannot be even for calculation with negative numbers.")
            End If

            If degree = 1 Then
                Return number
            End If

            Dim current As Double = 1
            Dim [next] = ((degree - 1) * current + number / std.Pow(current, degree - 1)) / degree

            While std.Abs([next] - current) > precision
                current = [next]
                [next] = ((degree - 1) * current + number / std.Pow(current, degree - 1)) / degree
            End While

            Return [next]
        End Function
    End Module
End Namespace
