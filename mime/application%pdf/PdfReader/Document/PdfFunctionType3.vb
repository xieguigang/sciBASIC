Imports System
Imports System.Collections.Generic

Namespace PdfReader
    Public Class PdfFunctionType3
        Inherits PdfFunction

        Private _functions As List(Of PdfFunction)
        Private _boundValues As Single()
        Private _encodeValues As Single()

        Public Sub New(ByVal parent As PdfObject, ByVal dictionary As PdfDictionary)
            MyBase.New(parent, dictionary)
        End Sub

        Public ReadOnly Property Functions As PdfArray
            Get
                Return Dictionary.MandatoryValue(Of PdfArray)("Functions")
            End Get
        End Property

        Public ReadOnly Property Bounds As PdfArray
            Get
                Return Dictionary.MandatoryValue(Of PdfArray)("Bounds")
            End Get
        End Property

        Public ReadOnly Property Encode As PdfArray
            Get
                Return Dictionary.MandatoryValue(Of PdfArray)("Encode")
            End Get
        End Property

        Public Overrides Function [Call](ByVal inputs As Single()) As Single()
            If inputs.Length <> 1 Then Throw New ArgumentOutOfRangeException($"Provided with '{inputs.Length}' values but Function Type 3 is defined to take 1 value.")

            ' Find the function that handles values below the Bounds value
            Dim i = 0, d = 0

            While i < _boundValues.Length
                If inputs(0) < _boundValues(i) Then Return _functions(i).Call(New Single() {Interpolate(inputs(0), If(i = 0, _domainValues(0), _boundValues(i - 1)), _boundValues(i), _encodeValues(d), _encodeValues(d + 1))})
                i += 1
                d += 2
            End While

            ' Value must be above the last bound, so use the last function
            Return _functions(_functions.Count - 1).Call(New Single() {Interpolate(inputs(0), _boundValues(_boundValues.Length - 1), _domainValues(_domainValues.Length - 1), _encodeValues(_encodeValues.Length - 2), _encodeValues(_encodeValues.Length - 1))})
        End Function

        Protected Overrides Sub Initialize()
            MyBase.Initialize()

            ' Extract and cache values from the dictionary
            _functions = New List(Of PdfFunction)()

            For Each obj In Functions.Objects
                _functions.Add(FromObject(Me, obj))
            Next

            _boundValues = Bounds.AsNumberArray()
            _encodeValues = Encode.AsNumberArray()
        End Sub
    End Class
End Namespace
