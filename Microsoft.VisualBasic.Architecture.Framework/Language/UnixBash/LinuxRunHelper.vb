Imports System.Reflection

Namespace Language.UnixBash

    Public Module LinuxRunHelper

        Public Function ScriptMe() As String
            Dim cmd As String = Assembly.GetEntryAssembly.Location
            Dim perl As String =
$"#!/usr/bin/perl

print @ARGV;
system(""mono {cmd.CliPath} @ARGV"");
"
            Return perl
        End Function

        Public Function PerlShell() As Integer
            Dim path As String = Assembly.GetEntryAssembly.Location.TrimFileExt
            Return ScriptMe.SaveTo(path)
        End Function
    End Module
End Namespace