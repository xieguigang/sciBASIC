#Region "Microsoft.VisualBasic::da54c1fc7a7c06212975fa89a844463b, Data_science\Mathematica\Math\Math\Algebra\Helpers\MatrixMathArithmetic.vb"

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

    '   Total Lines: 81
    '    Code Lines: 30 (37.04%)
    ' Comment Lines: 43 (53.09%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (9.88%)
    '     File Size: 3.64 KB


    '     Class MatrixMathArithmetic
    ' 
    '         Function: Evaluate, Factorial
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace BasicR.Helpers

    ''' <summary>
    ''' The basics arithmetic operators' definition of matrix object in mathematics.
    ''' (数学意义上的基本的四则运算符号的定义)  
    ''' </summary>
    ''' <remarks></remarks>
    Public Class MatrixMathArithmetic

        ''' <summary>
        ''' +-*/\%^!
        ''' </summary>
        ''' <remarks></remarks>
        Public Shared ReadOnly Arithmetic As System.Func(Of Double, Double, Double)() = {
            Function(a As Double, b As Double) a + b,
            Function(a As Double, b As Double) a - b,
            Function(a As Double, b As Double) a * b,
            Function(a As Double, b As Double) a / b,
            Function(a As Double, b As Double) a \ b,
            Function(a As Double, b As Double) a Mod b,
            Function(a As Double, b As Double) System.Math.Pow(a, b),
            AddressOf Factorial}

        ''' <summary>
        ''' A string constant that enumerate all of the arithmetic operators.
        ''' (一个枚举所有的基本运算符的字符串常数) 
        ''' </summary>
        ''' <remarks></remarks>
        Public Const OPERATORS As String = "+-*/\%^!"

        ''' <summary>
        ''' A string constant RegularExpressions that stands a double type number.
        ''' (一个用于表示一个双精度类型的实数的正则表达式)
        ''' </summary>
        ''' <remarks></remarks>
        Public Const DOUBLE_NUMBER_REGX As String = "-?\d+([.]\d+)?"

        ''' <summary>
        ''' Do a basically arithmetic calculation.
        ''' (进行一次简单的四则运算) 
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <param name="o">Arithmetic operator(运算符)</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function Evaluate(a As Double, b As Double, o As Char) As Double
            Dim idx As Integer = InStr(OPERATORS, o) - 1
            Return Arithmetic(idx)(a, b)
        End Function

        ''' <summary>
        ''' Calculate the factorial value of a number, as this function is the part of the arithmetic operation 
        ''' delegate type of 'System.Func(Of Double, Double, Double)', so it must keep the form of two double 
        ''' parameter, well, the parameter 'b As Double' is useless.
        ''' (计算某一个数的阶乘值，由于这个函数是四则运算操作委托'System.Func(Of Double, Double, Double)'中的一部分，
        ''' 故而本函数保持着两个双精度浮点型数的函数参数的输入形式，也就是说本函数的第二个参数'b'是没有任何用途的)  
        ''' </summary>
        ''' <param name="a">The number that will be calculated(将要被计算的数字)</param>
        ''' <param name="b">Useless parameter 'b'(无用的参数'b')</param>
        ''' <returns>
        ''' Return the factorial value of the number 'a', if 'a' is a negative number then this function 
        ''' return value 1.
        ''' (函数返回参数'a'的阶乘计算值，假若'a'是一个负数的话，则会返回1)
        ''' </returns>
        ''' <remarks></remarks>
        Public Shared Function Factorial(a As Double, b As Double) As Double
            If a <= 0 Then
                Return 1
            Else
                Dim n As Long = a

                For i As Long = n - 1 To 1 Step -1
                    n *= i
                Next

                Return n
            End If
        End Function
    End Class
End Namespace
