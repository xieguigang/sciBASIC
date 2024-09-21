Namespace Imaging

#If NET8_0_OR_GREATER Then
    Public Class Font

        Public ReadOnly Property FamilyName As String
        Public ReadOnly Property Size As Single
        Public ReadOnly Property SizeInPoints As Single
        Public ReadOnly Property FontStyle As FontStyle

        Sub New(name As String, size As Single, Optional style As FontStyle = FontStyle.Regular)
            Me.FamilyName = name
            Me.Size = size
            Me.FontStyle = style
        End Sub

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