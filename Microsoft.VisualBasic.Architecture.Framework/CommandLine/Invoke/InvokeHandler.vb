
'Namespace CommandLine.InvokeEntry

'    ''' <summary>
'    ''' 在方法内使用，根据方法的定义来生成命令行
'    ''' </summary>
'    ''' <remarks></remarks>
'    Public Module InvokeHandler

'        Public Class [Namespace] : Inherits Microsoft.VisualBasic.CommandLine.Reflection.Namespace

'            Dim _InternalAssemblyPath As String

'            Public ReadOnly Property Assembly As String
'                Get
'                    Return FileIO.FileSystem.GetFileInfo(_InternalAssemblyPath).FullName
'                End Get
'            End Property

'            ''' <summary>
'            ''' The name value of this namespace module.(本命名空间模块的名称值)
'            ''' </summary>
'            ''' <param name="Namespace">The name value of this namespace module.(本命名空间模块的名称值)</param>
'            ''' <param name="Assembly">可以是导出了内部资源之后的可执行文件的相对路径</param>
'            ''' <remarks></remarks>
'            Sub New([Namespace] As String, Assembly As String)
'                Call MyBase.New([Namespace])
'                Me._InternalAssemblyPath = Assembly
'            End Sub
'        End Class

'        Public Function Generate(EntryPoint As Object, ParamArray arg As Object()) As String

'        End Function
'    End Module
'End Namespace