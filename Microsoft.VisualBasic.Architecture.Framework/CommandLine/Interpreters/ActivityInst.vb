#Region "Microsoft.VisualBasic::86259a4665c36814b7cff1e63824e46a, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\CommandLine\Interpreters\ActivityInst.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Language

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

            Dim setValue =
                New SetValue(Of EntryPoints.APIEntryPoint)() _
                    .GetSet(NameOf(EntryPoints.APIEntryPoint.target))

            Call (From api As EntryPoints.APIEntryPoint
                  In Me.__API_InfoHash.Values
                  Where api.IsInstanceMethod  ' 只联系实例方法
                  Select setValue(api, api)).ToArray

            Me._obj = obj
        End Sub

        ''' <summary>
        ''' 静态加实例方法
        ''' </summary>
        ''' <param name="Type"></param>
        ''' <param name="[Throw]"></param>
        ''' <returns></returns>
        Protected Overrides Function __getsAllCommands(Type As Type, Optional [Throw] As Boolean = True) As List(Of Reflection.EntryPoints.APIEntryPoint)
            Dim baseList As List(Of EntryPoints.APIEntryPoint) =
                MyBase.__getsAllCommands(Type, [Throw])
            Dim commandAttribute As Type = GetType(ExportAPIAttribute)
            Dim commandsSource = From EntryPoint As MethodInfo
                                 In Type.GetMethods(BindingFlags.Public Or BindingFlags.Instance)
                                 Let Entry = EntryPoint.GetCustomAttributes(commandAttribute, True)
                                 Select Entry,
                                     MethodInfo = EntryPoint
            baseList += From methodInfo
                        In commandsSource
                        Where Not methodInfo.Entry.IsNullOrEmpty
                        Let api As ExportAPIAttribute = TryCast(methodInfo.Entry.First, ExportAPIAttribute)
                        Let commandInfo As EntryPoints.APIEntryPoint =
                            New EntryPoints.APIEntryPoint(api, methodInfo.MethodInfo)
                        Select commandInfo
                        Order By commandInfo.Name Ascending

            Return baseList
        End Function
    End Class
End Namespace
