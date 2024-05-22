#Region "Microsoft.VisualBasic::ba28388678cc39dee2b865c7423830fa, Microsoft.VisualBasic.Core\src\Extensions\MsgBoxStyle.vb"

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

    '   Total Lines: 137
    '    Code Lines: 33 (24.09%)
    ' Comment Lines: 103 (75.18%)
    '    - Xml Docs: 0.00%
    ' 
    '   Blank Lines: 1 (0.73%)
    '     File Size: 4.35 KB


    ' Enum MsgBoxStyle
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' Enum MsgBoxResult
    ' 
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

#If NETSTANDARD Then
'
' 摘要:
'     Indicates which buttons to display when calling the MsgBox function.
<Flags>
Public Enum MsgBoxStyle
    '
    ' 摘要:
    '     Application modal message box. This member is equivalent to the Visual Basic
    '     constant vbApplicationModal.
    ApplicationModal = 0
    '
    ' 摘要:
    '     First button is default. This member is equivalent to the Visual Basic constant
    '     vbDefaultButton1.
    DefaultButton1 = 0
    '
    ' 摘要:
    '     OK button only (default). This member is equivalent to the Visual Basic constant
    '     vbOKOnly.
    OkOnly = 0
    '
    ' 摘要:
    '     OK and Cancel buttons. This member is equivalent to the Visual Basic constant
    '     vbOKCancel.
    OkCancel = 1
    '
    ' 摘要:
    '     Abort, Retry, and Ignore buttons. This member is equivalent to the Visual Basic
    '     constant vbAbortRetryIgnore.
    AbortRetryIgnore = 2
    '
    ' 摘要:
    '     Yes, No, and Cancel buttons. This member is equivalent to the Visual Basic constant
    '     vbYesNoCancel.
    YesNoCancel = 3
    '
    ' 摘要:
    '     Yes and No buttons. This member is equivalent to the Visual Basic constant vbYesNo.
    YesNo = 4
    '
    ' 摘要:
    '     Retry and Cancel buttons. This member is equivalent to the Visual Basic constant
    '     vbRetryCancel.
    RetryCancel = 5
    '
    ' 摘要:
    '     Critical message. This member is equivalent to the Visual Basic constant vbCritical.
    Critical = 16
    '
    ' 摘要:
    '     Warning query. This member is equivalent to the Visual Basic constant vbQuestion.
    Question = 32
    '
    ' 摘要:
    '     Warning message. This member is equivalent to the Visual Basic constant vbExclamation.
    Exclamation = 48
    '
    ' 摘要:
    '     Information message. This member is equivalent to the Visual Basic constant vbInformation.
    Information = 64
    '
    ' 摘要:
    '     Second button is default. This member is equivalent to the Visual Basic constant
    '     vbDefaultButton2.
    DefaultButton2 = 256
    '
    ' 摘要:
    '     Third button is default. This member is equivalent to the Visual Basic constant
    '     vbDefaultButton3.
    DefaultButton3 = 512
    '
    ' 摘要:
    '     System modal message box. This member is equivalent to the Visual Basic constant
    '     vbSystemModal.
    SystemModal = 4096
    '
    ' 摘要:
    '     Help text. This member is equivalent to the Visual Basic constant vbMsgBoxHelp.
    MsgBoxHelp = 16384
    '
    ' 摘要:
    '     Foreground message box window. This member is equivalent to the Visual Basic
    '     constant vbMsgBoxSetForeground.
    MsgBoxSetForeground = 65536
    '
    ' 摘要:
    '     Right-aligned text. This member is equivalent to the Visual Basic constant vbMsgBoxRight.
    MsgBoxRight = 524288
    '
    ' 摘要:
    '     Right-to-left reading text (Hebrew and Arabic systems). This member is equivalent
    '     to the Visual Basic constant vbMsgBoxRtlReading.
    MsgBoxRtlReading = 1048576
End Enum

'
' 摘要:
'     Indicates which button was pressed on a message box, returned by the MsgBox function.
Public Enum MsgBoxResult
    '
    ' 摘要:
    '     OK button was pressed. This member is equivalent to the Visual Basic constant
    '     vbOK.
    Ok = 1
    '
    ' 摘要:
    '     Cancel button was pressed. This member is equivalent to the Visual Basic constant
    '     vbCancel.
    Cancel = 2
    '
    ' 摘要:
    '     Abort button was pressed. This member is equivalent to the Visual Basic constant
    '     vbAbort.
    Abort = 3
    '
    ' 摘要:
    '     Retry button was pressed. This member is equivalent to the Visual Basic constant
    '     vbRetry.
    Retry = 4
    '
    ' 摘要:
    '     Ignore button was pressed. This member is equivalent to the Visual Basic constant
    '     vbIgnore.
    Ignore = 5
    '
    ' 摘要:
    '     Yes button was pressed. This member is equivalent to the Visual Basic constant
    '     vbYes.
    Yes = 6
    '
    ' 摘要:
    '     No button was pressed. This member is equivalent to the Visual Basic constant
    '     vbNo.
    No = 7
End Enum
#End If
