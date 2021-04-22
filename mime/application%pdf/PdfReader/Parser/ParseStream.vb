#Region "Microsoft.VisualBasic::ba7da2a3b6ff5c1ba7318285d6c77cdd, mime\application%pdf\PdfReader\Parser\ParseStream.vb"

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

    '     Class ParseStream
    ' 
    '         Properties: Dictionary, HasFilter, StreamBytes, Value, ValueAsBytes
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: DecodeBytes, FlateDecode
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports System.Collections.Generic
Imports System.IO
Imports System.IO.Compression
Imports System.Text

Namespace PdfReader
    Public Class ParseStream
        Inherits ParseObjectBase

        Private _Dictionary As PdfReader.ParseDictionary, _StreamBytes As Byte()

        Public Sub New(ByVal dictionary As ParseDictionary, ByVal streamBytes As Byte())
            Me.Dictionary = dictionary
            Me.StreamBytes = streamBytes
        End Sub

        Public Property Dictionary As ParseDictionary
            Get
                Return _Dictionary
            End Get
            Private Set(ByVal value As ParseDictionary)
                _Dictionary = value
            End Set
        End Property

        Public Property StreamBytes As Byte()
            Get
                Return _StreamBytes
            End Get
            Private Set(ByVal value As Byte())
                _StreamBytes = value
            End Set
        End Property

        Public ReadOnly Property HasFilter As Boolean
            Get
                Return Dictionary.ContainsName("Filter")
            End Get
        End Property

        Public ReadOnly Property Value As String
            Get
                Return Encoding.ASCII.GetString(DecodeBytes(StreamBytes))
            End Get
        End Property

        Public ReadOnly Property ValueAsBytes As Byte()
            Get
                Return DecodeBytes(StreamBytes)
            End Get
        End Property

        Public Function DecodeBytes(ByVal bytes As Byte()) As Byte()
            If HasFilter Then
                ' Get the filtering as an array to be applied in order (if a single filter then convert from Name to an Array of one entry)
                Dim obj = Dictionary("Filter")
                Dim filters As ParseArray = TryCast(obj, ParseArray)
                If filters Is Nothing AndAlso (TypeOf obj Is ParseName) Then filters = New ParseArray(New List(Of ParseObjectBase)() From {
                    obj
                })

                For Each filter As ParseName In filters.Objects

                    Select Case filter.Value
                        Case "Fl", "FlateDecode"
                            bytes = FlateDecode(bytes)
                        Case "DCT", "DCTDecode"
                        Case Else
                            Throw New NotImplementedException($"Cannot process unrecognized stream filter '{filter.Value}'.")
                    End Select
                Next
            End If

            Return bytes
        End Function

        Private Function FlateDecode(ByVal bytes As Byte()) As Byte()
            Using inputStream As MemoryStream = New MemoryStream(bytes)

                Using outputStream As MemoryStream = New MemoryStream()

                    Using decodeStream As DeflateStream = New DeflateStream(inputStream, CompressionMode.Decompress)
                        ' Skip the zlib 2 byte header
                        inputStream.Position = 2
                        decodeStream.CopyTo(outputStream)
                        bytes = outputStream.GetBuffer()
                    End Using
                End Using
            End Using

            If Dictionary.ContainsName("Predictor") Then Throw New NotImplementedException($"Cannot process FlatDecode predictors.")
            Return bytes
        End Function
    End Class
End Namespace

