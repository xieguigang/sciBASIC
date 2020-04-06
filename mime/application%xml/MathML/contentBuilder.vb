#Region "Microsoft.VisualBasic::7220c76bb3218d0ca113b0f9a9c0fc32, mime\application%xml\MathML\contentBuilder.vb"

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

'     Module ContentBuilder
' 
'         Function: getTextSymbol, parseInternal, ParseXml, SimplyOperator, ToString
'                   TrimWhitespace
' 
' 
' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text

Namespace MathML

    Public Module ContentBuilder

        ReadOnly operators As Dictionary(Of String, mathOperators) = Enums(Of mathOperators).ToDictionary(Function(t) t.ToString)

        Public Function SimplyOperator(text As String) As String
            Return operators(text).Description
        End Function

        Friend Function ToString(lambda As BinaryExpression) As String
            Dim left As String = ""
            Dim right As String = ""

            If Not lambda.applyleft Is Nothing Then
                If lambda.applyleft Like GetType(SymbolExpression) Then
                    left = lambda.applyleft.TryCast(Of SymbolExpression).ToString
                Else
                    left = $"( {lambda.applyleft.TryCast(Of BinaryExpression).ToString} )"
                End If
            End If

            If Not lambda.applyright Is Nothing Then
                If lambda.applyright Like GetType(SymbolExpression) Then
                    right = lambda.applyright.TryCast(Of SymbolExpression).ToString
                Else
                    right = $"( {lambda.applyright.TryCast(Of BinaryExpression).ToString} )"
                End If
            End If

            If lambda.applyright Is Nothing Then
                Return $"{operators(lambda.[operator]).Description} {left}"
            Else
                Return $"{left} {operators(lambda.[operator]).Description} {right}"
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

            If lambdaElement Is Nothing Then
                Return Nothing
            Else
                Return New LambdaExpression With {
                    .parameters = parameters,
                    .lambda = lambdaElement.parseInternal
                }
            End If
        End Function

        ReadOnly symbols As Index(Of String) = {"apply", "ci", "cn"}

        <Extension>
        Private Function parseInternal(apply As XmlElement) As BinaryExpression
            Dim [operator] As XmlElement
            Dim left, right As [Variant](Of BinaryExpression, SymbolExpression)
            Dim applys = apply.getElementsByTagName("apply").ToArray

            If apply.elements(Scan0).name Like symbols Then
                [operator] = New XmlElement With {.name = "times"}
                apply.elements = {[operator]}.Join(apply.elements).ToArray
            Else
                [operator] = apply.elements(Scan0)
            End If

            If applys.Length = 1 Then
                If apply.elements.Length = 2 Then
                    If [operator].name = "minus" Then
                        left = New SymbolExpression With {.text = 0, .isNumericLiteral = True}
                        right = apply.elements(1).parseInternal
                    Else
                        Throw New NotImplementedException
                    End If

                Else
                    If apply.elements(1).name = "apply" Then
                        left = applys(Scan0).parseInternal
                        right = apply.elements(2).getTextSymbol
                    Else
                        left = apply.elements(1).getTextSymbol
                        right = applys(Scan0).parseInternal
                    End If
                End If
            ElseIf applys.Length = 2 Then
                left = applys(Scan0).parseInternal
                right = applys(1).parseInternal
            Else
                left = apply.elements(1).getTextSymbol

                If apply.elements.Length > 2 Then
                    right = apply.elements(2).getTextSymbol
                Else
                    right = Nothing
                End If
            End If

            Dim exp As New BinaryExpression With {
                .[operator] = [operator].name,
                .applyleft = left,
                .applyright = right
            }

            Return exp
        End Function

        <Extension>
        Private Function getTextSymbol(element As XmlElement) As SymbolExpression
            Dim value As String = element.text.TrimWhitespace

            If element.name = "ci" Then
                Return New SymbolExpression With {.text = value}
            ElseIf element.name = "cn" Then
                Return New SymbolExpression With {.text = value, .isNumericLiteral = True}
            Else
                Throw New NotImplementedException(element.ToString)
            End If
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Private Function TrimWhitespace(str As String) As String
            Return str.Trim(" "c, ASCII.TAB, ASCII.CR, ASCII.LF)
        End Function
    End Module
End Namespace
