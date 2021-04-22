Imports System
Imports System.Collections.Generic

Namespace PdfReader
    Public MustInherit Class PdfPageInherit
        Inherits PdfDictionary

        Public Sub New(ByVal parent As PdfObject, ByVal dictionary As ParseDictionary)
            MyBase.New(parent, dictionary)
        End Sub

        Public ReadOnly Property Inherit As PdfPageInherit
            Get
                Return TypedParent(Of PdfPageInherit)()
            End Get
        End Property

        Public MustOverride Sub FindLeafPages(ByVal pages As List(Of PdfPage))

        Public Function InheritableOptionalValue(Of T As PdfObject)(ByVal name As String) As T
            ' Try and get the value from this dictionary
            Dim here = OptionalValue(Of T)(name)

            ' If not present then inherit it from the parent
            If here Is Nothing AndAlso Inherit IsNot Nothing Then here = Inherit.InheritableOptionalValue(Of T)(name)
            Return here
        End Function

        Public Function InheritableOptionalRefValue(Of T As PdfObject)(ByVal name As String) As T
            ' Try and get the value from this dictionary
            Dim here = OptionalValueRef(Of T)(name)

            ' If not present then inherit it from the parent
            If here Is Nothing AndAlso Inherit IsNot Nothing Then here = Inherit.InheritableOptionalRefValue(Of T)(name)
            Return here
        End Function

        Public Function InheritableMandatoryValue(Of T As PdfObject)(ByVal name As String) As T
            ' Try and get the value from this dictionary
            Dim here = OptionalValue(Of T)(name)

            ' If not present then inherit it from the parent
            If here Is Nothing AndAlso Inherit IsNot Nothing Then here = Inherit.InheritableMandatoryValue(Of T)(name)

            ' Enforce mandatory existence
            If here Is Nothing Then Throw New ApplicationException($"Page is missing a mandatory inheritable value for '{name}'.")
            Return here
        End Function

        Public Function InheritableMandatoryRefValue(Of T As PdfObject)(ByVal name As String) As T
            ' Try and get the value from this dictionary
            Dim here = OptionalValueRef(Of T)(name)

            ' If not present then inherit it from the parent
            If here Is Nothing AndAlso Inherit IsNot Nothing Then here = Inherit.InheritableMandatoryRefValue(Of T)(name)

            ' Enforce mandatory existence
            If here Is Nothing Then Throw New ApplicationException($"Page is missing a mandatory inheritable value for '{name}'.")
            Return here
        End Function
    End Class
End Namespace
