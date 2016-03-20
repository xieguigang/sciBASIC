Imports Microsoft.VisualBasic.Mathematical.Types

''' <summary>
''' User define function.(用户自定义函数)
''' </summary>
Public Class Func

    ''' <summary>
    ''' 函数名
    ''' </summary>
    ''' <returns></returns>
    Public Property Name As String
    ''' <summary>
    ''' 参数列表
    ''' </summary>
    ''' <returns></returns>
    Public Property Args As String()
    ''' <summary>
    ''' 函数表达式
    ''' </summary>
    ''' <returns></returns>
    Public Property Expression As String

    ''' <summary>
    ''' 从数据模型之中创建对象模型
    ''' </summary>
    ''' <param name="engine"></param>
    ''' <returns></returns>
    Public Function GetExpression(engine As Expression) As SimpleExpression
        Dim expr As SimpleExpression = ExpressionParser.TryParse(Expression,  )
    End Function

    Public Overrides Function ToString() As String
        Dim args As String = Me.Args.JoinBy(", ")
        Return $"{Name}({args}) {Expression}"
    End Function
End Class
