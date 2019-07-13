#Region "Microsoft.VisualBasic::f289b2fab36ed193256bcd8c3ed7652c, Microsoft.VisualBasic.Core\Extensions\IO\Extensions\SerializationIO.vb"

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

    ' Module SerializationIO
    ' 
    '     Function: DumpSerial, SaveAsTabularMapping, SolveListStream
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.IO
Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Serialization.JSON
Imports Microsoft.VisualBasic.Text

Public Module SerializationIO

    ''' <summary>
    ''' Save as a tsv file, with data format like: 
    ''' 
    ''' ```
    ''' <see cref="NamedValue(Of String).Name"/>\t<see cref="NamedValue(Of String).Value"/>\t<see cref="NamedValue(Of String).Description"/>
    ''' ```
    ''' </summary>
    ''' <param name="source"></param>
    ''' <param name="path$"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    <Extension>
    Public Function SaveAsTabularMapping(source As IEnumerable(Of NamedValue(Of String)),
                                         path$,
                                         Optional saveDescrib As Boolean = False,
                                         Optional saveHeaders$() = Nothing,
                                         Optional encoding As Encodings = Encodings.ASCII) As Boolean
        Dim content = source _
            .Select(Function(row)
                        With row
                            If saveDescrib Then
                                Return $"{ .Name}{ASCII.TAB}{ .Value}{ASCII.TAB}{ .Description}"
                            Else
                                Return $"{ .Name}{ASCII.TAB}{ .Value}"
                            End If
                        End With
                    End Function)

        If saveHeaders.IsNullOrEmpty Then
            Return content.SaveTo(path, encoding.CodePage)
        Else
            Return {saveHeaders.JoinBy(ASCII.TAB)}.JoinIterates(content).SaveTo(path, encoding.CodePage)
        End If
    End Function

    ''' <summary>
    ''' Get string collection from input file 
    ''' </summary>
    ''' <param name="path">allows plain text/string array json/xml</param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    <Extension>
    Public Function SolveListStream(path$, Optional encoding As Encoding = Nothing) As IEnumerable(Of String)
        Select Case path.ExtensionSuffix.ToLower
            Case "", "txt"
                Return path.IterateAllLines
            Case "json"
                Return path.LoadJSON(Of String())
            Case "xml"
                Return path.LoadXml(Of String())
            Case Else
                Throw New NotImplementedException(path)
        End Select
    End Function

    ''' <summary>
    ''' Save a given list of <see cref="PointF"/> data into a csv file.
    ''' </summary>
    ''' <param name="points"></param>
    ''' <param name="csv"></param>
    ''' <param name="encoding"></param>
    ''' <returns></returns>
    <Extension>
    Public Function DumpSerial(points As IEnumerable(Of PointF), csv$, Optional encoding As Encodings = Encodings.UTF8WithoutBOM) As Boolean
        Using writer As StreamWriter = csv.OpenWriter(encoding)
            Call writer.WriteLine("X,Y")

            For Each pt As PointF In points
                Call writer.WriteLine($"{pt.X},{pt.Y}")
            Next
        End Using

        Return True
    End Function
End Module
