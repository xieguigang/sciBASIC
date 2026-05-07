''' <summary>
''' Tracks the state for multi-line string collection during pre-processing.
''' </summary>
Friend Class MultiLineState
    ''' <summary>The type of multi-line string: | (literal) or > (folded).</summary>
    Public Property Style As Char
    ''' <summary>Whether the block scalar has a '+' chomping indicator.</summary>
    Public Property ChompPlus As Boolean
    ''' <summary>Whether the block scalar has a '-' chomping indicator.</summary>
    Public Property ChompMinus As Boolean
    ''' <summary>The indentation indicator for the block scalar (0 if not specified).</summary>
    Public Property IndentIndicator As Integer
    ''' <summary>The key that this multi-line string is associated with.</summary>
    Public Property Key As String
    ''' <summary>The collected lines of the multi-line string.</summary>
    Public Property Lines As New List(Of String)
    ''' <summary>The base indentation level of the first content line.</summary>
    Public Property BaseIndent As Integer = -1
    ''' <summary>The line number where the block scalar started.</summary>
    Public Property StartLineNumber As Integer
End Class