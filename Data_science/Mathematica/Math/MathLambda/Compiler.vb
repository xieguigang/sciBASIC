Imports System.Linq.Expressions
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.MIME.application.xml
Imports ML = Microsoft.VisualBasic.MIME.application.xml.MathML.BinaryExpression
Imports MLlambda = Microsoft.VisualBasic.MIME.application.xml.MathML.LambdaExpression

''' <summary>
''' mathML -> lambda -> linq expression -> compile VB lambda
''' </summary>
Public Module Compiler

    Public Function CreateLambda(lambda As MLlambda) As LambdaExpression
        Dim parameters = lambda.parameters.Select(Function(name) Expression.Parameter(GetType(Double), name)).ToDictionary(Function(par) par.Name)
        Dim body As Expression = CreateBinary(lambda.lambda, parameters)
        Dim expr As LambdaExpression = Expression.Lambda(body, lambda.parameters.Select(Function(par) parameters(par)).ToArray)

        Return expr
    End Function

    Private Function CreateBinary(member As [Variant](Of ML, String), parameters As Dictionary(Of String, ParameterExpression)) As Expression
        If member Like GetType(String) Then
            Return parameters(member.TryCast(Of String))
        Else
            Return CreateBinary(member.TryCast(Of ML), parameters)
        End If
    End Function

    Private Function CreateBinary(member As ML, parameters As Dictionary(Of String, ParameterExpression)) As Expression
        Select Case MathML.ContentBuilder.SimplyOperator(member.operator)
            Case "+" : Return Expression.Add(CreateBinary(member.applyleft, parameters), CreateBinary(member.applyright, parameters))
            Case "-" : Return Expression.Subtract(CreateBinary(member.applyleft, parameters), CreateBinary(member.applyright, parameters))
            Case "*" : Return Expression.Multiply(CreateBinary(member.applyleft, parameters), CreateBinary(member.applyright, parameters))
            Case "/" : Return Expression.Divide(CreateBinary(member.applyleft, parameters), CreateBinary(member.applyright, parameters))
            Case "^" : Return Expression.Power(CreateBinary(member.applyleft, parameters), CreateBinary(member.applyright, parameters))
            Case Else
                Throw New InvalidCastException
        End Select
    End Function
End Module
