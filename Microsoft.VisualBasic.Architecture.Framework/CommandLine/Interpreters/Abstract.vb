Namespace CommandLine

    ''' <summary>
    ''' 假若所传入的命令行的name是文件路径，解释器就会执行这个函数指针
    ''' </summary>
    ''' <param name="path"></param>
    ''' <param name="args"></param>
    ''' <returns></returns>
    Public Delegate Function __ExecuteFile(path As String, args As CommandLine) As Integer
    ''' <summary>
    ''' 假若所传入的命令行是空的，就会执行这个函数指针
    ''' </summary>
    ''' <returns></returns>
    Public Delegate Function __ExecuteEmptyCLI() As Integer

    ''' <summary>
    ''' 假若查找不到命令的话，执行这个函数
    ''' </summary>
    ''' <param name="args"></param>
    ''' <returns></returns>
    Public Delegate Function __ExecuteNotFound(args As CommandLine) As Integer

End Namespace