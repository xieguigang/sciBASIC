Imports System.Text

Namespace Helpers

    ''' <summary>
    ''' Mathematics function calculation engine
    ''' (数学函数计算引擎) 
    ''' </summary>
    ''' <remarks></remarks>
    Public Class [Function] : Inherits MemoryCollection(Of System.Func(Of Double, Double, Double))

        ''' <summary>
        ''' The mathematics calculation delegates collection with its specific name.
        ''' (具有特定名称的数学计算委托方法的集合) 
        ''' </summary>
        ''' <remarks></remarks>
        Protected Shared ReadOnly SystemPrefixFunctions As Dictionary(Of String, System.Func(Of Double, Double, Double)) =
            New Dictionary(Of String, System.Func(Of Double, Double, Double)) From {
                {"abs", Function(a As Double, b As Double) Math.Abs(a)},
                {"acos", Function(a As Double, b As Double) Math.Acos(a)},
                {"asin", Function(a As Double, b As Double) Math.Asin(a)},
                {"atan", Function(a As Double, b As Double) Math.Atan(a)},
                {"atan2", Function(a As Double, b As Double) Math.Atan2(a, b)},
                {"bigmul", Function(a As Double, b As Double) Math.BigMul(a, b)},
                {"ceiling", Function(a As Double, b As Double) Math.Ceiling(a)},
                {"cos", Function(a As Double, b As Double) Math.Cos(a)},
                {"cosh", Function(a As Double, b As Double) Math.Cosh(a)},
                {"exp", Function(a As Double, b As Double) Math.Exp(a)},
                {"floor", Function(a As Double, b As Double) Math.Floor(a)},
                {"ieeeremainder", Function(a As Double, b As Double) Math.IEEERemainder(a, b)},
                {"log", Function(a As Double, b As Double) Math.Log(a)},
                {"log10", Function(a As Double, b As Double) Math.Log10(a)},
                {"max", Function(a As Double, b As Double) Math.Max(a, b)},
                {"min", Function(a As Double, b As Double) Math.Min(a, b)},
                {"pow", Function(a As Double, b As Double) Math.Pow(a, b)},
                {"round", Function(a As Double, b As Double) Math.Round(a)},
                {"sign", Function(a As Double, b As Double) Math.Sign(a)},
                {"sin", Function(a As Double, b As Double) Math.Sin(a)},
                {"sinh", Function(a As Double, b As Double) Math.Sinh(a)},
                {"sqrt", Function(a As Double, b As Double) Math.Sqrt(a)},
                {"tan", Function(a As Double, b As Double) Math.Tan(a)},
                {"tanh", Function(a As Double, b As Double) Math.Tanh(a)},
                {"truncate", Function(a As Double, b As Double) Math.Truncate(a)},
                {"rnd", AddressOf Microsoft.VisualBasic.Mathematical.Helpers.Function.RND},
                {"int", Function(a As Double, b As Double) CType(a, Integer)},
                {String.Empty, Function(a As Double, b As Double) a}}  'If no function name, then return the paramenter a directly. 

        REM (经过以函数名长度降序排序的在VisualBasic中可用的函数名称的字符串集合) 
        Public ReadOnly Property FunctionList As String()
            Get
                Return _ObjectCacheList.ToArray
            End Get
        End Property

        ReadOnly _constants As Constants = New Constants

        Sub New()
            For Each item In SystemPrefixFunctions
                Call _InnerObjectDictionary.Add(item.Key, item.Value)
            Next
            Call MyBase._ObjectCacheList.AddRange((From s As String In SystemPrefixFunctions.Keys
                                                   Select s
                                                   Order By Len(s) Descending).ToArray)  'A string list of available function name in visualbasic, it was sort by the length of the each function name.
        End Sub

        ''' <summary>
        ''' Function name 'ieeeremainder' is the max length of the function name
        ''' </summary>
        ''' <remarks></remarks>
        Public Const FUNC_NAME_MAX_LENGTH As Integer = 13

        ''' <summary>
        ''' The paramenter of a,b of the user function
        ''' </summary>
        ''' <remarks></remarks>
        Const X As String = "{EDD16007-1CB8-4B3D-AE48-6FFB966A9C3B}"
        Const Y As String = "{408E5229-98E1-4D19-A92B-372176749B54}"

        ''' <summary>
        ''' Add a user function from the user input from the console or a text file.
        ''' </summary>
        ''' <param name="Name">The name of the user function.</param>
        ''' <param name="Expression">The expression of the user function.</param>
        ''' <remarks>
        ''' function [function name] expression
        ''' </remarks>
        Public Overloads Sub Add(Name As String, Expression As String)
            Dim [Function] As System.Func(Of Double, Double, Double) 'The function delegate

            Expression = Expression.Replace(Constant.DictData, _constants.Objects)
            Expression = Replace(Expression)
            [Function] = Function(DblX As Double, DblY As Double) As Double
                             Dim sBuilder As StringBuilder = New StringBuilder(Expression)

                             sBuilder.Replace(X, DblX)
                             sBuilder.Replace(Y, DblY)

                             Return Microsoft.VisualBasic.Mathematical.Expression.Evaluate(sBuilder.ToString)
                         End Function

            Call Add(Name.ToLower, [Function])
        End Sub

        ''' <summary>
        ''' Parsing the use function definition from the user input value on the console 
        ''' and then add it to the function dictionary.
        ''' (从终端上面输入的用户函数的申明语句中解析出表达式，然后将其加入到用户字典中)
        ''' </summary>
        ''' <param name="statement">[function name] expression</param>
        ''' <remarks>function [function name] expression</remarks>
        Friend Overloads Sub Add(statement As String)
            Dim Name As String = statement.Split.First
            'The expression may be contains space character, and it maybe split into sevral peaces.
            Call Add(Name, Mid(statement, Len(Name) + 2))
        End Sub

        Private Function Replace(expression As String) As String
            Call Replace2("x", X, expression)
            Call Replace2("y", Y, expression)

            Return expression
        End Function

        Private Sub Replace2(param As String, value As String, ByRef expression As String)
            Dim p As Integer = InStr(expression, param)
            Dim s As String 'Constant token
            Dim Left, Right As Char 'Left, right operator
            Dim sBuilder As StringBuilder = New StringBuilder(expression)

            Do While p
                Right = expression(p)
                Left = expression(p - 2)

                'if this tokens is surrounded by two operators then it is a paramenter name, not part 
                'of the function name or other user define constant name.
                If InStr(Constants.LEFT_OPERATOR_TOKENS, Left) AndAlso InStr(Constants.RIGHT_OPERATOR_TOKENS, Right) Then
                    s = Mid(expression, p - 1, 3)

                    Call sBuilder.Replace(s, Left & value & Right)
                    expression = sBuilder.ToString

                    p = InStr(p + Len(value), expression, param)
                Else
                    p = InStr(p + 1, expression, param)
                End If
            Loop
        End Sub

        ''' <summary>
        ''' This function return a random number, you can specific the boundary of the random number in the parameters. 
        ''' </summary>
        ''' <param name="UpBound">
        ''' If this parameter is empty or value is zero, then return the randome number between 0 and 1.
        ''' (如果这个参数为空或者其值为0，那么函数就会返回0和1之间的随机数)
        ''' </param>
        ''' <param name="LowBound"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Shared Function RND(LowBound As Double, UpBound As Double) As Double
            Call Randomize()
            If UpBound = 0 Then
                Return Internal_getRandomDouble()
            Else
                Return Internal_getRandomDouble() * (UpBound - LowBound) + LowBound
            End If
        End Function

        ''' <summary>
        ''' Gets a random number in the region of [0,1]. (获取一个[0,1]区间之中的随机数)
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Private Shared Function Internal_getRandomDouble() As Double
            Call Randomize()
            Dim n As Double = Microsoft.VisualBasic.Rnd * 100
CHECKS:     If n > 1 Then
                n /= 10
                GoTo CHECKS
            End If

            Return n
        End Function
    End Class
End Namespace