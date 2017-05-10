Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.InteropService

' Microsoft VisualBasic CommandLine Code AutoGenerator
' assembly: G:/GCModeller/src/runtime/sciBASIC#/docs/guides/Example/CLI_Example/bin/Debug/CLI_Example.exe

Namespace TestApp


    ''' <summary>
    ''' Test code comments...
    ''' </summary>
    '''
    Public Class CLI_Example : Inherits InteropService


        Sub New(App$)
            MyBase._executableAssembly = App$
        End Sub

        ''' <summary>
        ''' Puts the brief description of this API command at here.
        ''' </summary>
        '''
        Public Function API1(_msg As String) As Integer
            Dim CLI$ = $"/API1 /msg ""{_msg}"""
            Dim proc As IIORedirectAbstract = RunDotNetApp(CLI$)
            Return proc.Run()
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        '''
        Public Function Code_vb(_namespace As String, Optional _out As String = "code.vb", Optional _booleantest As Boolean = False, Optional _boolean2_test As Boolean = False) As Integer
            Dim CLI$ = $"/Code.vb /namespace ""{_namespace}"" /out ""{_out}"" {If(_booleantest, "/booleantest", "")} {If(_boolean2_test, "/boolean2.test", "")}"
            Dim proc As IIORedirectAbstract = RunDotNetApp(CLI$)
            Return proc.Run()
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        '''
        Public Function Test_CLI_Scripting(_var As String, Optional __set As String = "var=value>;<var=value") As Integer
            Dim CLI$ = $"/Test.CLI_Scripting /var ""{_var}"" /@set ""{__set}"""
            Dim proc As IIORedirectAbstract = RunDotNetApp(CLI$)
            Return proc.Run()
        End Function
    End Class
End Namespace
