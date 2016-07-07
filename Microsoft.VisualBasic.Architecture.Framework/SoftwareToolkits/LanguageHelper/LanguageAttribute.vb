#Region "Microsoft.VisualBasic::42df0ff96bc945a8c19d0779b1668e7b, ..\VisualBasic_AppFramework\Microsoft.VisualBasic.Architecture.Framework\SoftwareToolkits\LanguageHelper\LanguageAttribute.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

#End Region

Namespace Logging.LanguageHelper

    <AttributeUsage(AttributeTargets.Property Or
        AttributeTargets.Field Or
        AttributeTargets.Enum, allowmultiple:=True, inherited:=True)>
    Public Class LanguageAttribute : Inherits Attribute
        Dim Language As LanguageHelper.Languages
        Dim Text As String

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Language">The language of the target object.</param>
        ''' <param name="Context"></param>
        ''' <remarks></remarks>
        Sub New(Language As LanguageHelper.Languages, Context As String)
            Me.Language = Language
            Me.Text = Context
        End Sub

        Public Overrides Function ToString() As String
            Return String.Format("@{0} ""{1}""", Language.ToString, Text)
        End Function

        Shared Narrowing Operator CType(e As LanguageAttribute) As LanguageHelper.Languages
            Return e.Language
        End Operator

        Shared Narrowing Operator CType(e As LanguageAttribute) As String
            Return e.Text
        End Operator
    End Class

    <AttributeUsage(AttributeTargets.Property Or
        AttributeTargets.Field Or
        AttributeTargets.Enum, allowmultiple:=True, inherited:=True)>
    Public Class LanguageManagerAttribute : Inherits Attribute
        Dim TagData As String

        Sub New(Tag As String)
            TagData = Tag
        End Sub

        Shared Narrowing Operator CType(e As LanguageManagerAttribute) As String
            Return e.TagData
        End Operator
    End Class
End Namespace
