
Imports System.Runtime.CompilerServices

Namespace ManagedSqlite.Core.Helpers
    Module StringHelper

        <Extension>
        Public Function ToHex(data As Byte()) As String
            Return BitConverter.ToString(data).Replace("-", "")
        End Function
    End Module
End Namespace
