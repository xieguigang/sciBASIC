Imports System.Text.RegularExpressions
Imports System.Text
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Mathematical.Helpers

Namespace Types

    ''' <summary>
    ''' A class object stand for a very simple mathematic expression that have no bracket or function.
    ''' It only contains limited operator such as +-*/\%!^ in it.
    ''' (一个用于表达非常简单的数学表达式的对象，在这个所表示的简单表达式之中不能够包含有任何括号或者函数，
    ''' 其仅包含有有限的计算符号在其中，例如：+-*/\%^!)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class SimpleExpression

        ''' <summary>
        ''' 在<see cref="SimpleExpression.Calculator"></see>之中由于移位操作的需要，需要使用类对象可以修改属性的特性来进行正常的计算，所以请不要修改为Structure类型
        ''' </summary>
        ''' <remarks></remarks>
        Protected Class MetaExpression

            <XmlAttribute> Public [Operator] As Char
            <XmlAttribute> Public LEFT As Double

            Public Overrides Function ToString() As String
                Return String.Format("{0} {1}", LEFT, [Operator])
            End Function
        End Class

        ''' <summary>
        ''' A simple expression can be view as a list collection of meta expression.
        ''' (可以将一个简单表达式看作为一个元表达式的集合)
        ''' </summary>
        ''' <remarks></remarks>
        Dim MetaList As New List(Of MetaExpression)

        ''' <summary>
        ''' Debugging displaying in VS IDE
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Overrides Function ToString() As String
            Return String.Join("", (From item In MetaList Let s As String = item.ToString Select s).ToArray)
        End Function

        ''' <summary>
        ''' Evaluate the specific simple expression class object.
        ''' (计算一个特定的简单表达式对象的值) 
        ''' </summary>
        ''' <returns>
        ''' Return the value of the specific simple expression object.
        ''' (返回目标简单表达式对象的值)
        ''' </returns>
        ''' <remarks></remarks>
        Public Function Evaluate() As Double
            If MetaList.Count = 1 Then 'When the list object only contains one element, that means this class object only stands for a number, return this number directly. 
                Return MetaList.First.LEFT
            Else
                Calculator("^")
                Calculator("*/\%")
                Calculator("+-")

                Return MetaList.First.LEFT
            End If
        End Function

        Private Sub Calculator(OperatorList As String)
            Dim LQuery As Generic.IEnumerable(Of MetaExpression) =
                From e As MetaExpression In MetaList
                Where InStr(OperatorList, e.Operator) > 0
                Select e 'Defines a LINQ query use for select the meta element that contains target operator.
            Dim Counts As Integer = LQuery.Count
            Dim M, NextElement As MetaExpression

            For index As Integer = 0 To MetaList.Count - 1  'Scan the expression object and do the calculation at the mean time
                If Counts = 0 OrElse MetaList.Count = 1 Then
                    Return      'No more calculation could be done since there is only one number in the expression, break at this situation.
                ElseIf OperatorList.IndexOf(MetaList(index).Operator) <> -1 Then 'We find a meta expression element that contains target operator, then we do calculation on this element and the element next to it.  
                    M = MetaList(index)  'Get current element and the element that next to him
                    NextElement = MetaList(index + 1)
                    NextElement.LEFT = Arithmetic.Evaluate(M.LEFT, NextElement.LEFT, M.Operator)  'Do some calculation of type target operator 
                    MetaList.RemoveAt(index) 'When the current element is calculated, it is no use anymore, we remove it
                    index -= 1  'Keep the reading position order

                    Counts -= 1  'If the target operator is position at the front side of the expression, using this flag will make the for loop exit when all of the target operator is calculated to improve the performance as no needs to scan all of the expression at this situation. 
                End If
            Next
        End Sub

        ''' <summary>
        ''' Evaluate the specific simple expression class object.
        ''' (计算一个特定的简单表达式对象的值) 
        ''' </summary>
        ''' <param name="e">A simple expression that will be evaluated.(待计算的简单表达式对象)</param>
        ''' <returns>
        ''' Return the value of the specific simple expression object.
        ''' (返回目标简单表达式对象的值)
        ''' </returns>
        ''' <remarks></remarks>
        Public Shared Narrowing Operator CType(e As SimpleExpression) As Double
            If e.MetaList.Count = 1 Then 'When the list object only contains one element, that means this class object only stands for a number, return this number directly. 
                Return e.MetaList.First.LEFT
            Else
                e.Calculator("^")
                e.Calculator("*/\%")
                e.Calculator("+-")

                Return e.MetaList.First.LEFT
            End If
        End Operator

        Public Shared Function Evaluate(expression As String) As Double
            Dim expression2 As SimpleExpression = expression
            Return expression2.Evaluate
        End Function

        ''' <summary>
        ''' A string constant that enumerate all of the arithmetic operators and treat the factoral operator 
        ''' as a part of number as well.
        ''' (一个枚举所有的基本运算符的字符串常数，并且将阶乘运算符也看作为数字的一部分) 
        ''' </summary>
        ''' <remarks></remarks>
        Const DOUBLE_NUMBER_REGX As String = Arithmetic.DOUBLE_NUMBER_REGX & "!{0,}"

        ''' <summary>
        ''' Convert a string mathematical expression to a simple expression class object.
        ''' (将一个字符串形式的数学表达式转换为一个'SimpleExpression'表达式对象)  
        ''' </summary>
        ''' <param name="expression">A string arithmetic expression to be converted.(一个待转换的数学表达式)</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Widening Operator CType(expression As String) As SimpleExpression
            Dim Numbers As MatchCollection = Regex.Matches(ClearOverlapOperator(expression), DOUBLE_NUMBER_REGX) 'Get all of the number that appears in this expression including factoral operator.
            Dim NewExpression As New SimpleExpression 'New object to return for this function
            Dim Last As Integer = Numbers.Count - 1  'The index of the last element in the list collection 'Numbers' 

            If Last = 0 Then   'The input expression is only a single number
                NewExpression.MetaList.Add(New MetaExpression With {.LEFT = Val(Numbers(0).Value)})
                Return NewExpression
            End If

            Dim s As String = Numbers(0).Value
            Dim p As Integer = Len(s) 'The current read position on the expression string
            Dim o As Char = expression.Chars(p)

            NewExpression.MetaList.Add(New MetaExpression With {.LEFT = Val(s), .Operator = o})
            p += 1

            For i As Integer = 1 To Last - 1  'Assume that the next char in each number string is the arithmetic operator character. 
                s = Numbers(i).Value
                If NewExpression.MetaList.Last.Operator = "-"c Then  'Horrible negative number controls!
                    p += Len(s) - 1
                    s = Mid(s, 2) 'This is not a negative number as the previous operator is a "-", we must remove the additional negative operaotr that was  matched success by the regular expression constant 'DOUBLE_NUMBER_REGX'
                Else
                    If expression.Chars(p) = "+"c Then
                        p += Len(s) + 1
                    Else
                        p += Len(s)
                    End If
                End If
                o = expression.Chars(p) 'Assume that the next char in each number string is the arithmetic operator character. 
                p += 1
                NewExpression.MetaList.Add(New MetaExpression With {.LEFT = Val(s), .Operator = o})
            Next

            If NewExpression.MetaList.Last.Operator = "-"c Then
                NewExpression.MetaList.Add(New MetaExpression With {.LEFT = Val(Mid(Numbers(Last).Value, 2))})  'add the last number in this expression that contains no operator.
            Else
                NewExpression.MetaList.Add(New MetaExpression With {.LEFT = Val(Numbers(Last).Value)})  'add the last number in this expression that contains no operator.
            End If

            Return NewExpression
        End Operator

        ''' <summary>
        ''' Type cast directly!
        ''' </summary>
        ''' <param name="expression"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Widening Operator CType(expression As StringBuilder) As SimpleExpression
            Return CType(expression.ToString, SimpleExpression)
        End Operator

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="s"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Friend Shared Function ClearOverlapOperator(ByRef s As String) As String
            Dim sBuilder As StringBuilder = New StringBuilder(value:="0+" & s)

            's = "0+" & sbr.ToString '0a=a; 0-a=-a; 0+a=a

            sBuilder.Replace("++", "+")
            sBuilder.Replace("--", "+")
            sBuilder.Replace("+-", "-")

            s = sBuilder.ToString

            Return s
        End Function

        ''' <summary>
        ''' Convert a string expression to double type value.
        ''' (将一个字符串表达式转换为双精度型的数字类型值)
        ''' </summary>
        ''' <param name="expression">
        ''' A string expression that use to stands for a number.
        ''' (一个用于求值的字符串形式的数字表达式)
        ''' </param>
        ''' <returns>
        ''' Return the value of the target evaluated string expression.
        ''' (返回目标字符串所代表的值)
        ''' </returns>
        ''' <remarks></remarks>
        Friend Shared Function Val(expression As String) As Double
            Dim FQuery As Generic.IEnumerable(Of Integer) =
                From c As Char In expression
                Where c = "!"c
                Select 1      'Calculate the number of the factoral operator in this expression using LINQ query. 
            Dim n As Integer = FQuery.Count 'EXEC the LINQ query for get the operators' count

            If expression.Chars(0) = "+"c Then
                expression = Mid(expression, 2)
            End If

            If n = 0 Then 'NONE
                Return Global.Microsoft.VisualBasic.Val(expression) 'if contains none of the factoral operaotr, return its value directly.
            Else 'Calculate the accumulation factorial calculation if the expression contains some factoral operator. 
                Dim result As Double = Global.Microsoft.VisualBasic.Val(expression)

                For i As Integer = 0 To n - 1
                    result = Arithmetic.Factorial(result, 0)
                Next

                Return result
            End If
        End Function
    End Class
End Namespace