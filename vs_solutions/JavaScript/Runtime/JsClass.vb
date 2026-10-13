
Namespace Runtime

    ''' <summary>
    ''' A JavaScript class: a constructor, an instance-member prototype and static
    ''' members.
    ''' <para>
    ''' Instances stay plain object dictionaries (<c>Dictionary(Of String, Object)</c>)
    ''' carrying two internal keys: <c>__proto__</c> → <see cref="Prototype"/> and
    ''' <c>__class__</c> → this class (used for method-home resolution by
    ''' <c>super</c>). Both keys are invisible to for-in / Object.keys.
    ''' </para>
    ''' </summary>
    Public NotInheritable Class JsClass

        Public ReadOnly Name As String
        ''' <summary>Constructor body (a callable), or Nothing for an empty constructor.</summary>
        Public ReadOnly Constructor As Object
        ''' <summary>Instance members: methods and accessor pairs.</summary>
        Public ReadOnly Prototype As New Dictionary(Of String, Object)
        ''' <summary>Static members: methods, accessors and static fields.</summary>
        Public ReadOnly Statics As New Dictionary(Of String, Object)
        ''' <summary>Superclass, or Nothing for a base class.</summary>
        Public ReadOnly SuperClass As JsClass
        ''' <summary>
        ''' Instance field initialisers (name → zero-argument callable), evaluated
        ''' per construction with <c>this</c> bound to the new instance.
        ''' </summary>
        Public ReadOnly Fields As New List(Of KeyValuePair(Of String, Object))

        Public Sub New(name As String, ctor As Object, superClass As JsClass)
            Me.Name = name
            Me.Constructor = ctor
            Me.SuperClass = superClass
        End Sub

        Public Overrides Function ToString() As String
            Return "class " & Name
        End Function

    End Class

End Namespace
