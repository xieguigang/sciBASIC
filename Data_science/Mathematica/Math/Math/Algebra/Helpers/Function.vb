#Region "Microsoft.VisualBasic::8331afb9b91f31f997916549edb4c860, Data_science\Mathematica\Math\Math\Algebra\Helpers\Function.vb"

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

    '   Total Lines: 205
    '    Code Lines: 0
    ' Comment Lines: 181
    '   Blank Lines: 24
    '     File Size: 10.95 KB


    ' 
    ' /********************************************************************************/

#End Region

'Imports System.Text

'Namespace BasicR.Helpers

'    ''' <summary>
'    ''' Mathematics function calculation engine
'    ''' (数学函数计算引擎) 
'    ''' </summary>
'    ''' <remarks></remarks>
'    Public Class [Function]

'        ''' <summary>
'        ''' The mathematics calculation delegates collection with its specific name.
'        ''' (具有特定名称的数学计算委托方法的集合) 
'        ''' </summary>
'        ''' <remarks></remarks>
'        Public Shared ReadOnly Functions As Dictionary(Of String, System.Func(Of MATRIX, MATRIX, MATRIX)) =
'            New Dictionary(Of String, System.Func(Of MATRIX, MATRIX, MATRIX)) From {
'                {"abs", Function(a As MATRIX, b As MATRIX) MatrixFunction(a, AddressOf sys.Abs)},
'                {"acos", Function(a As MATRIX, b As MATRIX) MatrixFunction(a, AddressOf sys.Acos)},
'                {"asin", Function(a As MATRIX, b As MATRIX) MatrixFunction(a, AddressOf sys.Asin)},
'                {"atan", Function(a As MATRIX, b As MATRIX) MatrixFunction(a, AddressOf sys.Atan)},
'                {"atan2", Function(a As MATRIX, b As MATRIX) MatrixFunction(a, b, AddressOf sys.Atan2)},
'                {"bigmul", Function(a As MATRIX, b As MATRIX) MatrixFunction(a, b, AddressOf sys.BigMul)},
'                {"ceiling", Function(a As MATRIX, b As MATRIX) MatrixFunction(a, AddressOf sys.Ceiling)},
'                {"cos", Function(a As MATRIX, b As MATRIX) MatrixFunction(a, AddressOf sys.Cos)},
'                {"cosh", Function(a As MATRIX, b As MATRIX) MatrixFunction(a, AddressOf sys.Cosh)},
'                {"exp", Function(a As MATRIX, b As MATRIX) MatrixFunction(a, AddressOf sys.Exp)},
'                {"floor", Function(a As MATRIX, b As MATRIX) MatrixFunction(a, AddressOf sys.Floor)},
'                {"ieeeremainder", Function(a As MATRIX, b As MATRIX) MatrixFunction(a, b, AddressOf sys.IEEERemainder)},
'                {"log", Function(a As MATRIX, b As MATRIX) MatrixFunction(a, AddressOf sys.Log)},
'                {"log10", Function(a As MATRIX, b As MATRIX) MatrixFunction(a, AddressOf sys.Log10)},
'                {"max", Function(a As MATRIX, b As MATRIX) MatrixFunction(a, b, AddressOf sys.Max)},
'                {"min", Function(a As MATRIX, b As MATRIX) MatrixFunction(a, b, AddressOf sys.Min)},
'                {"pow", Function(a As MATRIX, b As MATRIX) MatrixFunction(a, b, AddressOf sys.Pow)},
'                {"round", Function(a As MATRIX, b As MATRIX) MatrixFunction(a, AddressOf sys.Round)},
'                {"sign", Function(a As MATRIX, b As MATRIX) MatrixFunction(a, AddressOf sys.Sign)},
'                {"sin", Function(a As MATRIX, b As MATRIX) MatrixFunction(a, AddressOf sys.Sin)},
'                {"sinh", Function(a As MATRIX, b As MATRIX) MatrixFunction(a, AddressOf sys.Sinh)},
'                {"sqrt", Function(a As MATRIX, b As MATRIX) MatrixFunction(a, AddressOf sys.Sqrt)},
'                {"tan", Function(a As MATRIX, b As MATRIX) MatrixFunction(a, AddressOf sys.Tan)},
'                {"tanh", Function(a As MATRIX, b As MATRIX) MatrixFunction(a, AddressOf sys.Tanh)},
'                {"truncate", Function(a As MATRIX, b As MATRIX) MatrixFunction(a, AddressOf sys.Truncate)},
'                {"int", Function(a As MATRIX, b As MATRIX) MatrixFunction(a, AddressOf Int)},
'                {String.Empty, Function(a As MATRIX, b As MATRIX) a}}  'If no function name, then return the paramenter a directly. 

'        ''' <summary>
'        ''' A string list of available function name in visualbasic, it was sort by the length of the each function name.
'        ''' (经过以函数名长度降序排序的在VisualBasic中可用的函数名称的字符串集合) 
'        ''' </summary>
'        ''' <remarks></remarks>
'        Public Shared ReadOnly FunctionList As List(Of String) = (
'            From s As String In Functions.Keys
'            Select s
'            Order By Len(s) Descending).AsList

'        ''' <summary>
'        ''' Function name 'ieeeremainder' is the max length of the function name
'        ''' </summary>
'        ''' <remarks></remarks>
'        Public Const FUNC_NAME_MAX_LENGTH As Integer = 13

'        Public Shared Function MatrixFunction(MAT As MATRIX, [Function] As System.Func(Of Double, Double)) As MATRIX
'            Dim result As MATRIX = New MATRIX(Width:=MAT.Width, Height:=MAT.Height)
'            For row As Integer = 0 To MAT.Height - 1
'                For col As Integer = 0 To MAT.Width - 1
'                    result(row, col) = [Function](MAT(row, col))
'                Next
'            Next
'            Return result
'        End Function

