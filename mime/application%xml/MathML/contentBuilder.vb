Imports System.Runtime.CompilerServices
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
                If lambda.applyleft Like GetType(String) Then
                    left = lambda.applyleft.TryCast(Of String)
                Else
                    left = $"( {lambda.applyleft.TryCast(Of BinaryExpression).ToString} )"
                End If
            End If

            If Not lambda.applyright Is Nothing Then
                If lambda.applyright Like GetType(String) Then
                    right = lambda.applyright.TryCast(Of String)
                Else
                    right = $"( {lambda.applyright.TryCast(Of BinaryExpression).ToString} )"
                End If
            End If

            Return $"{left} {operators(lambda.[operator]).Description} {right}"
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
                                    .Trim(" "c, ASCII.TAB)
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

        <Extension>
        Private Function parseInternal(apply As XmlElement) As BinaryExpression
            Dim [operator] = apply.elements(Scan0)
            Dim left, right As [Variant](Of BinaryExpression, String)
            Dim applys = apply.getElementsByTagName("apply").ToArray

            If applys.Length = 1 Then
                If apply.elements(1).name = "apply" Then
                    left = applys(Scan0).parseInternal
                    right = apply.elements(2).text.Trim(" "c, ASCII.TAB)
                Else
                    left = apply.elements(1).text.Trim(" "c, ASCII.TAB)
                    right = applys(Scan0).parseInternal
                End If
            ElseIf applys.Length = 2 Then
                left = applys(Scan0).parseInternal
                right = applys(1).parseInternal
            Else
                left = apply.elements(1).text.Trim(" "c, ASCII.TAB)
                right = apply.elements(2).text.Trim(" "c, ASCII.TAB)
            End If

            Dim exp As New BinaryExpression With {
                .[operator] = [operator].name,
                .applyleft = left,
                .applyright = right
            }

            Return exp
        End Function
    End Module
End Namespace