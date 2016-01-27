Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.IEnumerations
Imports Microsoft.VisualBasic.Scripting.MetaData

Public Structure SerialsVarialble : Implements Microsoft.VisualBasic.ComponentModel.Collection.Generic.sIdEnumerable

    Public Property Identifier As String Implements Microsoft.VisualBasic.ComponentModel.Collection.Generic.sIdEnumerable.Identifier
    Dim SerialsData As Double()

    Public Overrides Function ToString() As String
        Return Identifier
    End Function

    Public Shared Function Load(CsvFile As DocumentFormat.Csv.DocumentStream.File) As SerialsVarialble()
        Dim LQuery = (From Line As Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream.RowObject
                      In CsvFile
                      Let Tag As String = Line.First
                      Let Data As Double() = (From col As String In Line.Skip(1) Select Val(col)).ToArray
                      Select New SerialsVarialble With {.Identifier = Tag, .SerialsData = Data}).ToArray
        Return LQuery
    End Function
End Structure

<[PackageNamespace]("Serials.PeriodAnalysis", Publisher:="xie.guigang@gmail.com")>
Public Class PeriodAnalysis

    Dim ChunkBuffer As SerialsVarialble()

    Sub New(ChunkBuffer As Generic.IEnumerable(Of SerialsVarialble))
        Me.ChunkBuffer = ChunkBuffer
    End Sub

    Public Class TimePoint
        Public Property Time As Integer
        Public Property Value As Double

        Public Overrides Function ToString() As String
            Return String.Format("{0} --> {1}", Time, Value)
        End Function

        Friend Shared Function GetData(Collection As Generic.IEnumerable(Of TimePoint), Time As Integer) As TimePoint
            Dim LQuery = (From p In Collection Where p.Time = Time Select p).First
            Return LQuery
        End Function

        Friend Shared Function GetData(Time As Integer, Collection As Generic.IEnumerable(Of TimePoint)) As Double
            Dim LQuery = (From p In Collection Where p.Time = Time Select p).ToArray
            If LQuery.IsNullOrEmpty Then
                Return 0
            Else
                Return LQuery.First.Value
            End If
        End Function

        Friend Shared Function CreateBufferObject(ChunkBuffer As Generic.IEnumerable(Of TimePoint)) As SortedDictionary(Of Integer, Double)
            Dim Temp As SortedDictionary(Of Integer, Double) = New SortedDictionary(Of Integer, Double)
            For Each p In ChunkBuffer
                Call Temp.Add(p.Time, p.Value)
            Next

            Return Temp
        End Function
    End Class

    Public Class SamplingData
        Public Property Peaks As List(Of TimePoint)
        Public Property Trough As List(Of TimePoint)
        Public Property FiltedData As List(Of TimePoint)
        Public Property TimePoints As Integer
        Public Property TSerials As TimePoint()
    End Class

    ''' <summary>
    ''' 返回的数据是周期变化数据，故而假若需要计算频率变化的话，还需要求倒数
    ''' </summary>
    ''' <param name="UniqueId"></param>
    ''' <param name="WindowSize"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    Public Function Analysis(UniqueId As String, WindowSize As UInteger) As SamplingData
        Dim SerialsData = ChunkBuffer.GetItem(uniqueId:=UniqueId)
        Return Analysis(SerialsData, WindowSize)
    End Function

    <ExportAPI("Analysis")>
    Public Shared Function Analysis(<Parameter("Data.Serials")> SerialsData As SerialsVarialble, <Parameter("Window.Size")> WindowSize As UInteger) As SamplingData
        Dim Peaks As List(Of TimePoint) = New List(Of TimePoint)
        Dim Trough As List(Of TimePoint) = New List(Of TimePoint)
        Dim TempChunk As Double() = New Double(WindowSize - 1) {}

        For i As Integer = 0 To SerialsData.SerialsData.Count - 1 - WindowSize
            Call Array.ConstrainedCopy(SerialsData.SerialsData, i, TempChunk, 0, WindowSize)

            Dim Max As Double = Double.MinValue
            Dim MaxIndex As Integer = -1
            Dim Min As Double = Double.MaxValue
            Dim MinIndex As Integer = -1

            For Time As Integer = 0 To WindowSize - 1
                If TempChunk(Time) > Max Then
                    MaxIndex = Time
                    Max = TempChunk(Time)
                End If

                If TempChunk(Time) < Min Then
                    MinIndex = Time
                    Min = TempChunk(Time)
                End If
            Next

            '  If MaxIndex >= 0.2 * WindowSize AndAlso MaxIndex <= 0.8 * WindowSize Then '丢掉边界点
            Call Peaks.Add(New TimePoint With {.Time = MaxIndex + i, .Value = Max})
            ' End If

            '  If MinIndex >= 0.2 * WindowSize AndAlso MinIndex <= 0.8 * WindowSize Then
            Call Trough.Add(New TimePoint With {.Time = MinIndex + i, .Value = Min})
            '   End If
        Next

        For i As Integer = SerialsData.SerialsData.Count - 1 - WindowSize To SerialsData.SerialsData.Count - 2
            TempChunk = SerialsData.SerialsData.Skip(i).ToArray '       Call Array.ConstrainedCopy(SerialsData.SerialsData, i, TempChunk, 0, WindowSize)

            Dim Max As Double = Double.MinValue
            Dim MaxIndex As Integer = -1
            Dim Min As Double = Double.MaxValue
            Dim MinIndex As Integer = -1

            For Time As Integer = 0 To TempChunk.Count - 1
                If TempChunk(Time) > Max Then
                    MaxIndex = Time
                    Max = TempChunk(Time)
                End If

                If TempChunk(Time) < Min Then
                    MinIndex = Time
                    Min = TempChunk(Time)
                End If
            Next

            '  If MaxIndex >= 0.2 * WindowSize AndAlso MaxIndex <= 0.8 * WindowSize Then '丢掉边界点
            Call Peaks.Add(New TimePoint With {.Time = MaxIndex + i, .Value = Max})
            ' End If

            '  If MinIndex >= 0.2 * WindowSize AndAlso MinIndex <= 0.8 * WindowSize Then
            Call Trough.Add(New TimePoint With {.Time = MinIndex + i, .Value = Min})
            '   End If
        Next

        Return FilteringData(Peaks, Trough, SerialsData.SerialsData.Count)
    End Function

    Private Shared Function FilteringData(Peaks As List(Of TimePoint), Trough As List(Of TimePoint), OriginalTimePoints As Integer) As SamplingData
        Dim PeaksId = (From p In Peaks Select p.Time Distinct).ToArray, TroughsId = (From p In Trough Select p.Time Distinct).ToArray
        Dim Sample As SamplingData = New SamplingData With {.Peaks = (From p In PeaksId Select TimePoint.GetData(Peaks, p)).ToList, .Trough = (From p In TroughsId Select TimePoint.GetData(Trough, p)).ToList}
        Dim Chunkbuffer As List(Of TimePoint) = New List(Of TimePoint)
        Call Chunkbuffer.AddRange(Sample.Peaks)
        Call Chunkbuffer.AddRange(Sample.Trough)
        Sample.FiltedData = (From p In Chunkbuffer Select p Order By p.Time Ascending).ToList

        Dim PeaksBuffer = TimePoint.CreateBufferObject(Sample.Peaks), TroughBuffer = TimePoint.CreateBufferObject(Sample.Trough)
        Dim PreData = Sample.FiltedData.First
        Dim Turn_CheckPeak As Boolean = PeaksBuffer.ContainsKey(PreData.Time)
        Dim FiltedList As New List(Of TimePoint)
        Dim TList As List(Of TimePoint) = New List(Of TimePoint)

        If Turn_CheckPeak Then
            Call TList.Add(PreData)
        End If

        For Each NextData In Sample.FiltedData.Skip(1)
            If Turn_CheckPeak Then  '假若PreData是一个峰值的话，则检查NextData是否为一个峰谷值
                If TroughBuffer.ContainsKey(NextData.Time) Then
                    Call FiltedList.Add(PreData) '假若为波谷，则添加  'PreData为波峰数据
                    Call TList.Add(PreData)
                    PreData = NextData
                    Turn_CheckPeak = False
                Else
                    '否则，PreData为最大的的值
                    If PreData.Value < NextData.Value Then
                        PreData = NextData
                    End If
                End If
            Else
                If PeaksBuffer.ContainsKey(NextData.Time) Then
                    Call FiltedList.Add(PreData)
                    PreData = NextData
                    Turn_CheckPeak = True
                Else
                    If PreData.Value > NextData.Value Then
                        PreData = NextData
                    End If
                End If
            End If
        Next

        Sample.FiltedData = FiltedList
        Sample.TimePoints = OriginalTimePoints
        Dim NewTList As List(Of TimePoint) = New List(Of TimePoint)  '得到周期之差
        Dim pre = TList.First
        For i As Integer = 1 To TList.Count - 1
            Dim n = TList(i)
            Call NewTList.Add(New TimePoint With {.Value = (n.Time - pre.Time), .Time = n.Time})
            pre = n
        Next

        Dim value = NewTList.First
        Dim idx As Integer = 0

        Call Chunkbuffer.Clear()

        For i As Integer = 0 To Sample.TimePoints
            Call Chunkbuffer.Add(New TimePoint With {.Time = i, .Value = value.Value})

            If i > value.Time Then
                idx += 1
                If idx > NewTList.Count - 1 Then
                    For j = value.Time To Sample.TimePoints
                        Call Chunkbuffer.Add(New TimePoint With {.Time = j, .Value = value.Value})
                    Next
                    Exit For
                End If
                value = NewTList(idx)
            End If
        Next

        ' Call Console.WriteLine("[Original {0}] Sampling {1}", OriginalTimePoints, Chunkbuffer.Count)

        Sample.TSerials = Chunkbuffer.ToArray  '根据这个周期之差来计算多普勒效应

        Return Sample
    End Function

    <ExportAPI("Data.ConvertToCsv")>
    Public Shared Function ConvertData(sample As SamplingData) As Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream.File
        Dim DataFile As Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream.File = New DocumentFormat.Csv.DocumentStream.File
        Dim Row = New Microsoft.VisualBasic.DocumentFormat.Csv.DocumentStream.RowObject From {"Sampling"}
        For i As Integer = 0 To sample.TimePoints
            Dim n = TimePoint.GetData(i, sample.Peaks)
            If n = 0.0R Then
                n = TimePoint.GetData(i, sample.Trough)
            End If
            Call Row.Add(n)
        Next
        Call DataFile.Add(Row)
        Row = New DocumentFormat.Csv.DocumentStream.RowObject From {"Filted"}

        Dim avg = (From p In sample.FiltedData Select p.Value).ToArray.Average
        For i As Integer = 0 To sample.TimePoints
            Dim n = TimePoint.GetData(i, sample.FiltedData)
            If n = 0.0R Then
                n = avg
            End If
            Call Row.Add(n)
        Next
        Call DataFile.Add(Row)

        Return DataFile
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="path"></param>
    ''' <returns></returns>
    ''' <remarks></remarks>
    <ExportAPI("Load.From.Csv")>
    Public Shared Function LoadDataFromCsv(path As String) As PeriodAnalysis
        Dim Chunkbuffer = SerialsVarialble.Load(path)
        Return New PeriodAnalysis(Chunkbuffer)
    End Function

    <ExportAPI("Analysis")>
    Public Shared Function Analysis([operator] As PeriodAnalysis,
                                    <Parameter("Sample.ID")> sampleId As String,
                                    <Parameter("Window.Size")> WindowSize As Integer) As SamplingData
        Return [operator].Analysis(sampleId, WindowSize)
    End Function
End Class
