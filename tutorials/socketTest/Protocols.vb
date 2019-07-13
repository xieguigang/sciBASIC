#Region "Microsoft.VisualBasic::da0fe537e502eaa234a58447f83a8cce, tutorials\socketTest\Protocols.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    ' Module Protocols
    ' 
    ' 
    '     Enum Test
    ' 
    '         A, B, C
    ' 
    ' 
    ' 
    '  
    ' 
    '     Properties: EntryPoint
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.Net.Protocols.Reflection

Module Protocols

    Public Enum Test
        A
        B
        C
    End Enum

    Public ReadOnly Property EntryPoint As Long = New Protocol(GetType(Test)).EntryPoint
End Module
