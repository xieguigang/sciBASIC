#Region "3b46134272087a7bc468e537a0275979, ..\Microsoft.VisualBasic.Architecture.Framework\Language\UnixBash\LinuxRunHelper.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Reflection

Namespace Language.UnixBash

    ''' <summary>
    ''' mono shortcuts
    ''' </summary>
    Public Module LinuxRunHelper

        ''' <summary>
        ''' perl ./<see cref="Assembly"/> @ARGV
        ''' </summary>
        ''' <returns></returns>
        Public Function ScriptMe() As String
            Dim cmd As String = Assembly.GetEntryAssembly.Location
            Dim perl As String =
$"#!/usr/bin/perl

use strict;
use warnings;
use File::Basename;
use File::Spec;

my $cli = join "" "", @ARGV;

print ""{App.AssemblyName} << $cli\n"";
system(""mono {cmd.CliPath} $cli"");
"
            Return perl
        End Function

        ''' <summary>
        ''' Run from bash shell
        ''' </summary>
        ''' <returns></returns>
        Public Function BashRun() As String
            Dim cmd As String = Assembly.GetEntryAssembly.Location
            Dim bash As String =
$"#!/bin/bash

cli=""$@"";

echo ""{App.AssemblyName} <<< $@"";
mono ""{cmd}"" $cli
"
            Return bash.Replace(vbCr, "")
        End Function

        ''' <summary>
        ''' 这里比perl脚本掉调用有一个缺点，在运行前还需要使用命令修改为可执行权限
        ''' 'sudo chmod 777 cmd.sh'
        ''' </summary>
        ''' <returns></returns>
        Public Function BashShell() As Integer
            Dim path As String = Assembly.GetEntryAssembly.Location.TrimFileExt
            Return BashRun.SaveTo(path)
        End Function

        ''' <summary>
        ''' Execute command using perl script
        ''' </summary>
        ''' <returns></returns>
        Public Function PerlShell() As Integer
            Dim path As String = Assembly.GetEntryAssembly.Location.TrimFileExt & ".pl"
            Return ScriptMe.SaveTo(path)
        End Function
    End Module
End Namespace
