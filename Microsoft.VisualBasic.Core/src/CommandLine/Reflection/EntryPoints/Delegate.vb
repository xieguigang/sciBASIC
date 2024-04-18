﻿#Region "Microsoft.VisualBasic::c3b88c258c452568f121a4468c11d487, sciBASIC#\Microsoft.VisualBasic.Core\src\CommandLine\Reflection\EntryPoints\Delegate.vb"

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

    '   Total Lines: 140
    '    Code Lines: 97
    ' Comment Lines: 19
    '   Blank Lines: 24
    '     File Size: 5.44 KB


    '     Class APIDelegate
    ' 
    '         Properties: Example, Info, Name, Usage
    ' 
    '         Constructor: (+2 Overloads) Sub New
    '         Function: Execute, HelpInformation, ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Reflection
Imports System.Text
Imports Microsoft.VisualBasic.ApplicationServices
Imports Microsoft.VisualBasic.CommandLine.ManView
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Text

Namespace CommandLine.Reflection.EntryPoints

    Public Class APIDelegate : Implements IExportAPI

        Protected _NumberOfParameters As Integer
        Protected _metaData As Binding(Of ExportAPIAttribute, MethodInfo)
        Protected __funcInvoker As Func(Of Object(), Integer)

        ''' <summary>
        ''' The usage name of this command line entry point.(本命令行对象的调用命令名称)
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public ReadOnly Property Name() As String Implements IExportAPI.Name
            Get
                Return _metaData.Bind.Name
            End Get
        End Property

#Disable Warning

        Public ReadOnly Property Info() As String Implements IExportAPI.Info
            Get
                Return _metaData.Bind.Info
            End Get
        End Property

        Public ReadOnly Property Usage() As String Implements IExportAPI.Usage
            Get
                Return _metaData.Bind.Usage
            End Get
        End Property

        Public ReadOnly Property Example() As String Implements IExportAPI.Example
            Get
                Return _metaData.Bind.Example
            End Get
        End Property
#Enable Warning

        ''' <summary>
        ''' 不可以使用本方法初始化目标对象为实例方法的类型
        ''' </summary>
        ''' <param name="attribute"></param>
        ''' <param name="Invoke"></param>
        ''' <remarks></remarks>
        Public Sub New(attribute As Binding(Of ExportAPIAttribute, MethodInfo), [Invoke] As Func(Of Object(), Integer))
            _metaData = attribute
            __funcInvoker = Invoke
            _metaData = attribute
            _NumberOfParameters = attribute.Target.GetParameters.Length
        End Sub

        Protected Sub New()
        End Sub

        Public Overridable Function HelpInformation(Optional md As Boolean = False) As String
            Dim sb As New StringBuilder(1024)

            If md Then
                sb.AppendLine($"{Name}</h3>")
            Else
                sb.AppendLine($"Help for command ""{Name}"":")
            End If

            Call sb.AppendLine()

            If md Then
                Dim prototype$ = APIPrototype(_metaData.Target.GetFullName)

                Call sb.AppendLine(Info)
                Call sb.AppendLine()
                Call sb.AppendLine($"**Prototype**: ``{prototype}``")
                Call sb.AppendLine()
                Call sb.AppendLine("###### Usage")
                Call sb.AppendLine()
                Call sb.AppendLine("```bash")
                Call sb.AppendLine($"{App.AssemblyName} {Usage}")
                Call sb.AppendLine("```")

                If Not String.IsNullOrEmpty(Example) Then
                    Call sb.AppendLine("###### Example")
                    Call sb.AppendLine("```bash")
                    Call sb.AppendLine($"{App.AssemblyName} {Example}")
                    Call sb.AppendLine("```")
                End If
            Else
                Dim infoLines$() = Paragraph.SplitParagraph(Info, 90).ToArray

                sb.AppendLine(String.Format("  Information:  {0}", infoLines.FirstOrDefault))

                If infoLines.Length > 1 Then
                    For Each line$ In infoLines.Skip(1)
                        Call sb.AppendLine($"                {line}")
                    Next
                End If

                sb.AppendLine(String.Format("  Usage:        {0} {1}", Application.ExecutablePath, Usage))
                sb.AppendLine(String.Format("  Example:      {0} {1}", App.AssemblyName, Example))
            End If

            Return sb.ToString
        End Function

        ''' <summary>
        ''' + 如果目标方法是一个Function函数,则退出代码为目标函数返回值
        ''' + 如果目标方法是一个无返回的Sub,则退出代码永远都是零
        ''' </summary>
        ''' <param name="parameters">数组的长度必须与目标函数的参数的数目一致，否则短于目标函数的参数的数目的数组会使用Nothing来填充缺少的部分，而多于目标函数的参数会被截断</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function Execute(args As CommandLine) As Integer
            Dim callParameters() As Object = New Object(_NumberOfParameters - 1) {}
            Dim pars As ParameterInfo() = _metaData.Target.GetParameters

            For i As Integer = 0 To pars.Length - 1
                Dim type As Type = pars(i).ParameterType
                Dim name As String = pars(i).Name
                Dim parAlias As Parameter = pars(i).GetCustomAttribute(Of Parameter)

                name = MatchArgumentName(args, name, parAlias)

                If name.StringEmpty Then
                    ' no matches inside the commandline argument inputs
                    ' check for optional
                    If pars(i).IsOptional Then
                        callParameters(i) = pars(i).DefaultValue
                    ElseIf type Is GetType(CommandLine) Then
                        callParameters(i) = args
                    Else
                        If parAlias Is Nothing Then
                            name = "--" & pars(i).Name
                        Else
                            name = parAlias.Alias
                        End If

                        ' missing required argument!
                        Throw New MissingMemberException($"missing the required argument({name}) inside your commandline input!")
                    End If
                Else
                    Dim val_str As String = args(name)
                    Dim value As Object = Scripting.CTypeDynamic(val_str, type)

                    callParameters(i) = value
                End If
            Next

            Return __funcInvoker.Invoke(callParameters)
        End Function

        ''' <summary>
        ''' convert the clr function parameter name as the commandline argument name
        ''' </summary>
        ''' <param name="args"></param>
        ''' <param name="name"></param>
        ''' <param name="[alias]"></param>
        ''' <returns></returns>
        Private Shared Function MatchArgumentName(args As CommandLine, name As String, [alias] As Parameter) As String
            If Not [alias] Is Nothing Then
                name = [alias].Alias.TrimParamPrefix
            End If

            name = args.Keys _
                .Where(Function(name2)
                           Return name2.TrimParamPrefix.TextEquals(name)
                       End Function) _
                .FirstOrDefault

            Return name
        End Function

        Public Overrides Function ToString() As String
            Return Name
        End Function
    End Class
End Namespace
