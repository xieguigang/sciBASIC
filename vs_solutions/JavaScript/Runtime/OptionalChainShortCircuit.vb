
Namespace Runtime

    ''' <summary>
    ''' Internal control-flow signal thrown when an optional-chain access
    ''' (<c>a?.b</c> / <c>a?.[i]</c> / <c>a?.()</c>) hits a nullish base.
    ''' <para>
    ''' Caught at the ROOT of the chain (both the interpreter and generated code
    ''' wrap the whole chain), where it becomes the value <c>undefined</c> — so
    ''' <c>a?.b.c</c> never evaluates <c>.c</c> when <c>a</c> is nullish.
    ''' Public so that translated VB programs can catch it too; it is deliberately
    ''' NOT a <see cref="JsRuntimeException"/>, so a JS try/catch cannot swallow it.
    ''' </para>
    ''' </summary>
    Public NotInheritable Class OptionalChainShortCircuit
        Inherits Exception

        Public Sub New()
            MyBase.New()
        End Sub

    End Class

End Namespace
