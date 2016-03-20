Imports Microsoft.VisualBasic.Mathematical.Types
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.ComponentModel

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
    Public Function GetExpression(engine As Expression) As Func(Of Double(), Double)
        Dim helper As New __callerHelper(Args.ToArray(Function(x) x.ToLower)) With {
            .__engine = engine
        }
        Dim expr As SimpleExpression =
            ExpressionParser.TryParse(Expression,
                                      AddressOf helper.getValue,
                                      AddressOf engine.Functions.Evaluate)
        helper.__expr = expr
        Return AddressOf helper.Evaluate
    End Function

    Private Class __callerHelper

        Public __engine As Expression

        ''' <summary>
        ''' 默认全部都是变量
        ''' </summary>
        ReadOnly __args As Dictionary(Of String, Value(Of Double))
        ReadOnly __names As String()

        Sub New(args As String())
            __args = args.ToDictionary(Function(x) x, Function(null) New Value(Of Double))
            __names = args
        End Sub

        Public Function getValue(name As String) As Double
            If __args.ContainsKey(name.ToLower.ShadowCopy(name)) Then
                Return __args(name).Value
            Else
                Return __engine.GetValue(name)
            End If
        End Function

        Public __expr As SimpleExpression

        Public Function Evaluate(args As Double()) As Double
            For Each x As SeqValue(Of Double) In args.SeqIterator  ' 对lambda表达式设置环境变量
                __args(__names(x.Pos)).Value = x.obj
            Next

            Return __expr.Evaluate
        End Function
    End Class

    Public Overrides Function ToString() As String
        Dim args As String = Me.Args.JoinBy(", ")
        Return $"{Name}({args}) {Expression}"
    End Function
End Class
