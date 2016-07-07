#Region "Microsoft.VisualBasic::2db428dd99b74d049d68a17a53b76f9c, ..\VisualBasic_AppFramework\Microsoft.VisualBasic.Architecture.Framework\CommandLine\Reflection\ManualBuilder.vb"

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

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints

Namespace CommandLine.Reflection

    ''' <summary>
    ''' 用来生成帮助信息
    ''' </summary>
    Module ManualBuilder

        ''' <summary>
        ''' Prints the formatted help information on the console.
        ''' </summary>
        ''' <param name="api"></param>
        ''' <returns></returns>
        <Extension>
        Public Function PrintHelp(api As APIEntryPoint) As Integer

            Call Console.WriteLine($"Help for command '{api.Name}':")
            Call Console.WriteLine()
            Call Console.WriteLine($"  Information:  {api.Info}")
            Call Console.Write($"  Usage:        ")

            Dim fore As ConsoleColor = Console.ForegroundColor

            Console.ForegroundColor = ConsoleColor.Cyan
            Call Console.Write(App.ExecutablePath & " ")
            Console.ForegroundColor = ConsoleColor.Green
            Call Console.WriteLine(api.Usage)
            Console.ForegroundColor = fore

            If String.IsNullOrEmpty(api.Example) Then
                Call Console.WriteLine($"  Example:      CLI usage example not found!")
            Else
                Call Console.WriteLine($"  Example:      {App.AssemblyName} {api.Name} {api.Example}")
            End If

            If Not api.ParameterInfo.IsNullOrEmpty Then
                Call Console.WriteLine(vbCrLf & vbCrLf)
                Call Console.WriteLine("   Parameters information:" & vbCrLf & "   ---------------------------------------")

                Dim maxLen As Integer = (From x In api.ParameterInfo Select x.Name.Length + 2).Max
                Dim l As Integer

                For Each param As ParameterInfo In api.ParameterInfo.Select(Function(x) x.x)
                    fore = Console.ForegroundColor

                    If param.[Optional] Then
                        Call Console.Write("   [")
                        Console.ForegroundColor = ConsoleColor.Green
                        Call Console.Write(param.Name)
                        Console.ForegroundColor = fore
                        Call Console.Write("]")
                        l = param.Name.Length
                    Else
                        Call Console.Write("    " & param.Name)
                        l = param.Name.Length - 1
                    End If

                    Dim blank As String = New String(" "c, maxLen - l + 1)

                    Call Console.Write(blank)
                    Call Console.WriteLine($"Description:  {param.Description}")
                    Call Console.Write(New String(" "c, maxLen + 5))
                    Call Console.WriteLine($">Example:      {param.Name} ""{param.Example}""")
                    Call Console.WriteLine()
                Next
            End If

            Return 0
        End Function
    End Module
End Namespace
