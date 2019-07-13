#Region "Microsoft.VisualBasic::c61d31cbf3bfcf6a60745eb5670cb52f, mime\text%yaml\1.1\Base\Emitter.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
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



    ' /********************************************************************************/

    ' Summaries:

    '     Class Emitter
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: DecreaseIntent, IncreaseIntent, (+2 Overloads) Write, (+2 Overloads) WriteClose, WriteLine
    '                   WriteSeparator, WriteWhitespace
    ' 
    '         Sub: WriteDelayed, WriteIndent, WriteMeta
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO

Namespace Grammar11

    Friend Class Emitter

        ReadOnly m_stream As TextWriter

        Private m_indent As Integer = 0
        Private m_isNeedWhitespace As Boolean = False
        Private m_isNeedSeparator As Boolean = False
        Private m_isNeedLineBreak As Boolean = False

        Public Sub New(writer As TextWriter)
            If writer Is Nothing Then
                Throw New ArgumentNullException(NameOf(writer))
            End If
            m_stream = writer
        End Sub

        Public Function IncreaseIntent() As Emitter
            m_indent += 1
            Return Me
        End Function

        Public Function DecreaseIntent() As Emitter
            If m_indent = 0 Then
                Throw New Exception("Increase/decrease intent mismatch")
            End If
            m_indent -= 1
            Return Me
        End Function

        Public Function Write([char] As Char) As Emitter
            WriteDelayed()
            m_stream.Write([char])

            Return Me
        End Function

        Public Function Write([string] As String) As Emitter
            If [string] <> String.Empty Then
                WriteDelayed()
                m_stream.Write([string])
            End If

            Return Me
        End Function

        Public Function WriteClose([char] As Char) As Emitter
            m_isNeedSeparator = False
            m_isNeedWhitespace = False
            m_isNeedLineBreak = False
            Return Write([char])
        End Function

        Public Function WriteClose([string] As String) As Emitter
            m_isNeedSeparator = False
            m_isNeedWhitespace = False
            Return Write([string])
        End Function

        Public Function WriteWhitespace() As Emitter
            m_isNeedWhitespace = True
            Return Me
        End Function

        Public Function WriteSeparator() As Emitter
            m_isNeedSeparator = True
            Return Me
        End Function

        Public Function WriteLine() As Emitter
            m_isNeedLineBreak = True
            Return Me
        End Function

        Public Sub WriteMeta(type As MetaType, value As String)
            Write("%").Write(type.ToString()).WriteWhitespace()
            Write(value).WriteLine()
        End Sub

        Private Sub WriteDelayed()
            If m_isNeedLineBreak Then
                m_stream.Write(ControlChars.Lf)
                m_isNeedSeparator = False
                m_isNeedWhitespace = False
                m_isNeedLineBreak = False
                WriteIndent()
            End If
            If m_isNeedSeparator Then
                m_stream.Write(","c)
                m_isNeedSeparator = False
            End If
            If m_isNeedWhitespace Then
                m_stream.Write(" "c)
                m_isNeedWhitespace = False
            End If
        End Sub

        Private Sub WriteIndent()
            For i As Integer = 0 To m_indent * 2 - 1
                m_stream.Write(" "c)
            Next
        End Sub
    End Class
End Namespace
