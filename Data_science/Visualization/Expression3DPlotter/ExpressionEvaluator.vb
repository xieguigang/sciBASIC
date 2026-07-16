Imports Microsoft.VisualBasic.Math.Scripting.MathExpression

''' <summary>
''' 对用户输入的数学表达式进行预编译，并在网格/参数采样点上安全求值。
''' 解析失败会向上抛出异常，由界面捕获并提示用户。
''' </summary>
Public Class ExpressionEvaluator

    Private ReadOnly engine As New ExpressionEngine()
    Private ReadOnly expr As Impl.Expression

    ''' <summary>
    ''' 预编译一个数学表达式字符串（仅解析一次）。
    ''' </summary>
    Public Sub New(expression As String)
        Me.expr = ExpressionEngine.Parse(expression)
    End Sub

    ''' <summary>
    ''' 计算 f(x, y)，用于三维曲面 z=f(x,y) 的网格采样。
    ''' </summary>
    Public Function Evaluate(x As Double, y As Double) As Double
        engine.SetSymbol("x", x)
        engine.SetSymbol("y", y)
        Return engine.Evaluate(expr)
    End Function

    ''' <summary>
    ''' 计算 f(t)，用于参数化三维曲线的参数采样。
    ''' </summary>
    Public Function Evaluate(t As Double) As Double
        engine.SetSymbol("t", t)
        Return engine.Evaluate(expr)
    End Function
End Class
