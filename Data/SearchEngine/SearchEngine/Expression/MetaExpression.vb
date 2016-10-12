Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' ``&lt;expr> &lt;opr>``，假如是以NOT操作符起始的元表达式，则<see cref="MetaExpression.Expression"/>属性为空
''' </summary>
Public Class MetaExpression

    Public Property [Operator] As Tokens
    ''' <summary>
    ''' Public <see cref="System.Delegate"/> Function <see cref="IAssertion"/>(data As <see cref="IObject"/>) As <see cref="Boolean"/>.
    ''' (这个可能是包含有括号运算的表达式)
    ''' </summary>
    ''' <returns></returns>
    Public Property Expression As IAssertion

    Public Overrides Function ToString() As String
        Return Me.GetJson
    End Function
End Class