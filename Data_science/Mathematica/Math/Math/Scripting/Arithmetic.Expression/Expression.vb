#Region "Microsoft.VisualBasic::c9be4719b477f994b4bed40ae0fb3930, Data_science\Mathematica\Math\Math\Scripting\Arithmetic.Expression\Expression.vb"

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

    '     Class Expression
    ' 
    '         Properties: Constant, DefaultEngine, Functions, Variables
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: Compile, Evaluate, Evaluation, GetValue, SetVariable
    ' 
    '         Sub: AddConstant, SetVariable
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.Scripting.Helpers
Imports Microsoft.VisualBasic.Math.Scripting.Types

Namespace Scripting

    ''' <summary>
    ''' Expression Evaluation Engine
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Expression

        Public ReadOnly Property Constant As Helpers.Constants
        Public ReadOnly Property Variables As Variable
        Public ReadOnly Property Functions As [Function]

        ''' <summary>
        ''' Gets constant or variable value, but only sets variable value.
        ''' </summary>
        ''' <param name="var"></param>
        ''' <returns></returns>
        Default Public Property value(var As String) As Double
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return GetValue(var)
            End Get
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Set(value As Double)
                Call Variables.Set(var, value)
            End Set
        End Property

        ''' <summary>
        ''' The default expression evaluation engine.
        ''' </summary>
        ''' <returns></returns>
        Public Shared ReadOnly Property DefaultEngine As New Expression

        ''' <summary>
        ''' Creates a new mathematics expression evaluation engine
        ''' </summary>
        Public Sub New()
            Variables = New Variable(Me)
            Functions = New [Function](Me)
            Constant = New Helpers.Constants(Me)
        End Sub

        ''' <summary>
        ''' This shared method using the default expression engine for the evaluation.
        ''' </summary>
        ''' <param name="expr"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function Evaluate(expr As String) As Double
            Return DefaultEngine.Evaluation(expr)
        End Function

        ''' <summary>
        ''' Evaluate the a specific mathematics expression string to a double value, the functions, constants, 
        ''' bracket pairs can be include in this expression but the function are those were originally exists 
        ''' in the visualbasic. I'm sorry for this...
        ''' (对一个包含有函数、常数和匹配的括号的一个复杂表达式进行求值，但是对于表达式中的函数而言：仅能够使用在
        ''' VisualBaisc语言中存在的有限的几个数学函数。)  
        ''' </summary>
        ''' <param name="expr"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Evaluation(expr As String) As Double
            Dim sep As SimpleExpression = ExpressionParser.TryParse(expr, Me)
            Return sep.Evaluate
        End Function

        ''' <summary>
        ''' 当需要进行重复大量计算的时候，反复解析表达式字符串会浪费大量的计算时间，
        ''' 则可以使用这个解析出表达式，后面使用<see cref="SetVariable"/>更新变量
        ''' 的值即可快速的进行重复计算
        ''' </summary>
        ''' <param name="expr$"></param>
        ''' <returns></returns>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Compile(expr$) As SimpleExpression
            Return ExpressionParser.TryParse(expr, Me)
        End Function

        ''' <summary>
        ''' 先常量，后变量
        ''' </summary>
        ''' <param name="x"></param>
        ''' <returns></returns>
        Public Function GetValue(x As String) As Double
            Dim isConst As Boolean = False
            Dim n As Double = Constant.[GET](x, isConst)

            If isConst Then
                Return n
            Else
                Return Variables(x)
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub SetVariable(Name As String, expr As String)
            Call Variables.Set(Name, expr)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function SetVariable(name$, value#) As Expression
            Call Variables.Set(name, value)
            Return Me
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Sub AddConstant(Name As String, expr As String)
            Dim val As Double = Me.Evaluation(expr)
            Call Constant.Add(Name, val)
        End Sub
    End Class
End Namespace
