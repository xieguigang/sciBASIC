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
                Call Console.WriteLine("    " & api.ParameterInfo.ToString)
            End If

            Return 0
        End Function
    End Module
End Namespace