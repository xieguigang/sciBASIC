Imports System

Namespace Microsoft.VisualBasic
    Public NotInheritable Class ControlChars
        ' Fields
        Public Const Back As Char = ChrW(8)
        Public Const Cr As Char = ChrW(13)
        Public Const CrLf As String = ChrW(13) & ChrW(10)
        Public Const FormFeed As Char = ChrW(12)
        Public Const Lf As Char = ChrW(10)
        Public Const NewLine As String = ChrW(13) & ChrW(10)
        Public Const NullChar As Char = ChrW(0)
        Public Const Quote As Char = """"c
        Public Const Tab As Char = ChrW(9)
        Public Const VerticalTab As Char = ChrW(11)
    End Class
End Namespace

