Imports System.Globalization
Imports System.Text

Namespace Xpt

    Public Class SimpleDateFormat

        Private formatStr As String

        Public Sub New(format As String)
            formatStr = format
        End Sub

        Public Function format(dt As Date) As String
            Return dt.ToString(formatStr, CultureInfo.InvariantCulture)
        End Function
    End Class


    Public Class XPTReaderUtils

        Public Shared DATE5_FORMAT As SimpleDateFormat = New SimpleDateFormat("ddMMM ")
        Public Shared DATE6_FORMAT As SimpleDateFormat = New SimpleDateFormat(" ddMMM ")
        Public Shared DATE7_FORMAT As SimpleDateFormat = New SimpleDateFormat("ddMMMYY ")
        Public Shared DATE8_FORMAT As SimpleDateFormat = New SimpleDateFormat(" ddMMMYY ")
        Public Shared DATE9_FORMAT As SimpleDateFormat = New SimpleDateFormat("ddMMMYYYY")
        Public Shared DATE11_FORMAT As SimpleDateFormat = New SimpleDateFormat("dd-MMM-YYYY")

        Public Shared Function getString(line As Byte(), offset As Integer, len As Integer) As String

            Dim data = New Byte(len - 1) {}
            For i = 0 To len - 1
                data(i) = line(offset + i)
            Next
            Return Encoding.UTF8.GetString(data).Trim()
        End Function

        Public Shared Function getInteger(line As Byte(), offset As Integer, len As Integer) As Integer
            Dim val = getString(line, offset, len)
            If val.Length <= 0 Then
                Return 0
            End If
            Return Integer.Parse(val)
        End Function

        Public Shared Function getShort(line As Byte(), offset As Integer, len As Integer) As Short
            Dim val = getString(line, offset, 2)
            If val.Length <= 0 Then
                Return 0
            End If
            Return Short.Parse(val)
        End Function

        Public Shared Function getPrimitiveInteger(buffer As Byte(), offset As Integer) As Integer

            Dim val = (buffer(offset + 0) And &HFF) << 24 Or (buffer(offset + 1) And &HFF) << 16 Or (buffer(offset + 2) And &HFF) << 8 Or (buffer(offset + 3) And &HFF) << 0
            Return val
        End Function

        Public Shared Function getPrimitiveShort(buffer As Byte(), offset As Integer) As Short
            Dim val As Short = (buffer(offset + 0) And &HFF) << 8 Or (buffer(offset + 1) And &HFF) << 0
            Return val
        End Function

        Public Shared Function convertSASDate9ToString(dtformat As String, [date] As Double) As String
            Dim num As Integer = [date]
            Dim format = DATE9_FORMAT
            If "date5".Equals(dtformat) Then
                format = DATE5_FORMAT
            ElseIf "date6".Equals(dtformat) Then
                format = DATE6_FORMAT
            ElseIf "date7".Equals(dtformat) Then
                format = DATE7_FORMAT
            ElseIf "date8".Equals(dtformat) Then
                format = DATE8_FORMAT
            ElseIf "date9".Equals(dtformat) Then
                format = DATE9_FORMAT
            ElseIf "date11".Equals(dtformat) Then
                format = DATE11_FORMAT
            End If

            Dim cal As Date = New DateTime()
            cal = New DateTime(1960, 0, 1)
            cal.AddDays(num)

            Dim formatted = format.format(cal)
            Return formatted
        End Function
    End Class

End Namespace
