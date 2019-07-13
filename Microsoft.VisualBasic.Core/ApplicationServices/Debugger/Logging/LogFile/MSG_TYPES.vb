#Region "Microsoft.VisualBasic::a5a87ff1247563c2eb0d404bbf3c6774, Microsoft.VisualBasic.Core\ApplicationServices\Debugger\Logging\LogFile\MSG_TYPES.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Enum MSG_TYPES
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.ComponentModel

Namespace ApplicationServices.Debugging.Logging

    ''' <summary>
    ''' Logging levels, the types enumeration of the log file message.
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum MSG_TYPES As Integer

        ''' <summary>
        ''' The normal information message.[WHITE]
        ''' </summary>
        ''' <remarks></remarks>
        <Description("INFOM")> INF = ConsoleColor.White
        ''' <summary>
        ''' The program error information message.[RED]
        ''' </summary>
        ''' <remarks></remarks>
        <Description("ERROR")> ERR = ConsoleColor.Red
        ''' <summary>
        ''' Warnning message from the program.[YELLOW]
        ''' </summary>
        ''' <remarks></remarks>
        <Description("WARNG")> WRN = ConsoleColor.Yellow
        ''' <summary>
        ''' The program debug information message.[BLUE]
        ''' </summary>
        ''' <remarks></remarks>
        <Description("DEBUG")> DEBUG = ConsoleColor.Blue
    End Enum
End Namespace
