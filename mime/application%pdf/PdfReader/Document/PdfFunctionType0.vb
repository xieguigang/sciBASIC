#Region "Microsoft.VisualBasic::2cd5731ee6f08bb67c3f8c7f3e3f5462, mime\application%pdf\PdfReader\Document\PdfFunctionType0.vb"

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

    '     Class PdfFunctionType0
    ' 
    '         Properties: BitsPerSample, Decode, Encode, Order, Size
    '                     Stream
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: [Call]
    ' 
    '         Sub: Initialize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System
Imports stdNum = System.Math

Namespace PdfReader
    Public Class PdfFunctionType0
        Inherits PdfFunction

        Private _Stream As PdfReader.PdfStream
        Private _sizeValues As Single()
        Private _bitsPerSampleValue As Integer
        Private _orderValue As Integer
        Private _encodeValues As Single()
        Private _decodeValues As Single()
        Private _samplesValues As Integer()

        Public Sub New(ByVal parent As PdfObject, ByVal stream As PdfStream)
            MyBase.New(parent, stream.Dictionary)
            Me.Stream = stream
        End Sub

        Public Property Stream As PdfStream
            Get
                Return _Stream
            End Get
            Private Set(ByVal value As PdfStream)
                _Stream = value
            End Set
        End Property

        Public ReadOnly Property Size As PdfArray
            Get
                Return Dictionary.MandatoryValue(Of PdfArray)("Size")
            End Get
        End Property

        Public ReadOnly Property BitsPerSample As PdfInteger
            Get
                Return Dictionary.MandatoryValue(Of PdfInteger)("BitsPerSample")
            End Get
        End Property

        Public ReadOnly Property Order As PdfInteger
            Get
                Return Dictionary.OptionalValue(Of PdfInteger)("Order")
            End Get
        End Property

        Public ReadOnly Property Encode As PdfArray
            Get
                Return Dictionary.OptionalValue(Of PdfArray)("Encode")
            End Get
        End Property

        Public ReadOnly Property Decode As PdfArray
            Get
                Return Dictionary.OptionalValue(Of PdfArray)("Decode")
            End Get
        End Property

        Public Overrides Function [Call](ByVal inputs As Single()) As Single()
            If inputs.Length <> _sizeValues.Length Then Throw New ArgumentOutOfRangeException($"Provided with '{inputs.Length}' values but Function Type 0 is defined to take '{_sizeValues.Length}' values.")
            Dim sampleNumber = 0
            Dim i = 0, d = 0

            While i < inputs.Length
                ' Limit check the input to the domain range
                inputs(i) = stdNum.Max(_domainValues(d), stdNum.Min(_domainValues(d + 1), inputs(i)))

                ' Interpolate each input from the domain to the set of encoded values
                inputs(i) = CInt(Interpolate(inputs(i), _domainValues(d), _domainValues(d + 1), _encodeValues(d), _encodeValues(d + 1)))

                ' Limit check to the encoded values
                inputs(i) = stdNum.Max(_encodeValues(d), stdNum.Min(_encodeValues(d + 1), inputs(i)))

                ' Find sample position within array
                sampleNumber += CInt(inputs(i) * _samplesValues(i))
                i += 1
                d += 2
            End While

            Dim numOutputs As Integer = _rangeValues.Length / 2

            ' Find the offset in bits to the first output sample value
            Dim bitsOffset = sampleNumber * _bitsPerSampleValue * numOutputs
            Dim outputs = New Single(numOutputs - 1) {}

            i = 0
            d = 0

            While i < numOutputs
                Dim sampleValue = 0
                Dim sampleValueMax = 0

                Select Case _bitsPerSampleValue
                    Case 8
                        ' Find the byte that contains the output
                        sampleValue = Stream.ValueAsBytes(bitsOffset / 8)
                        sampleValueMax = 255
                    Case Else
                        Throw New NotImplementedException($"Function Type 0  with BitsPerSample of '{BitsPerSample}' not implemented.")
                End Select

                ' Interpolate each output from te sample to the decode values
                outputs(i) = Interpolate(sampleValue, 0, sampleValueMax, _decodeValues(d), _decodeValues(d + 1))

                ' Move to next output sample
                bitsOffset += _bitsPerSampleValue
                i += 1
                d += 2
            End While

            Return outputs
        End Function

        Protected Overrides Sub Initialize()
            MyBase.Initialize()

            ' Extract and cache values from the dictionary
            _sizeValues = Size.AsNumberArray()
            _bitsPerSampleValue = BitsPerSample.Value
            _orderValue = If(Order IsNot Nothing, Order.Value, 1)

            If Encode IsNot Nothing Then
                _encodeValues = Encode.AsNumberArray()
            Else
                _encodeValues = New Single(_sizeValues.Length - 1) {}
                Dim i = 0, j = 1

                While i < _sizeValues.Length
                    _encodeValues(j) = _sizeValues(i) - 1
                    i += 1
                    j += 2
                End While
            End If

            If Decode IsNot Nothing Then
                _decodeValues = Decode.AsNumberArray()
            Else
                _decodeValues = _rangeValues
            End If

            ' Calculate number of samples by dimension
            _samplesValues = New Integer(_sizeValues.Length - 1) {}
            _samplesValues(0) = 1

            For i = 1 To _sizeValues.Length - 1
                _samplesValues(i) = CInt(_samplesValues(i - 1) * _sizeValues(i))
            Next
        End Sub
    End Class
End Namespace

