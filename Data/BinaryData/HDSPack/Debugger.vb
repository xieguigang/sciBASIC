Imports System.IO
Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataStorage.HDSPack.FileSystem
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text

Public Module Debugger

    <Extension>
    Public Function ListFiles(hds As StreamPack) As IEnumerable(Of StreamObject)
        Return New StreamObject() {hds.superBlock}.JoinIterates(hds.superBlock.ListFiles)
    End Function

    <Extension>
    Public Iterator Function ListFiles(dir As StreamGroup) As IEnumerable(Of StreamObject)
        For Each file As StreamObject In dir.files
            If TypeOf file Is StreamBlock Then
                Yield file
            Else
                Yield file

                For Each child As StreamObject In DirectCast(file, StreamGroup).ListFiles
                    Yield child
                Next
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
                    Optional pack As StreamPack = Nothing)

        Call text.WriteLine(dir.ToString)
        Call treeInternal(dir, text, indent, pack)
    End Sub

    Private Sub treeInternal(dir As StreamGroup,
                             text As TextWriter,
                             indent As Integer,
                             pack As StreamPack)

        For Each file As StreamObject In From f As StreamObject
                                         In dir.files
                                         Order By If(TypeOf f Is StreamGroup, 0, 1)

            If TypeOf file Is StreamBlock Then
                Call text.WriteLine($"{New String(" "c, indent * 4)}|-- " & file.ToString)
            Else
                Call text.WriteLine()
                Call text.WriteLine($"{New String(" "c, indent * 4)}|-- " & file.ToString)
                Call treeInternal(dir:=file, text, indent + 1, pack)
            End If
        Next

        Dim readme As StreamObject = dir.files _
            .Where(Function(f)
                       Return f.fileName.TextEquals("readme.txt")
                   End Function) _
            .FirstOrDefault

        If readme IsNot Nothing AndAlso pack IsNot Nothing AndAlso TypeOf readme Is StreamBlock Then
            Dim txt As String = New StreamReader(pack.OpenBlock(readme)).ReadToEnd
            Dim par As String() = txt.SplitParagraph(len:=80).ToArray

            Call Console.WriteLine()
            Call Console.WriteLine($"{New String("="c, 35)} readme.txt {New String("="c, 35)}")
            Call par.DoEach(AddressOf Console.WriteLine)
            Call Console.WriteLine(New String("="c, 35 * 2 + 12))
        End If
    End Sub
End Module
