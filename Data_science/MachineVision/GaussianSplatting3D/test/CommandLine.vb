' ---------------------------------------------------------------------------
' 极简命令行选项解析（--name value / --flag）
' ---------------------------------------------------------------------------

Imports System.Globalization

Friend Class CommandLine

    Private ReadOnly _values As New Dictionary(Of String, String)(StringComparer.OrdinalIgnoreCase)
    Private ReadOnly _flags As New HashSet(Of String)(StringComparer.OrdinalIgnoreCase)

    Public Sub New(args As String())
        Dim i = 0

        While i < args.Length
            Dim token = args(i)

            If token.StartsWith("--") Then
                Dim name = token.Substring(2)

                If i + 1 < args.Length AndAlso Not args(i + 1).StartsWith("--") Then
                    _values(name) = args(i + 1)
                    i += 2
                Else
                    _flags.Add(name)
                    i += 1
                End If
            Else
                i += 1
            End If
        End While
    End Sub

    Public Function Has(name As String) As Boolean
        Return _flags.Contains(name) OrElse _values.ContainsKey(name)
    End Function

    Public Function GetInt(name As String, defaultValue As Integer) As Integer
        Dim text As String = Nothing

        If _values.TryGetValue(name, text) Then
            Dim value As Integer

            If Integer.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, value) Then Return value
        End If

        Return defaultValue
    End Function

    Public Function GetDouble(name As String, defaultValue As Double) As Double
        Dim text As String = Nothing

        If _values.TryGetValue(name, text) Then
            Dim value As Double

            If Double.TryParse(text, NumberStyles.Float, CultureInfo.InvariantCulture, value) Then Return value
        End If

        Return defaultValue
    End Function

    Public Function GetString(name As String, defaultValue As String) As String
        Dim text As String = Nothing

        If _values.TryGetValue(name, text) Then Return text

        Return defaultValue
    End Function

End Class
