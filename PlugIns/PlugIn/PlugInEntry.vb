Imports System.Windows.Forms
Imports System.ComponentModel
Imports System.Drawing
Imports System.Reflection

''' <summary>
''' Module PlugInsMain.(目标模块，在本模块之中包含有一系列插件命令信息，本对象定义了插件在菜单之上的根菜单项目)
''' </summary>
''' <remarks></remarks>
<AttributeUsage(AttributeTargets.Class, AllowMultiple:=False, Inherited:=True)>
Public Class PlugInEntry : Inherits CommandBase

    Protected Friend IconImage As Image
    Protected Friend MainModule As Type
    Protected Friend EntryList As EntryFlag()
    Protected Friend AssemblyPath As String
    Protected Friend Assembly As Assembly

    Public Property ShowOnMenu As Boolean = True

    Friend Function GetEntry(EntryType As EntryTypes) As EntryFlag
        Dim LQuery = From Entry As EntryFlag In EntryList
                     Where Entry.EntryType = EntryType
                     Select Entry '
        LQuery = LQuery.ToArray
        If LQuery.Count = 0 Then Return Nothing Else Return LQuery.First
    End Function

    Friend Function Initialize([Module] As Type) As PlugInEntry
        MainModule = [Module]
        Return Me
    End Function

    Public Overrides Function ToString() As String
        Return String.Format("PlugInEntry: {0}, //{1}", Name, AssemblyPath)
    End Function

    ''' <summary>
    ''' 从一个DLL文件之中加载命令
    ''' </summary>
    ''' <param name="Menu"></param>
    ''' <param name="AssemblyPath">Target DLL assembly file.(目标程序集模块的文件名)</param>
    ''' <returns>返回成功加载的命令的数目</returns>
    ''' <remarks></remarks>
    Public Shared Function LoadPlugIn(Menu As MenuStrip, AssemblyPath As String) As PlugInEntry
        If Not FileIO.FileSystem.FileExists(AssemblyPath) Then 'When the filesystem object can not find the assembly file, then this loading operation was abort.
            Return Nothing
        Else
            AssemblyPath = IO.Path.GetFullPath(AssemblyPath) 'Assembly.LoadFile required full path of a program assembly file.
            Return New PlugInLoader() With {.Menu = Menu, .AssemblyPath = AssemblyPath}.Load
        End If
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="Parameters">Method calling parameters object array.</param>
    ''' <param name="Method">Target method reflection information.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function Invoke(Parameters As Object(), Method As MethodInfo) As Object
        Dim NumberOfParameters = Method.GetParameters().Length
        Dim CallParameters() As Object

        If Parameters.Length < NumberOfParameters Then
            CallParameters = New Object(NumberOfParameters - 1) {}
            Parameters.CopyTo(CallParameters, 0)
        ElseIf Parameters.Length > NumberOfParameters Then
            CallParameters = New Object(NumberOfParameters - 1) {}
            Call Array.ConstrainedCopy(Parameters, 0, CallParameters, 0, NumberOfParameters)
        Else
            CallParameters = Parameters
        End If

        Return Method.Invoke(Nothing, CallParameters)
    End Function

    ''' <summary>
    ''' 从一个指定的窗体对象之上获取一个特定类型的控件的集合
    ''' </summary>
    ''' <typeparam name="T"></typeparam>
    ''' <param name="Target"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Shared Function GetControls(Of T As Component)(Target As Form) As T()
        Dim LQuery = From ctl As Control
                     In Target.Controls
                     Where TypeOf ctl Is T
                     Let component As Component = ctl
                     Select DirectCast(component, T) ' 
        Return LQuery.ToArray
    End Function
End Class