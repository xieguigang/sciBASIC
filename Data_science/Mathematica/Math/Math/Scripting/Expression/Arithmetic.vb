#Region "Microsoft.VisualBasic::f89e8889d87af6042d88bea79aa8aede, Data_science\Mathematica\Math\Math\Scripting\Expression\Arithmetic.vb"

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

    '   Total Lines: 97
    '    Code Lines: 35 (36.08%)
    ' Comment Lines: 54 (55.67%)
    '    - Xml Docs: 98.15%
    ' 
    '   Blank Lines: 8 (8.25%)
    '     File Size: 4.37 KB


    '     Module Arithmetic
    ' 
    '         Properties: Arithmetic
    ' 
    '         Function: Evaluate, Factorial, RND
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports rand = Microsoft.VisualBasic.Math.RandomExtensions
Imports std = System.Math

Namespace Scripting.MathExpression

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
            Function(a, b) If(a = 0.0 OrElse b = 0.0, 0.0, a * b),
            Function(a, b) If(a = 0.0, 0.0, a / b),
            Function(a, b) If(a = 0.0, 0.0, a \ b),
            Function(a, b) If(a = 0.0, 0.0, a Mod b),
            AddressOf std.Pow,
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
        ''' <param name="b">Useless parameter 'b'(为了保持函数接口兼容性而设置的一个无用的参数'b')</param>
        ''' <returns>
        ''' Return the factorial value of the number 'a', if 'a' is a negative number then this function
        ''' return value 1.
        ''' (函数返回参数'a'的阶乘计算值，假若'a'是一个负数的话，则会返回1)
        ''' </returns>
        ''' <remarks></remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function Factorial(a As Double, b As Double) As Double
            Return VBMath.Factorial(a)
        End Function

        ''' <summary>
        ''' This function return a random number, you can specific the boundary of the random number in the parameters. 
        ''' </summary>
        ''' <param name="UpBound">
        ''' If this parameter is empty or value is zero, then return the randome number between 0 and 1.
        ''' (如果这个参数为空或者其值为0，那么函数就会返回0和1之间的随机数)
        ''' </param>
        ''' <param name="LowBound"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function RND(LowBound As Double, UpBound As Double) As Double
            If UpBound = 0R OrElse UpBound < LowBound Then
                Return rand.NextDouble
            Else
                Return LowBound + rand.NextDouble * (UpBound - LowBound)
            End If
        End Function
    End Module
End Namespace
