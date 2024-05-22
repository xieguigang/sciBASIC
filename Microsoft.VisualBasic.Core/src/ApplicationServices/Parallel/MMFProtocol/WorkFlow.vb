#Region "Microsoft.VisualBasic::aa9a6c158df2b0f6ff46e52ef7b79e28, Microsoft.VisualBasic.Core\src\ApplicationServices\Parallel\MMFProtocol\WorkFlow.vb"

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

    '   Total Lines: 42
    '    Code Lines: 26 (61.90%)
    ' Comment Lines: 12 (28.57%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 4 (9.52%)
    '     File Size: 1.87 KB


    '     Module WorkFlow
    ' 
    '         Function: FolkProc
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.Parallel.MMFProtocol.MapStream
Imports Microsoft.VisualBasic.SecurityString

Namespace Parallel.MMFProtocol

    Public Module WorkFlow

        Const map As String = "/std_map"

        ''' <summary>
        ''' 创建出一个子进程，然后按照命令行参数<paramref name="CLI"/>执行制定的命令，同时通过内存映射传递复杂参数，最后结束后通过内存映射传递回数据
        ''' 主要是通过内存映射减少数据IO的时间，加快计算流程
        ''' </summary>
        ''' <typeparam name="TIn"></typeparam>
        ''' <typeparam name="TOut"></typeparam>
        ''' <param name="exe"></param>
        ''' <param name="CLI"></param>
        ''' <param name="[in]"></param>
        ''' <param name="writer"></param>
        ''' <param name="reader"></param>
        ''' <returns></returns>
        <Extension>
        Public Function FolkProc(Of TIn, TOut)(exe As String,
                                               CLI As String,
                                               [in] As TIn,
                                               writer As Func(Of TIn, Byte()),
                                               reader As Func(Of Byte(), TOut)) As TOut
            Dim data As Byte() = writer([in])
            Dim uid As String = (Now.ToString & [in].ToString & CLI).GetMd5Hash
            Dim socket As New MSWriter(uid, data.Length + 1024)
            CLI = CLI & $" {map} {uid}"
            socket.WriteStream(data)
            Dim IO As New IORedirectFile(exe, CLI)
            Call IO.Run()
            data = socket.Read.byteData
            Dim obj As TOut = reader(data)
            Return obj
        End Function
    End Module
End Namespace
