Imports System

Namespace Microsoft.VisualBasic
    <Flags> _
    Public Enum VbStrConv
        ' Fields
        Hiragana = &H20
        Katakana = &H10
        LinguisticCasing = &H400
        Lowercase = 2
        Narrow = 8
        None = 0
        ProperCase = 3
        SimplifiedChinese = &H100
        TraditionalChinese = &H200
        Uppercase = 1
        Wide = 4
    End Enum
End Namespace

