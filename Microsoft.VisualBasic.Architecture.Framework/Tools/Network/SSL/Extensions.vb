Imports System.Reflection
Imports Microsoft.VisualBasic.Win32

Namespace Net.SSL

    Module CAExtensions

        Public Function InstallCommon(ByRef privateKeys As Dictionary(Of Long, Net.SSL.Certificate),
                                      CA As Certificate,
                                      [overrides] As Boolean,
                                      trace As String,
                                      invoke As MethodInfo) As Boolean

            If privateKeys.ContainsKey(CA.uid) Then
                If WindowsServices.Initialized Then
                    Call ServicesLogs.WriteEntry({$"{invoke.DeclaringType.Name} private key dictionary contains a certificate which its uid: {CA.uid} is conflict with the new install certificate.",
                                                 If([overrides], "And the old conflicting certificates was overrides by the new one.", "The certificates operation was skipped!"),
                                                 $"Current:  {privateKeys(CA.uid).ToString}",
                                                 $"Certificates_going_to_install:  {CA.ToString}",
                                                 $"install_trace:  {trace}"},
                                                 invoke.FullName,
                                                 EventLogEntryType.SuccessAudit)
                End If

                If [overrides] Then
                    Call privateKeys.Remove(CA.uid)
                Else
                    Return False
                End If
            End If

            Call privateKeys.Add(CA.uid, CA)

            If WindowsServices.Initialized Then
                Call ServicesLogs.WriteEntry({$"New certificates was installed on server!", CA.ToString},
                                             $"{trace} ==> {invoke.FullName}",
                                             EventLogEntryType.SuccessAudit)
            End If

            Return True
        End Function
    End Module
End Namespace