Namespace Imaging

#If NET8_0_OR_GREATER Then
    Public Class Font

        Public ReadOnly Property Name As String
        Public ReadOnly Property Size As Single
        Public ReadOnly Property SizeInPoints As Single
        Public ReadOnly Property Style As FontStyle

        Public ReadOnly Property Height As Single

        Sub New(name As String, size As Single, Optional style As FontStyle = FontStyle.Regular)
            Me.Name = name
            Me.Size = size
            Me.Style = style
        End Sub

        Sub New(baseFont As Font, style As FontStyle)
            _Name = baseFont.Name
            _Size = baseFont.Size
            _Style = style
        End Sub

        Public Function Clone() As Object
            Return New Font(Name, Size, Style)
        End Function

    End Class

    <Flags>
    Public Enum FontStyle
        Regular = 0
        Bold = 1
        Italic = 2
        Underline = 4
        Strikeout = 8
    End Enum
#End If
End Namespace