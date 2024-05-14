#Region "Microsoft.VisualBasic::faaaf7c6824495d9c6fe9a5a76d436a6, mime\text%yaml\1.2\TextInput.vb"

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


    ' Code Statistics:

    '   Total Lines: 78
    '    Code Lines: 64
    ' Comment Lines: 0
    '   Blank Lines: 14
    '     File Size: 3.04 KB


    '     Class TextInput
    ' 
    '         Properties: Length
    ' 
    '         Constructor: (+1 Overloads) Sub New
    ' 
    '         Function: FormErrorMessage, GetInputSymbol, GetSubSection, GetSubString, HasInput
    ' 
    '         Sub: GetLineColumnNumber
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Grammar

    Public Class TextInput : Implements ParserInput(Of Char)

        Dim InputText As String
        Dim LineBreaks As List(Of Integer)

        Public Sub New(text As String)
            InputText = text

            LineBreaks = New List(Of Integer)()
            LineBreaks.Add(0)
            For index As Integer = 0 To InputText.Length - 1
                If InputText(index) = ControlChars.Lf Then
                    LineBreaks.Add(index + 1)
                End If
            Next
            LineBreaks.Add(InputText.Length)
        End Sub

#Region "ParserInput<char> Members"

        Public ReadOnly Property Length() As Integer Implements ParserInput(Of Char).Length
            <MethodImpl(MethodImplOptions.AggressiveInlining)>
            Get
                Return InputText.Length
            End Get
        End Property

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function HasInput(pos As Integer) As Boolean Implements ParserInput(Of Char).HasInput
            Return pos < InputText.Length
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetInputSymbol(pos As Integer) As Char Implements ParserInput(Of Char).GetInputSymbol
            Return InputText(pos)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetSubSection(position As Integer, length As Integer) As Char() Implements ParserInput(Of Char).GetSubSection
            Return InputText.Substring(position, length).ToCharArray()
        End Function

        Public Function FormErrorMessage(position As Integer, message As String) As String Implements ParserInput(Of Char).FormErrorMessage
            Dim line As Integer
            Dim col As Integer
            GetLineColumnNumber(position, line, col)
            Dim ch As String = If(HasInput(position), "'" & GetInputSymbol(position) & "'", Nothing)
            Return String.Format("Line {0}, Col {1} {2}: {3}", line, col, ch, message)
        End Function

#End Region

        Public Sub GetLineColumnNumber(pos As Integer, ByRef line As Integer, ByRef col As Integer)
            col = 1
            For line = 1 To LineBreaks.Count - 1
                If LineBreaks(line) > pos Then
                    For p As Integer = LineBreaks(line - 1) To pos - 1
                        If InputText(p) = ControlChars.Tab Then
                            col += 4
                        Else
                            col += 1
                        End If
                    Next
                    Exit For
                End If
            Next
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function GetSubString(start As Integer, length As Integer) As String
            Return InputText.Substring(start, length)
        End Function
    End Class
End Namespace
