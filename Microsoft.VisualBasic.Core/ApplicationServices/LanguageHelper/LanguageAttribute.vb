#Region "Microsoft.VisualBasic::b25e743ee7591c1e71fdaa2fd2db4d63, Microsoft.VisualBasic.Core\ApplicationServices\LanguageHelper\LanguageAttribute.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class LanguageAttribute
    ' 
    '         Properties: Language, Text
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ApplicationServices.Globalization

    <AttributeUsage(AttributeTargets.Property Or
        AttributeTargets.Field Or
        AttributeTargets.Enum, AllowMultiple:=True, Inherited:=True)>
    Public Class LanguageAttribute : Inherits Attribute

        Public ReadOnly Property Language As Integer
        Public ReadOnly Property Text As String

        ReadOnly _langEnum As Type

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="Language">The language of the target object.</param>
        ''' <param name="Context"></param>
        ''' <param name="langEnum">This type define should be a enum type.</param>
        ''' <remarks></remarks>
        Sub New(Language As Integer, Context As String, Optional langEnum As Type = Nothing)
            Me.Language = Language
            Me.Text = Context
            Me._langEnum = langEnum
        End Sub

        Public Overrides Function ToString() As String
            If _langEnum Is Nothing Then
                Return Text
            Else
                Dim language As String = CTypeDynamic(Me.Language, _langEnum)
                Return $"@{language}  ""{Text}"""
            End If
        End Function

        Public Shared Narrowing Operator CType(context As LanguageAttribute) As String
            Return context.Text
        End Operator
    End Class
End Namespace
