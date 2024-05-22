#Region "Microsoft.VisualBasic::d5121dbd8630bbdc719f8e610ad9b8ce, Data_science\Mathematica\Math\Math\Scripting\ParameterCompute.vb"

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

    '   Total Lines: 172
    '    Code Lines: 107 (62.21%)
    ' Comment Lines: 43 (25.00%)
    '    - Xml Docs: 53.49%
    ' 
    '   Blank Lines: 22 (12.79%)
    '     File Size: 7.94 KB


    '     Module ParameterExpressionScript
    ' 
    '         Function: (+2 Overloads) Evaluate, GetValue
    ' 
    '         Sub: Apply
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Linq.Expressions
Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Emit.Parameters
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Math.Scripting.MathExpression
Imports Microsoft.VisualBasic.Scripting.Expressions

Namespace Scripting

    ''' <summary>
    ''' 在vb之中由于可选参数的值只能够是常量，假若变量之间还存在关联，则必须要用表达式，
    ''' 但是表达式不是常量，所以使用这个模块之中的代码来模拟R语言之中的可选参数表达式
    ''' </summary>
    Public Module ParameterExpressionScript

        'Public Function Demo(c#,
        '                     Optional x$ = "c*33+5!",
        '                     Optional y$ = "log(x)+sin(9)",
        '                     Optional title$ = "This is a title string, not numeric expression") As Double()

        '    Dim parameters As Dictionary(Of String, Double) = Evaluate(Function() {c, x, y})

        '    Return {
        '        c,
        '        parameters(NameOf(x)),
        '        parameters(NameOf(y))
        '    }
        'End Function

        '<Extension>
        'Public Function Evaluate(params As IEnumerable(Of Object)) As Dictionary(Of String, Double)
        '    Dim expressions As Expression(Of Func(Of Object))() = params _
        '        .Select(Function(value) DirectCast(value, Expression(Of Func(Of Object)))) _
        '        .ToArray
        '    Return GetMyCaller.Acquire(expressions).Evaluate
        'End Function

        ''' <summary>
        ''' 进行参数计算的时候只会接受数值类型以及字符串类型的参数
        ''' </summary>
        ''' <param name="params">假若参数是不需要进行计算的，则在生成字典的时候不放进去就行了</param>
        ''' <returns></returns>
        <Extension>
        Public Function Evaluate(params As Expression(Of Func(Of Object()))) As Dictionary(Of String, Double)
            Dim caller As MethodBase = GetMyCaller()
            Return caller.InitTable(params).Evaluate(caller, Nothing)
        End Function

        ''' <summary>
        ''' 在计算参数表达式的同时，也将计算之后的值更新回原来的参数变量之上
        ''' </summary>
        Public Sub Apply(array As Expression(Of Func(Of Object())))
            Dim caller As MethodBase = GetMyCaller()
            Dim expressions As Dictionary(Of Value) = caller.InitTable(array)
            Dim strings As List(Of String) = Nothing
            ' 在这里已经计算出表达式的值了，下面只需要将值赋值回原始的参数变量就好了
            Dim values As Dictionary(Of String, Double) = expressions.Evaluate(caller, strings)
            Dim unaryExpression As NewArrayExpression = DirectCast(array.Body, NewArrayExpression)
            Dim arrayData As Expressions.UnaryExpression() = unaryExpression _
                .Expressions _
                .Select(Function(e) DirectCast(e, Expressions.UnaryExpression)) _
                .ToArray
            Dim getValue As Func(Of String, String) = values.GetValue()

            For Each expr As Expressions.UnaryExpression In arrayData
                Dim member = DirectCast(expr.Operand, MemberExpression)
                Dim constantExpression As ConstantExpression = DirectCast(member.Expression, ConstantExpression)
                Dim name As String = member.Member.Name.Replace("$VB$Local_", "")
                Dim field As FieldInfo = DirectCast(member.Member, FieldInfo)
                Dim target As Object = constantExpression.Value
                Dim value As Object

                If strings.IndexOf(name) > -1 Then ' 进行字符串插值处理
                    value = expressions(name).Value
                    value = Mid(CStr(value), 2).Interpolate(getValue)
                Else
                    value = values(name)

                    Select Case field.FieldType
                        Case GetType(String)
                            value = CStr(value)
                        Case GetType(Integer)
                            value = CInt(value)
                        Case GetType(Long)
                            value = CLng(value)
                        Case GetType(Byte)
                            value = CByte(value)
                        Case GetType(Single)
                            value = CSng(value)
                        Case GetType(Decimal)
                            value = CDec(value)
                        Case GetType(Short)
                            value = CShort(value)
                    End Select
                End If

                Call field.SetValue(target, value)
            Next
        End Sub

        ''' <summary>
        ''' 因为字符串插值过程之中变量名称的大小写可能不会敏感，所以还需要在这个函数值中对Key进行额外的小写转换处理
        ''' </summary>
        ''' <param name="values"></param>
        ''' <returns></returns>
        <Extension>
        Private Function GetValue(values As Dictionary(Of String, Double)) As Func(Of String, String)
            values = values.ToDictionary(
                Function(x) x.Key.ToLower,
                Function(x) x.Value)

            Return Function(v$)
                       v = v.ToLower

                       If values.ContainsKey(v) Then
                           Return values(v)
                       Else
                           Return Nothing
                       End If
                   End Function
        End Function

        ''' <summary>
        ''' 进行参数计算的时候只会接受数值类型以及字符串类型的参数，字符串插值计算会被自动的忽略掉
        ''' </summary>
        ''' <param name="params">假若参数是不需要进行计算的，则在生成字典的时候不放进去就行了</param>
        ''' <param name="strings">进行字符串插值的变量名称列表</param>
        ''' <returns></returns>
        <Extension>
        Private Function Evaluate(params As Dictionary(Of Value), caller As MethodBase, ByRef strings As List(Of String)) As Dictionary(Of String, Double)
            Dim callerParameters As ParameterInfo() = caller _
                .GetParameters _
                .Where(Function(n) params.ContainsKey(n.Name)) _
                .ToArray   ' 按顺序计算
            Dim out As New List(Of String)
            Dim expression As New ExpressionEngine

            strings = New List(Of String)

            For Each name As ParameterInfo In callerParameters
                Dim value As Value = params(name.Name)

                If value.IsNumeric Then
                    Call expression.SetSymbol(name.Name, CDbl(value.Value))
                ElseIf value.IsString Then
                    Dim s$ = CStr(value.Value)

                    If s.First = "@"c Then
                        strings += name.Name
                        Continue For ' 跳过字符串插值计算
                    Else
                        Call expression.SetSymbol(name.Name, s)
                    End If
                Else
                    ' 忽略掉其他的类型
                    Continue For
                End If

                out += name.Name
            Next

            Dim values As Dictionary(Of String, Double) = out _
                .ToDictionary(Function(name) name,
                              Function(name)
                                  Return expression.GetSymbolValue(name)
                              End Function)
            Return values
        End Function
    End Module
End Namespace
