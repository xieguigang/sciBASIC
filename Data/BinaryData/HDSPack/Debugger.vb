#Region "Microsoft.VisualBasic::5ff772a9f875d2a4faa2e329c7e2aeaa, Data\BinaryData\HDSPack\Debugger.vb"

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

    '   Total Lines: 122
    '    Code Lines: 83 (68.03%)
    ' Comment Lines: 23 (18.85%)
    '    - Xml Docs: 95.65%
    ' 
    '   Blank Lines: 16 (13.11%)
    '     File Size: 4.92 KB


    ' Module Debugger
    ' 
    '     Function: (+3 Overloads) ListFiles
    ' 
    '     Sub: Tree, treeInternal
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataStorage.HDSPack.FileSystem
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text

Public Module Debugger

    ''' <summary>
    ''' this function returns all sub folder and files inside the <see cref="StreamPack.superBlock"/>
    ''' </summary>
    ''' <param name="hds"></param>
    ''' <param name="recursive"></param>
    ''' <returns>a collection of <see cref="StreamObject"/>: <see cref="StreamGroup"/> mixed with <see cref="StreamBlock"/></returns>
    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    <Extension>
    Public Function ListFiles(hds As StreamPack, Optional recursive As Boolean = True) As IEnumerable(Of StreamObject)
        Return New StreamObject() {hds.superBlock}.JoinIterates(hds.superBlock.ListFiles(recursive:=recursive))
    End Function

    <Extension>
    Public Function ListFiles(hds As StreamPack, dir As String, Optional recursive As Boolean = True) As IEnumerable(Of StreamObject)
        Dim folder = hds.GetObject(dir & "/")

        If folder Is Nothing Then
            Return {}
        ElseIf TypeOf folder Is StreamBlock Then
            Return New StreamObject() {folder}
        Else
            Return DirectCast(folder, StreamGroup).ListFiles(safe:=True, recursive)
        End If
    End Function

    ''' <summary>
    ''' enumerate all data file object inside current group dir
    ''' </summary>
    ''' <param name="dir"></param>
    ''' <returns>
    ''' contains data file and file group object, for filter only data objects, the linq 
    ''' extension function <see cref="Enumerable.OfType(Of StreamBlock)(IEnumerable)"/> 
    ''' with generic parameter type <see cref="StreamBlock"/> constrained
    ''' is recommended.
    ''' </returns>
    ''' <remarks>
    ''' get all the files/dirs inside current dir object and its child dirs.
    ''' this function will returns an empty collection if the given dir object
    ''' is nothing andalso <paramref name="safe"/> parameter value is configured 
    ''' as true.
    ''' </remarks>
    <Extension>
    Public Iterator Function ListFiles(dir As StreamGroup,
                                       Optional safe As Boolean = True,
                                       Optional recursive As Boolean = True) As IEnumerable(Of StreamObject)

        If dir Is Nothing AndAlso safe Then
            ' empty collection will be generates if safe mode is turn on
            Return
        End If

        For Each file As StreamObject In dir.files
            If TypeOf file Is StreamBlock Then
                Yield file
            Else
                Yield file

                If recursive Then
                    For Each child As StreamObject In DirectCast(file, StreamGroup).ListFiles
                        Yield child
                    Next
                End If
            End If
        Next
    End Function

    ''' <summary>
    ''' a linux tree liked command for show structure 
    ''' inside of the HDS stream pack file.
    ''' </summary>
    ''' <param name="dir"></param>
    ''' <param name="text"></param>
    <Extension>
    Public Sub Tree(dir As StreamGroup, text As TextWriter,
                    Optional indent As Integer = 0,
                    Optional pack As StreamPack = Nothing,
                    Optional showReadme As Boolean = True)

        Call text.WriteLine(dir.ToString)
        Call treeInternal(dir, text, indent, pack, showReadme)
    End Sub

    Private Sub treeInternal(dir As StreamGroup,
                             text As TextWriter,
                             indent As Integer,
                             pack As StreamPack,
                             showReadme As Boolean)

        For Each file As StreamObject In From f As StreamObject
                                         In dir.files
                                         Order By If(TypeOf f Is StreamGroup, 0, 1)

            If TypeOf file Is StreamBlock Then
                Call text.WriteLine($"{New String(" "c, indent * 4)}|-- " & file.ToString)
            Else
                Call text.WriteLine()
                Call text.WriteLine($"{New String(" "c, indent * 4)}|-- " & file.ToString)
                Call treeInternal(dir:=file, text, indent + 1, pack, showReadme)
            End If
        Next

        If showReadme Then
            Dim readme As StreamObject = dir.files _
                .Where(Function(f)
                           Return f.fileName.TextEquals("readme.txt")
                       End Function) _
                .FirstOrDefault

            If readme IsNot Nothing AndAlso pack IsNot Nothing AndAlso TypeOf readme Is StreamBlock Then
                Dim txt As String = New StreamReader(pack.OpenBlock(readme)).ReadToEnd
                Dim par As String() = txt.SplitParagraph(len:=80).ToArray

                Call text.WriteLine()
                Call text.WriteLine($"{New String("="c, 35)} readme.txt {New String("="c, 35)}")
                Call par.DoEach(AddressOf text.WriteLine)
                Call text.WriteLine(New String("="c, 35 * 2 + 12))
            End If
        End If
    End Sub
End Module
