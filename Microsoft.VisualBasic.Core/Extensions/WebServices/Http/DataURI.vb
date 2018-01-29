Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices

Namespace Net.Http

    Public Class DataURI

        ReadOnly mime$
        ReadOnly base64$

        Sub New(file As String)
            mime = Strings.LCase(file.FileMimeType.MIMEType)
            base64 = file.ReadBinary.ToBase64String
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function FromFile(file As String) As DataURI
            Return New DataURI(file)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            Return $"data:{mime};base64,{base64}"
        End Function
    End Class
End Namespace