'        ''' <summary>
'        ''' A must have the same size with B!
'        ''' </summary>
'        ''' <param name="A"></param>
'        ''' <param name="B"></param>
'        ''' <param name="Function"></param>
'        ''' <returns></returns>
'        ''' <remarks></remarks>
'        Public Shared Function MatrixFunction(A As MATRIX, B As MATRIX, [Function] As System.Func(Of Double, Double, Double)) As MATRIX
'            Dim result As MATRIX = New MATRIX(Width:=A.Width, Height:=A.Height)
'            For row As Integer = 0 To A.Height - 1
'                For col As Integer = 0 To A.Width - 1
'                    result(row, col) = [Function](A(row, col), B(row, col))
'                Next
'            Next
'            Return result
'        End Function

'        ''' <summary>
'        ''' Add a user function for your program code
'        ''' </summary>
'        ''' <param name="Name">The name of the user function.</param>
'        ''' <param name="Function">Function delegate.</param>
'        ''' <remarks></remarks>
'        Public Shared Sub Add(Name As String, [Function] As System.Func(Of MATRIX, MATRIX, MATRIX))
'            Name = Name.ToLower
'            Functions.Add(Name, [Function])

'            Call FunctionList.Clear()
'            Dim Query As Generic.IEnumerable(Of String) =
'                From s As String In Functions.Keys
'                Select s
'                Order By Len(s) Descending   'Regenerate the function name index list.
'            Call FunctionList.AddRange(Query.ToArray)
'        End Sub

'        ''' <summary>
'        ''' The paramenter of a,b of the user function
'        ''' </summary>
'        ''' <remarks></remarks>
'        Const X As String = "{EDD16007-1CB8-4B3D-AE48-6FFB966A9C3B}"
'        Const Y As String = "{408E5229-98E1-4D19-A92B-372176749B54}"

'        ''' <summary>
'        ''' Add a user function from the user input from the console or a text file.
'        ''' </summary>
'        ''' <param name="Name">The name of the user function.</param>
'        ''' <param name="Expression">The expression of the user function.</param>
'        ''' <remarks>
'        ''' function [function name] expression
'        ''' </remarks>
'        Public Shared Sub Add(Name As String, Expression As String)
'            Dim [Function] As System.Func(Of MATRIX, MATRIX, MATRIX) 'The function delegate

'            Expression = Common.Replace(Expression, Mathematical.Expression.Constant.Constants, Mathematical.Expression.Constant.ConstantList)
'            Expression = Replace(Expression)
'            [Function] = Function(DblX As MATRIX, DblY As MATRIX) As MATRIX
'                             Dim sBuilder As StringBuilder = New StringBuilder(Expression)

'                             'sBuilder.Replace(X, DblX)
'                             'sBuilder.Replace(Y, DblY)

'                             Return Microsoft.VisualBasic.Mathematical.Expression.Evaluate(sBuilder.ToString)
'                         End Function

'            Call Add(Name.ToLower, [Function])
'        End Sub

'        ''' <summary>
'        ''' Parsing the use function definition from the user input value on the console 
'        ''' and then add it to the function dictionary.
'        ''' (从终端上面输入的用户函数的申明语句中解析出表达式，然后将其加入到用户字典中)
'        ''' </summary>
'        ''' <param name="statement">[function name] expression</param>
'        ''' <remarks>function [function name] expression</remarks>
'        Friend Shared Sub Add(statement As String)
'            Dim Name As String = statement.Split.First
'            'The expression may be contains space character, and it maybe split into sevral peaces.
'            Call Add(Name, Mid(statement, Len(Name) + 2))
'        End Sub

'        Private Shared Function Replace(expression As String) As String
'            Call Replace2("x", X, expression)
'            Call Replace2("y", Y, expression)

'            Return expression
'        End Function

'        Private Shared Sub Replace2(param As String, value As String, ByRef expression As String)
'            Dim p As Integer = InStr(expression, param)
'            Dim s As String 'Constant token
'            Dim Left, Right As Char 'Left, right operator
'            Dim sBuilder As StringBuilder = New StringBuilder(expression)

'            Do While p
'                Right = expression(p)
'                Left = expression(p - 2)

'                'if this tokens is surrounded by two operators then it is a paramenter name, not part 
'                'of the function name or other user define constant name.
'                If InStr(Constants.LEFT_OPERATOR_TOKENS, Left) AndAlso InStr(Constants.RIGHT_OPERATOR_TOKENS, Right) Then
'                    s = Mid(expression, p - 1, 3)

'                    Call sBuilder.Replace(s, Left & value & Right)
'                    expression = sBuilder.ToString

'                    p = InStr(p + Len(value), expression, param)
'                Else
'                    p = InStr(p + 1, expression, param)
'                End If
'            Loop
'        End Sub

'        ''' <summary>
'        ''' This function return a random number, you can specific the boundary of the random number in the parameters. 
'        ''' </summary>
'        ''' <param name="UpBound">
'        ''' If this parameter is empty or value is zero, then return the randome number between 0 and 1.
'        ''' (如果这个参数为空或者其值为0，那么函数就会返回0和1之间的随机数)
'        ''' </param>
'        ''' <param name="LowBound"></param>
'        ''' <returns></returns>
'        ''' <remarks></remarks>
'        Public Shared Function RND(LowBound As Double, UpBound As Double) As Double
'            Call Randomize()
'            If UpBound = 0 Then
'                Return Microsoft.VisualBasic.Rnd
'            Else
'                Return Microsoft.VisualBasic.Rnd * (UpBound - LowBound) + LowBound
'            End If
'        End Function
'    End Class
'End Namespace
