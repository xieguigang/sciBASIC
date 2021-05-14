#Region "Microsoft.VisualBasic::b909b540252635d1a7b179838160e385, Microsoft.VisualBasic.Core\src\ApplicationServices\FileSystem\TempFileSystem.vb"

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

    '     Class TempFileSystem
    ' 
    '         Function: __sysTEMP, CreateTempFilePath, GenerateTemp, GetAppSysTempFile
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports Microsoft.VisualBasic.Linq
Imports FS = Microsoft.VisualBasic.FileIO.FileSystem

Namespace ApplicationServices

    ''' <summary>
    ''' return name and handle of a temporary file safely
    ''' </summary>
    Public Class TempFileSystem

        ''' <summary>
        ''' Get temp file name in app system temp directory.
        ''' </summary>
        ''' <param name="ext"></param>
        ''' <param name="sessionID">It is recommended that use <see cref="App.PID"/> for this parameter.</param>
        ''' <returns></returns>
        '''
        Public Shared Function GetAppSysTempFile(Optional ext$ = ".tmp", Optional sessionID$ = "", Optional prefix$ = Nothing) As String
            Return CreateTempFilePath(App.SysTemp, ext, sessionID, prefix)
        End Function

        Public Shared Function CreateTempFilePath(tmpdir$, Optional ext$ = ".tmp", Optional sessionID$ = "", Optional prefix$ = Nothing) As String
            Dim tmp As String = tmpdir & "/" & App.GetNextUniqueName(prefix) & ext

            tmp = GenerateTemp(tmp, sessionID)
            tmp.DoCall(AddressOf FS.GetParentPath).DoCall(AddressOf FS.CreateDirectory)
            tmp = FS.GetFileInfo(tmp).FullName.Replace("\", "/")

            Return tmp
        End Function

        ''' <summary>
        ''' create temp file path
        ''' </summary>
        ''' <param name="sysTemp">临时文件路径</param>
        ''' <returns></returns>
        '''
        Public Shared Function GenerateTemp(sysTemp$, sessionID$) As String
            Dim dirt As String = FS.GetParentPath(sysTemp)
            Dim name As String = FS.GetFileInfo(sysTemp).Name

            sysTemp = $"{dirt}/{App.AssemblyName}/{sessionID}/{name}"

            Return sysTemp
        End Function

        ''' <summary>
        ''' <see cref="FS.GetParentPath"/>(<see cref="FS.GetTempFileName"/>)
        ''' 当临时文件夹被删除掉了的时候，会出现崩溃。。。。所以弃用改用读取环境变量
        ''' </summary>
        ''' <returns></returns>
        Friend Shared Function __sysTEMP() As String
            Dim dir As String = Environment.GetEnvironmentVariable("TMP") ' Linux系统可能没有这个东西

            If String.IsNullOrEmpty(dir) Then
                dir = Path.GetTempPath
            End If

            Try
                Call FS.CreateDirectory(dir)
            Catch ex As Exception
                ' 不知道应该怎样处理，但是由于只是得到一个路径，所以在这里干脆忽略掉这个错误就可以了
                Call New Exception(dir, ex).PrintException
            End Try

            Return dir
        End Function
    End Class
End Namespace
