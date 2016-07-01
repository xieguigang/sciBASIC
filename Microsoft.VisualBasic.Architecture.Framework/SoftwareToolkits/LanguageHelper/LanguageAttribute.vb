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