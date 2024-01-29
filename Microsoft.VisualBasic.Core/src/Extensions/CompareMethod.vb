#If NETSTANDARD1_2_OR_GREATER Then
    '
    ' 摘要:
    '     Indicates how to compare strings when calling comparison functions.
    Public Enum CompareMethod
        '
        ' 摘要:
        '     Performs a binary comparison. This member is equivalent to the Visual Basic constant
        '     vbBinaryCompare.
        Binary = 0
        '
        ' 摘要:
        '     Performs a textual comparison. This member is equivalent to the Visual Basic
        '     constant vbTextCompare.
        Text = 1
    End Enum
#End If