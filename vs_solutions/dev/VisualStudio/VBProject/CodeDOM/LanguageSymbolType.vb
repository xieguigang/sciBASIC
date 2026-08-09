#Region "Microsoft.VisualBasic::0a7a67f7b133aaf0b487024ed4ffd12f, vs_solutions\dev\VisualStudio\VBProject\LanguageSymbolType.vb"

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

'   Total Lines: 198
'    Code Lines: 71 (35.86%)
' Comment Lines: 81 (40.91%)
'    - Xml Docs: 100.00%
' 
'   Blank Lines: 46 (23.23%)
'     File Size: 7.27 KB


'     Class LanguageSymbolType
' 
'         Properties: Attributes, GenericTypeArguments, Modifiers, Name, Parent
'                     XmlDoc
' 
'     Class TypeContainerSymbol
' 
'         Properties: ImplementsInterfaces, InheritsType, InternalNested, Members
' 
'     Class NamespaceSymbol
' 
'         Properties: Type
' 
'     Class ClassSymbol
' 
'         Properties: Type
' 
'     Class ModuleSymbol
' 
'         Properties: Type
' 
'     Class StructureSymbol
' 
'         Properties: Type
' 
'     Class InterfaceSymbol
' 
'         Properties: Type
' 
'     Class EnumSymbol
' 
'         Properties: EnumBaseType, Type
' 
'     Class MemberSymbol
' 
' 
' 
'     Class CallableMemberSymbol
' 
'         Properties: Locals, Parameters, ReturnType
' 
'     Class MethodSymbol
' 
'         Properties: Type
' 
'         Constructor: (+1 Overloads) Sub New
' 
'     Class PropertySymbol
' 
'         Properties: Type
' 
'     Class EventSymbol
' 
'         Properties: DelegateType, Type
' 
'     Class DelegateSymbol
' 
'         Properties: Parameters, Type, ValueType
' 
'     Class VariableSymbol
' 
'         Properties: Type, ValueType
' 
' 
' /********************************************************************************/

#End Region

Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Scripting.MetaData
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace VBProj.CodeDOM

    ''' <summary>
    ''' the base class of all code symbol types in a VB language project.
    ''' the concrete <see cref="Type"/> is fixed by the derived class so that
    ''' symbol categories (container / member / variable) can never be confused.
    ''' </summary>
    Public MustInherit Class LanguageSymbolType

        ''' <summary>
        ''' the exact kind of this symbol, fixed by the derived class.
        ''' </summary>
        Public MustOverride ReadOnly Property Type As SymbolType

        ''' <summary>
        ''' the symbol name
        ''' </summary>
        Public Property Name As String

        ''' <summary>
        ''' the parent symbol that owns this symbol (a type container or a member).
        ''' </summary>
        Public Property Parent As LanguageSymbolType

        ''' <summary>
        ''' generic type argument for XXX(Of T)
        ''' </summary>
        Public Property GenericTypeArguments As TypeInfo()

        ''' <summary>
        ''' access and custom modifiers, e.g. "Public Shared Overloads"
        ''' </summary>
        Public Property Modifiers As String

        ''' <summary>
        ''' attribute declaration blocks applied on this symbol, e.g. &lt;ExportAPI()&gt;
        ''' </summary>
        Public Property Attributes As List(Of String)

        ''' <summary>
        ''' the xml documentation comment lines (''') that precedes this symbol
        ''' </summary>
        Public Property XmlDoc As String

        Public Property Source As SourceLocations

    End Class

    Public Class SourceLocations : Implements Enumeration(Of Source)

        Dim SourceLocations As New List(Of Source)

        ''' <summary>
        ''' true when this symbol is composed of several partial declarations.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property IsMultiplePartial As Boolean
            Get
                Return SourceLocations.TryCount > 1
            End Get
        End Property

        ''' <summary>
        ''' the number of recorded source locations (partial declarations).
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property Count As Integer
            Get
                Return SourceLocations.Count
            End Get
        End Property

        ''' <summary>
        ''' the first recorded source location; nothing when empty.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property First As Source
            Get
                If SourceLocations.Count = 0 Then
                    Return Nothing
                End If
                Return SourceLocations(0)
            End Get
        End Property

        Public Function ToArray() As Source()
            Return SourceLocations.ToArray()
        End Function

        Public Sub Add(loc As Source)
            Call SourceLocations.Add(loc)
        End Sub

        Public Sub Add(file As String, startLine As Integer, endLine As Integer, Optional declarationLine As Integer = -1)
            Call SourceLocations.Add(New Source With {
                .FilePath = file,
                .LineRange = New IntRange(startLine, endLine),
                .DeclarationLine = If(declarationLine <= 0, startLine, declarationLine)
            })
        End Sub

        Private Iterator Function GenericEnumerator() As IEnumerator(Of Source) Implements Enumeration(Of Source).GenericEnumerator
            For Each src As Source In SourceLocations
                Yield src
            Next
        End Function
    End Class

    Public Class Source

        Public Property FilePath As String

        ''' <summary>
        ''' the 1-based physical line range of the whole code block, including
        ''' any leading xml documentation comment (''') / attribute block
        ''' (&lt;...&gt;) and ending at the matching End XXX statement.
        ''' </summary>
        Public Property LineRange As IntRange

        ''' <summary>
        ''' the 1-based physical line where the declaration keyword
        ''' (Class / Function / ...) actually starts. May differ from
        ''' <see cref="LineRange"/>.Min when a leading xml doc / attribute
        ''' block is present.
        ''' </summary>
        Public Property DeclarationLine As Integer

        ''' <summary>
        ''' extract the raw VB.NET source text of this symbol from its file.
        ''' </summary>
        ''' <remarks>
        ''' This property reads the whole file on every access (lazy, on
        ''' demand). Do not call it inside hot loops or ToString overrides.
        ''' </remarks>
        Public ReadOnly Property CodeBlock As String
            Get
                If String.IsNullOrEmpty(FilePath) OrElse LineRange Is Nothing Then
                    Return ""
                End If

                If Not System.IO.File.Exists(FilePath) Then
                    Return ""
                End If

                Dim min As Integer = Math.Max(0, LineRange.Min - 1)

                Return FilePath _
                    .ReadAllLines _
                    .Skip(min) _
                    .Take(LineRange.Interval + 1) _
                    .JoinBy(vbCrLf)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{FilePath} {LineRange.GetMinMax.GetJson}"
        End Function

    End Class
End Namespace
