
''' <summary>
''' 
''' </summary>
''' <typeparam name="T">Token type</typeparam>
''' <typeparam name="O">Operator type</typeparam>
Public Class MetaExpression(Of T, O)

    Public Property [Operator] As O

    ''' <summary>
    ''' 自动根据类型来计算出结果
    ''' </summary>
    ''' <returns></returns>
    Public Overridable Property LEFT As T

    Sub New()
    End Sub

    Sub New(x As T)
        LEFT = x
    End Sub
End Class
