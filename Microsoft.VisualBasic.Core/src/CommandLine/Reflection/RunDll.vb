#Region "Microsoft.VisualBasic::c28059f86a578d80a1dcd736f65960ff, Microsoft.VisualBasic.Core\src\CommandLine\Reflection\RunDll.vb"

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

    '   Total Lines: 85
    '    Code Lines: 44 (51.76%)
    ' Comment Lines: 30 (35.29%)
    '    - Xml Docs: 90.00%
    ' 
    '   Blank Lines: 11 (12.94%)
    '     File Size: 2.98 KB


    '     Class RunDllEntryPoint
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetDllMainFunc, GetDllMethod, GetPoint
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language

Namespace CommandLine.Reflection

    ''' <summary>
    ''' 
    ''' </summary>
    Public Class RunDllEntryPoint : Inherits [Namespace]

        ''' <summary>
        ''' rundll namespace::api
        ''' </summary>
        ''' <param name="Name"></param>
        Sub New(Optional name As String = "")
            Call MyBase.New(name, "")
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="entrypoint"></param>
        ''' <returns>
        ''' 假若没有api的名称的话，是默认使用一个名字为``Main``的主函数来运行的
        ''' </returns>
        Public Shared Function GetPoint(entrypoint As String) As NamedValue(Of String)
            Dim entry = entrypoint.GetTagValue("::", trim:=True)

            If entry.Value.StringEmpty Then
                entry.Value = "Main"
            End If

            Return entry
        End Function

        ''' <summary>
        ''' 大小写不敏感
        ''' </summary>
        ''' <param name="assembly"></param>
        ''' <param name="entryPoint">
        ''' namespace::funcName
        ''' </param>
        ''' <returns></returns>
        Public Shared Function GetDllMethod(assembly As Assembly, entryPoint$) As MethodInfo
            Dim entry As NamedValue(Of String) = GetPoint(entryPoint)
            Dim types As Type() = GetTypesHelper(assm:=assembly)
            Dim dll As Type = LinqAPI.DefaultFirst(Of Type) _
                                                            _
                () <= From type As Type
                      In types
                      Let load = type.GetCustomAttribute(Of RunDllEntryPoint)
                      Let name = load?.Namespace
                      Where Not load Is Nothing AndAlso name.TextEquals(entry.Name)
                      Select type

            If dll Is Nothing Then
                Return Nothing
            Else
                Return GetDllMainFunc(dll, entry.Value)
            End If
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="dll"></param>
        ''' <param name="entry"></param>
        ''' <returns>
        ''' this function returns nothing if the method is not found
        ''' </returns>
        Public Shared Function GetDllMainFunc(dll As Type, Optional entry As String = "Main") As MethodInfo
            Dim matchName = Function(m As MethodInfo)
                                Return m.Name.TextEquals(entry)
                            End Function
            Dim method As MethodInfo = dll _
                .GetMethods(PublicShared) _
                .Where(predicate:=matchName) _
                .FirstOrDefault

            Return method
        End Function
    End Class

End Namespace
