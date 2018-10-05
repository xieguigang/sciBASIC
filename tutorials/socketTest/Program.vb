#Region "Microsoft.VisualBasic::c8132cea4e9fbd7231c5e680eaa87731, tutorials\socketTest\Program.vb"

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

' Module Program
' 
'     Sub: Main
' 
' /********************************************************************************/

#End Region

Imports System.Threading
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.Net.Protocols
Imports Microsoft.VisualBasic.Parallel

Module Program

    Sub Main()

        Dim testPort% = 2233

        Call New ThreadStart(AddressOf New server(testPort).Run).RunTask
        Call Thread.Sleep(1000)

        Dim invoke As New ProtocolInvoker(Of Protocols.Test)("localhost", testPort)

        Dim response As RequestStream


        response = invoke.SendMessage(Test.A, "[AAAA]")
        response = invoke.SendMessage(Test.B, "adada")
        response = invoke.SendMessage(Test.C, "54555")

        Pause()
    End Sub
End Module
