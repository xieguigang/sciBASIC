Imports System.Drawing
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.Runtime

Namespace Drawing2D

    Public Class PrinterDimension

        Public Const A0 As String = "1189,841"
        Public Const A1 As String = "841,594"
        Public Const A2 As String = "594,420"
        Public Const A3 As String = "420,297"
        Public Const A4 As String = "297,210"
        Public Const A5 As String = "210,148"

        Public Const B0 As String = "1092,787"
        Public Const B1 As String = "740,520"
        Public Const B2 As String = "520,370"
        Public Const B3 As String = "370,260"
        Public Const B4 As String = "260,185"
        Public Const B5 As String = "185,130"

        ''' <summary>
        ''' The size expression parser
        ''' </summary>
        ''' <param name="expr">
        ''' 1. [w,h]: 100,200
        ''' 2. term_names: A4 
        ''' 3. times(size/term_names): 5(A4) = 5*297,5*210 = 1485,1050
        ''' 4. portal(times(size))
        ''' </param>
        ''' <returns></returns>
        Public Shared Function SizeOf(expr As String) As SizeF
            If expr.IsPattern("\d+\s*,\s*\d+") Then
                With expr.StringSplit("\s*,\s*") _
                    .Select(AddressOf Val) _
                    .ToArray

                    Return New SizeF(.GetValue(0), .GetValue(1))
                End With
            ElseIf expr.IsPattern("[AB]\d") Then
                Return expr _
                    .DoCall(AddressOf SizeOfPrinterPapers) _
                    .SizeParser
            ElseIf expr.IsPattern("portal\(.+\)") Then
                Dim size As SizeF = expr _
                    .GetStackValue("(", ")") _
                    .DoCall(AddressOf SizeOf)

                Return New SizeF With {
                    .Height = size.Width,
                    .Width = size.Height
                }
            Else
                Dim times As Double = expr.Split("("c).First.ParseDouble
                Dim size As SizeF = expr.GetStackValue("(", ")").DoCall(AddressOf SizeOf)

                Return New SizeF With {
                    .Width = size.Width * times,
                    .Height = size.Height * times
                }
            End If
        End Function

        Public Shared Function SizeOfPrinterPapers(term As String) As String
            Select Case term.ToUpper
                Case NameOf(A0) : Return A0
                Case NameOf(A1) : Return A1
                Case NameOf(A2) : Return A2
                Case NameOf(A3) : Return A3
                Case NameOf(A4) : Return A4
                Case NameOf(A5) : Return A5

                Case NameOf(B0) : Return B0
                Case NameOf(B1) : Return B1
                Case NameOf(B2) : Return B2
                Case NameOf(B3) : Return B3
                Case NameOf(B4) : Return B4
                Case NameOf(B5) : Return B5

                Case Else
                    Throw New NotImplementedException(term)
            End Select
        End Function
    End Class
End Namespace