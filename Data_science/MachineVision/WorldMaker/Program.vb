Imports WorldMaker.Cli

''' <summary>
''' WorldMaker 入口：真实照片 → 3D 高斯溅射点云 → 三维场景模型。
''' </summary>
Module Program

    Function Main(args As String()) As Integer
        Dim cli As CommandLine = CommandLine.Parse(args)

        If args.Length = 0 OrElse cli.Has("help") Then
            Console.WriteLine(PipelineOptions.Usage())
            Return 0
        End If

        If cli.Unknown.Count > 0 Then
            Console.WriteLine($"[warn] 忽略了无法识别的参数: {String.Join(" ", cli.Unknown)}")
        End If

        Dim options As PipelineOptions

        Try
            options = PipelineOptions.FromCommandLine(cli)
            Call options.Validate()
        Catch ex As Exception
            Console.WriteLine($"[error] 参数错误: {ex.Message}")
            Console.WriteLine()
            Console.WriteLine(PipelineOptions.Usage())
            Return 2
        End Try

        Try
            Return Pipeline.WorldPipeline.Run(options)
        Catch ex As Exception
            Console.WriteLine()
            Console.WriteLine("[fatal] 流水线中断:")
            Console.WriteLine(ex.ToString())
            Return 1
        End Try
    End Function

End Module
