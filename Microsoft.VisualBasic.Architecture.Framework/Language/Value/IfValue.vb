Namespace Language.Values

    Public Class IfValue(Of T As {IComparable, Structure})
        Inherits Value(Of T)

        Public Overloads Shared Widening Operator CType(o As T) As IfValue(Of T)
            Return New IfValue(Of T) With {.value = o}
        End Operator

        ''' <summary>
        ''' 如果相等就返回<paramref name="o"/>
        ''' </summary>
        ''' <param name="[if]"></param>
        ''' <param name="o"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator =([if] As IfValue(Of T), o As T) As T
            If [if].value.CompareTo(o) = 0 Then
                Return o
            Else
                Return Nothing
            End If
        End Operator

        Public Overloads Shared Operator <>([if] As IfValue(Of T), o As T) As T
            Throw New NotSupportedException
        End Operator

        ''' <summary>
        ''' 如果相等就返回<paramref name="o"/>
        ''' </summary>
        ''' <param name="[if]"></param>
        ''' <param name="o"></param>
        ''' <returns></returns>
        Public Overloads Shared Operator =([if] As IfValue(Of T), o As IfValue(Of T)) As T
            If [if].value.CompareTo(o.value) = 0 Then
                Return o.value
            Else
                Return Nothing
            End If
        End Operator

        Public Overloads Shared Operator <>([if] As IfValue(Of T), o As IfValue(Of T)) As T
            Throw New NotSupportedException
        End Operator
    End Class
End Namespace