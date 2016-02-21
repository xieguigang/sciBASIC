Namespace ComponentModel

    ''' <summary>
    ''' you can applying this data type into a dictionary object to makes the mathematics calculation more easily.
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class Value(Of T)

        ''' <summary>
        ''' The object value with a specific type define.
        ''' </summary>
        ''' <returns></returns>
        Public Property Value As T

        Sub New(value As T)
            Me.Value = value
        End Sub

        Sub New()
            Value = Nothing
        End Sub

        Public Overrides Function ToString() As String
            Return Scripting.InputHandler.ToString(Value)
        End Function

        Public Overloads Shared Operator +(list As List(Of Value(Of T)), x As Value(Of T)) As List(Of Value(Of T))
            Call list.Add(x)
            Return list
        End Operator

        Public Overloads Shared Operator -(list As List(Of Value(Of T)), x As Value(Of T)) As List(Of Value(Of T))
            Call list.Remove(x)
            Return list
        End Operator
    End Class

    ''' <summary>
    ''' 假若需要通过字典对象实现一些统计操作，则这个对象类型可能十分有用
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    Public Class DictHashValue(Of T)
        ''' <summary>
        ''' 由于字典对象的元素为值对象，所以无法进行元素值的修改，所以可以使用这个对象进行修改
        ''' </summary>
        ''' <returns></returns>
        Public Property value As T

        Public Overrides Function ToString() As String
            Return value.ToString
        End Function
    End Class
End Namespace