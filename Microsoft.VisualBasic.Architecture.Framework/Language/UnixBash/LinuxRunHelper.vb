Imports System.Reflection

Namespace Language.UnixBash

    Public Module LinuxRunHelper

        Public Function ScriptMe() As String
            Dim cmd As String = Assembly.GetExecutingAssembly.Location
            Dim perl As String =
$"#!/usr/bin/perl

system(""mono {cmd.CliPath} $ARGV"");
"
            Return perl
        End Function

        Public Function PerlShell() As Integer
            Return ScriptMe.SaveTo(Assembly.GetExecutingAssembly.Location.TrimFileExt)
        End Function
    End Module
End Namespace