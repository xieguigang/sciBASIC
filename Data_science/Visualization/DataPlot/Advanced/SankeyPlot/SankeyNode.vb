
Imports System.Drawing

Public Class SankeyNode
    Public Property Id As String
    Public Property Label As String
    Public Property Layer As Integer
    Public Property Color As Color? = Nothing
    ' 运行时
    Public Property InFlow As Double
    Public Property OutFlow As Double
    Public Property Total As Double
    Public Property X As Single
    Public Property Y As Single
    Public Property Height As Single
    Public Property OutOffsets As New Dictionary(Of String, Double)()
    Public Property InOffsets As New Dictionary(Of String, Double)()
End Class
