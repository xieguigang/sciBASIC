Imports System

Namespace Microsoft.VisualBasic
    <Flags> _
    Public Enum FileAttribute
        ' Fields
        Archive = &H20
        Directory = &H10
        Hidden = 2
        Normal = 0
        [ReadOnly] = 1
        System = 4
        Volume = 8
    End Enum
End Namespace

