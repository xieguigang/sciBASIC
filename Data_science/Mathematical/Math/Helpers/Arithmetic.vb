#Region "Microsoft.VisualBasic::83cefa8daeefbce31ef9d1e9a2d04a66, ..\visualbasic_App\Data_science\Mathematical\Math\Helpers\Arithmetic.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Namespace Helpers

    ''' <summary>
    ''' The basics arithmetic operators' definition.
    ''' (基本的四则运算符号的定义)
    ''' </summary>
    ''' <remarks></remarks>
    Public Module Arithmetic

        ''' <summary>
        ''' +-*/\%^!
        ''' </summary>
        ''' <remarks></remarks>
        Public ReadOnly Property Arithmetic As Func(Of Double, Double, Double)() = {
 _
            Function(a, b) a + b,
            Function(a, b) a - b,
            Function(a, b) a * b,
            Function(a, b) a / b,
            Function(a, b) a \ b,
            Function(a, b) a Mod b,
            AddressOf Math.Pow,
            AddressOf Factorial
        }

        ''' <summary>
        ''' A string constant that enumerate all of the arithmetic operators.
        ''' (一个枚举所有的基本运算符的字符串常数)
        ''' </summary>
        ''' <remarks></remarks>
        Public Const Operators$ = "+-*/\%^!"

        ''' <summary>
        ''' A string constant RegularExpressions that stands a double type number.
        ''' (一个用于表示一个双精度类型的实数的正则表达式)
        ''' </summary>
        ''' <remarks></remarks>
        Public Const NumericRegexp$ = "-?\d+([.]\d+)?"

        ''' <summary>
        ''' Do a basically arithmetic calculation.
        ''' (进行一次简单的四则运算)
        ''' </summary>
        ''' <param name="a"></param>
        ''' <param name="b"></param>
        ''' <param name="o">Arithmetic operator(运算符)</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Evaluate(a#, b#, o As Char) As Double
            Dim idx As Integer = Operators.IndexOf(o)
            Return _Arithmetic(idx)(a, b)
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
        Public Function Factorial(a As Double, b As Double) As Double
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
    End Module
End Namespace
