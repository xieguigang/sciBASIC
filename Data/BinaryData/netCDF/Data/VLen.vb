Imports System.Runtime.InteropServices

Namespace Data

    <StructLayout(LayoutKind.Sequential)>
    Public Structure vlen_t

        ''' <summary>
        ''' size_t
        ''' </summary>
        Public len As Integer
        ''' <summary>
        ''' void *
        ''' </summary>
        Public data As IntPtr

    End Structure

End Namespace