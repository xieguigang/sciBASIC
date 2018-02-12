#Region "Microsoft.VisualBasic::51f2c0a11b5c20cb2990641215564fd1, CLI.vb"

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
Imports Microsoft.VisualBasic.CommandLine.Reflection

<ExceptionHelp("12345", "XXXXXX", "gg@sssss.com")>
Module CLI

    Function Main() As Integer
        Return GetType(CLI).RunCLI(App.CommandLine)
    End Function

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


