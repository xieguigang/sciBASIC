#Region "Microsoft.VisualBasic::adec478ab0683caaeb480cf13b7bac4a, mime\text%yaml\1.2\ParserInput.vb"

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

    '     Interface ParserInput
    ' 
    '         Properties: Length
    ' 
    '         Function: FormErrorMessage, GetInputSymbol, GetSubSection, HasInput
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Collections.Generic
Imports System.Text

Namespace Grammar

    Public Interface ParserInput(Of T)

        ReadOnly Property Length() As Integer

        Function HasInput(pos As Integer) As Boolean
        Function GetInputSymbol(pos As Integer) As T
        Function GetSubSection(position As Integer, length As Integer) As T()
        Function FormErrorMessage(position As Integer, message As String) As String

    End Interface
End Namespace
