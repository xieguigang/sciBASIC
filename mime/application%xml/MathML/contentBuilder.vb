#Region "Microsoft.VisualBasic::3066a2f79b0bf842e2ea90586865e071, mime\application%xml\MathML\contentBuilder.vb"

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

    '   Total Lines: 193
    '    Code Lines: 154 (79.79%)
    ' Comment Lines: 12 (6.22%)
    '    - Xml Docs: 66.67%
    ' 
    '   Blank Lines: 27 (13.99%)
    '     File Size: 7.67 KB


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
Imports Microsoft.VisualBasic.Serialization.JSON
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
        ''' </summary>
        ReadOnly stdMathFunc As Index(Of String) = {"abs", "cos", "sin", "tan", "max", "min", "exp", "log", "ln"}

        <Extension>
        Private Function parseInternal(apply As XmlElement) As MathExpression
            Dim [operator] As XmlElement

            ' 如果第一个元素是变量，常数或者apply表达式
            ' 则默认操作符为乘法操作？
            If apply.elements(Scan0).name Like symbols Then
                If apply.elements.Length < 3 Then
                    [operator] = New XmlElement With {.name = "times"}
                    apply.elements = {[operator]}.Join(apply.elements).ToArray
                Else
                    [operator] = apply.elements(1)
                    apply.elements = {
                        apply.elements(0),
                        apply.elements(2)
                    }
                End If
            Else
                [operator] = apply.elements(Scan0)
            End If

            If [operator].name Like stdMathFunc Then
                Return New MathFunctionExpression With {
                    .name = [operator].name,
                    .parameters = apply.elements _
                        .Skip(1) _
                        .Select(AddressOf ExpressionComponent) _
                        .ToArray
                }
            Else
                Dim left, right As MathExpression

                If apply.elements.Length = 2 Then
                    If apply.elements(Scan0).name = "minus" Then
                        apply.elements = {apply.elements(Scan0)} _
                            .Join({New XmlElement With {.name = "cn", .text = "0"}}) _
                            .Join(apply.elements.Skip(1)) _
                            .ToArray
                    Else
                        Throw New NotImplementedException(apply.elements(Scan0).name)
                    End If
                End If

                left = apply.elements(1).ExpressionComponent
                right = apply.elements(2).ExpressionComponent

                Dim exp As New BinaryExpression With {
                    .[operator] = [operator].name,
                    .applyleft = left,
                    .applyright = right
                }

                Return exp
            End If
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
