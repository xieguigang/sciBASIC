Imports System.Xml.Serialization

Namespace Types

    ''' <summary>
    ''' 在<see cref="SimpleExpression.Calculator"></see>之中由于移位操作的需要，需要使用类对象可以修改属性的特性来进行正常的计算，所以请不要修改为Structure类型
    ''' </summary>
    ''' <remarks></remarks>
    Public Class MetaExpression

        <XmlAttribute> Public Property [Operator] As Char

        ''' <summary>
        ''' 自动根据类型来计算出结果
        ''' </summary>
        ''' <returns></returns>
        <XmlAttribute> Public Property LEFT As Double
            Get
                If __left Is Nothing Then
                    Return _left
                Else
                    Return __left()
                End If
            End Get
            Set(value As Double)
                _left = value
                __left = Nothing
            End Set
        End Property

        Dim _left As Double
        Dim __left As Func(Of Double)

        Sub New()
        End Sub

        Sub New(n As Double)
            LEFT = n
        End Sub

        Sub New(handle As Func(Of Double))
            __left = handle
        End Sub

        Sub New(simple As SimpleExpression)
            Call Me.New(AddressOf simple.Evaluate)
        End Sub

        Public Overrides Function ToString() As String
            If __left Is Nothing Then
                Return String.Format("{0} {1}", LEFT, [Operator])
            Else
                Return $"{__left.ToString} {[Operator]}"
            End If
        End Function
    End Class
End Namespace