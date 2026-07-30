Namespace Models

    ''' <summary>
    ''' A single symbol (function / public / data) extracted from the symbol stream.
    ''' </summary>
    Public Class Symbol

        ''' <summary>
        ''' Symbol / function name.
        ''' </summary>
        Public Property Name As String

        ''' <summary>
        ''' Section index (segment) where the symbol lives, 1-based. 0 for flat addressing.
        ''' </summary>
        Public Property Section As UShort

        ''' <summary>
        ''' Offset of the symbol from the start of <see cref="Section"/> (or from image base
        ''' when <see cref="Section"/> is 0).
        ''' </summary>
        Public Property Offset As UInteger

        ''' <summary>
        ''' Length of the symbol in bytes (function body size), 0 when unknown.
        ''' </summary>
        Public Property Length As UInteger

        ''' <summary>
        ''' Kind of symbol (Public / Function / Data / ...). See <see cref="SymbolKind"/>.
        ''' </summary>
        Public Property Kind As SymbolKind

        ''' <summary>
        ''' Flags of the symbol (e.g. code / function).
        ''' </summary>
        Public Property Flags As UShort

        Public Overrides Function ToString() As String
            Return $"{Kind} {Name} @[{Section}:{Offset:X}+#{Length}]"
        End Function
    End Class
End Namespace