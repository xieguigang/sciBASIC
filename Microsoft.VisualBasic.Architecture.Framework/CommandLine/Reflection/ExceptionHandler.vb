#Region "14871a446cfc4cb5d99e3972c3f10034, ..\Microsoft.VisualBasic.Architecture.Framework\CommandLine\Reflection\ExceptionHandler.vb"

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
Imports System.Text
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Terminal

Namespace CommandLine.Reflection

    <AttributeUsage(AttributeTargets.Class, AllowMultiple:=False, Inherited:=True)>
    Public Class ExceptionHelp : Inherits Attribute
        Public Property Documentation As String
        Public Property Debugging As String
        Public Property HelpsLink As String

        Public Overrides Function ToString() As String
            Return Me.GetJson
        End Function
    End Class

    Public Module ExceptionHandler

        'debuggroup summary 0.79s welcome To circos v0.69 6 Dec 2015 On Perl 5.018002
        'debuggroup summary 0.80s current working directory /home/xieguigang/circos/bin
        'debuggroup summary 0.80s command ./circos [no flags]
        'debuggroup summary 0.80s guessing configuration file
        'Missing argument In sprintf at /home/xieguigang/circos/bin/../Lib/Circos/Error.pm line 356.

        '  *** CIRCOS ERROR ***

        '      cwd: /home/xieguigang/circos/bin

        '      command: ./circos

        '  CONFIGURATION FILE Error

        '  Circos could Not find the configuration file []. To run Circos, you need To
        '  specify this file Using the -conf flag. The configuration file contains all
        '  the parameters that define the image, including input files, image size,
        '  formatting, etc.

        '  If you Do Not use the -conf flag, Circos will attempt To look For a file
        '  circos.conf in several reasonable places such as . etc/ ../etc

        '  To see where Circos looks for the file, use

        '      circos -debug_flag io

        '  To see how configuration files work, create the example image, whose
        '  configuration And data are found in example/. From the Circos distribution
        '  directory,

        '      cd example

        '        ../bin/circos -conf ./circos.conf

        '  Or use the 'run' script (UNIX only).

        '  Configuration files are described here

        '      http://circos.ca/tutorials/lessons/configuration/configuration_files/

        '  And the use of command-line flags, such as -conf, Is described here

        '      http://circos.ca/tutorials/lessons/configuration/runtime_parameters/

        '  Windows users unfamiliar With Perl should read

        '      http://circos.ca/tutorials/lessons/configuration/unix_vs_windows/

        '  This error can also be produced if supporting configuration files, such as
        '  track defaults, cannot be read.

        '  If you are having trouble debugging this Error, first read the best practices
        '  tutorial for helpful tips that address many common problems

        '      http://www.circos.ca/documentation/tutorials/reference/best_practices

        '  The debugging facility Is helpful To figure out what's happening under the
        '  hood

        '      http://www.circos.ca/documentation/tutorials/configuration/debugging

        '  If you're still stumped, get support in the Circos Google Group. Please
        '  include this Error And all your configuration And data files.

        '      http://groups.google.com/group/circos-data-visualization

        '  Stack trace : 
        '  at /home/xieguigang/circos/bin/../lib/Circos/Error.pm line 423.
        '        Circos: Error: fatal_error('configuration', 'missing') called at /home/xieguigang/circos/bin/../lib/Circos.pm line 141
        '        Circos:run('Circos', '_argv', '', '_cwd', '/home/xieguigang/circos/bin') called at ./circos line 529

        Public Sub Print(ex As Exception, method As MethodInfo)
            Dim type As Type = method.DeclaringType
            Dim helps As ExceptionHelp = type.GetAttribute(Of ExceptionHelp)

            If helps Is Nothing Then
                Call ex.PrintException
            Else
                Call Console.WriteLine("Environment summary:")
                Call Console.WriteLine("CLI:  " & App.Command)
                Call Console.WriteLine("Program: " & App.ExecutablePath.ToFileURL)
                Call Console.WriteLine("CWD: " & App.CurrentDirectory)
                Call Console.WriteLine("Start From: " & App.StartupDirectory)
                Call Console.WriteLine("Exception: ")
                Call STDIO.print(ex.Message, ConsoleColor.Red)
                Call Console.WriteLine("If you are having trouble debugging this Error, first read the best practices tutorial for helpful tips that address many common problems")
                Call STDIO.print(helps.Documentation, ConsoleColor.Blue)
                Call Console.WriteLine("The debugging facility Is helpful To figure out what's happening under the hood")
                Call STDIO.print(helps.Debugging, ConsoleColor.Blue)
                Call Console.WriteLine("If you're still stumped, you can try get help from author directly from E-mail:")
                Call STDIO.print(helps.HelpsLink, ConsoleColor.Blue)
                Call Console.WriteLine("Stack trace : ")
                Call STDIO.print(ex.ToString, ConsoleColor.Red)
            End If
        End Sub
    End Module
End Namespace
