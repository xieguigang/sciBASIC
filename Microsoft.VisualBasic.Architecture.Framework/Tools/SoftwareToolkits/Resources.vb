Imports System.ComponentModel.Composition
Imports System.Reflection

Namespace SoftwareToolkits

    <Export(GetType(Global.System.Resources.ResourceManager))>
    Public Class Resources

        Public ReadOnly Property FileName As String

        Sub New()
            Call Me.New(Assembly.GetExecutingAssembly)
        End Sub

        Sub New(type As Type)
            Call Me.New(type.Assembly)
        End Sub

        ''' <summary>
        ''' 默认是<see cref="App.HOME"/>/Resources/assmFile
        ''' </summary>
        ''' <param name="assm"></param>
        Sub New(assm As Assembly)
            FileName = assm.Location.ParentPath & "/Resources/" & FileIO.FileSystem.GetFileInfo(assm.Location).Name
            Call __resParser()
        End Sub

        Sub New(dll As String)
            FileName = FileIO.FileSystem.GetFileInfo(dll).FullName
            Call __resParser()
        End Sub

        Private Sub __resParser()
            Call __load(Assembly.LoadFile(FileName))
        End Sub

        Private Sub __load(assm As Assembly)

        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="my">null</param>
        ''' <param name="assm"></param>
        Private Sub New(my As Type, assm As Assembly)
            Call __load(assm)
        End Sub

        ''' <summary>
        ''' 从自身的程序集之中加载数据
        ''' </summary>
        ''' <returns></returns>
        Public Shared Function LoadMy() As Resources
            Return New Resources(Nothing, Assembly.GetExecutingAssembly)
        End Function

        ''' <summary>
        ''' Returns the cached ResourceManager instance used by this class.
        ''' </summary>
        ''' <returns></returns>
        ''' 
        <Export(GetType(Global.System.Resources.ResourceManager))>
        Public Shared ReadOnly Property MyResource As Global.System.Resources.ResourceManager
            Get
                Return My.Resources.ResourceManager
            End Get
        End Property
    End Class
End Namespace