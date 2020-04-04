#Region "Microsoft.VisualBasic::38ba31e1a8eadeaf43e1372674303e37, Data_science\Mathematica\Math\Math\Scripting\Arithmetic.Expression\ExpressionParser.vb"

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

    '     Module ExpressionParser
    ' 
    '         Function: GetTokens, TryParse
    '         Delegate Function
    ' 
    '             Function: (+4 Overloads) TryParse
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Emit.Marshal
Imports Microsoft.VisualBasic.Math.Scripting.Expression
Imports Microsoft.VisualBasic.Math.Scripting.Types
Imports Microsoft.VisualBasic.Scripting.TokenIcer

Namespace Scripting

    ''' <summary>
    ''' Parser for complex expressions
    ''' </summary>
    Public Module ExpressionParser

        Public Function GetTokens(s As String) As List(Of Token(Of ExpressionTokens))
            Return TokenIcer.TryParse(s.ClearOverlapOperator)
        End Function

        ''' <summary>
        ''' Using defaul engine
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        Public Function TryParse(s As String) As SimpleExpression
            Return TryParse(s, DefaultEngine)
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="var">Constant or variable name.</param>
        ''' <returns></returns>
        Public Delegate Function GetValue(var As String) As Double

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="tokens"></param>
        ''' <param name="engine">The expression engine.</param>
        ''' <returns></returns>
        <Extension> Public Function TryParse(tokens As Pointer(Of Token(Of ExpressionTokens)), engine As Expression) As SimpleExpression
            Return tokens.TryParse(AddressOf engine.GetValue, AddressOf engine.Functions.Evaluate, False)
        End Function

        Public Function TryParse(s As String, Engine As Expression) As SimpleExpression
            Return s.TryParse(AddressOf Engine.GetValue, AddressOf Engine.Functions.Evaluate)
        End Function

        ''' <summary>
        ''' 这个解析器还需要考虑Stack的问题
        ''' </summary>
        ''' <returns></returns>
        <Extension> Public Function TryParse(s$, getValue As GetValue, evaluate As IFuncEvaluate) As SimpleExpression
            Dim tokens As List(Of Token(Of ExpressionTokens)) =        ' Get all of the number that appears in this
                TokenIcer.TryParse(s.ClearOverlapOperator)   ' expression including factoral operator.

            If tokens.Count = 1 Then
                Dim token As Token(Of ExpressionTokens) = tokens.First

                If token.Type = ExpressionTokens.Number Then
                    Return New SimpleExpression(Val(token.Text))
                Else  ' Syntax error
                    Throw New SyntaxErrorException(s)
                End If
            Else
#If Not DEBUG Then
                Try
#End If
                ' 2017-1-26
                ' 可能有些错误没有被发现，所以在调试模式下就不进行错误处理的，因为错误处理会吞掉原来的错误，增加调试难度
                Return New Pointer(Of Token(Of ExpressionTokens))(tokens).TryParse(getValue, evaluate, False)
#If Not DEBUG Then
                Catch ex As Exception
                    ex = New Exception(s, ex)
                    Throw ex
                End Try
