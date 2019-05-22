Imports System.Runtime.CompilerServices

Namespace HDF5

    <HideModuleName> Public Module FileDump

        <Extension>
        Public Sub CreateFileDump(reader As HDF5Reader, out As System.IO.StringWriter)
            Call DirectCast(reader, IFileDump).printValues(out)
        End Sub
    End Module

    Public Interface IFileDump

        Sub printValues(console As System.IO.StringWriter)
    End Interface
End Namespace