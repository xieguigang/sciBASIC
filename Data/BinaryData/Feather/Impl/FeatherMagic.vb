Imports System

Namespace Impl
    Friend Module FeatherMagic
        Public Const ARROW_ALIGNMENT As Integer = 8  ' 64-bits per https://arrow.apache.org/docs/memory_layout.html
        Public Const FEATHER_VERSION As Integer = 2
        Public Const NULL_BITMASK_ALIGNMENT As Integer = 8
        Public Const MAGIC_HEADER_SIZE As Integer = 4
        Public Const MAGIC_HEADER As Integer = Microsoft.VisualBasic.AscW("F"c) << 8 * 0 Or Microsoft.VisualBasic.AscW("E"c) << 8 * 1 Or Microsoft.VisualBasic.AscW("A"c) << 8 * 2 Or Microsoft.VisualBasic.AscW("1"c) << 8 * 3 ' 'FEA1', little endian

        Public ReadOnly DATETIME_EPOCH As Date = New DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc)
    End Module
End Namespace
