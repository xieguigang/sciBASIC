'Imports System.Reflection

Namespace Debugging

    ' ''' <summary>
    ' ''' .NET程序调试器
    ' ''' </summary>
    ' ''' <remarks></remarks>
    'Public Class NETDebugger

    '    ''' <summary>
    '    ''' 
    '    ''' </summary>
    '    ''' <param name="AssemblyPath">.NET应用程序可执行文件或者包含有Main定义的dll文件</param>
    '    ''' <param name="argvs">调试所使用的命令行参数</param>
    '    ''' <returns></returns>
    '    ''' <remarks></remarks>
    '    Public Function Start(AssemblyPath As String, argvs As String()) As Integer

    '        AssemblyPath = FileIO.FileSystem.GetFileInfo(AssemblyPath).FullName

    '        Dim Assembly As System.Reflection.Assembly = System.Reflection.Assembly.LoadFile(AssemblyPath)
    '        Dim GetMainEntryLQuery = (From TypeEntry As System.Reflection.TypeInfo
    '                                  In Assembly.DefinedTypes
    '                                  Let EntryMain As MethodInfo = GetMainEntry(TypeEntry)
    '                                  Where EntryMain IsNot Nothing
    '                                  Select EntryMain).ToArray

    '        If GetMainEntryLQuery.IsNullOrEmpty Then
    '            Return -100
    '        End If

    '        Dim Main = GetMainEntryLQuery.First
    '        Return RunMain(Main, argvs)
    '    End Function

    '    ''' <summary>
    '    ''' 函数会递归查询
    '    ''' </summary>
    '    ''' <param name="TypeInfo"></param>
    '    ''' <returns></returns>
    '    ''' <remarks></remarks>
    '    Public Shared Function GetMainEntry(TypeInfo As Type) As MethodInfo
    '        Dim LQuery = (From Method As System.Reflection.MethodInfo
    '                      In TypeInfo.GetMethods(BindingFlags.Public Or BindingFlags.Static)
    '                      Where String.Equals(Method.Name, "Main", StringComparison.OrdinalIgnoreCase) AndAlso IsStandardMainReturnType(Method.ReturnType) AndAlso IsStandardMainParameterType(Method)
    '                      Select Method).ToArray
    '        Return LQuery.FirstOrDefault
    '    End Function

    '    Private Shared Function IsStandardMainReturnType(Type As Type) As Boolean
    '        Return System.Type.Equals(Type, GetType(Void)) OrElse System.Type.Equals(Type, GetType(Integer))
    '    End Function

    '    Private Shared Function IsStandardMainParameterType(MethodInfo As MethodInfo) As Boolean
    '        Dim pList = MethodInfo.GetParameters

    '        If pList.Count > 1 Then
    '            Return False
    '        ElseIf pList.IsNullOrEmpty Then
    '            Return True
    '        End If

    '        Dim pTypeInfo As System.Type = pList(0).ParameterType
    '        Return pTypeInfo.Equals(GetType(String)) OrElse pTypeInfo.Equals(GetType(String()))
    '    End Function

    '    ''' <summary>
    '    ''' 启动Main函数
    '    ''' </summary>
    '    ''' <param name="Main"></param>
    '    ''' <param name="argvs"></param>
    '    ''' <returns></returns>
    '    ''' <remarks></remarks>
    '    Private Function RunMain(Main As MethodInfo, argvs As String()) As Integer

    '        Dim pValue As Object()
    '        Dim pInfo = Main.GetParameters

    '        If pInfo.IsNullOrEmpty Then
    '            pValue = New Object() {} '函数没有参数
    '        Else
    '            If pInfo(0).Equals(GetType(String)) Then
    '                pValue = {argvs(0)}  '字符串
    '            Else
    '                pValue = {argvs} '字符串数组
    '            End If
    '        End If

    '        Dim result = InternalRunFunction(Main, pValue)

    '        If Main.ReturnType.Equals(GetType(Integer)) Then
    '            Return DirectCast(result, Integer)
    '        Else
    '            Return 0
    '        End If
    '    End Function

    '    ''' <summary>
    '    ''' 在终端之上打印出每一行代码
    '    ''' </summary>
    '    ''' <param name="EntryPoint"></param>
    '    ''' <param name="argvs"></param>
    '    ''' <returns></returns>
    '    ''' <remarks></remarks>
    '    Private Function InternalRunFunction(EntryPoint As MethodInfo, argvs As Object()) As Object



    '    End Function

    'End Class
End Namespace