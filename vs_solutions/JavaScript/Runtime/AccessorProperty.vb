
Namespace Runtime

    ''' <summary>
    ''' Getter/setter pair created by object-literal accessors
    ''' (<c>{ get x() {...}, set x(v) {...} }</c>) and by class accessors.
    ''' <para>
    ''' Stored as the property VALUE in the object's dictionary;
    ''' <see cref="JsRuntime.JsGet"/> / <see cref="JsRuntime.JsSet"/> recognise it
    ''' and invoke the corresponding accessor instead of reading/writing a slot.
    ''' </para>
    ''' </summary>
    Public NotInheritable Class AccessorProperty

        ''' <summary>Getter body (a callable), or Nothing for a setter-only property.</summary>
        Public ReadOnly Getter As Object
        ''' <summary>Setter body (a callable), or Nothing for a getter-only property.</summary>
        Public ReadOnly Setter As Object

        Public Sub New(getter As Object, setter As Object)
            Me.Getter = getter
            Me.Setter = setter
        End Sub

    End Class

End Namespace
