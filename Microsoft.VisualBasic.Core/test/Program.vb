#Region "Microsoft.VisualBasic::319fc3e7b06ed60f2ea803087ec9ee1e, Microsoft.VisualBasic.Core\test\Program.vb"

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

    '   Total Lines: 41
    '    Code Lines: 28 (68.29%)
    ' Comment Lines: 4 (9.76%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 9 (21.95%)
    '     File Size: 1.24 KB


    ' Module Program
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports System

Module Program
    Sub Main(args As String())
        ' FTP 客户端命令行测试入口:
        '   test.exe --ftp <host> <remotePath> [localPath] [user] [password] [--port N] [--ssl]
        If args.Length > 0 AndAlso args(0) = "--ftp" Then
            Call FtpTest.Run(args.Skip(1).ToArray()).GetAwaiter().GetResult()
            Return
        End If

        ' runs the markdown console renderer regression checks only:
        '   test.exe --markdown
        If args.Length > 0 AndAlso args(0) = "--markdown" Then
            Call markdownRenderVerify.Run()
            Call markdownDisplayTest.Main1()
            Return
        End If

        Call qgramTestSearch.Run()
        Call progrsssBarTest.testLoop()
        Call streamTest.dataUriStreamtest()

        Call logprint()
        Call memoryTest.runTest()
        Call numberParserTest.Main1()

        Call logfiletest.readerTest()
        Call group_test.RunGroup()

        Call enumeratorTestProgram.Mai2n()
        Call terminalTest.Main1()

        Console.WriteLine("Hello World!")

        Call markdownRenderVerify.Run()

        Call SIMDTest.Main1()
        Call streamTest.Main1()
    End Sub
End Module
