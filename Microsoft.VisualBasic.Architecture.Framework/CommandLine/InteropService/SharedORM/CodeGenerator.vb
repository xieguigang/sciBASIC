#Region "Microsoft.VisualBasic::0b3259a96ba77f0b7cce163f943da72b, ..\sciBASIC#\Microsoft.VisualBasic.Architecture.Framework\CommandLine\InteropService\SharedORM\CodeGenerator.vb"

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

        Public MustOverride Function GetSourceCode() As String

        Public Iterator Function EnumeratesAPI() As IEnumerable(Of NamedValue(Of CommandLine))
            For Each api As APIEntryPoint In App.APIList
                Try
                    Yield New NamedValue(Of CommandLine) With {
                        .Name = api.EntryPoint.Name,
                        .Description = $"```
                        {api.Usage.Replace("<", "&lt;")}
                        ```" & vbCrLf & api.Info,
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
