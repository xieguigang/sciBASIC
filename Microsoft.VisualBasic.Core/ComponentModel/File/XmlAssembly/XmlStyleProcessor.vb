Imports System.IO
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language

Namespace ComponentModel

    Public Class XmlStyleProcessor

        Public Property href As String
        Public Property alternate As Boolean
        Public Property title As String
        Public Property media As String

        Public ReadOnly Property type As String
            Get
                Select Case href.ExtensionSuffix.ToLower
                    Case "css" : Return "text/css"
                    Case "xsl" : Return ""
                    Case Else
                        Throw New InvalidDataException("Unknown file type!")
                End Select
            End Get
        End Property

        Private Iterator Function getAttributes() As IEnumerable(Of NamedValue(Of String))
            If href.StringEmpty Then
                Throw New EntryPointNotFoundException("No style file was specific!")
            Else
                Yield New NamedValue(Of String) With {.Name = NameOf(href), .Value = href}
            End If
            If Not title.StringEmpty Then
                Yield New NamedValue(Of String) With {.Name = NameOf(title), .Value = title}
            End If
            If Not media.StringEmpty Then
                Yield New NamedValue(Of String) With {.Name = NameOf(media), .Value = media}
            End If

            Yield New NamedValue(Of String) With {.Name = NameOf(type), .Value = type}
            Yield New NamedValue(Of String) With {
                    .Name = NameOf(alternate),
                    .Value = "no" Or "yes".When(alternate)
                }
        End Function

        Public Overrides Function ToString() As String
            Dim attrs As NamedValue(Of String)() = getAttributes.ToArray
            Dim attrVals$() = attrs _
                    .Select(Function(a) $"{a.Name}=""{a.Value}""") _
                    .ToArray
            Dim declares$ = $"<?xml-stylesheet {attrVals.JoinBy(" ")} ?>"

            Return declares
        End Function
    End Class
End Namespace