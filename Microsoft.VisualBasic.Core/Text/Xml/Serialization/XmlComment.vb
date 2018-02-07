Imports System.Reflection
Imports System.Runtime.CompilerServices
Imports System.Xml

Namespace Text.Xml.Serialization

    <AttributeUsage(AttributeTargets.Property, AllowMultiple:=False)>
    Public Class XmlCommentAttribute : Inherits Attribute

        Public Property Value As String

        Sub New(value As String)
            _Value = value
        End Sub

        Public Overrides Function ToString() As String
            Return Value
        End Function
    End Class

    Public Module XmlCommentExtensions

        Const XmlCommentPropertyPostfix$ = "XmlComment"

        <Extension>
        Public Function GetXmlCommentAttribute(type As Type, <CallerMemberName> Optional memberName$ = Nothing) As XmlCommentAttribute
            Dim member = type.GetProperty(memberName)

            If (member Is Nothing) Then
                Return Nothing
            Else
                Dim attr = member.GetCustomAttribute(Of XmlCommentAttribute)()
                Return attr
            End If
        End Function

        <Extension>
        Public Function GetXmlComment(type As Type, <CallerMemberName> Optional memberName$ = "") As XmlComment
            Dim attr = GetXmlCommentAttribute(type, memberName)

            If (attr Is Nothing) Then
                If (memberName.EndsWith(XmlCommentPropertyPostfix)) Then
                    attr = type.GetXmlCommentAttribute(memberName.Substring(0, memberName.Length - XmlCommentPropertyPostfix.Length))
                End If
            End If

            If (attr Is Nothing OrElse String.IsNullOrEmpty(attr.Value)) Then
                Return Nothing
            Else
                Return New XmlDocument().CreateComment(attr.Value)
            End If
        End Function
    End Module
End Namespace