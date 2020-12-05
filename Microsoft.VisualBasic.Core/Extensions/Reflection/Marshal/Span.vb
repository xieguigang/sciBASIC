Imports System.Runtime.CompilerServices

Namespace Emit.Marshal

    <HideModuleName>
    Public Module SpanHelper

        <Extension>
        Public Sub Flush(Of T)(src As T(), span As T(), start As Integer)
            For i As Integer = 0 To span.Length - 1
                src(start + i) = span(i)
            Next
        End Sub

    End Module
End Namespace