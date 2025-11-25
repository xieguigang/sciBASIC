#Region "Microsoft.VisualBasic::3027b843c983021fe8daff25ac82e847, Microsoft.VisualBasic.Core\src\ApplicationServices\FileSystem\ProgramPathSearchTool.vb"

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

'   Total Lines: 243
'    Code Lines: 156 (64.20%)
' Comment Lines: 50 (20.58%)
'    - Xml Docs: 90.00%
' 
'   Blank Lines: 37 (15.23%)
'     File Size: 10.25 KB


'     Class ProgramPathSearchTool
' 
'         Properties: CustomDirectories, Directories
' 
'         Constructor: (+1 Overloads) Sub New
'         Function: BranchRule, FindProgram, FindScript, safeGetFiles, SearchDirectory
'                   SearchDrive, searchImpl, SearchProgram, SearchScriptFile
' 
' 
' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.InteropServices
Imports System.Text
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Linq

Namespace ApplicationServices

    ''' <summary>
    ''' Program helper for search dir like ``C:\Program Files`` or ``C:\Program Files(x86)``
    ''' </summary>
    ''' <remarks>
    ''' Works on windows, have not test on Linux/Mac yet.
    ''' </remarks>
    Public Class ProgramPathSearchTool

        ReadOnly customDirectories As String()

        Sub New(ParamArray customDirectories As String())
            Me.customDirectories = customDirectories
        End Sub

        ''' <summary>
        ''' linux ``which`` liked file search tool 
        ''' </summary>
        ''' <param name="name"></param>
        ''' <returns></returns>
        Public Shared Function Which(name As String, Optional customDirectories As IEnumerable(Of String) = Nothing) As String
            Dim foundPath As String
            Dim programFiles As New ProgramPathSearchTool(customDirectories.SafeQuery.ToArray)

            If RuntimeInformation.IsOSPlatform(OSPlatform.Windows) Then
                foundPath = programFiles.FindExecutableOnWindows(name)
            Else
                foundPath = FindExecutableOnUnix(name)
            End If

            Return foundPath
        End Function

#Region "Windows Implementation"

        ''' <summary>
        ''' 在 Windows 系统上查找可执行文件。
        ''' </summary>
        Private Function FindExecutableOnWindows(name As String) As String
            Dim pathExt As String()
            Dim pathDirs As String()

            If String.IsNullOrWhiteSpace(name) Then
                Return ""
            End If

            ' 检查是否已经是完整路径
            If Path.IsPathRooted(name) Then
                If File.Exists(name) AndAlso IsValidWindowsExecutable(name) Then
                    Return ToShortPathName(name)
                Else
                    ' 如果是路径但不存在或不是可执行文件，尝试添加扩展名
                    pathExt = GetPathExtensions()

                    For Each ext In pathExt
                        Dim fullPathWithExt = Path.ChangeExtension(name, ext)
                        If File.Exists(fullPathWithExt) Then
                            Return ToShortPathName(fullPathWithExt)
                        End If
                    Next
                End If
                Return "" ' 给定路径无效
            End If

            ' 在 PATH 环境变量中搜索
            pathDirs = GetPathDirectories().JoinIterates(customDirectories).ToArray
            pathExt = GetPathExtensions()

            For Each dir As String In pathDirs
                If String.IsNullOrWhiteSpace(dir) Then Continue For

                For Each ext In pathExt
                    Dim potentialPath = Path.Combine(dir, name + ext)
                    If File.Exists(potentialPath) Then
                        Return ToShortPathName(potentialPath)
                    End If
                Next
            Next

            Return "" ' 未找到
        End Function

        ''' <summary>
        ''' 获取 PATH 环境变量中的目录列表。
        ''' </summary>
        Private Function GetPathDirectories() As String()
            Dim pathEnv As String = Environment.GetEnvironmentVariable("PATH")
            If String.IsNullOrEmpty(pathEnv) Then
                Return Array.Empty(Of String)()
            End If
            Return pathEnv.Split(New Char() {";"c}, StringSplitOptions.RemoveEmptyEntries)
        End Function

        ''' <summary>
        ''' 获取 PATHEXT 环境变量中的可执行文件扩展名列表。
        ''' </summary>
        Private Shared Function GetPathExtensions() As String()
            Dim pathextEnv As String = Environment.GetEnvironmentVariable("PATHEXT")
            Dim extensions As New List(Of String)()
            If Not String.IsNullOrEmpty(pathextEnv) Then
                For Each ext In pathextEnv.Split(New Char() {";"c}, StringSplitOptions.RemoveEmptyEntries)
                    extensions.Add(ext.ToLower())
                Next
            Else
                ' 如果 PATHEXT 未设置，则使用默认值
                extensions.AddRange({".com", ".exe", ".bat", ".cmd"})
            End If
            Return extensions.ToArray()
        End Function

        ''' <summary>
        ''' 检查文件是否具有有效的 Windows 可执行扩展名。
        ''' </summary>
        Private Shared Function IsValidWindowsExecutable(filePath As String) As Boolean
            Dim ext = Path.GetExtension(filePath).ToLower()
            Dim validExts = GetPathExtensions()
            Return validExts.Contains(ext)
        End Function

        ''' <summary>
        ''' P/Invoke 声明，用于获取文件的短路径名 (8.3 格式)。
        ''' </summary>
        <DllImport("kernel32.dll", CharSet:=CharSet.Auto, SetLastError:=True)>
        Private Shared Function GetShortPathName(longPath As String, shortPath As StringBuilder, shortPathLength As Integer) As Integer
        End Function

        ''' <summary>
        ''' 将长路径名转换为 Windows 短路径名。
        ''' </summary>
        Private Shared Function ToShortPathName(longPath As String) As String
            If String.IsNullOrEmpty(longPath) Then Return longPath

            Dim shortPathBuilder As New StringBuilder(260)
            Dim result As Integer = GetShortPathName(longPath, shortPathBuilder, shortPathBuilder.Capacity)

            If result > 0 AndAlso result < shortPathBuilder.Capacity Then
                Return shortPathBuilder.ToString()
            Else
                ' 如果缓冲区不够大，重新分配
                shortPathBuilder.Capacity = result
                result = GetShortPathName(longPath, shortPathBuilder, shortPathBuilder.Capacity)
                If result > 0 Then
                    Return shortPathBuilder.ToString()
                End If
            End If

            ' 如果转换失败，返回原始路径
            Return longPath
        End Function

#End Region

#Region "Unix/Linux Implementation"

        ''' <summary>
        ''' 在 Unix-like 系统上通过调用 'which' 命令来查找可执行文件。
        ''' </summary>
        Private Shared Function FindExecutableOnUnix(name As String) As String
            If String.IsNullOrWhiteSpace(name) Then
                Return ""
            End If

            Try
                Dim psi As New ProcessStartInfo()
                psi.FileName = "which"
                psi.Arguments = name
                psi.RedirectStandardOutput = True
                psi.RedirectStandardError = True
                psi.UseShellExecute = False
                psi.CreateNoWindow = True

                Using process As New Process()
                    process.StartInfo = psi
                    process.Start()

                    ' 读取输出，并移除末尾的换行符
                    Dim output As String = process.StandardOutput.ReadToEnd().Trim()
                    process.WaitForExit()

                    ' 'which' 命令在找不到文件时返回非零退出码
                    If process.ExitCode = 0 Then
                        Return output
                    Else
                        Return ""
                    End If
                End Using
            Catch ex As Exception
                ' 如果 'which' 命令不存在或执行失败，返回空字符串
                ' 在一个没有 'which' 的极简 Unix 系统上，可能需要模拟，但这种情况非常罕见
                Return ""
            End Try
        End Function

#End Region
    End Class
End Namespace
