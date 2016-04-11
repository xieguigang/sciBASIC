Imports System.ComponentModel.Composition
Imports System.Reflection

Namespace SoftwareToolkits

    <Export(GetType(Global.System.Resources.ResourceManager))>
    Public Class Resources

        Public ReadOnly Property FileName As String
        Public ReadOnly Property Resources As Global.System.Resources.ResourceManager

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
            Dim __resEXPORT As Type = GetType(ExportAttribute)
            Dim typeDef As Type = (From type As Type
                                   In assm.GetTypes
                                   Let exp As ExportAttribute = type.GetCustomAttribute(__resEXPORT)
                                   Where Not exp Is Nothing AndAlso
                                       exp.ContractType.Equals(GetType(Global.System.Resources.ResourceManager))
                                   Select type).FirstOrDefault
            If Not typeDef Is Nothing Then
                Dim myRes As PropertyInfo = (From prop As PropertyInfo
                                             In typeDef.GetProperties(BindingFlags.Public Or BindingFlags.Static)
                                             Let exp As ExportAttribute = prop.GetCustomAttribute(__resEXPORT)
                                             Where prop.CanRead AndAlso
                                                 Not exp Is Nothing AndAlso
                                                 exp.ContractType.Equals(GetType(Global.System.Resources.ResourceManager))
                                             Select prop).FirstOrDefault
                If Not myRes Is Nothing Then
                    Dim value As Object = myRes.GetValue(Nothing, Nothing)
                    _Resources = DirectCast(value, Global.System.Resources.ResourceManager)
                End If
            End If
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