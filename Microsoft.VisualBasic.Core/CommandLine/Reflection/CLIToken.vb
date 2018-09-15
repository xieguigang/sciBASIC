#Region "Microsoft.VisualBasic::386ff910fde789efea51b884da740b0a, Microsoft.VisualBasic.Core\CommandLine\Reflection\CLIToken.vb"

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

    '     Class RunDllEntryPoint
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GetDllMethod, GetPoint
    ' 
    '     Class CLIToken
    ' 
    '         Properties: Name
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Class CLIParameter
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language

Namespace CommandLine.Reflection

    Public Class RunDllEntryPoint : Inherits [Namespace]

        ''' <summary>
        ''' rundll namespace::api
        ''' </summary>
        ''' <param name="Name"></param>
        Sub New(Name As String)
            Call MyBase.New(Name, "")
        End Sub

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="entrypoint$"></param>
        ''' <returns>
        ''' 假若没有api的名称的话，是默认使用一个名字为``Main``的主函数来运行的
        ''' </returns>
        Public Shared Function GetPoint(entrypoint$) As NamedValue(Of String)
            Dim entry = entrypoint.GetTagValue("::", trim:=True)
            If entry.Value.StringEmpty Then
                entry.Value = "Main"
            End If
            Return entry
        End Function

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
                Dim matchName = Function(m As MethodInfo)
                                    Return m.Name.TextEquals(entry.Value)
                                End Function
                Dim method As MethodInfo = dll _
                    .GetMethods(PublicShared) _
                    .Where(predicate:=matchName) _
                    .FirstOrDefault

                Return method
            End If
        End Function
    End Class

    ''' <summary>
    ''' A very basically type in the <see cref="CommandLine"/>
    ''' </summary>
    Public MustInherit Class CLIToken : Inherits Attribute
        Implements IReadOnlyId

        ''' <summary>
        ''' Name of this token object, this can be parameter name or api name.
        ''' </summary>
        ''' <returns></returns>
        Public Overridable ReadOnly Property Name As String Implements IReadOnlyId.Identity

        ''' <summary>
        ''' Init this token by using <see cref="name"/> value.
        ''' </summary>
        ''' <param name="name">Token name</param>
        Sub New(name As String)
            Me.Name = name
        End Sub

        Public Overrides Function ToString() As String
            Return Name
        End Function
    End Class

    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False, Inherited:=True)>
    Public Class CLIParameter : Inherits CLIToken

        Sub New(name As String)
            Call MyBase.New(name)
        End Sub
    End Class
End Namespace
