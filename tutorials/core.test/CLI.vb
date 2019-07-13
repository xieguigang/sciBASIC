#Region "Microsoft.VisualBasic::9140f8dec533bc2fddb52916894e07ca, tutorials\core.test\CLI.vb"

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

    ' Module CLI
    ' 
    '     Function: CLIDocumentTest, ExceptionHandlerTest, Main
    ' 
    '     Sub: ParserTest
    ' 
    ' /********************************************************************************/

#End Region

#Region "Microsoft.VisualBasic::ee401c25e9cdc2b8fc874b1113ef8c4b, core.test"

' Author:
' 
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 


' Source file summaries:

' Module CLI
' 
'     Function: CLIDocumentTest, ExceptionHandlerTest, Main
' 
' 

#End Region

#Region "Microsoft.VisualBasic::5aba667920bb9c6fb79d7ef5a020f704, core.test"

' Author:
' 
' 
' Copyright (c) 2018 GPL3 Licensed
' 
' 
' GNU GENERAL PUBLIC LICENSE (GPL3)
' 


' Source file summaries:

' Module CLI
' 
'     Function: CLIDocumentTest, ExceptionHandlerTest, Main
' 
' 
' 

#End Region

Imports System.ComponentModel
Imports Microsoft.VisualBasic.CommandLine
Imports Microsoft.VisualBasic.CommandLine.ManView
Imports Microsoft.VisualBasic.CommandLine.Reflection

<ExceptionHelp("12345", "XXXXXX", "gg@sssss.com")>
Module CLI

    Function Main() As Integer
        Call ParserTest()


        Return GetType(CLI).RunCLI(App.CommandLine)
    End Function

    Sub ParserTest()
        Dim cli$ = "/POST /url ""http://metauhealth.com/api/index.php?act=user&op=verifyBindingManually"" openId=9IofUCKdgfdddddddd5lEeWiUDseee barcode=123456 name=xieguigang /out D:/test.json"
        Dim argv = CLITools.TryParse(cli)

        Pause()
    End Sub

    <ExportAPI("/CLI.docs.test")>
    <Description("Test description text")>
    <Usage("/CLI.docs.test [/echo ""hello world!"" /out <out.txt>]")>
    <Argument("/echo", True, CLITypes.String, PipelineTypes.std_in, AcceptTypes:={GetType(String)},
              Description:="Echo input text.")>
    <Argument("/out", True, CLITypes.File, PipelineTypes.std_out, AcceptTypes:={GetType(String)},
              Description:="The output file location. looooooooooooooooooooooooooooooooooooooooooooong and looooooooooooooooooooooooooooooooooooooooooooooooooong 2404:6800:4008:c00::71 storage.cloud.google.com")>
    <Argument("/b", True, CLITypes.Boolean, Description:="Unknown")>
    <Argument("/x", False, Description:="not sure......................... looooooooooooooooooooooooooooooooooooooooooooong and looooooooooooooooooooooooooooooooooooooooooooooooooong 2404:6800:4008:c00::71 storage.cloud.google.com")>
    Public Function CLIDocumentTest(args As CommandLine) As Integer

    End Function

    <ExportAPI("/ExceptionHandler.Test")>
    <Usage("Whatever")>
    Public Function ExceptionHandlerTest(args As CommandLine) As Integer
        Throw New Exception
    End Function
End Module
