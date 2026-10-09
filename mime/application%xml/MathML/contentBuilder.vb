#Region "Microsoft.VisualBasic::e35885b897a4f182a9be68ec3ba8b5d7, mime\application%xml\MathML\contentBuilder.vb"

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

    '   Total Lines: 204
    '    Code Lines: 163 (79.90%)
    ' Comment Lines: 13 (6.37%)
    '    - Xml Docs: 61.54%
    ' 
    '   Blank Lines: 28 (13.73%)
    '     File Size: 8.09 KB


    '     Module ContentBuilder
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ExpressionComponent, getTextSymbol, parseInternal, ParseXml, safeGetOperator
    '                   SimplyOperator, ToString, TrimWhitespace
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Data
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text

Namespace MathML

    Public Module ContentBuilder

        ReadOnly operators As Dictionary(Of String, mathOperators)

        Sub New()
            operators = Enums(Of mathOperators).ToDictionary(Function(t) t.ToString)

            ' add simples
            Call operators.Add("^", mathOperators.power)
            Call operators.Add("+", mathOperators.plus)
            Call operators.Add("-", mathOperators.minus)
            Call operators.Add("*", mathOperators.times)
            Call operators.Add("/", mathOperators.divide)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function SimplyOperator(text As String) As String
            Return operators(text).Description
        End Function

        Friend Function ToString(lambda As BinaryExpression) As String
            Dim left As String = ""
            Dim right As String = ""

            If Not lambda.applyleft Is Nothing Then
                If TypeOf lambda.applyleft Is BinaryExpression Then
                    left = $"( {lambda.applyleft} )"
                Else
                    left = lambda.applyleft.ToString
                End If
            End If

            If Not lambda.applyright Is Nothing Then
                If TypeOf lambda.applyright Is SymbolExpression Then
                    right = $"{lambda.applyright}"
                Else
                    right = lambda.applyright.ToString
                End If
            End If

            If lambda.applyright Is Nothing Then
                Return $"({safeGetOperator(lambda)} {left})"
            Else
                Return $"({left} {safeGetOperator(lambda)} {right})"
            End If
        End Function

        Private Function safeGetOperator(lambda As BinaryExpression) As String
            If operators.ContainsKey(lambda.operator) Then
                Return operators(lambda.operator).Description
            ElseIf "+-/*".IndexOf(lambda.operator) > -1 Then
                Return lambda.operator
            Else
                Throw New InvalidExpressionException(lambda.operator)
            End If
        End Function

        ''' <summary>
        ''' 因为反序列化存在一个元素顺序的bug，所以在这里不可以通过反序列化来进行表达式的解析
        ''' </summary>
        ''' <param name="mathML"></param>
        ''' <returns></returns>
        ''' 
        <Extension>
        Public Function ParseXml(mathML As XmlElement) As LambdaExpression
            Dim lambdaElement As XmlElement = mathML.getElementsByTagName("lambda").FirstOrDefault
            Dim parameters As String()

            If lambdaElement Is Nothing Then
                Return Nothing
            Else
                parameters = lambdaElement _
                    .getElementsByTagName("bvar") _
                    .Select(Function(b)
                                Return b.getElementsByTagName("ci") _
                                    .First.text _
                                    .TrimWhitespace
                            End Function) _
                    .ToArray
                lambdaElement = lambdaElement.getElementsByTagName("apply").FirstOrDefault
            End If

            ' Call Console.WriteLine(lambdaElement.GetJson(indent:=True))

            If lambdaElement Is Nothing Then
                Return New LambdaExpression With {
                    .parameters = parameters,
                    .lambda = Nothing
                }
            Else
                Return New LambdaExpression With {
                    .parameters = parameters,
                    .lambda = lambdaElement.parseInternal
                }
            End If
        End Function

        ReadOnly symbols As Index(Of String) = {"apply", "ci", "cn"}
        ''' <summary>
        ''' a list of standard math function
        ''' （sqrt/root 为一元函数：root 的 &lt;degree&gt; 子元素在解析时被忽略，按平方根处理）
        ''' </summary>
        ReadOnly stdMathFunc As Index(Of String) =
            {"abs", "cos", "sin", "tan", "cot", "sec", "csc",
             "max", "min", "exp", "log", "ln", "sqrt", "root",
             "arcsin", "arccos", "arctan", "sinh", "cosh", "tanh"}

        ''' <summary>
        ''' 可以折叠为左结合二元表达式树的运算符名称集合。
        ''' 包括四则运算、幂运算以及关系运算符（eq/leq/geq/neq），
        ''' 例如 apply(plus, a, b, c) => ((a + b) + c)。
        ''' </summary>
        ReadOnly chainableOperators As Index(Of String) =
            {"plus", "minus", "times", "divide", "power", "eq", "leq", "geq", "neq"}

        ''' <summary>判断元素名是否为已知的二元/n元运算符或关系符。</summary>
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function isChainable(name As String) As Boolean
            Return name Like chainableOperators
        End Function

        ''' <summary>
        ''' 将 n 元同类运算符操作数序列折叠为左结合的二元表达式树。
        ''' </summary>
        Private Function foldChain([operator] As String, operands As MathExpression()) As MathExpression
            Dim exp As MathExpression = operands(Scan0)

            For i As Integer = 1 To operands.Length - 1
                exp = New BinaryExpression With {
                    .[operator] = [operator],
                    .applyleft = exp,
                    .applyright = operands(i)
                }
            Next

            Return exp
        End Function

        ''' <summary>
        ''' 解析 &lt;apply&gt; 表达式节点为表达式树。
        ''' 
        ''' 支持：
        '''   - 二元与 n 元运算链（自动折叠为左结合二元树，修复旧实现丢弃多余操作数的缺陷）；
        '''   - 一元负号 minus（自动补 0 - x）；
        '''   - 关系运算符 eq/leq/geq/neq；
        '''   - 标准数学函数（见 <see cref="stdMathFunc"/>，root 忽略 degree 子元素）；
        '''   - 隐式乘法（apply 首元素为操作数时默认 times）。
        ''' </summary>
        <Extension>
        Private Function parseInternal(apply As XmlElement) As MathExpression
            Dim [operator] As XmlElement

            ' 如果第一个元素是变量，常数或者apply表达式
            ' 则判断第二个元素是否为运算符：是则按 [操作数, 运算符, 操作数...] 处理，
            ' 否则默认为隐式乘法操作
            If apply.elements(Scan0).name Like symbols Then
                If apply.elements.Length >= 3 AndAlso isChainable(apply.elements(1).name) Then
                    [operator] = apply.elements(1)
                    apply.elements = {[operator]} _
                        .Join(apply.elements.Where(Function(e, i) i <> 1)) _
                        .ToArray
                Else
                    [operator] = New XmlElement With {.name = "times"}
                    apply.elements = {[operator]}.Join(apply.elements).ToArray
                End If
            Else
                [operator] = apply.elements(Scan0)
            End If

            ' 标准数学函数：apply(func, args...)
            If [operator].name Like stdMathFunc Then
                Dim args As XmlElement() = apply.elements _
                    .Skip(1) _
                    .Where(Function(e) [operator].name <> "root" OrElse e.name <> "degree") _
                    .ToArray

                Return New MathFunctionExpression With {
                    .name = [operator].name,
                    .parameters = args _
                        .Select(AddressOf ExpressionComponent) _
                        .ToArray
                }
            End If

            ' 二元/n 元运算符与关系运算符：apply(op, a, b, ...)
            If isChainable([operator].name) Then
                Dim operands As MathExpression() = apply.elements _
                    .Skip(1) _
                    .Select(AddressOf ExpressionComponent) _
                    .ToArray

                If operands.Length = 1 Then
                    ' 一元 minus：0 - x（取负）
                    If [operator].name = "minus" Then
                        Return New BinaryExpression With {
                            .[operator] = "minus",
                            .applyleft = New SymbolExpression With {.text = "0", .isNumericLiteral = True},
                            .applyright = operands(Scan0)
                        }
                    End If

                    Throw New InvalidExpressionException(
                        $"apply/{[operator].name} requires at least 2 operands!")
                End If

                Return foldChain([operator].name, operands)
            End If

            Throw New NotImplementedException($"Unsupported MathML apply operator: {[operator].name}")
        End Function

        <Extension>
        Private Function ExpressionComponent(element As XmlElement) As MathExpression
            If element.name = "apply" Then
                Return element.parseInternal
            Else
                Return element.getTextSymbol
            End If
        End Function

        <Extension>
        Private Function getTextSymbol(element As XmlElement) As SymbolExpression
            Dim value As String = element.text.TrimWhitespace

            If element.name = "ci" Then
                Return New SymbolExpression With {.text = value}
            ElseIf element.name = "cn" Then
                If element.attributes.TryGetValue("type") = "rational" Then
                    Dim a = element.elements(0).text.TrimWhitespace
                    Dim b = element.elements(2).text.TrimWhitespace

                    Return New SymbolExpression With {.text = $"{a}/{b}", .isNumericLiteral = True}
                Else
                    Return New SymbolExpression With {.text = value, .isNumericLiteral = True}
                End If
            Else
                Throw New NotImplementedException(element.ToString)
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Private Function TrimWhitespace(str As String) As String
            Return Strings.Trim(str).Trim(" "c, ASCII.TAB, ASCII.CR, ASCII.LF)
        End Function
    End Module
End Namespace