#End If
            End If
        End Function

        ''' <summary>
        ''' 这个解析器还需要考虑Stack的问题
        ''' </summary>
        ''' <param name="tokens"></param>
        ''' <returns></returns>
        <Extension> Public Function TryParse(tokens As Pointer(Of Token(Of ExpressionTokens)), getValue As GetValue, evaluate As IFuncEvaluate, ByRef funcStack As Boolean) As SimpleExpression
            Dim sep As New SimpleExpression
            Dim e As Token(Of ExpressionTokens)
            Dim o As Char
            Dim pre As Token(Of ExpressionTokens) = Nothing
            Dim func As FuncCaller = Nothing

            Do While Not tokens.EndRead
                Dim meta As MetaExpression = Nothing

                e = +tokens

                Select Case e.Type
                    Case ExpressionTokens.OpenBracket, ExpressionTokens.OpenStack
                        If pre Is Nothing Then  ' 前面不是一个未定义的标识符，则在这里是一个括号表达式
                            meta = New MetaExpression(TryParse(tokens, getValue, evaluate, False))
                        Else
                            Dim fstack As Boolean = True

                            func = New FuncCaller(pre.Text, evaluate)  ' Get function name, and then removes the last of the expression
                            o = sep.RemoveLast().Operator

                            Do While fstack   ' 在这里进行函数的参数列表的解析
                                Dim exp = TryParse(tokens, getValue, evaluate, fstack)
                                If exp.IsNullOrEmpty Then
                                    Exit Do
                                Else
                                    func.Params.Add(exp)
                                End If
                            Loop

                            meta = New Types.MetaExpression(AddressOf func.Evaluate)
                            ' o = If(tokens.EndRead, "+"c, (+tokens).Text.First)
                            ' meta.Operator = o
                            pre = Nothing
                            ' Call sep.Add(meta)

                            ' Continue Do
                        End If
                    Case ExpressionTokens.CloseStack, ExpressionTokens.CloseBracket, ExpressionTokens.Delimiter
                        Return sep ' 退出递归栈
                    Case ExpressionTokens.Number
                        meta = New Types.MetaExpression(Val(e.Text))
                    Case ExpressionTokens.UNDEFINE

                        Dim x As String = e.Text
                        meta = New Types.MetaExpression(Function() getValue(x))

                        If tokens.EndRead Then
                            pre = Nothing
                        Else
                            If tokens.Current.name = ExpressionTokens.Operator Then
                                pre = Nothing
                            Else
                                pre = e ' probably is a function name
                            End If
                        End If

                    Case ExpressionTokens.Operator
                        If String.Equals(e.Text, "-") Then

                            If Not sep.IsNullOrEmpty Then
                                If tokens.Current.Type = ExpressionTokens.Number Then
                                    meta = New Types.MetaExpression(-1 * Val((++tokens).Text))
                                Else
                                    Throw New SyntaxErrorException
                                End If
                            Else
                                ' 是一个负数
                                meta = New Types.MetaExpression(0)
                                meta.Operator = "-"c
                                Call sep.Add(meta)
                                Continue Do
                            End If
                        End If
                End Select

                If tokens.EndRead Then
                    meta.Operator = "+"c
                    Call sep.Add(meta)
                Else
                    o = (++tokens).Text.First  ' tokens++ 移动指针到下一个元素

                    If o = "!"c Then
                        Dim stackMeta = New MetaExpression(Function() VBMath.Factorial(meta.LEFT))

                        If tokens.EndRead Then
                            Call sep.Add(stackMeta)
                            Exit Do
                        Else
                            o = (++tokens).Text.First
                            If o = ")"c Then
                                ' 2017-1-26
                                ' 在这里是因为需要结束括号，进行退栈，所以指针会往回移动
                                ' 假若在这里是函数调用的结束符号右括号的话，假若这里是表达式的最后一个位置，则可能会出错
                                ' 现在这个错误已经被修复
                                'If Not tokens.EndRead Then
                                '    e = (-tokens)
                                'End If
                                stackMeta.Operator = "+"c
                                funcStack = False  ' 已经是括号的结束了，则退出栈
                                Call sep.Add(stackMeta)
                                'If Not tokens.EndRead Then
                                '    e = (-tokens)
                                'End If
                                Return sep
                            ElseIf o = ","c Then
                                meta.Operator = "+"c
                                Call sep.Add(meta)
                                ' e = (-tokens)
                                Exit Do ' 退出递归栈
                            Else
                                stackMeta.Operator = o
                                Call sep.Add(stackMeta)
                                Continue Do
                            End If
                        End If
                    ElseIf o = ","c Then
                        meta.Operator = "+"c
                        Call sep.Add(meta)
                        ' e = (-tokens)
                        funcStack = True

                        Exit Do ' 退出递归栈
                    ElseIf IsCloseStack(o) Then
                        meta.Operator = "+"c
                        Call sep.Add(meta)
                        funcStack = False
                        'If funcStack AndAlso Not tokens.EndRead Then
                        '    e = (-tokens)
                        'End If
                        Exit Do ' 退出递归栈
                    ElseIf IsOpenStack(o) Then
                        e = -tokens  ' 指针回退一步
                    End If

                    meta.Operator = o
                    Call sep.Add(meta)
                End If
            Loop

            Return sep
        End Function
    End Module
End Namespace
