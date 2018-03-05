#Region "Microsoft.VisualBasic::88d12848b6118f8ddbb52ff613da00f8, Microsoft.VisualBasic.Core\CommandLine\InteropService\SharedORM\CodeGenerator.vb"

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

    '     Class CodeGenerator
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: EnumeratesAPI, GetManualPage
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine.ManView
Imports Microsoft.VisualBasic.CommandLine.Reflection.EntryPoints
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace CommandLine.InteropService.SharedORM

    Public MustInherit Class CodeGenerator

        Protected ReadOnly App As Interpreter
        ''' <summary>
        ''' 目标应用程序模块的文件名，不包含有文件拓展名
        ''' </summary>
        Protected ReadOnly exe$

        Sub New(CLI As Type)
            App = New Interpreter(type:=CLI)
            exe = CLI.Assembly.CodeBase.BaseName
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetManualPage() As String
            Return App.HelpSummary(markdown:=False)
        End Function

        Public MustOverride Function GetSourceCode() As String

        Public Iterator Function EnumeratesAPI() As IEnumerable(Of NamedValue(Of CommandLine))
            Dim help$

            For Each api As APIEntryPoint In App.APIList
                Try
                    help =
$"```
{api.Usage.Replace("<", "&lt;")}
```" & vbCrLf & api.Info
                    Yield New NamedValue(Of CommandLine) With {
                        .Name = api.EntryPoint.Name,
                        .Description = help,
                        .Value = api.Usage.CommandLineModel
                    }
                Catch ex As Exception
                    ex = New Exception(api.EntryPointFullName(False), ex)
                    Throw ex
                End Try
            Next
        End Function
    End Class
End Namespace
