Imports System

Namespace Microsoft.VisualBasic
    <Flags> _
    Public Enum MsgBoxStyle
        ' Fields
        AbortRetryIgnore = 2
        ApplicationModal = 0
        Critical = &H10
        DefaultButton1 = 0
        DefaultButton2 = &H100
        DefaultButton3 = &H200
        Exclamation = &H30
        Information = &H40
        MsgBoxHelp = &H4000
        MsgBoxRight = &H80000
        MsgBoxRtlReading = &H100000
        MsgBoxSetForeground = &H10000
        OkCancel = 1
        OkOnly = 0
        Question = &H20
        RetryCancel = 5
        SystemModal = &H1000
        YesNo = 4
        YesNoCancel = 3
    End Enum
End Namespace

