Imports System.Collections.Generic
Imports System.Drawing

Namespace NetworkEditor.Models

    ''' <summary>
    ''' 分组名 -> System.Drawing.Color 映射，并提供默认调色板
    ''' </summary>
    Public Class GroupColorMap

        Private ReadOnly map As New Dictionary(Of String, Color)
        Private paletteIndex As Integer = 0

        Private Shared ReadOnly Palette As Color() = {
            Color.FromArgb(&HFF36E2C2),
            Color.FromArgb(&HFF4A90D9),
            Color.FromArgb(&HFFFFB454),
            Color.FromArgb(&HFFFF5C5C),
            Color.FromArgb(&HFF9B6DFF),
            Color.FromArgb(&HFF5CD6FF),
            Color.FromArgb(&HFF7CE38B),
            Color.FromArgb(&HFFFF8FA3),
            Color.FromArgb(&HFFE8C35A),
            Color.FromArgb(&HFF6FE0C2),
            Color.FromArgb(&HFFC28DFF),
            Color.FromArgb(&HFFB0B8C4)
        }

        Public ReadOnly Property Groups As String()
            Get
                Return map.Keys.ToArray()
            End Get
        End Property

        Default Public Property Item(group As String) As Color
            Get
                If map.ContainsKey(group) Then
                    Return map(group)
                Else
                    Return Color.FromArgb(&HFF9AA4B2)
                End If
            End Get
            Set(value As Color)
                map(group) = value
            End Set
        End Property

        Public Function Contains(group As String) As Boolean
            Return map.ContainsKey(group)
        End Function

        Public Function Add(group As String, Optional color As Color? = Nothing) As Color
            Dim c As Color
            If color Is Nothing Then
                c = Palette(paletteIndex Mod Palette.Length)
                paletteIndex += 1
            Else
                c = color.Value
            End If
            map(group) = c
            Return c
        End Function

        Public Sub Remove(group As String)
            If map.ContainsKey(group) Then
                Call map.Remove(group)
            End If
        End Sub

        Public Function NextColor() As Color
            Dim c = Palette(paletteIndex Mod Palette.Length)
            paletteIndex += 1
            Return c
        End Function
    End Class

End Namespace
