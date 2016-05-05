Imports System.Reflection
Imports System.Text

Namespace CommandLine.Reflection.EntryPoints

    Public Class APIDelegate : Implements IExportAPI

        Protected _NumberOfParameters As Integer
        Protected _metaData As ExportAPIAttribute
        Protected __funcInvoker As Func(Of Object(), Integer)

        ''' <summary>
        ''' The usage name of this command line entry point.(本命令行对象的调用命令名称)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Name() As String Implements IExportAPI.Name
            Get
                Return _metaData.Name
            End Get
        End Property

        Public ReadOnly Property Info() As String Implements IExportAPI.Info
            Get
                Return _metaData.Info
            End Get
        End Property

        Public ReadOnly Property Usage() As String Implements IExportAPI.Usage
            Get
                Return _metaData.Usage
            End Get
        End Property

        Public ReadOnly Property Example() As String Implements IExportAPI.Example
            Get
                Return _metaData.Example
            End Get
        End Property

        ''' <summary>
        ''' 不可以使用本方法初始化目标对象为实例方法的类型
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <param name="Invoke"></param>
        ''' <remarks></remarks>
        Public Sub New(attribute As ExportAPIAttribute, [Invoke] As Func(Of Object(), Integer))
            _metaData = attribute
            __funcInvoker = Invoke
            _metaData = attribute
            _NumberOfParameters = 32
        End Sub

        Protected Sub New()
        End Sub

        Public Overridable Function HelpInformation() As String
            Dim sBuilder As StringBuilder = New StringBuilder(1024)

            sBuilder.AppendLine(String.Format("Help for command '{0}':", _metaData.Name))
            sBuilder.AppendLine()
            sBuilder.AppendLine(String.Format("  Information:  {0}", Info))
            sBuilder.AppendLine(String.Format("  Usage:        {0} {1}", Application.ExecutablePath, Usage))
            sBuilder.AppendLine(String.Format("  Example:      {0} {1} {2}", IO.Path.GetFileNameWithoutExtension(Application.ExecutablePath), Name, Example))

            Return sBuilder.ToString
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="parameters">数组的长度必须与目标函数的参数的数目一致，否则短于目标函数的参数的数目的数组会使用Nothing来填充缺少的部分，而多于目标函数的参数会被截断</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Execute(parameters As Object()) As Integer
            Dim callParameters() As Object

            If parameters.Length < _NumberOfParameters Then
                callParameters = New Object(_NumberOfParameters - 1) {}
                Call parameters.CopyTo(callParameters, 0)
            ElseIf parameters.Length > _NumberOfParameters Then
                callParameters = New Object(_NumberOfParameters - 1) {}
                Call Array.ConstrainedCopy(parameters, 0, callParameters, 0, _NumberOfParameters)
            Else
                callParameters = parameters
            End If

            Return __funcInvoker.Invoke(callParameters)
        End Function

        Public Overrides Function ToString() As String
            Return _metaData.Name
        End Function
    End Class
End Namespace