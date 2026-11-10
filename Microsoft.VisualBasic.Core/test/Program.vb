#Region "Microsoft.VisualBasic::cd712ccc91a7097feada336b404a3e39, Microsoft.VisualBasic.Core\test\Program.vb"

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

'   Total Lines: 61
'    Code Lines: 39 (63.93%)
' Comment Lines: 10 (16.39%)
'    - Xml Docs: 0.00%
' 
'   Blank Lines: 12 (19.67%)
'     File Size: 1.98 KB


' Module Program
' 
'     Sub: Main
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.CommandLine.Reflection

Module Program
    ''' <summary>
    ''' dispatches the commandline to the ExportAPI test entry points
    ''' </summary>
    ''' <returns></returns>
    Public Function Main(args As String()) As Integer
        Return GetType(Program).RunCLI(App.CommandLine, AddressOf test)
    End Function

    ''' <summary>
    ''' runs the FTP client commandline demo only:
    ''' test.exe --ftp [host] [remotePath] [localPath] [user] [password] [--port N] [--ssl]
    ''' </summary>
    ''' <returns></returns>
    <ExportAPI("--ftp")>
    Public Function test_ftp() As Integer
        Call FtpTest.Run(App.CommandLine.Parameters).GetAwaiter().GetResult()
        Return 0
    End Function

    ''' <summary>
    ''' runs the markdown console renderer regression checks only: test.exe --markdown
    ''' </summary>
    ''' <returns></returns>
    <ExportAPI("--markdown")>
    Public Function test_markdown() As Integer
        Call markdownRenderVerify.Run()
        Call markdownDisplayTest.Main1()
        Return 0
    End Function

    ''' <summary>
    ''' test commandline arguments
    ''' </summary>
    ''' <param name="a">the first required integer number</param>
    ''' <param name="b">the second required integer number</param>
    ''' <param name="c">an optional scale factor of the expression, defaults to -100</param>
    ''' <returns></returns>
    <ExportAPI("--test-arguments")>
    Public Function test_arguments(a As Integer, b As Integer, Optional c As Double = -100) As Integer
        Call Console.WriteLine((a + b) * c)
        Return 0
    End Function

    ''' <summary>
    ''' runs the jsonl store / WAL (Data.Repository.TextStore) demo checks only: test.exe --jsonl
    ''' </summary>
    ''' <returns></returns>
    <ExportAPI("--jsonl")>
    Public Function test_jsonl() As Integer
        Call jsonlStoreTest.Run()
        Return 0
    End Function

    ''' <summary>
    ''' runs the runtime debug audit module (DebugAudit) regression checks only: test.exe --debugaudit
    ''' </summary>
    ''' <returns></returns>
    <ExportAPI("--debugaudit")>
    Public Function test_debugaudit() As Integer
        Call DebugAuditSelfTest.Run()
        Return 0
    End Function

    ''' <summary>
    ''' runs the SIMD math module correctness regression and benchmark only: test.exe --simd
    ''' </summary>
    ''' <returns></returns>
    <ExportAPI("--simd")>
    Public Function test_simd() As Integer
        Call SIMDTest.Run()
        Return 0
    End Function

    ''' <summary>
    ''' runs the NumericTable dataframe(...) builder regression checks only: test.exe --numeric-table
    ''' </summary>
    ''' <returns></returns>
    <ExportAPI("--numeric-table")>
    Public Function test_numericTable() As Integer
        Call NumericTableExtensionsTest.Run()
        Return 0
    End Function

    ''' <summary>
    ''' other test
    ''' </summary>
    ''' <returns></returns>
    Public Function test() As Integer
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

        Call streamTest.Main1()

        Return 0
    End Function

End Module
