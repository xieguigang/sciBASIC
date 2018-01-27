Imports System.Runtime.CompilerServices

Namespace ApplicationServices.Development.XmlDoc.Serialization

    Public Module VBSpecific

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function IsMyResource([declare] As String) As Boolean
            Return InStr([declare], "My.Resources", CompareMethod.Binary) > 0
        End Function
    End Module
End Namespace