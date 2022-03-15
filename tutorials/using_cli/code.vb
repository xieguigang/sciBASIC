#Region "Microsoft.VisualBasic::8bec22e7bb958f2a0f0bbb6d576e3941, sciBASIC#\tutorials\using_cli\code.vb"

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


    ' Code Statistics:

    '   Total Lines: 76
    '    Code Lines: 43
    ' Comment Lines: 18
    '   Blank Lines: 15
    '     File Size: 2.60 KB


    '     Class CLI_Example
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: API1, Code_vb, Test_CLI_Scripting
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.InteropService

' Microsoft VisualBasic CommandLine Code AutoGenerator
' assembly: G:/GCModeller/src/runtime/sciBASIC#/docs/guides/Example/CLI_Example/bin/Debug/CLI_Example.exe

Namespace TestApp


    ''' <summary>
    '''Test code comments...
    ''' </summary>
    '''
    Public Class CLI_Example : Inherits InteropService


        Sub New(App$)
            MyBase._executableAssembly = App$
        End Sub

        ''' <summary>
        '''Puts the brief description of this API command at here.
        ''' </summary>
        '''
        Public Function API1(_msg As String, Optional _msg2 As String = "2333 yes or not?") As Integer
            Dim CLI As New StringBuilder("/API1")
            Call CLI.Append("/msg " & """" & _msg & """ ")
            If Not _msg2.StringEmpty Then
                Call CLI.Append("/msg2 " & """" & _msg2 & """ ")
            End If


            Dim proc As IIORedirectAbstract = RunDotNetApp(CLI.ToString())
            Return proc.Run()
        End Function

        ''' <summary>
        '''
        ''' </summary>
        '''
        Public Function Code_vb(_namespace As String, Optional _out As String = "code.vb", Optional _booleantest As Boolean = False, Optional _boolean2_test As Boolean = False) As Integer
            Dim CLI As New StringBuilder("/Code.vb")
            Call CLI.Append("/namespace " & """" & _namespace & """ ")
            If Not _out.StringEmpty Then
                Call CLI.Append("/out " & """" & _out & """ ")
            End If
            If _booleantest Then
                Call CLI.Append("/booleantest ")
            End If
            If _boolean2_test Then
                Call CLI.Append("/boolean2.test ")
            End If


            Dim proc As IIORedirectAbstract = RunDotNetApp(CLI.ToString())
            Return proc.Run()
        End Function

        ''' <summary>
        '''
        ''' </summary>
        '''
        Public Function Test_CLI_Scripting(_var As String, Optional __set As String = "") As Integer
            Dim CLI As New StringBuilder("/Test.CLI_Scripting")
            Call CLI.Append("/var " & """" & _var & """ ")
            If Not __set.StringEmpty Then
                Call CLI.Append("/@set " & """" & __set & """ ")
            End If


            Dim proc As IIORedirectAbstract = RunDotNetApp(CLI.ToString())
            Return proc.Run()
        End Function
    End Class
End Namespace
