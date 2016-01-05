Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports System.Reflection

Namespace CommandLine

    ''' <summary>
    ''' Interpreter for object instance.(对于<see cref="Interpreter"></see>而言，其仅解析静态的方法，二本对象则实例方法和静态方法都进行解析)
    ''' </summary>
    ''' <remarks></remarks>
    Public NotInheritable Class ActivityInstance : Inherits Interpreter

        ReadOnly _obj As Object

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="obj">An instance object.</param>
        ''' <remarks></remarks>
        Sub New(obj As Object)
            Call MyBase.New(obj.GetType)

            Dim associationLQuery = (From methodEntry As EntryPoints.APIEntryPoint
                                         In Me._CommandInfoHash.Values.AsParallel
                                     Where methodEntry.IsInstanceMethod  ' 只联系实例方法
                                     Select methodEntry.InvokeSet(Of Object)(NameOf(EntryPoints.APIEntryPoint.InvokeOnObject), methodEntry)).ToArray
            Me._obj = obj
        End Sub

        ''' <summary>
        ''' 静态加实例方法
        ''' </summary>
        ''' <param name="Type"></param>
        ''' <param name="[Throw]"></param>
        ''' <returns></returns>
        Protected Overrides Function __getsAllCommands(Type As Type, Optional [Throw] As Boolean = True) As List(Of Reflection.EntryPoints.APIEntryPoint)
            Dim ChunkList = MyBase.__getsAllCommands(Type, [Throw])
            Dim commandAttribute As System.Type = GetType(ExportAPIAttribute)
            Dim commandsSource = (From EntryPoint In Type.GetMethods(BindingFlags.Public Or BindingFlags.Instance)
                                  Let Entry = EntryPoint.GetCustomAttributes(commandAttribute, True)
                                  Select Entry, MethodInfo = EntryPoint).ToArray
            Dim commandsInfo = (From methodInfo
                                In commandsSource
                                Where Not methodInfo.Entry.IsNullOrEmpty
                                Let commandInfo = New EntryPoints.APIEntryPoint(TryCast(methodInfo.Entry.First, ExportAPIAttribute), methodInfo.MethodInfo)
                                Select commandInfo
                                Order By commandInfo.Name Ascending).ToArray
            Call ChunkList.AddRange(commandsInfo)

            Return ChunkList
        End Function
    End Class
End Namespace