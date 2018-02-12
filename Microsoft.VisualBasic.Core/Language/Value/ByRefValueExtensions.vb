Imports System.Runtime.CompilerServices
Imports ByRefString = Microsoft.VisualBasic.Language.Value(Of String)

Namespace Language.Values

    Public Module ByRefValueExtensions

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension> Public Function Split(s As ByRefString, ParamArray delimiter As Char()) As String()
            Return s.Value.Split(delimiter)
        End Function
    End Module
End Namespace