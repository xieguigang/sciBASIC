#Region "Microsoft.VisualBasic::0b848a98496e6d2493a2d60ccc20469a, mime\application%vnd.openxmlformats-officedocument.spreadsheetml.sheet\Excel\IO\xl\sharedStrings.xml.vb"

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

    '     Class sharedStrings
    ' 
    '         Properties: count, strings, uniqueCount
    ' 
    '         Function: ToHashTable
    '         Operators: +
    ' 
    '     Class si
    ' 
    '         Properties: phoneticPr, t
    ' 
    '         Function: ToString
    ' 
    '     Class phoneticPr
    ' 
    '         Properties: fontId, type
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Xml.Serialization
Imports Microsoft.VisualBasic.Linq

Namespace XML.xl

    <XmlRoot("sst", [Namespace]:="http://schemas.openxmlformats.org/spreadsheetml/2006/main")>
    Public Class sharedStrings

        <XmlAttribute> Public Property count As Integer
        <XmlAttribute> Public Property uniqueCount As Integer

        <XmlElement("si")>
        Public Property strings As si()

        Public Function ToHashTable() As Dictionary(Of String, Integer)
            Return strings _
                .SeqIterator _
                .ToDictionary(Function(x) x.value.t,
                              Function(x) x.i)
        End Function

        ''' <summary>
        ''' Append new values to <see cref="strings"/>
        ''' </summary>
        ''' <param name="strings"></param>
        ''' <param name="table"></param>
        ''' <returns></returns>
        Public Shared Operator +(strings As sharedStrings, table As Dictionary(Of String, Integer)) As sharedStrings
            Dim newValues = table _
                .OrderBy(Function(x) x.Value) _
                .Skip(strings.strings.Length) _
                .Select(Function(x)
                            Return New si With {
                                .t = x.Key
                            }
                        End Function) _
                .ToArray

            If newValues.Length > 0 Then
                strings.strings.Add(newValues)
                strings.count = strings.strings.Length
                strings.uniqueCount = strings.count
            End If

            Return strings
        End Operator
    End Class

    Public Class si

        Public Property t As String
        Public Property phoneticPr As phoneticPr

        Public Overrides Function ToString() As String
            Return t
        End Function
    End Class

    Public Class phoneticPr
        <XmlAttribute> Public Property fontId As String
        <XmlAttribute> Public Property type As String
    End Class
End Namespace
