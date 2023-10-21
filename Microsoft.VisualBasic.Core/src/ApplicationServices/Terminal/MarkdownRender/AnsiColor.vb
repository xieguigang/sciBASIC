Imports System.Globalization
Imports System.Runtime.InteropServices
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel

Namespace ApplicationServices.Terminal
#Region "License Header"
    ' This Source Code Form is subject to the terms of the Mozilla Public
    ' License, v. 2.0. If a copy of the MPL was not distributed with this
    ' file, You can obtain one at https://mozilla.org/MPL/2.0/.
#End Region


    ''' <summary>
    ''' ANSI color definitions for the terminal.
    ''' Each color has a different code depending on if it's applied as a foreground or background color.
    ''' </summary>
    ''' <remarks>https://en.wikipedia.org/wiki/ANSI_escape_code#Colors</remarks>
    Public Structure AnsiColor
        Implements IEquatable(Of AnsiColor)

        Private ReadOnly foregroundCode As String
        Private ReadOnly backgroundCode As String
        Private ReadOnly friendlyName As String

        Public Shared ReadOnly Black As New AnsiColor("30", "40", NameOf(Black))
        Public Shared ReadOnly Red As New AnsiColor("31", "41", NameOf(Red))
        Public Shared ReadOnly Green As New AnsiColor("32", "42", NameOf(Green))
        Public Shared ReadOnly Yellow As New AnsiColor("33", "43", NameOf(Yellow))
        Public Shared ReadOnly Blue As New AnsiColor("34", "44", NameOf(Blue))
        Public Shared ReadOnly Magenta As New AnsiColor("35", "45", NameOf(Magenta))
        Public Shared ReadOnly Cyan As New AnsiColor("36", "46", NameOf(Cyan))
        Public Shared ReadOnly White As New AnsiColor("37", "47", NameOf(White))
        Public Shared ReadOnly BrightBlack As New AnsiColor("90", "100", NameOf(BrightBlack))
        Public Shared ReadOnly BrightRed As New AnsiColor("91", "101", NameOf(BrightRed))
        Public Shared ReadOnly BrightGreen As New AnsiColor("92", "102", NameOf(BrightGreen))
        Public Shared ReadOnly BrightYellow As New AnsiColor("93", "103", NameOf(BrightYellow))
        Public Shared ReadOnly BrightBlue As New AnsiColor("94", "104", NameOf(BrightBlue))
        Public Shared ReadOnly BrightMagenta As New AnsiColor("95", "105", NameOf(BrightMagenta))
        Public Shared ReadOnly BrightCyan As New AnsiColor("96", "106", NameOf(BrightCyan))
        Public Shared ReadOnly BrightWhite As New AnsiColor("97", "107", NameOf(BrightWhite))

        Private Shared ReadOnly ansiColorNames As Dictionary(Of String, AnsiColor) = GetType(AnsiColor) _
            .GetFields(DataFramework.PublicShared) _
            .Where(Function(f) f.FieldType Is GetType(AnsiColor)) _
            .ToDictionary(Function(f) f.Name,
                          Function(f)
                              Return CType(f.GetValue(Nothing), AnsiColor)
                          End Function, StringComparer.OrdinalIgnoreCase)

        Public Shared Function Rgb(ByVal r As Byte, ByVal g As Byte, ByVal b As Byte) As AnsiColor
            Return New AnsiColor($"38;2;{r};{g};{b}", $"48;2;{r};{g};{b}", $"#{r:X2}{g:X2}{b:X2}")
        End Function

        Public Sub New(ByVal foregroundCode As String, ByVal backgroundCode As String, ByVal friendlyName As String)
            Me.foregroundCode = foregroundCode
            Me.backgroundCode = backgroundCode
            Me.friendlyName = friendlyName
        End Sub

        Public Function GetEscapeSequence(Optional ByVal type As Type = Type.Foreground) As String
            Return AnsiEscapeCodes.ToAnsiEscapeSequence(GetCode(type))
        End Function
        Friend Function GetCode(Optional ByVal type As Type = Type.Foreground) As String
            Return If(type = AnsiColor.Type.Foreground, foregroundCode, backgroundCode)
        End Function

        Public Overrides Function Equals(ByVal obj As Object) As Boolean
            Dim tempVar As Boolean = TypeOf obj Is AnsiColor
            Dim other As AnsiColor = If(tempVar, CType(obj, AnsiColor), Nothing)
            Return tempVar AndAlso Equals(other)
        End Function
        Public Overloads Function Equals(ByVal other As AnsiColor) As Boolean Implements IEquatable(Of AnsiColor).Equals
            Return foregroundCode = other.foregroundCode AndAlso backgroundCode = other.backgroundCode
        End Function
        Public Overrides Function GetHashCode() As Integer
            Return foregroundCode.GetHashCode()
        End Function

        Public Shared Operator =(ByVal left As AnsiColor, ByVal right As AnsiColor) As Boolean
            Return EqualityComparer(Of AnsiColor).Default.Equals(left, right)
        End Operator
        Public Shared Operator <>(ByVal left As AnsiColor, ByVal right As AnsiColor) As Boolean
            Return Not (left = right)
        End Operator

        Public Overrides Function ToString() As String
            Return friendlyName
        End Function

        Public Shared Function TryParse(ByVal input As String, <Out()> ByRef result As AnsiColor, Optional HasUserOptedOutFromColor As Boolean = False) As Boolean
            If HasUserOptedOutFromColor Then
                result = White
                Return True
            End If

            Dim r As Byte
            Dim g As Byte
            Dim b As Byte

#If NETCOREAPP Then
            Dim span = input.AsSpan()

            If input.StartsWith("#"c) AndAlso span.Length = 7 AndAlso
                Byte.TryParse(span.Slice(1, 2), NumberStyles.AllowHexSpecifier, Nothing, r) AndAlso
                Byte.TryParse(span.Slice(3, 2), NumberStyles.AllowHexSpecifier, Nothing, g) AndAlso
                Byte.TryParse(span.Slice(5, 2), NumberStyles.AllowHexSpecifier, Nothing, b) Then
                result = Rgb(r, g, b)
                Return True
            End If
#Else
            Throw New NotImplementedException
#End If

            Dim color As AnsiColor
            If ansiColorNames.TryGetValue(input, color) Then
                result = color
                Return True
            End If

            result = Nothing
            Return False
        End Function

        Public Enum Type
            Foreground
            Background
        End Enum
    End Structure
End Namespace