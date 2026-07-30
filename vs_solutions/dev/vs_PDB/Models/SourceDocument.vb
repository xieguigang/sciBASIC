#Region "Microsoft.VisualBasic::f9261c2f6f465edc05a490cab4d2444f, vs_solutions\dev\vs_PDB\Models\SourceDocument.vb"

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

    '   Total Lines: 61
    '    Code Lines: 29 (47.54%)
    ' Comment Lines: 24 (39.34%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 8 (13.11%)
    '     File Size: 2.28 KB


    '     Class SourceDocument
    ' 
    '         Properties: Checksum, FilePath, GitHubUrl, HashAlgorithm, Language
    '                     LanguageName
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Models

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
End Namespace
