#Region "Microsoft.VisualBasic::0f5ca985063733fc5bab5a550d86aead, ..\visualbasic_App\Microsoft.VisualBasic.DataMining.Framework\PeriodAnalysis\PeriodAnalysis.vb"

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
Imports Microsoft.VisualBasic.CommandLine.Reflection
Imports Microsoft.VisualBasic.ComponentModel.Collection.Generic
Imports Microsoft.VisualBasic.Data.csv
Imports Microsoft.VisualBasic.Data.csv.DocumentStream
Imports Microsoft.VisualBasic.IEnumerations
Imports Microsoft.VisualBasic.Scripting.MetaData

Namespace Serials.PeriodAnalysis

    <[PackageNamespace]("Serials.PeriodAnalysis", Publisher:="xie.guigang@gmail.com")>
    Public Module PeriodAnalysis

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="UniqueId"></param>
        ''' <param name="WindowSize"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <ExportAPI("Analysis")>
        <Extension> Public Function Analysis(source As IEnumerable(Of SerialsVarialble),
                                             UniqueId As String,
                                             WindowSize As UInteger) As SamplingData
            Dim data As SerialsVarialble = source.GetItem(uniqueId:=UniqueId)
            Return Analysis(data, WindowSize)
        End Function

        ''' <summary>
        ''' 返回的数据是周期变化数据，故而假若需要计算频率变化的话，还需要求倒数
        ''' </summary>
        ''' <param name="SerialsData"></param>
        ''' <param name="WindowSize"></param>
        ''' <returns></returns>
        <ExportAPI("Analysis")>
        Public Function Analysis(<Parameter("Data.Serials")> SerialsData As SerialsVarialble,
                                 <Parameter("Window.Size")> WindowSize As UInteger) As SamplingData

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

        Private Function FilteringData(Peaks As List(Of TimePoint), Trough As List(Of TimePoint), OriginalTimePoints As Integer) As SamplingData
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
        Public Function ConvertData(sample As SamplingData) As DocumentStream.File
            Dim DataFile As New DocumentStream.File
            Dim Row As New DocumentStream.RowObject From {"Sampling"}

            For i As Integer = 0 To sample.TimePoints
                Dim n = TimePoint.GetData(i, sample.Peaks)
                If n = 0.0R Then
                    n = TimePoint.GetData(i, sample.Trough)
                End If
                Call Row.Add(n)
            Next

            Call DataFile.Add(Row)
            Row = New DocumentStream.RowObject From {"Filted"}

            Dim avg = (From p In sample.FiltedData Select p.Value).Average
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
        Public Function LoadDataFromCsv(path As String) As SerialsVarialble()
            Return SerialsVarialble.Load(path)
        End Function
    End Module
End Namespace
