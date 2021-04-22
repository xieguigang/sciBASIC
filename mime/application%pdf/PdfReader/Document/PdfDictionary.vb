Imports System
Imports System.Collections.Generic
Imports Microsoft.VisualBasic.Language

Namespace PdfReader
    Public Class PdfDictionary
        Inherits PdfObject

        Private _wrapped As Dictionary(Of String, PdfObject)

        Public Sub New(ByVal parent As PdfObject, ByVal dictionary As ParseDictionary)
            MyBase.New(parent, dictionary)
        End Sub

        Public Overrides Sub Visit(ByVal visitor As IPdfObjectVisitor)
            visitor.Visit(Me)
        End Sub

        Public ReadOnly Property ParseDictionary As ParseDictionary
            Get
                Return TryCast(ParseObject, ParseDictionary)
            End Get
        End Property

        Public ReadOnly Property Count As Integer
            Get
                Return ParseDictionary.Count
            End Get
        End Property

        Public Function ContainsName(ByVal name As String) As Boolean
            Return ParseDictionary.ContainsName(name)
        End Function

        Public ReadOnly Property Keys As Dictionary(Of String, PdfObject).KeyCollection
            Get
                WrapAllNames()
                Return _wrapped.Keys
            End Get
        End Property

        Public ReadOnly Property Values As Dictionary(Of String, PdfObject).ValueCollection
            Get
                WrapAllNames()
                Return _wrapped.Values
            End Get
        End Property

        Public Function GetEnumerator() As Dictionary(Of String, PdfObject).Enumerator
            WrapAllNames()
            Return _wrapped.GetEnumerator()
        End Function

        Default Public ReadOnly Property Item(ByVal name As String) As PdfObject
            Get
                WrapName(name)
                Return _wrapped(name)
            End Get
        End Property

        Public Function OptionalValue(Of T As PdfObject)(ByVal name As String) As T
            Dim entry As PdfObject = Nothing

            If ParseDictionary.ContainsName(name) Then
                WrapName(name)

                If _wrapped.TryGetValue(name, entry) Then
                    If TypeOf entry Is T Then
                        Return entry
                    Else
                        Throw New ApplicationException($"Dictionary entry is type '{entry.GetType().Name}' instead of mandatory type of '{GetType(T).Name}'.")
                    End If
                End If
            End If

            Return Nothing
        End Function

        Public Function OptionalValueRef(Of T As PdfObject)(ByVal name As String) As T
            Dim entry As PdfObject = Nothing
            Dim reference As New Value(Of PdfObjectReference)

            If ParseDictionary.ContainsName(name) Then
                WrapName(name)

                If _wrapped.TryGetValue(name, entry) Then
                    If (reference = TryCast(entry, PdfObjectReference)) IsNot Nothing Then
                        If Document.IndirectObjects.ContainsId(reference.Value.Id) Then
                            Dim id = Document.IndirectObjects(reference.Value.Id)

                            If id.ContainsGen(reference.Value.Gen) Then
                                entry = Document.ResolveReference(reference)

                                If TypeOf entry Is T Then
                                    Return entry
                                Else
                                    Throw New ApplicationException($"Dictionary entry is type '{entry.GetType().Name}' instead of mandatory type of '{GetType(T).Name}'.")
                                End If
                            End If
                        Else
                            Return Nothing
                        End If
                    ElseIf TypeOf entry Is T Then
                        Return entry
                    End If

                    Throw New ApplicationException($"Dictionary entry is type '{entry.GetType().Name}' instead of mandatory type of '{GetType(T).Name}'.")
                End If
            End If

            Return Nothing
        End Function

        Public Function OptionalDateTime(ByVal name As String) As PdfDateTime
            Dim str = OptionalValue(Of PdfString)(name)
            If str IsNot Nothing Then Return New PdfDateTime(Me, str)
            Return Nothing
        End Function

        Public Function MandatoryValue(Of T As PdfObject)(ByVal name As String) As T
            Dim entry As PdfObject = Nothing

            If ParseDictionary.ContainsName(name) Then
                WrapName(name)

                If _wrapped.TryGetValue(name, entry) Then
                    If TypeOf entry Is T Then
                        Return entry
                    Else
                        Throw New ApplicationException($"Dictionary entry is type '{entry.GetType().Name}' instead of mandatory type of '{GetType(T).Name}'.")
                    End If
                Else
                    Throw New ApplicationException($"Dictionary is missing mandatory name '{name}'.")
                End If
            Else
                Throw New ApplicationException($"Dictionary is missing mandatory name '{name}'.")
            End If
        End Function

        Public Function MandatoryValueRef(Of T As PdfObject)(ByVal name As String) As T
            Dim entry As PdfObject = Nothing
            Dim reference As New Value(Of PdfObjectReference)

            If ParseDictionary.ContainsName(name) Then
                WrapName(name)

                If _wrapped.TryGetValue(name, entry) Then
                    If (reference = TryCast(entry, PdfObjectReference)) IsNot Nothing Then
                        entry = Document.ResolveReference(reference)
                        If TypeOf entry Is T Then Return entry
                    ElseIf TypeOf entry Is T Then
                        Return entry
                    End If

                    Throw New ApplicationException($"Dictionary entry is type '{entry.GetType().Name}' instead of mandatory type of '{GetType(T).Name}'.")
                Else
                    Throw New ApplicationException($"Dictionary is missing mandatory name '{name}'.")
                End If
            Else
                Throw New ApplicationException($"Dictionary is missing mandatory name '{name}'.")
            End If
        End Function

        Private Sub WrapName(ByVal name As String)
            If _wrapped Is Nothing Then _wrapped = New Dictionary(Of String, PdfObject)()
            If Not _wrapped.ContainsKey(name) Then _wrapped.Add(name, WrapObject(ParseDictionary(name)))
        End Sub

        Private Sub WrapAllNames()
            If _wrapped Is Nothing Then _wrapped = New Dictionary(Of String, PdfObject)()

            ' Are there any dictionary entries that still need wrapping?
            If ParseDictionary.Count > _wrapped.Count Then
                For Each name In ParseDictionary.Keys
                    If Not _wrapped.ContainsKey(name) Then _wrapped.Add(name, WrapObject(ParseDictionary(name)))
                Next
            End If
        End Sub
    End Class
End Namespace
