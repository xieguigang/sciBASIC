Imports System.Text

Namespace Logging

    ''' <summary>
    ''' The types enumeration of the log file message.
    ''' </summary>
    ''' <remarks></remarks>
    Public Enum MSG_TYPES As Integer

        ''' <summary>
        ''' The normal information message.[WHITE]
        ''' </summary>
        ''' <remarks></remarks>
        INF
        ''' <summary>
        ''' The program error information message.[RED]
        ''' </summary>
        ''' <remarks></remarks>
        ERR
        ''' <summary>
        ''' Warnning message from the program.[YELLOW]
        ''' </summary>
        ''' <remarks></remarks>
        WRN
        ''' <summary>
        ''' The program debug information message.[BLUE]
        ''' </summary>
        ''' <remarks></remarks>
        DEBUG
    End Enum

End Namespace