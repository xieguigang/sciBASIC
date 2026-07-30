#Region "Microsoft.VisualBasic::00000000000000000000000000000000, sciBASIC#\vs_solutions\dev\vs_PDB\Model.vb"

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

#End Region

' Shared debug-information model used by both the classic MSF PDB reader and the
' Portable PDB reader. Both back-ends fill these objects so that the <see cref="PDB"/>
' facade can expose a single, uniform view.

Namespace Models

    ''' <summary>
    ''' Classification of a <see cref="Symbol"/>.
    ''' </summary>
    Public Enum SymbolKind
        Unknown
        Public_
        Function_
        Data
        Procedure
        Thunk
        Label
        Constant
    End Enum

    ''' <summary>
    ''' Classification of a <see cref="TypeRecord"/>.
    ''' </summary>
    Public Enum TypeKind
        Unknown
        Primitive
        Pointer
        [Class]
        [Structure]
        [Enum]
        Array
        FunctionType
        Typedef
    End Enum

    ''' <summary>
    ''' Well-known language GUIDs used by source documents.
    ''' </summary>
    Public Module LanguageGuids

        Public ReadOnly CSharp As New Guid("3f5162f8-07c6-11d3-9053-00c04fa302a1")
        Public ReadOnly VisualBasic As New Guid("3a12d0b8-c26c-11d0-b442-00a0244a03e2")
        Public ReadOnly FSharp As New Guid("ab4f38c9-b6e6-43ba-be3b-58080b2ccce3")
        Public ReadOnly Cpp As New Guid("3a12d0b7-c26c-11d0-b442-00a0244a03e2")

        ''' <summary>
        ''' Try to parse a language GUID from its canonical dashed string form.
        ''' </summary>
        Public Function TryParse(guidText As String, ByRef value As Guid) As Boolean
            If String.IsNullOrEmpty(guidText) Then
                value = Guid.Empty
                Return False
            End If

            If Guid.TryParse(guidText, value) Then
                Return True
            End If

            ' Portable PDB stores language as a GUID in #GUID stream; when the raw
            ' value is a byte blob we cannot parse it here, just return empty.
            value = Guid.Empty
            Return False
        End Function
    End Module

End Namespace