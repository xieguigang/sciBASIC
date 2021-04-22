Imports System
Imports System.Collections.Generic

Namespace PdfReader
    Public Class ParseDictionary
        Inherits ParseObjectBase

        Private _names As List(Of String)
        Private _values As List(Of ParseObjectBase)
        Private _dictionary As Dictionary(Of String, ParseObjectBase)

        Public Sub New(ByVal names As List(Of String), ByVal values As List(Of ParseObjectBase))
            _names = names
            _values = values
        End Sub

        Public ReadOnly Property Count As Integer
            Get
                Return If(_names IsNot Nothing, _names.Count, _dictionary.Count)
            End Get
        End Property

        Public Function ContainsName(ByVal name As String) As Boolean
            BuildDictionary()
            Return _dictionary.ContainsKey(name)
        End Function

        Public ReadOnly Property Keys As Dictionary(Of String, ParseObjectBase).KeyCollection
            Get
                BuildDictionary()
                Return _dictionary.Keys
            End Get
        End Property

        Public ReadOnly Property Values As Dictionary(Of String, ParseObjectBase).ValueCollection
            Get
                BuildDictionary()
                Return _dictionary.Values
            End Get
        End Property

        Public Function GetEnumerator() As Dictionary(Of String, ParseObjectBase).Enumerator
            BuildDictionary()
            Return _dictionary.GetEnumerator()
        End Function

        Default Public Property Item(ByVal name As String) As ParseObjectBase
            Get
                BuildDictionary()
                Return _dictionary(name)
            End Get
            Set(ByVal value As ParseObjectBase)
                BuildDictionary()
                _dictionary(name) = value
            End Set
        End Property

        Public Function OptionalValue(Of T As ParseObjectBase)(ByVal name As String) As T
            BuildDictionary()
            Dim entry As ParseObjectBase = Nothing

            If _dictionary.TryGetValue(name, entry) Then
                If TypeOf entry Is T Then
                    Return entry
                Else
                    Throw New ApplicationException($"Dictionary entry is type '{entry.GetType().Name}' instead of mandatory type of '{GetType(T).Name}'.")
                End If
            End If

            Return Nothing
        End Function

        Public Function MandatoryValue(Of T As ParseObjectBase)(ByVal name As String) As T
            BuildDictionary()
            Dim entry As ParseObjectBase = Nothing

            If _dictionary.TryGetValue(name, entry) Then
                If TypeOf entry Is T Then
                    Return entry
                Else
                    Throw New ApplicationException($"Dictionary entry is type '{entry.GetType().Name}' instead of mandatory type of '{GetType(T).Name}'.")
                End If
            Else
                Throw New ApplicationException($"Dictionary is missing mandatory name '{name}'.")
            End If
        End Function

        Private Sub BuildDictionary()
            If _dictionary Is Nothing Then
                _dictionary = New Dictionary(Of String, ParseObjectBase)()
                Dim count = Me.Count

                For i = 0 To count - 1
                    _dictionary.Add(_names(i), _values(i))
                Next

                _names = Nothing
                _values = Nothing
            End If
        End Sub
    End Class
End Namespace
