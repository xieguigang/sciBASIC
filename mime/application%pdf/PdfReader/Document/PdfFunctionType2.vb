Imports System
Imports stdNum = System.Math

Namespace PdfReader
    Public Class PdfFunctionType2
        Inherits PdfFunction

        Private _n As Single
        Private _c0 As Single()
        Private _c1 As Single()

        Public Sub New(ByVal parent As PdfObject, ByVal dictionary As PdfDictionary)
            MyBase.New(parent, dictionary)
        End Sub

        Public ReadOnly Property N As PdfInteger
            Get
                Return Dictionary.MandatoryValue(Of PdfInteger)("N")
            End Get
        End Property

        Public ReadOnly Property C0 As PdfArray
            Get
                Return Dictionary.OptionalValue(Of PdfArray)("C0")
            End Get
        End Property

        Public ReadOnly Property C1 As PdfArray
            Get
                Return Dictionary.OptionalValue(Of PdfArray)("C1")
            End Get
        End Property

        Public Overrides Function [Call](ByVal inputs As Single()) As Single()
            If inputs.Length <> 1 Then Throw New ArgumentOutOfRangeException($"Provided with '{inputs.Length}' values but Function Type 2 is defined to take 1 value.")
            Dim input = inputs(0)
            Dim outputs = New Single(_c0.Length - 1) {}

            ' Exponential interpolation between the c0 and c1 values
            For i = 0 To outputs.Length - 1
                outputs(i) = _c0(i) + CSng(stdNum.Pow(input, _n)) * (_c1(i) - _c0(i))
            Next

            Return outputs
        End Function

        Protected Overrides Sub Initialize()
            MyBase.Initialize()

            ' Extract and cache values from the dictionary
            _n = N.Value

            If C0 IsNot Nothing Then
                _c0 = C0.AsNumberArray()
            Else
                _c0 = New Single() {0F}
            End If

            If C1 IsNot Nothing Then
                _c1 = C1.AsNumberArray()
            Else
                _c1 = New Single() {1F}
            End If
        End Sub
    End Class
End Namespace
