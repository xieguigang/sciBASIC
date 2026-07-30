Namespace Models

    ''' <summary>
    ''' A type record decoded from the TPI stream (classic PDB) or the metadata type tables
    ''' (Portable PDB).
    ''' </summary>
    Public Class TypeRecord

        ''' <summary>
        ''' Type id (leaf index for classic PDB, metadata token for Portable PDB).
        ''' </summary>
        Public Property TypeId As UInteger

        ''' <summary>
        ''' Type name, e.g. ``System.Int32`` or ``MyNamespace.MyClass``.
        ''' </summary>
        Public Property Name As String

        ''' <summary>
        ''' Category of the type. See <see cref="TypeKind"/>.
        ''' </summary>
        Public Property Kind As TypeKind

        ''' <summary>
        ''' Size of the type in bytes, 0 when unknown / not applicable.
        ''' </summary>
        Public Property Size As UInteger

        ''' <summary>
        ''' Field / member names of the type (for structs / classes).
        ''' </summary>
        Public Property Fields As New List(Of String)()

        Public Overrides Function ToString() As String
            Return $"{Kind} {Name} (#{TypeId})"
        End Function
    End Class
End Namespace