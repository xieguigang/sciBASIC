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
