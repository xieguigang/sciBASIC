#Region "ea16b4a2a99f7d8b15c94bf14ff1aed4, ..\Microsoft.VisualBasic.Architecture.Framework\Scripting\Casting.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports System.Text.RegularExpressions
Imports Microsoft.VisualBasic.Imaging

Namespace Scripting

    Public Module Casting

        ''' <summary>
        ''' DirectCast(obj, T)
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        <Extension> Public Function [As](Of T)(obj) As T
            If obj Is Nothing Then
                Return Nothing
            End If
            Return DirectCast(obj, T)
        End Function

        Private Function val(s As String) As Double
            If String.IsNullOrEmpty(s) Then
                Return 0R
            ElseIf String.Equals(s, "NaN", StringComparison.Ordinal) Then
                Return Double.NaN
            End If
            s = s.Replace(",", "")
            Return Conversion.Val(s)
        End Function

        Public Function CastString(obj As String) As String
            Return obj
        End Function

        Public Function CastChar(obj As String) As Char
            Return If(String.IsNullOrEmpty(obj), NIL, obj.First)
        End Function

        ''' <summary>
        ''' 出错会返回默认是0
        ''' </summary>
        ''' <param name="obj"></param>
        ''' <returns></returns>
        Public Function CastInteger(obj As String) As Integer
            Return CInt(val(obj))
        End Function

        Public Function CastDouble(obj As String) As Double
            Return val(obj)
        End Function

        Public Function CastLong(obj As String) As Long
            Return CLng(val(obj))
        End Function

        Public Function CastBoolean(obj As String) As Boolean
            Return obj.getBoolean
        End Function

        Public Function CastCharArray(obj As String) As Char()
            Return obj.ToArray
        End Function

        Public Function CastDate(obj As String) As DateTime
            Return DateTime.Parse(obj)
        End Function

        Public Function CastStringBuilder(obj As String) As StringBuilder
            Return New StringBuilder(obj)
        End Function

        Public Function CastCommandLine(obj As String) As CommandLine.CommandLine
            Return CommandLine.TryParse(obj)
        End Function

        Public Function CastImage(path As String) As Image
            Return LoadImage(path)
        End Function

        Public Function CastFileInfo(path As String) As FileInfo
            Return FileIO.FileSystem.GetFileInfo(path)
        End Function

        Public Function CastGDIPlusDeviceHandle(path As String) As GDIPlusDeviceHandle
            Return GDIPlusDeviceHandleFromImageFile(path)
        End Function

        Public Function CastColor(rgb As String) As Color
            Return rgb.ToColor
        End Function

        Public Function CastFont(face As String) As Font
            Return New Font(face, 10)
        End Function

        Public Function CastIPEndPoint(addr As String) As System.Net.IPEndPoint
            Return New Net.IPEndPoint(addr).GetIPEndPoint
        End Function

        Public Function CastLogFile(path As String) As Logging.LogFile
            Return New Logging.LogFile(path)
        End Function

        Public Function CastProcess(exe As String) As Process
            Return Process.Start(exe)
        End Function

        Public Function CastRegexOptions(name As String) As RegexOptions
            If String.Equals(name, RegexExtensions.NameOf.Compiled, StringComparison.OrdinalIgnoreCase) Then
                Return RegexOptions.Compiled
            ElseIf String.Equals(name, RegexExtensions.NameOf.CultureInvariant, StringComparison.OrdinalIgnoreCase) Then
                Return RegexOptions.CultureInvariant
            ElseIf String.Equals(name, RegexExtensions.NameOf.ECMAScript, StringComparison.OrdinalIgnoreCase) Then
                Return RegexOptions.ECMAScript
            ElseIf String.Equals(name, RegexExtensions.NameOf.ExplicitCapture, StringComparison.OrdinalIgnoreCase) Then
                Return RegexOptions.ExplicitCapture
            ElseIf String.Equals(name, RegexExtensions.NameOf.IgnoreCase, StringComparison.OrdinalIgnoreCase) Then
                Return RegexOptions.IgnoreCase
            ElseIf String.Equals(name, RegexExtensions.NameOf.IgnorePatternWhitespace, StringComparison.OrdinalIgnoreCase) Then
                Return RegexOptions.IgnorePatternWhitespace
            ElseIf String.Equals(name, RegexExtensions.NameOf.Multiline, StringComparison.OrdinalIgnoreCase) Then
                Return RegexOptions.Multiline
            ElseIf String.Equals(name, RegexExtensions.NameOf.RightToLeft, StringComparison.OrdinalIgnoreCase) Then
                Return RegexOptions.RightToLeft
            ElseIf String.Equals(name, RegexExtensions.NameOf.Singleline, StringComparison.OrdinalIgnoreCase) Then
                Return RegexOptions.Singleline
            Else
                Return RegexOptions.None
            End If
        End Function
    End Module
End Namespace
