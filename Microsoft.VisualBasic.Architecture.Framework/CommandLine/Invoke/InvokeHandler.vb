#Region "Microsoft.VisualBasic::9c084b7bb524e1677e1ff2abd7d9f66c, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\CommandLine\Invoke\InvokeHandler.vb"

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

'Namespace CommandLine.InvokeEntry

'    ''' <summary>
'    ''' 在方法内使用，根据方法的定义来生成命令行
'    ''' </summary>
'    ''' <remarks></remarks>
'    Public Module InvokeHandler

'        Public Class [Namespace] : Inherits Microsoft.VisualBasic.CommandLine.Reflection.Namespace

'            Dim _InternalAssemblyPath As String

'            Public ReadOnly Property Assembly As String
'                Get
'                    Return FileIO.FileSystem.GetFileInfo(_InternalAssemblyPath).FullName
'                End Get
'            End Property

'            ''' <summary>
'            ''' The name value of this namespace module.(本命名空间模块的名称值)
'            ''' </summary>
'            ''' <param name="Namespace">The name value of this namespace module.(本命名空间模块的名称值)</param>
'            ''' <param name="Assembly">可以是导出了内部资源之后的可执行文件的相对路径</param>
'            ''' <remarks></remarks>
'            Sub New([Namespace] As String, Assembly As String)
'                Call MyBase.New([Namespace])
'                Me._InternalAssemblyPath = Assembly
'            End Sub
'        End Class

'        Public Function Generate(EntryPoint As Object, ParamArray arg As Object()) As String

'        End Function
'    End Module
'End Namespace
