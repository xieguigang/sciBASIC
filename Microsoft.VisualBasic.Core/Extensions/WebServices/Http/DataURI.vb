Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ApplicationServices

Namespace Net.Http

    ''' <summary>
    ''' Data URI scheme
    ''' </summary>
    Public Class DataURI

        ReadOnly mime$
        ReadOnly base64$
        ReadOnly chartSet$

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="file"></param>
        ''' <param name="codepage$">The chartset codepage name, by default is ``ASCII``.</param>
        Sub New(file As String, Optional codepage$ = Nothing)
            mime = Strings.LCase(file.FileMimeType.MIMEType)
            base64 = file.ReadBinary.ToBase64String
            codepage = codepage
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Shared Function FromFile(file As String) As DataURI
            Return New DataURI(file)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Overrides Function ToString() As String
            If chartSet.StringEmpty Then
                Return $"data:{mime};base64,{base64}"
            Else
                Return $"data:{mime};charset={chartSet};base64,{base64}"
            End If
        End Function
    End Class
End Namespace