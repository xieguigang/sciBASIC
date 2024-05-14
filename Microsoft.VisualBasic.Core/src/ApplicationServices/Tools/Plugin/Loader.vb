#Region "Microsoft.VisualBasic::63c9989b77b6b1b04de901c0e5f438ac, Microsoft.VisualBasic.Core\src\ApplicationServices\Tools\Plugin\Loader.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 39
    '    Code Lines: 27
    ' Comment Lines: 6
    '   Blank Lines: 6
    '     File Size: 1.61 KB


    '     Module Loader
    ' 
    '         Function: GetPluginMethod
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
