Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Serialization.BinaryDumping

Namespace BIFF

    Module VB6

        'the memory copy API is used in the MKI$ function which converts an integer
        'value to a 2-byte string value to write to the file. (used by the Horizontal
        'Page Break function).
        Private Declare Sub CopyMemory Lib "KERNEL32" Alias "RtlMoveMemory" (lpvDest As Object, lpvSource As Object, ByVal cbCopy As Long)

        Friend Function MKI$(x As Integer)
            'used for writing integer array values to the disk file
            Dim temp$ = Space$(2)
            CopyMemory(temp$, x%, 2)
            MKI$ = temp$
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Friend Sub Put(Of T As Structure)(file As BinaryWriter, struct As T)
            Call file.Write(struct)
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Friend Sub Put(file As BinaryWriter, b As Byte)
            Call file.Write(b)
        End Sub
    End Module
End Namespace