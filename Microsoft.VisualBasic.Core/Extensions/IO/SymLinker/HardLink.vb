#Region "Microsoft.VisualBasic::11141ac6b69c11c3cc556ac585589f7e, Microsoft.VisualBasic.Core\Extensions\IO\SymLinker\HardLink.vb"

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

    '     Module HardLink
    ' 
    '         Function: CreateHardLink
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
