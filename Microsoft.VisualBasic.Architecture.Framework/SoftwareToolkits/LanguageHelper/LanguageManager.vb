Namespace Logging.LanguageHelper

    Public Class LanguageManager

        Dim ListData As Dictionary(Of String, LanguageHelper)

        <LanguageManager("")>
        <Language(LanguageHelper.Languages.ZhCN, "")>
        <Language(LanguageHelper.Languages.FrFR, "")>
        Default Public ReadOnly Property Item(Tag As String) As LanguageHelper
            Get
                Return ListData(Tag)
            End Get
        End Property
    End Class
End Namespace