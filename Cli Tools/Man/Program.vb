Module Program

    Sub Main()
        Dim CommandLine As CommandLine.CommandLine = App.CommandLine

        If CommandLine.IsNullOrEmpty Then
            Call Console.WriteLine("Usage for man command: " & vbCrLf & "     man <program>")
            Return
        End If

        Dim AssemblyName As String = CommandLine.Name
        If Not AssemblyName.FileExists Then
            AssemblyName = AssemblyName & ".exe"
            If Not AssemblyName.FileExists Then
                Call Console.WriteLine($"Could not found the assembly command which is named ""{CommandLine.Name}""! (Is it right?  {FileIO.FileSystem.GetFileInfo(AssemblyName).FullName.ToFileURL })")
            End If
        End If

        If CommandLine.Parameters.IsNullOrEmpty Then
            '默认显示出全部帮助信息
        Else
            '只显示出指定的命令对象
            Dim commandName As String = CommandLine.Parameters.First
            Call Console.WriteLine($"Help for command ""{commandName}"" in program ""{AssemblyName.ToFileURL}"":")
            Call Console.WriteLine()
        End If

    End Sub
End Module
