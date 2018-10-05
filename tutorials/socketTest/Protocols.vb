Imports Microsoft.VisualBasic.Net.Protocols.Reflection

Module Protocols

    Public Enum Test
        A
        B
        C
    End Enum

    Public ReadOnly Property EntryPoint As Long = New Protocol(GetType(Test)).EntryPoint
End Module
