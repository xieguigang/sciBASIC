Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Linq
Imports any = Microsoft.VisualBasic.Scripting

Public Module cast

    <Extension>
    Public Function as_string(x As Array) As String()
        Return x.AsObjectEnumerator.Select(Function(o) any.ToString(o)).ToArray
    End Function

End Module
