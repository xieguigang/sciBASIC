Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel

Namespace FuzzyLogic.Models

    Public Class FuzzyModel : Inherits ITextFile

        Public Property Input As Value
        Public Property Output As Value
        Public Property Fuzzify As Fuzzify
        Public Property Defuzzify As Defuzzify
        Public Property Rules As RuleBlock

        Public Overrides Function Save(Optional FilePath As String = "", Optional Encoding As Encoding = Nothing) As Boolean
            Return Me.GetXml.SaveTo(getPath(FilePath), Encoding)
        End Function
    End Class
End Namespace