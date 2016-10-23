#Region "Microsoft.VisualBasic::20c8934f82c5ff1e72bdaf61e4bceff1, ..\visualbasic_App\Data\DataFrame\Linq\BatchQueue.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
    '       xie (genetics@smrucc.org)
    ' 
    ' Copyright (c) 2016 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
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

Imports System.Runtime.CompilerServices
Imports System.Text
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Text

Namespace DocumentStream.Linq

    Public Module BatchQueue

        ''' <summary>
        ''' {<see cref="IO.Path.GetFileNameWithoutExtension(String)"/>, <typeparamref name="T"/>()}
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="files"></param>
        ''' <returns></returns>
        ''' <remarks>在服务器上面可能会出现IO很慢的情况，这个时候可以试一下这个函数进行批量数据加载</remarks>
        ''' 
        <Extension>
        Public Iterator Function ReadQueue(Of T As Class)(
                                 files As IEnumerable(Of String),
                                 Optional encoding As Encodings = Encodings.Default) As IEnumerable(Of NamedValue(Of T()))

            Call "Wait for the IO queue.....".__DEBUG_ECHO

            Dim sw As Stopwatch = Stopwatch.StartNew
            Dim encode As Encoding = encoding.GetEncodings
            Dim IO As IEnumerable(Of NamedValue(Of String())) =
 _
                From path As String
                In files.AsParallel
                Let echoIni As String = $"{path.ToFileURL} init start...".__DEBUG_ECHO
                Let buf As String() = path.ReadAllLines(encode)
                Let echo As String = $"{path.ToFileURL} I/O read done!".__DEBUG_ECHO
                Let name As String = path.BaseName
                Select New NamedValue(Of String())(name, buf) ' 不清楚为什么服务器上面有时候的IO会非常慢，则在这里可以一次性的先读完所有数据，然后再返回数据

            Call $"All I/O queue job done!   {sw.ElapsedMilliseconds}ms...".__DEBUG_ECHO

            For Each data As NamedValue(Of String()) In IO
                Dim buf As T() = data.x.LoadStream(Of T)(False)

                Yield New NamedValue(Of T())(data.Name, buf)

                Call GC.SuppressFinalize(data.x)    ' 数据量非常大的话，在这里进行内存的释放
                Call GC.SuppressFinalize(data)
                Call Console.Write(".")
            Next

            Call GC.SuppressFinalize(IO)
        End Function

        ''' <summary>
        ''' Reads all data in the directory as a single data source.
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <param name="files">Csv files list</param>
        ''' <param name="encoding"></param>
        ''' <returns></returns>
        <Extension>
        Public Iterator Function IteratesAll(Of T As Class)(
                                 files As IEnumerable(Of String),
                        Optional encoding As Encodings = Encodings.Default) As IEnumerable(Of T)

            Dim queue = files.ReadQueue(Of T)(encoding)

            For Each data As NamedValue(Of T()) In queue
                For Each x As T In data.x
                    Yield x
                Next
            Next
        End Function

#Region "How this workflow works?"

        ' The data loading workflow can be explained by the example code show below:
        ' ===========================================================================

        ' Sub Main()
        '    For Each x As String In TestWorkflowBase()
        '        Console.WriteLine(x)
        '    Next
        ' End Sub

        ' Private Iterator Function TestWorkflowBase() As IEnumerable(Of String)
        '    Dim LQuery As IEnumerable(Of String) = From s As Integer
        '                                           In {0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11, 12, 13, 14, 15}.AsParallel
        '                                           Let exc = MsgBox(s)
        '                                           Select CStr(s)
        '    For Each s As String In LQuery
        '        Yield s
        '    Next
        ' End Function
#End Region

    End Module
End Namespace
