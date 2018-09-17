#Region "Microsoft.VisualBasic::684b0b62a717929ca1123ca139351092, Microsoft.VisualBasic.Core\ApplicationServices\Tools\Network\SSL\Extensions.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:

    '     Module CAExtensions
    ' 
    '         Function: InstallCommon
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
