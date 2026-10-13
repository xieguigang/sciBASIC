
Namespace Runtime

    ''' <summary>
    ''' Eager generator collector: a <c>function*</c> body runs to completion on the
    ''' first call and every <c>yield</c> value is recorded here.
    ''' <para>
    ''' A tree-walking interpreter cannot suspend a body mid-flight without a
    ''' continuation or a thread, so generators are materialised eagerly. That is
    ''' observationally equivalent for the usual <c>for...of</c> / <c>next()</c>
    ''' consumption patterns, but an INFINITE generator would never return — a
    ''' documented limitation.
    ''' </para>
    ''' </summary>
    Public NotInheritable Class JsYieldCollector

        Public ReadOnly Values As New List(Of Object)

        ''' <summary>Record a yielded value; the yield expression evaluates to it.</summary>
        Public Function AddValue(value As Object) As Object
            Values.Add(value)
            Return value
        End Function

    End Class

    ''' <summary>
    ''' The iterator object handed back by a generator function; supports
    ''' <c>next()</c> (returning <c>{value, done}</c>) and for-of/re-spreading
    ''' through <see cref="JsRuntime.JsIterableValues"/>.
    ''' </summary>
    Public NotInheritable Class JsGenerator

        Public ReadOnly Values As List(Of Object)
        Private _index As Integer

        Public Sub New(values As List(Of Object))
            Me.Values = values
        End Sub

        ''' <summary>One iteration step as a {value, done} object.</summary>
        Public Function Next2() As Object
            Dim d As New Dictionary(Of String, Object)
            If _index < Values.Count Then
                d("value") = Values(_index)
                d("done") = False
                _index += 1
            Else
                d("value") = JsRuntime.Undef
                d("done") = True
            End If
            Return d
        End Function

        ''' <summary>Remaining values — used by for-of and spread.</summary>
        Public Function Remaining() As List(Of Object)
            Dim rest As New List(Of Object)
            While _index < Values.Count
                rest.Add(Values(_index))
                _index += 1
            End While
            Return rest
        End Function

        Public Overrides Function ToString() As String
            Return "[object Generator]"
        End Function

    End Class

End Namespace
