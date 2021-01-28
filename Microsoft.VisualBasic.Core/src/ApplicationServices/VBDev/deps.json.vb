#Region "Microsoft.VisualBasic::e6b1a252538a6ee7d7c23d74ab2c6ce6, Microsoft.VisualBasic.Core\src\ApplicationServices\VBDev\deps.json.vb"

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

    '     Class deps
    ' 
    '         Properties: compilationOptions, libraries, runtimeTarget, targets
    ' 
    '         Function: GetReferenceProject, RetriveLoadedAssembly
    ' 
    '         Sub: TryHandleNetCore5AssemblyBugs
    ' 
    '     Class target
    ' 
    '         Properties: dependencies
    ' 
    '     Class library
    ' 
    '         Properties: hashPath, path, serviceable, sha512, type
    ' 
    '     Class runtimeTarget
    ' 
    '         Properties: name, signature
    ' 
    '     Class compilationOptions
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace ApplicationServices.Development.NetCore5

    ''' <summary>
    ''' read deps.json for .net 5 assembly
    ''' </summary>
    Public Class deps

        Public Property runtimeTarget As runtimeTarget
        Public Property compilationOptions As compilationOptions
        Public Property targets As Dictionary(Of String, target)
        Public Property libraries As Dictionary(Of String, library)

        ''' <summary>
        ''' get list of project reference name
        ''' </summary>
        ''' <returns></returns>
        Public Function GetReferenceProject() As IEnumerable(Of String)
            Return From entry As KeyValuePair(Of String, library)
                   In libraries
                   Let ref As library = entry.Value
                   Where ref.type = "project"
                   Select entry.Key.StringReplace("/\d+(\.\d+)+", "")
        End Function

        Private Shared Function RetriveLoadedAssembly() As IEnumerable(Of String)
            Return From assembly As Assembly
                   In AppDomain.CurrentDomain.GetAssemblies()
                   Where Not assembly.IsDynamic
                   Select assembly.Location.BaseName
        End Function

        ''' <summary>
        ''' handling of the bugs on .NET 5 runtime
        ''' missing assembly when load reference type module
        ''' </summary>
        ''' <param name="package"></param>
        Public Shared Sub TryHandleNetCore5AssemblyBugs(package As Type)
            Dim home As String = package.Assembly.Location.ParentPath
            Dim moduleName As String = package.Assembly.GetName.Name
            Dim deps As deps = $"{home}/{moduleName}.deps.json".LoadJsonFile(Of deps)
            Dim referenceList As String() = deps _
                .GetReferenceProject _
                .Where(Function(name) name <> moduleName) _
                .ToArray
            Dim asmsIndex As Index(Of String) = RetriveLoadedAssembly.Indexing

            Static globalsReference As New Dictionary(Of String, Assembly)

            For Each name As String In referenceList _
                .Where(Function(nameKey)
                           Return (Not globalsReference.ContainsKey(nameKey)) AndAlso Not nameKey Like asmsIndex
                       End Function)

                ' 由于.net5环境下没有办法将dll自动生成在library文件夹之中
                ' 所以在这里就直接在应用程序文件夹之中查找了
                Dim dllName As String = $"{home}/{name}.dll"
                Dim assembly As Assembly = Assembly.LoadFrom(dllName)

                globalsReference(name) = assembly
            Next
        End Sub
    End Class

    Public Class target
        Public Property dependencies As Dictionary(Of String, String)
    End Class

    Public Class library
        Public Property type As String
        Public Property serviceable As Boolean
        Public Property sha512 As String
        Public Property path As String
        Public Property hashPath As String
    End Class

    Public Class runtimeTarget

        Public Property name As String
        Public Property signature As String

    End Class

    Public Class compilationOptions

    End Class
End Namespace
