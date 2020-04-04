Imports System.Linq.Expressions
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.application.xml
Imports ML = Microsoft.VisualBasic.MIME.application.xml.MathML.BinaryExpression
Imports MLLambda = Microsoft.VisualBasic.MIME.application.xml.MathML.LambdaExpression
Imports MLSymbol = Microsoft.VisualBasic.MIME.application.xml.MathML.SymbolExpression

''' <summary>
''' mathML -> lambda -> linq expression -> compile VB lambda
''' </summary>
Public Module MathMLCompiler

    Public Function CreateLambda(lambda As MLLambda) As LambdaExpression
        Dim parameters = lambda.parameters.Select(Function(name) Expression.Parameter(GetType(Double), name)).ToDictionary(Function(par) par.Name)
        Dim body As Expression = CreateBinary(lambda.lambda, parameters)
        Dim expr As LambdaExpression = Expression.Lambda(body, lambda.parameters.Select(Function(par) parameters(par)).ToArray)

        Return expr
    End Function

    Private Function CreateBinary(member As [Variant](Of ML, MLSymbol), parameters As Dictionary(Of String, ParameterExpression)) As Expression
        If member Like GetType(MLSymbol) Then
            With member.TryCast(Of MLSymbol)
                If .isNumericLiteral Then
                    Return Expression.Constant(Val(.text), GetType(Double))
                Else
                    Return parameters(.text)
                End If
            End With
        Else
            Return CreateBinary(member.TryCast(Of ML), parameters)
        End If
    End Function

    Private Function CreateBinary(member As ML, parameters As Dictionary(Of String, ParameterExpression)) As Expression
        Select Case MathML.ContentBuilder.SimplyOperator(member.operator)
            Case "+" : Return Expression.Add(CreateBinary(member.applyleft, parameters), CreateBinary(member.applyright, parameters))
            Case "-"
                If member.applyright Is Nothing Then
                    Return Expression.Negate(CreateBinary(member.applyleft, parameters))
                Else
                    Return Expression.Subtract(
                        CreateBinary(member.applyleft, parameters),
                        CreateBinary(member.applyright, parameters)
                    )
                End If
            Case "*" : Return Expression.Multiply(CreateBinary(member.applyleft, parameters), CreateBinary(member.applyright, parameters))
            Case "/" : Return Expression.Divide(CreateBinary(member.applyleft, parameters), CreateBinary(member.applyright, parameters))
            Case "^" : Return Expression.Power(CreateBinary(member.applyleft, parameters), CreateBinary(member.applyright, parameters))
            Case Else
                Throw New InvalidCastException
        End Select
    End Function
End Module
