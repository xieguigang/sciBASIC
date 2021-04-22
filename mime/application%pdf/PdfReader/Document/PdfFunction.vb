Imports System
Imports Microsoft.VisualBasic.Language

Namespace PdfReader
    Public MustInherit Class PdfFunction
        Inherits PdfObject

        Private __domainValues As Single(), __rangeValues As Single(), _Dictionary As PdfReader.PdfDictionary

        Protected Property _domainValues As Single()
            Get
                Return __domainValues
            End Get
            Private Set(ByVal value As Single())
                __domainValues = value
            End Set
        End Property

        Protected Property _rangeValues As Single()
            Get
                Return __rangeValues
            End Get
            Private Set(ByVal value As Single())
                __rangeValues = value
            End Set
        End Property

        Public Sub New(ByVal parent As PdfObject, ByVal dictionary As PdfDictionary)
            MyBase.New(parent)
            Me.Dictionary = dictionary
            Initialize()
        End Sub

        Public Property Dictionary As PdfDictionary
            Get
                Return _Dictionary
            End Get
            Private Set(ByVal value As PdfDictionary)
                _Dictionary = value
            End Set
        End Property

        Public ReadOnly Property FunctionType As PdfInteger
            Get
                Return Dictionary.MandatoryValue(Of PdfInteger)("FunctionType")
            End Get
        End Property

        Public ReadOnly Property Domain As PdfArray
            Get
                Return Dictionary.MandatoryValue(Of PdfArray)("Domain")
            End Get
        End Property

        Public ReadOnly Property Range As PdfArray
            Get
                Return Dictionary.OptionalValue(Of PdfArray)("Range")
            End Get
        End Property

        Public MustOverride Function [Call](ByVal inputs As Single()) As Single()

        Public Shared Function FromObject(ByVal parent As PdfObject, ByVal obj As PdfObject) As PdfFunction
            Dim referece As New Value(Of PdfObjectReference)
            If (referece = TryCast(obj, PdfObjectReference)) IsNot Nothing Then Return FromObject(parent, parent.Document.ResolveReference(referece))
            Dim stream As New Value(Of PdfStream)
            If (stream = TryCast(obj, PdfStream)) IsNot Nothing Then Return FromStream(parent, stream)
            Dim dictionary As New Value(Of PdfDictionary)
            If (dictionary = TryCast(obj, PdfDictionary)) IsNot Nothing Then Return FromDictionary(parent, dictionary)
            Throw New NotImplementedException($"Function cannot be created from object of type '{obj.GetType().Name}'.")
        End Function

        Public Shared Function FromStream(ByVal parent As PdfObject, ByVal stream As PdfStream) As PdfFunction
            Dim functionType = stream.Dictionary.MandatoryValue(Of PdfInteger)("FunctionType")

            Select Case functionType.Value
                Case 0 ' Sampled Function
                    Return New PdfFunctionType0(parent, stream)
                Case Else
                    Throw New NotImplementedException($"Function type '{functionType.Value}' not implemented.")
            End Select
        End Function

        Public Shared Function FromDictionary(ByVal parent As PdfObject, ByVal dictionary As PdfDictionary) As PdfFunction
            Dim functionType = dictionary.MandatoryValue(Of PdfInteger)("FunctionType")

            Select Case functionType.Value
                Case 2 ' Exponential Interpolation Function
                    Return New PdfFunctionType2(parent, dictionary)
                Case 3 ' Stitching Function
                    Return New PdfFunctionType3(parent, dictionary)
                Case Else
                    Throw New NotImplementedException($"Function type '{functionType.Value}' not implemented.")
            End Select
        End Function

        Protected Overridable Sub Initialize()
            _domainValues = Domain.AsNumberArray()

            ' Range is optional for some types of function
            If Range IsNot Nothing Then _rangeValues = Range.AsNumberArray()
        End Sub

        Protected Function Interpolate(ByVal value As Single, ByVal domain1 As Single, ByVal domain2 As Single, ByVal range1 As Single, ByVal range2 As Single) As Single
            Return (value - domain1) * (range2 - range1) / (domain2 - domain1) + range1
        End Function
    End Class
End Namespace
