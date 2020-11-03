Namespace CommandLine.Reflection

    Public Interface IExportAPI

        ''' <summary>
        ''' The name of the commandline object.(这个命令的名称)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Name As String
        ''' <summary>
        ''' Something detail of help information.(详细的帮助信息)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Info As String
        ''' <summary>
        ''' The usage of this command.(这个命令的用法，本属性仅仅是一个助记符，当用户没有编写任何的使用方法信息的时候才会使用本属性的值)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Usage As String
        ''' <summary>
        ''' A example that to useing this command.(对这个命令的使用示例，本属性仅仅是一个助记符，当用户没有编写任何示例信息的时候才会使用本属性的值)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ReadOnly Property Example As String
    End Interface
End Namespace