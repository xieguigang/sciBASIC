Imports System.Text
Imports Microsoft.VisualBasic.Mathematical.Helpers
Imports Microsoft.VisualBasic.Mathematical.Types

''' <summary>
''' Expression Evaluation Engine
''' </summary>
''' <remarks></remarks>
Public Class Expression

    Public ReadOnly Property Constant As Helpers.Constants = New Helpers.Constants
    Public ReadOnly Property Variables As Helpers.Variable = New Variable
    Public ReadOnly Property Functions As Helpers.Function = New [Function]

    ''' <summary>
    ''' The default expression evaluation engine.
    ''' </summary>
    ''' <returns></returns>
    Public Shared ReadOnly Property DefaultEngine As New Expression

    Public Shared Function Evaluate(expr As String) As Double
        Return DefaultEngine.Evaluation(expr)
    End Function

    ''' <summary>
    ''' Evaluate the a specific mathematics expression string to a double value, the functions, constants, 
    ''' bracket pairs can be include in this expression but the function are those were originally exists 
    ''' in the visualbasic. I'm sorry for this...
    ''' (对一个包含有函数、常数和匹配的括号的一个复杂表达式进行求值，但是对于表达式中的函数而言：仅能够使用在
    ''' VisualBaisc语言中存在的有限的几个数学函数。)  
    ''' </summary>
    ''' <param name="expr"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Evaluation(expr As String) As Double
        Dim sep As SimpleExpression = ExpressionParser.TryParse(expr)
        Return sep.Evaluate
    End Function

    ''' <summary>
    ''' 先常量，后变量
    ''' </summary>
    ''' <param name="x"></param>
    ''' <returns></returns>
    Public Function GetValue(x As String) As Double
        Dim isConst As Boolean = False
        Dim n = Constant.[GET](x, isConst)

        If isConst Then
            Return n
        Else
            Return Variables(x)
        End If
    End Function

    Public Sub SetVariable(Name As String, expr As String)
        Call Variables.Set(Name, expr)
    End Sub

    Public Sub AddConstant(Name As String, expr As String)
        Call Constant.Add(Name, expr)
    End Sub
End Class
