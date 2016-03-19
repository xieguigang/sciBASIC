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

        Public Overloads Shared Operator +(list As Generic.List(Of Value(Of T)), x As Value(Of T)) As Generic.List(Of Value(Of T))
            Call list.Add(x)
            Return list
        End Operator

        Public Overloads Shared Operator -(list As Generic.List(Of Value(Of T)), x As Value(Of T)) As Generic.List(Of Value(Of T))
            Call list.Remove(x)
            Return list
        End Operator
    End Class
End Namespace