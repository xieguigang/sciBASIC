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

Imports System.Text

''' <summary>
''' Shared debug-information model used by both the classic MSF PDB reader and the
''' Portable PDB reader. Both back-ends fill these objects so that the <see cref="PDB"/>
''' facade can expose a single, uniform view.
''' </summary>
Namespace sciBASIC.PDB

    ''' <summary>
    ''' A source file referenced by the debug symbols (the ``*.vb`` / ``*.cs`` / ``*.cpp``
    ''' source that was compiled into the binary).
    ''' </summary>
    Public Class SourceDocument

        ''' <summary>
        ''' Absolute (or sometimes relative) path of the source file as stored inside the PDB.
        ''' </summary>
        Public Property FilePath As String

        ''' <summary>
        ''' Optional remapped URL of the source file on a Git host (e.g. GitHub), filled by
        ''' <see cref="Extensions.PointLocal2Github"/>. Empty until remapped.
        ''' </summary>
        Public Property GitHubUrl As String

        ''' <summary>
        ''' Language of the source file. For the classic PDB this is usually empty; for the
        ''' Portable PDB this is the language GUID (C# / VB / F# / ...) from the Document table.
        ''' </summary>
        Public Property Language As Guid

        ''' <summary>
        ''' Hash algorithm GUID used to compute <see cref="Checksum"/>. Empty when unknown.
        ''' </summary>
        Public Property HashAlgorithm As Guid

        ''' <summary>
        ''' Source file content checksum, may be empty when not embedded.
        ''' </summary>
        Public Property Checksum As Byte()

        ''' <summary>
        ''' Human readable language name derived from <see cref="Language"/>.
        ''' </summary>
        Public ReadOnly Property LanguageName As String
            Get
                If Language = LanguageGuids.CSharp Then
                    Return "C#"
                ElseIf Language = LanguageGuids.VisualBasic Then
                    Return "Visual Basic"
                ElseIf Language = LanguageGuids.FSharp Then
                    Return "F#"
                ElseIf Language = LanguageGuids.Cpp Then
                    Return "C++"
                ElseIf Language = Guid.Empty Then
                    Return "Unknown"
                Else
                    Return Language.ToString()
                End If
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return FilePath
        End Function
    End Class

    ''' <summary>
    ''' A line-number / sequence-point mapping between a method and a source document.
    ''' </summary>
    Public Class LineInfo

        ''' <summary>
        ''' The source document this line range belongs to.
        ''' </summary>
        Public Property Document As SourceDocument

        ''' <summary>
        ''' IL / native offset at which this line range begins (best-effort, 0 when not available).
        ''' </summary>
        Public Property Offset As Long

        ''' <summary>
        ''' Method or function name this line range is part of (best-effort; may be empty for
        ''' classic PDBs that do not carry method names in the line stream).
        ''' </summary>
        Public Property MethodName As String

        ''' <summary>
        ''' 1-based start line in the source document.
        ''' </summary>
        Public Property StartLine As Integer

        ''' <summary>
        ''' 1-based end line in the source document.
        ''' </summary>
        Public Property EndLine As Integer

        ''' <summary>
        ''' Start column (0-based within the line), or 0 when not available.
        ''' </summary>
        Public Property StartColumn As Integer

        ''' <summary>
        ''' End column (0-based within the line), or 0 when not available.
        ''' </summary>
        Public Property EndColumn As Integer

        Public Overrides Function ToString() As String
            Return $"{If(Document?.FilePath, "?")}({StartLine},{StartColumn})-({EndLine},{EndColumn}) {If(MethodName, "")}"
        End Function
    End Class

    ''' <summary>
    ''' A single symbol (function / public / data) extracted from the symbol stream.
    ''' </summary>
    Public Class Symbol

        ''' <summary>
        ''' Symbol / function name.
        ''' </summary>
        Public Property Name As String

        ''' <summary>
        ''' Section index (segment) where the symbol lives, 1-based. 0 for flat addressing.
        ''' </summary>
        Public Property Section As UShort

        ''' <summary>
        ''' Offset of the symbol from the start of <see cref="Section"/> (or from image base
        ''' when <see cref="Section"/> is 0).
        ''' </summary>
        Public Property Offset As UInteger

        ''' <summary>
        ''' Length of the symbol in bytes (function body size), 0 when unknown.
        ''' </summary>
        Public Property Length As UInteger

        ''' <summary>
        ''' Kind of symbol (Public / Function / Data / ...). See <see cref="SymbolKind"/>.
        ''' </summary>
        Public Property Kind As SymbolKind

        ''' <summary>
        ''' Flags of the symbol (e.g. code / function).
        ''' </summary>
        Public Property Flags As UShort

        Public Overrides Function ToString() As String
            Return $"{Kind} {Name} @[{Section}:{Offset:X}+#{Length}]"
        End Function
    End Class

    ''' <summary>
    ''' A type record decoded from the TPI stream (classic PDB) or the metadata type tables
    ''' (Portable PDB).
    ''' </summary>
    Public Class TypeRecord

        ''' <summary>
        ''' Type id (leaf index for classic PDB, metadata token for Portable PDB).
        ''' </summary>
        Public Property TypeId As UInteger

        ''' <summary>
        ''' Type name, e.g. ``System.Int32`` or ``MyNamespace.MyClass``.
        ''' </summary>
        Public Property Name As String

        ''' <summary>
        ''' Category of the type. See <see cref="TypeKind"/>.
        ''' </summary>
        Public Property Kind As TypeKind

        ''' <summary>
        ''' Size of the type in bytes, 0 when unknown / not applicable.
        ''' </summary>
        Public Property Size As UInteger

        ''' <summary>
        ''' Field / member names of the type (for structs / classes).
        ''' </summary>
        Public Property Fields As New List(Of String)()

        Public Overrides Function ToString() As String
            Return $"{Kind} {Name} (#{TypeId})"
        End Function
    End Class

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
