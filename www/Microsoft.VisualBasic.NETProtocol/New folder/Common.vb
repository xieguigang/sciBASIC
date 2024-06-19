Imports System
Imports System.Runtime.InteropServices

Namespace SuperSimpleTcp
    Friend Module Common
        Friend Sub ParseIpPort(ipPort As String, <Out> ByRef ip As String, <Out> ByRef port As Integer)
            If String.IsNullOrEmpty(ipPort) Then Throw New ArgumentNullException(NameOf(ipPort))

            ip = Nothing
            port = -1

            Dim colonIndex = ipPort.LastIndexOf(":"c)
            If colonIndex <> -1 Then
                ip = ipPort.Substring(0, colonIndex)
                port = Convert.ToInt32(ipPort.Substring(colonIndex + 1))
            End If
        End Sub
    End Module
End Namespace
