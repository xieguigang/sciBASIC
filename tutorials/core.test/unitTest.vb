#Region "Microsoft.VisualBasic::582aa8308621545cc9a989afc86a8c49, tutorials\core.test\unitTest.vb"

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

    ' Module unitTest
    ' 
    '     Sub: Main
    ' 
    ' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Ranges

Module unitTest

    Sub Main()
        Dim KB As New UnitValue(Of ByteSize)(128 * 1024 * 1024, ByteSize.KB)

        Call KB.__DEBUG_ECHO
        Call KB.Scale(ByteSize.GB).__DEBUG_ECHO
        Call KB.Scale(ByteSize.B).__DEBUG_ECHO
        Call KB.Scale(ByteSize.MB).__DEBUG_ECHO
        Call KB.Scale(ByteSize.TB).__DEBUG_ECHO
        Call KB.Scale(ByteSize.KB).__DEBUG_ECHO
        Call KB.Scale(ByteSize.TB).Scale(ByteSize.MB).__DEBUG_ECHO

        Pause()
    End Sub
End Module
