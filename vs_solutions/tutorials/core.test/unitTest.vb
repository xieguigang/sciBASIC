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
