Imports System.Text
Imports Path = System.String

''' <summary>
''' A FASTA file that contains multiple sequence data.
''' (一个包含有多条序列数据的FASTA文件)
''' </summary>
''' <remarks></remarks>
<LINQ.Framework.Reflection.LINQEntity("fasta")>
Public Class FASTA2 : Inherits File
    Implements LINQ.Framework.ILINQCollection
    Implements System.IDisposable
    Implements Generic.IEnumerable(Of FASTA)

    Dim FASTAList As List(Of FASTA) = New List(Of FASTA)
    Dim _SourceFile As String

    Const Scan0 = 0

    ''' <summary>
    ''' 本FASTA数据文件对象的文件位置
    ''' </summary>
    ''' <value></value>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public ReadOnly Property SourceFile As String
        Get
            Return _SourceFile
        End Get
    End Property

    Public Sub Add(Seq As FASTA)
        Call FASTAList.Add(Seq)
    End Sub

    Public Function AddRange(FASTACollection As Generic.IEnumerable(Of FASTA)) As Long
        Call FASTAList.AddRange(FASTACollection)
        Return FASTAList.LongCount
    End Function

    ''' <summary>
    ''' Get a new fasta2 object which is been clear the duplicated records in the collection.
    ''' (获取去除集合中的重复的记录新列表，原有列表中数据未被改变)
    ''' </summary>
    ''' <remarks></remarks>
    Public Function Distinct() As FASTA2
        Dim List = (From fsa In FASTAList Select fsa Order By fsa.Title Ascending).ToList
        For i As Integer = 1 To List.Count - 1
            If String.Equals(List(i).Title, List(i - 1).Title) Then
                Call List.RemoveAt(i)
            End If

            If i = List.Count Then
                Exit For
            End If
        Next

        Return List
    End Function

    Public Shared Function Read(File As Path) As FASTA2
        Dim FASTA As New FASTA2 With {._SourceFile = File}, DtFile As File = File
        Dim sBuilder As List(Of String) = New List(Of String)

        For Each Line As String In DtFile.Data
            If String.IsNullOrEmpty(Line) Then
                Continue For
            ElseIf Line.Chars(Scan0) = ">"c Then  'New FASTA Object
                FASTA.FASTAList.Add(Global.TestLINQEntity.FASTA.Parse(sBuilder))
                sBuilder.Clear()
            End If

            sBuilder.Add(Line)
        Next

        Call FASTA.FASTAList.RemoveAt(Scan0)
        Call FASTA.FASTAList.Add(Global.TestLINQEntity.FASTA.Parse(sBuilder))

        Dim OrderQuery = From e As FASTA In FASTA.FASTAList Select e Order By e.ToString Ascending '

        FASTA.FASTAList = OrderQuery.ToList

        Return FASTA
    End Function

    Public Sub Split(SaveDir As Path)
        Call FileIO.FileSystem.CreateDirectory(SaveDir)

        Dim Index As Integer

        For Each FASTA In FASTAList
            Index += 1
            FASTA.Save(String.Format("{0}/{1}.fasta", SaveDir, Index))
        Next
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="KeyWord">A key string that to search in this fasta file.</param>
    ''' <param name="CaseSensitive">For text compaired method that not case sensitive, otherwise in the method od binary than case sensitive.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Query2(KeyWord As String, Optional CaseSensitive As CompareMethod = CompareMethod.Text) As FASTA2
        Dim LQuery As Generic.IEnumerable(Of FASTA) = From FASTA In FASTAList.AsParallel Where Find(FASTA.Attributes, KeyWord, CaseSensitive)
                                                      Select FASTA '
        Return New FASTA2 With {.FASTAList = LQuery.ToList}
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="KeyWord">A key string that to search in this fasta file.</param>
    ''' <param name="CaseSensitive">For text compaired method that not case sensitive, otherwise in the method od binary than case sensitive.</param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Query(KeyWord As String, Optional CaseSensitive As CompareMethod = CompareMethod.Text) As FASTA
        Dim LQuery As Generic.IEnumerable(Of FASTA) = From FASTA In FASTAList.AsParallel Where Find(FASTA.Attributes, KeyWord, CaseSensitive)
                                                      Select FASTA '
        LQuery = LQuery.ToArray
        If LQuery.Count = 0 Then
            Return Nothing
        Else
            Return LQuery.First
        End If
    End Function

    Public Function Query(Keyword As String, Index As Integer, Optional CaseSensitive As CompareMethod = CompareMethod.Text) As FASTA()
        Dim List = (From fsa In FASTAList Where fsa.Attributes.Count - 1 >= Index Select fsa).ToArray
        Dim LQuery = From fsa In List Where InStr(fsa.Attributes(Index), Keyword, CaseSensitive) > 0 Select fsa '
        Return LQuery.ToArray
    End Function

    Private Shared Function Find(AttributeList As String(), KeyWord As String, CaseSensitive As CompareMethod) As Boolean
        For i As Integer = 0 To AttributeList.Length - 1
            If InStr(AttributeList(i), KeyWord, CaseSensitive) Then
                Return True
            End If
        Next

        Return False
    End Function

    Public Function Take(KeyWordList As List(Of String), Optional CaseSensitive As CompareMethod = CompareMethod.Text) As FASTA2
        Dim FASTA2List As New List(Of FASTA)
        For Each KeyWord As String In KeyWordList
            Dim LQuery = From FASTA In FASTAList Where InStr(FASTA.Data.First, KeyWord, CaseSensitive) Select FASTA '
            FASTA2List.AddRange(LQuery.ToArray)
        Next

        Return New FASTA2 With {.FASTAList = FASTA2List}
    End Function

    Public Overrides Sub Save(File As Path)
        Dim sBuilder As StringBuilder = New StringBuilder(10 * 1024)

        For Each FASTA In FASTAList
            sBuilder.AppendLine(FASTA.Generate)
        Next

        Call FileIO.FileSystem.WriteAllText(File, sBuilder.ToString, append:=False)
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="File">
    ''' The target FASTA file that to append this FASTA sequences.(将要拓展这些FASTA序列的目标FASTA文件)
    ''' </param>
    ''' <remarks></remarks>
    Public Sub AppendToFile(File As Path)
        Dim sBuilder As StringBuilder = New StringBuilder(10 * 1024)

        For Each FASTA In FASTAList
            sBuilder.AppendLine(FASTA.Generate)
        Next

        Call FileIO.FileSystem.WriteAllText(File, sBuilder.ToString, append:=True)
    End Sub

    Public Overrides Function ToString() As String
        Return String.Format("{0}; [{1} records]", _SourceFile, Count)
    End Function

    Public Shared Shadows Widening Operator CType(Collection As FASTA()) As FASTA2
        Return New FASTA2 With {.FASTAList = Collection.ToList}
    End Operator

    Public Shared Shadows Widening Operator CType(Collection As List(Of FASTA)) As FASTA2
        Return New FASTA2 With {.FASTAList = Collection}
    End Operator

    Public Shared Shadows Widening Operator CType(fsa As FASTA) As FASTA2
        Return New FASTA2 With {.FASTAList = New List(Of FASTA) From {fsa}}
    End Operator

    Public Shadows Iterator Function GetEnumerator() As IEnumerator(Of FASTA) Implements IEnumerable(Of FASTA).GetEnumerator
        For i As Integer = 0 To FASTAList.Count - 1
            Yield FASTAList(i)
        Next
    End Function

    Public Shadows Iterator Function GetEnumerator1() As IEnumerator Implements IEnumerable.GetEnumerator
        Yield GetEnumerator()
    End Function

    Public Overrides Function GetCollection(FilePath As String) As Object()
        Dim File = FASTA2.Read(FilePath)
        Return File.FASTAList.ToArray
    End Function

    Public Overrides Function GetEntityType() As Type
        Return GetType(FASTA)
    End Function
End Class

''' <summary>
''' The FASTA format file of a bimolecular sequence.(Notice that this file is 
''' only contains on sequence.)
''' FASTA格式的生物分子序列文件。(但是请注意：文件中只包含一条序列的情况)
''' </summary>
''' <remarks></remarks>
Public Class FASTA : Inherits File

    ''' <summary>
    ''' The attribute header of this FASTA file.
    ''' (这个FASTA文件的属性头)
    ''' </summary>
    ''' <remarks></remarks>
    Public Attributes As String()
    ''' <summary>
    ''' The sequence data that contains in this FASTA file.
    ''' (包含在这个FASTA文件之中的序列数据)
    ''' </summary>
    ''' <remarks></remarks>
    Public Sequence As String

    ''' <summary>
    ''' 返回FASTA对象的标题
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Overrides Function ToString() As String
        Dim sBuilder As StringBuilder = New StringBuilder("> ", 1024)

        For Each attr As String In Attributes
            sBuilder.Append(attr & "|")
        Next
        sBuilder.Remove(sBuilder.Length - 1, 1)
        Return sBuilder.ToString
    End Function

    Public ReadOnly Property Title As String
        Get
            Return Me.ToString
        End Get
    End Property

    Public Shared Shadows Widening Operator CType(Path As String) As FASTA
        Return Load(File:=Path)
    End Operator

    Public Shared Function Load(File As String) As FASTA
        Dim DataFile As File = File
        Dim FASTA As FASTA = New FASTA

        Call DataFile.CopyTo(FASTA)

        FASTA.Attributes = DataFile.Data.First.Split("|")
        FASTA.Sequence = Contact(DataFile.Data.Skip(1))  'Linux mono does not support <Extension> attribute!

        Return FASTA
    End Function

    Public Shared Function Parse(FASTAStream As Generic.IEnumerable(Of String)) As FASTA
        If FASTAStream Is Nothing OrElse FASTAStream.Count = 0 Then Return Nothing

        Dim DataFile As File = New File With {.Data = FASTAStream.ToArray}
        Dim FASTA As FASTA = New FASTA

        Call DataFile.CopyTo(FASTA)

        FASTA.Attributes = DataFile.Data.First.Replace(">", "").Split("|")
        FASTA.Sequence = Contact(DataFile.Data.Skip(1))  'Linux mono does not support <Extension> attribute!

        Return FASTA
    End Function

    ''' <summary>
    ''' Generate a FASTA file string.
    ''' (将这个FASTA对象转换为文件格式以方便进行存储)
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Generate() As String
        Dim sBuilder As StringBuilder = New StringBuilder(">", 10 * 1024)

        For Each Attribute In Attributes
            sBuilder.AppendFormat("{0}|", Attribute)
        Next
        sBuilder.Remove(sBuilder.Length - 1, 1)
        sBuilder.AppendLine()

        For i As Integer = 1 To Len(Sequence) Step 60
            Dim Segment As String = Mid(Sequence, i, 60)
            sBuilder.AppendLine(Segment)
        Next

        Return sBuilder.ToString
    End Function

    Public Overrides Function Equals(obj As Object) As Boolean
        If TypeOf obj Is FASTA Then
            Dim [Object] = DirectCast(obj, FASTA)
            Return String.Equals([Object].Title, Me, Title) AndAlso String.Equals([Object].Sequence, Me.Sequence)
        Else
            Return False
        End If
    End Function

    Public Overrides Sub Save(Path As String)
        Call FileIO.FileSystem.WriteAllText(Path, Me.Generate, append:=False)
    End Sub

    ''' <summary>
    ''' Enumerate all of the amino acid.
    ''' (字符串常量枚举所有的氨基酸分子)
    ''' </summary>
    ''' <remarks></remarks>
    Const AAALL As String = "BDEFHIJKLMNOPQRSVWXYZ"

    ''' <summary>
    ''' (判断这条序列是否为蛋白质序列)
    ''' </summary>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function IsProtein() As Boolean
        Dim Query = From c As Char In Sequence.ToUpper Where InStr(AAALL, c) Select 1 '
        Try
            Return Query.First > 0
        Catch ex As Exception
            Return False
        End Try
    End Function

    Public Function Reverse() As FASTA
        Dim Attributes As List(Of String) = Me.Attributes.ToList
        Dim FASTA As FASTA = New FASTA
        Call Attributes.Add("Reversed_sequence")
        FASTA.Attributes = Attributes.ToArray
        FASTA.Sequence = Sequence.Reverse.ToArray

        Return FASTA
    End Function

    Public Shared Shadows Narrowing Operator CType(e As FASTA) As String
        Return e.Generate
    End Operator
End Class
