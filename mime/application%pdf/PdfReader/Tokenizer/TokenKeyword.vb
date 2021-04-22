Imports System
Imports System.Collections.Generic
Imports System.ComponentModel

Namespace PdfReader
    Public Class TokenKeyword
        Inherits TokenObject

        Private _Value As PdfReader.ParseKeyword
        Private Shared _lookup As Dictionary(Of String, TokenKeyword)

        Shared Sub New()
            _lookup = New Dictionary(Of String, TokenKeyword)()

            For Each val As Object In [Enum].GetValues(GetType(ParseKeyword))
                Dim name = [Enum].GetName(GetType(ParseKeyword), val)
                Dim keyword = name
                Dim attrs = GetType(ParseKeyword).GetMember(name)(0).GetCustomAttributes(GetType(DescriptionAttribute), False)
                If attrs IsNot Nothing AndAlso attrs.Length > 0 Then keyword = CType(attrs(0), DescriptionAttribute).Description
                Call _lookup.Add(keyword, New TokenKeyword(val))
            Next
        End Sub

        Public Sub New(ByVal keyword As ParseKeyword)
            Value = keyword
        End Sub

        Public Property Value As ParseKeyword
            Get
                Return _Value
            End Get
            Private Set(ByVal value As ParseKeyword)
                _Value = value
            End Set
        End Property

        Public Shared Function GetToken(ByVal keyword As String) As TokenKeyword
            Dim tokenKeyword As TokenKeyword = Nothing
            _lookup.TryGetValue(keyword, tokenKeyword)
            Return tokenKeyword
        End Function
    End Class
End Namespace
