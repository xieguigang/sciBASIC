#Region "Microsoft.VisualBasic::2ea8673eecd4a8bbd78db8d5c2f814ff, Microsoft.VisualBasic.Core\Text\Xml\Serialization\XmlComment.vb"

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

    '     Class XmlCommentAttribute
    ' 
    '         Properties: Value
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Module XmlCommentExtensions
    ' 
    '         Function: GetXmlComment, GetXmlCommentAttribute
    ' 
    ' 
    ' /********************************************************************************/

#End Region

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
