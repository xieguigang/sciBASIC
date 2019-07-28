Imports System.Reflection

Namespace ApplicationServices.Plugin

    Public Module Loader

        ''' <summary>
        ''' 只会查找静态方法
        ''' </summary>
        ''' <param name="assembly$"></param>
        ''' <param name="guid"></param>
        ''' <returns></returns>
        Public Function GetPluginMethod(assembly$, guid As String) As MethodInfo
            Dim assm As Assembly = System.Reflection.Assembly.UnsafeLoadFrom(assembly.GetFullPath)
            Dim types = assm.DefinedTypes.ToArray

            For Each type As TypeInfo In types
                Dim methods As MethodInfo() = type.GetMethods(BindingFlags.Public Or BindingFlags.Static)
                Dim pluginMethod As MethodInfo = methods _
                    .Where(Function(m)
                               Return Not m.GetCustomAttribute(Of PluginAttribute) Is Nothing
                           End Function) _
                    .FirstOrDefault(Function(m)
                                        Return m _
                                            .GetCustomAttributes(Of PluginAttribute) _
                                            .Any(Function(a)
                                                     Return a.UniqueKey.TextEquals(guid)
                                                 End Function)
                                    End Function)

                If Not pluginMethod Is Nothing Then
                    Return pluginMethod
                End If
            Next

            Return Nothing
        End Function
    End Module
End Namespace