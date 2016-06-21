Imports System.Runtime.InteropServices

Namespace FileIO.SymLinker

    ''' <summary>
    ''' Provides access to NTFS hard links in .Net.
    ''' </summary>
    Public Module HardLink

        <DllImport("Kernel32.dll", CharSet:=CharSet.Unicode)>
        Public Function CreateHardLink(lpFileName As String, lpExistingFileName As String, lpSecurityAttributes As IntPtr) As Boolean
        End Function
    End Module
End Namespace