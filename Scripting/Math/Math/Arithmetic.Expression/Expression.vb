Imports System.Text
Imports Microsoft.VisualBasic.Mathematical.Helpers

''' <summary>
''' Expression Evaluation Engine
''' </summary>
''' <remarks></remarks>
Public Module Expression

    Private Const OPERATORS As String = Arithmetic.OPERATORS & ","

    Friend ReadOnly Constant As Helpers.Constants = New Helpers.Constants
    Friend ReadOnly Variables As Helpers.Variable = New Variable
    Friend ReadOnly Functions As Helpers.Function = New [Function]

    Public Const SIMPLE_BRACKET_EXPRESSION As String = "\([^(^)^,]+?\)"
    Public Const FUNCTION_CALLING As String = "[0-9a-zA-Z_]+\([^(^)]+?\)"

    ''' <summary>
    ''' Evaluate the a specific mathematics expression string to a double value, the functions, constants, 
    ''' bracket pairs can be include in this expression but the function are those were originally exists 
    ''' in the visualbasic. I'm sorry for this...
    ''' (对一个包含有函数、常数和匹配的括号的一个复杂表达式进行求值，但是对于表达式中的函数而言：仅能够使用在
    ''' VisualBaisc语言中存在的有限的几个数学函数。)  
    ''' </summary>
    ''' <param name="expression"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Evaluate(expression As String) As Double
        expression = Replace(expression, Constant.DictData, Constant.Objects) 'Replace the constant value
        expression = Replace(expression, Variables.DictData, Variables.Objects)

        Dim LBStack As New Stack(Of Integer) 'The position stack of left bracket character. We push the reading position to this stack when we met a character '(' left bracket, pop up then position when we met a character ')' right bracket in the expression string. 
        Dim r As Microsoft.VisualBasic.Mathematical.Types.SimpleExpression, se As String 'se' is a simple expression string
        Dim LBLocation As Integer
        Dim Expression2 As StringBuilder = New StringBuilder(value:=expression)
        Dim a, b As Double 'Parameter a, b of a function 
        Dim p As Integer, CalcFunction As System.Action(Of String, Double, Double, Integer) = Sub(Func As String, pa As Double, pb As Double, d As Integer)
                                                                                                  pa = Functions.DictData(Func)(pa, pb)
                                                                                                  LBLocation -= Len(Func) + d
                                                                                                  se = Mid(Expression2.ToString, LBLocation, p - LBLocation + 2)
                                                                                                  Expression2.Replace(se, pa)
                                                                                                  p -= Len(se)
                                                                                              End Sub

        Do While p <= Expression2.Length - 1  'Scaning the whole expression, the loop var 'p' is the reading position on the expression string
            If Expression2.Chars(p) = "("c Then
                LBStack.Push(item:=p + 1)
            ElseIf Expression2.Chars(p) = ")"c Then 'The expression string between two paired bracket is a mostly simple expression.
                LBLocation = LBStack.Pop
                se = Mid(Expression2.ToString, LBLocation + 1, p - LBLocation)
                r = SimpleParser.TryParse(se)
                LBLocation += 1

                If LBLocation < Expression2.Length AndAlso OPERATORS.IndexOf(Expression2.Chars(LBLocation)) = -1 Then  'The previous character not is a operator, then it maybe a function name. 
                    Dim f = GetFunctionName(Expression2, LBLocation)   'Get the function name
                    Call CalcFunction(f, r.Evaluate, 0, 1)  'Calculate the function with only one paramenter. 
                Else
                    Expression2.Replace("(" & se & ")", r.Evaluate)
                    p -= Len(se)
                End If
            ElseIf Expression2.Chars(p) = ","c Then 'Meet a parameter seperator of a function, that means we should calculate this parameter as a simple expression as the bracket calculation has been done before. 
                LBLocation = LBStack.Peek 'We get a function paramenter 'a', it maybe a simple expression, do some calculation for this parameter. 
                se = Mid(Expression2.ToString, LBLocation + 1, p - LBLocation)
                a = SimpleParser.TryParse(se).Evaluate
                LBStack.Push(item:=p + 1)  'Push the position of seperator character ',' to the stack
                p += 1
                'Calculate the function parameter 'b'
                Dim LBStack2 As New Stack(Of Integer)
                Do While p <= Expression2.Length - 1   'Using a loop to get the paramenter 'b'
                    If Expression2.Chars(p) = "("c Then
                        LBStack2.Push(item:=p + 1)
                    ElseIf Expression2.Chars(p) = ")"c Then 'The expression string between two paired bracket is a mostly simple expression.
                        If LBStack2.Count = 0 Then Exit Do Else LBStack2.Pop()
                    End If

                    p += 1
                Loop
                LBLocation = LBStack.Pop 'Parse the pramenter 'b'
                se = Mid(Expression2.ToString, LBLocation + 1, p - LBLocation)  'Paramenter 'b' maybe a complex expression. 
                b = Evaluate(se) 'Get the value of the parameter 'b'
                'Calculate the value of the function
                LBLocation = LBStack.Pop
                Dim f = GetFunctionName(Expression2, LBLocation)   'Get the function name
                Call CalcFunction(f, a, b, 0)  'Calculate the function with two paramenters. 
            End If

            p += 1
        Loop

        'No more bracket pairs or any function in the expression, it only left a simple expression, evaluate this simple expression and return the result.  
        Return SimpleParser.TryParse(Expression2.ToString).Evaluate
    End Function

    ''' <summary>
    ''' Get the function name from a expression back from a specific left bracket position. 
    ''' (从表达式的某一个特定的左侧括号的位置往回解析其所处的函数的函数名成)
    ''' </summary>
    ''' <param name="expression">Target expression</param>
    ''' <param name="LBLocation">
    ''' The position of the left bracket char which has a function name on its left side.
    ''' (在其左侧有函数名的某一个指定的左括号在字符串中的位置)
    ''' </param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Friend Function GetFunctionName(expression As StringBuilder, LBLocation As Integer) As String
        Dim p As Integer
        If LBLocation < Microsoft.VisualBasic.Mathematical.Helpers.[Function].FUNC_NAME_MAX_LENGTH Then
            p = LBLocation
        Else
            p = Microsoft.VisualBasic.Mathematical.Helpers.[Function].FUNC_NAME_MAX_LENGTH
        End If
        Dim Tokens As String() = Mid(expression.ToString, LBLocation - p + 1, p - 1).Split("("c)
        Dim s As String

        If String.IsNullOrEmpty(Tokens.Last) Then
            s = Tokens(Tokens.Count - 2)
        Else
            s = Tokens.Last
        End If

        For Each name As String In Functions.Objects
            If InStr(s, name) > 0 Then
                Return name
            End If
        Next

        Return String.Empty
    End Function

    Public Sub SetVariable(Name As String, expression As String)
        Call Mathematical.Expression.Variables.Set(Name, expression)
    End Sub

    Public Sub AddConstant(Name As String, expression As String)
        Call Mathematical.Expression.Constant.Add(Name, expression)
    End Sub

End Module
