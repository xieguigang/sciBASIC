#Region "Microsoft.VisualBasic::2621fd00e98a8aa1a909643797005457, ..\visualbasic_App\Microsoft.VisualBasic.Architecture.Framework\Tools\Network\Protocol\Streams\String.vb"

' Author:
' 
'       asuka (amethyst.asuka@gcmodeller.org)
'       xieguigang (xie.guigang@live.com)
'       xie (genetics@smrucc.org)
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

Imports System.Text
Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Text

Namespace Net.Protocols.Streams

    ''' <summary>
    ''' 字符串序列流
    ''' </summary>
    Public Class [String] : Inherits RawStream

        <XmlAttribute> Public Property value As String

        <XmlAttribute> Public Property Encoding As Encodings
            Get
                Return TextEncodings.GetEncodings(_encoding)
            End Get
            Set(value As Encodings)
                _encoding = value.GetEncodings
            End Set
        End Property

        Dim _encoding As System.Text.Encoding

        Sub New()
        End Sub

        Sub New(s As String, Optional encoding As Encodings = Encodings.UTF8)
            Call Me.New(s, encoding.GetEncodings)
        End Sub

        Sub New(s As String, Optional encoding As Encoding = Nothing)
            _value = s
            _encoding = encoding

            If _encoding Is Nothing Then
                _encoding = System.Text.Encoding.UTF8
            End If
        End Sub

        Sub New(raw As Byte())
            Dim encoding As Byte = raw(Scan0)
            Dim s As Byte() = raw.Skip(1).ToArray

            _encoding = CType(encoding, Encodings).GetEncodings
            _value = _encoding.GetString(s)
        End Sub

        Public Overrides Function ToString() As String
            Return value
        End Function

        Public Overrides Function Serialize() As Byte()
            Dim s As Byte() = _encoding.GetBytes(value)
            Dim buffer As Byte() = New Byte(s.Length) {}

            buffer(Scan0) = CType(Encoding, Byte)
            Call System.Array.ConstrainedCopy(s, Scan0, buffer, 1, s.Length)

            Return buffer
        End Function
    End Class
End Namespace
