
Imports System.Runtime.CompilerServices

Namespace MBW.Utilities.ManagedSqlite.Core.Helpers
    Module StringHelper

        <Extension>
        Public Function ToHex(data As Byte()) As String
            Return BitConverter.ToString(data).Replace("-", "")
        End Function
    End Module
End Namespace
