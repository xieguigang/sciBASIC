#Region "Microsoft.VisualBasic::09b3196034616e16a56cec2d69cb6809, Data\BinaryData\HDF5\test\Hdf5Diagnostics.vb"

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

    '   Total Lines: 399
    '    Code Lines: 328 (82.21%)
    ' Comment Lines: 16 (4.01%)
    '    - Xml Docs: 31.25%
    ' 
    '   Blank Lines: 55 (13.78%)
    '     File Size: 16.38 KB


    '     Module Hdf5Diagnostics
    ' 
    '         Function: describeSuperblock, Diagnose, firstFrame, FormatElement, getDataspace
    '                   getDataType, isGroupObject, renderSample
    ' 
    '         Sub: readDatasetSample, visitAttribute, visitDataset, visitGroup
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.IO
Imports System.Text
Imports Microsoft.VisualBasic.Data.IO.HDF5
Imports Microsoft.VisualBasic.Data.IO.HDF5.dataset
Imports Microsoft.VisualBasic.Data.IO.HDF5.struct
Imports Microsoft.VisualBasic.Data.IO.HDF5.struct.messages
Imports Microsoft.VisualBasic.Data.IO.HDF5.type

Namespace test

    ''' <summary>
    ''' 对单个 10x Genomics HDF5 文件执行完整诊断：
    ''' 打开文件 -> 打印超级块 -> 递归遍历对象树 -> 读取 attributes -> 抽样读取 dataset。
    ''' 每个对象独立 try/catch，失败写入报告后继续，单点失败不中断整体流程。
    ''' </summary>
    Public Module Hdf5Diagnostics

        Public Function Diagnose(filePath As String,
                                 Optional maxSampleElements As Integer = 12,
                                 Optional largeDatasetThreshold As Long = 1000000L) As DiagnosticReport

            Dim report As New DiagnosticReport(filePath)

            Dim hdf5 As HDF5File = Nothing
            Dim rootGroup As Group = Nothing
            Dim sb As Superblock = Nothing

            Try
                hdf5 = HDF5File.Open(filePath)
                sb = hdf5.superblock

                report.Add(describeSuperblock(sb, filePath))

                ' 以与 HDF5File.parseHeader 一致的方式重建根组，避免依赖内部字段
                Dim rootHeaderAddress As Long = sb.rootGroupHeaderAddress
                Dim rootFacade As New DataObjectFacade(sb, "root", rootHeaderAddress)
                rootGroup = New Group(sb, rootFacade)

            Catch ex As Exception
                Dim entry As New DiagnosticEntry() With {
                    .path = "(open)",
                    .kind = "file",
                    .succeeded = False,
                    .errorType = ex.GetType().Name,
                    .errorMessage = ex.Message,
                    .errorFrame = firstFrame(ex)
                }
                report.Add(entry)
            End Try

            ' 遍历阶段独立成段：单点失败不中断整体，错误以对象路径记录
            If rootGroup IsNot Nothing AndAlso sb IsNot Nothing Then
                Try
                    visitGroup(report, rootGroup, "/", sb, maxSampleElements, largeDatasetThreshold)
                Catch ex As Exception
                    report.Add(New DiagnosticEntry() With {
                        .path = "(traverse)",
                        .kind = "file",
                        .succeeded = False,
                        .errorType = ex.GetType().Name,
                        .errorMessage = ex.Message,
                        .errorFrame = firstFrame(ex)
                    })
                End Try
            End If

            If hdf5 IsNot Nothing Then
                hdf5.Dispose()
            End If

            Return report
        End Function

        Private Function describeSuperblock(sb As Superblock, filePath As String) As DiagnosticEntry
            Dim entry As New DiagnosticEntry() With {
                .path = "(superblock)",
                .kind = "file",
                .succeeded = True,
                .shape = "v" & sb.versionOfSuperblock,
                .dataType = "offsets=" & sb.sizeOfOffsets & " lengths=" & sb.sizeOfLengths,
                .layout = "rootGroup=" & sb.rootGroupHeaderAddress,
                .filters = "K(leaf=" & sb.groupLeafNodeK & ",internal=" & sb.groupInternalNodeK & ")"
            }
            entry.sampleValues = Path.GetFileName(filePath)
            Return entry
        End Function

        Private Sub visitGroup(report As DiagnosticReport,
                               group As Group,
                               parentPath As String,
                               sb As Superblock,
                               maxSample As Integer,
                               largeThreshold As Long)

            ' 组自身的属性
            Try
                For Each attr In group.attributes
                    visitAttribute(report, attr, parentPath, sb)
                Next
            Catch ex As Exception
                report.Add(New DiagnosticEntry() With {
                    .path = parentPath & "@attributes",
                    .kind = "attribute",
                    .succeeded = False,
                    .errorType = ex.GetType().Name,
                    .errorMessage = ex.Message,
                    .errorFrame = firstFrame(ex)
                })
            End Try

            For Each child In group.objects
                Dim name = If(String.IsNullOrEmpty(child.symbolName), "?", child.symbolName)
                Dim childPath = parentPath & name

                Dim isGroup As Boolean

                Try
                    isGroup = isGroupObject(child)
                Catch
                    isGroup = False
                End Try

                If isGroup Then
                    Dim entry As New DiagnosticEntry() With {
                        .path = childPath & "/",
                        .kind = "group",
                        .succeeded = True,
                        .shape = "",
                        .dataType = "",
                        .layout = "",
                        .filters = ""
                    }
                    report.Add(entry)

                    Dim subGroup As Group = Nothing
                    Try
                        subGroup = New Group(sb, child)
                    Catch ex As Exception
                        report.Add(New DiagnosticEntry() With {
                            .path = childPath & "/",
                            .kind = "group",
                            .succeeded = False,
                            .errorType = ex.GetType().Name,
                            .errorMessage = ex.Message,
                            .errorFrame = firstFrame(ex)
                        })
                        Continue For
                    End Try

                    visitGroup(report, subGroup, childPath & "/", sb, maxSample, largeThreshold)
                Else
                    visitDataset(report, child, childPath, sb, maxSample, largeThreshold)
                End If
            Next
        End Sub

        Private Function isGroupObject(facade As DataObjectFacade) As Boolean
            If facade.dataObject Is Nothing OrElse facade.dataObject.messages Is Nothing Then
                Return False
            End If

            For Each msg In facade.dataObject.messages
                If msg.headerMessageType Is ObjectHeaderMessageType.Group Then
                    Return True
                End If
                If msg.headerMessageType Is ObjectHeaderMessageType.GroupInfo Then
                    Return True
                End If
                If msg.headerMessageTypeNumber = ObjectHeaderMessages.SymbolTableMessage Then
                    Return True
                End If
                If msg.headerMessageTypeNumber = ObjectHeaderMessages.LinkInfo Then
                    Return True
                End If
            Next

            ' 没有 dataspace 消息且不是 dataset 的，多半是组
            Dim hasDataspace As Boolean = False
            For Each m In facade.dataObject.messages
                If m.headerMessageType Is ObjectHeaderMessageType.SimpleDataspace Then
                    hasDataspace = True
                    Exit For
                End If
            Next

            Return Not hasDataspace
        End Function

        Private Sub visitDataset(report As DiagnosticReport,
                                 facade As DataObjectFacade,
                                 path As String,
                                 sb As Superblock,
                                 maxSample As Integer,
                                 largeThreshold As Long)

            Dim entry As New DiagnosticEntry() With {
                .path = path,
                .kind = "dataset"
            }

            Try
                Dim lm = facade.layoutMessage
                Dim fm = facade.filterMessage
                Dim dm = getDataspace(facade)
                Dim dtm = getDataType(facade)

                ' 形状
                If dm IsNot Nothing Then
                    entry.shape = "[" & String.Join(" x ", dm.dimensionLength.Select(Function(d) d.ToString())) & "]"
                End If

                ' 类型
                If dtm IsNot Nothing Then
                    entry.dataType = dtm.type.ToString() & " (" & dtm.byteSize & "B)"
                End If

                ' 布局
                If lm IsNot Nothing Then
                    entry.layout = lm.type.ToString()
                    If lm.type = LayoutClass.ChunkedStorage AndAlso lm.chunkSize IsNot Nothing Then
                        entry.layout &= " chunk=[" & String.Join(" x ", lm.chunkSize.Select(Function(c) c.ToString())) & "]"
                    End If
                End If

                ' 过滤器
                If fm IsNot Nothing AndAlso fm.description IsNot Nothing Then
                    entry.filters = String.Join(",", fm.description.Select(Function(f) f.id.ToString()))
                End If

                ' 读取数据
                If lm IsNot Nothing AndAlso lm.dataset IsNot Nothing Then
                    readDatasetSample(entry, lm.dataset, sb, dm, maxSample, largeThreshold)
                Else
                    entry.succeeded = True
                    entry.sampleValues = "(无 dataset 对象)"
                End If

            Catch ex As Exception
                entry.succeeded = False
                entry.errorType = ex.GetType().Name
                entry.errorMessage = ex.Message
                entry.errorFrame = firstFrame(ex)
            End Try

            report.Add(entry)
        End Sub

        Private Sub readDatasetSample(entry As DiagnosticEntry,
                                      dataset As Hdf5Dataset,
                                      sb As Superblock,
                                      dm As DataspaceMessage,
                                      maxSample As Integer,
                                      largeThreshold As Long)

            Dim totalElements As Long = 0
            If dm IsNot Nothing AndAlso dm.dimensionLength IsNot Nothing Then
                totalElements = 1
                For Each d In dm.dimensionLength
                    totalElements *= d
                Next
            End If

            ' 超大数组只抽样：避免一次性整体载入内存导致 OOM。
            ' 先仅记录元数据，待底层补齐分块抽样能力后再读取前 N 个元素。
            If totalElements > largeThreshold Then
                entry.succeeded = True
                entry.sampleValues = "(大数组, 元素数=" & totalElements & ", 跳过整体载入，仅记录结构)"
                Return
            End If

            Try
                Dim raw = dataset.data(sb)
                If raw Is Nothing Then
                    entry.succeeded = True
                    entry.sampleValues = "(null)"
                    Return
                End If

                entry.succeeded = True
                entry.sampleValues = renderSample(raw, maxSample)

                If String.IsNullOrEmpty(entry.sanityWarning) Then
                    entry.sanityWarning = SanityCheck.Check(entry.path, entry.sampleValues)
                    If String.IsNullOrEmpty(entry.sanityWarning) Then
                        Dim hint = SanityCheck.Hint(entry.path)
                        If Not String.IsNullOrEmpty(hint) Then
                            entry.sanityWarning = ""
                        End If
                    End If
                End If
            Catch ex As Exception
                entry.succeeded = False
                entry.errorType = ex.GetType().Name
                entry.errorMessage = ex.Message
                entry.errorFrame = firstFrame(ex)
            End Try
        End Sub

        Private Function renderSample(raw As Object, maxSample As Integer) As String
            If raw Is Nothing Then Return "(null)"

            If TypeOf raw Is Array Then
                Dim arr = DirectCast(raw, Array)
                Dim total = arr.Length

                If total <= maxSample Then
                    Dim parts As New List(Of String)()
                    For i = 0 To total - 1
                        parts.Add(FormatElement(arr.GetValue(i)))
                    Next
                    Return "[" & String.Join(", ", parts) & "]"
                Else
                    Dim parts As New List(Of String)()
                    For i = 0 To maxSample - 1
                        parts.Add(FormatElement(arr.GetValue(i)))
                    Next
                    Return "[" & String.Join(", ", parts) & ", ... +" & (total - maxSample) & "]"
                End If
            End If

            Return FormatElement(raw)
        End Function

        Private Function FormatElement(v As Object) As String
            If v Is Nothing Then Return "null"
            If TypeOf v Is String Then
                Dim s = DirectCast(v, String)
                If s.Length > 40 Then
                    Return """" & s.Substring(0, 40) & "...(" & s.Length & ")"""
                End If
                Return """" & s & """"
            End If
            Return v.ToString()
        End Function

        Private Sub visitAttribute(report As DiagnosticReport,
                                   attr As AttributeMessage,
                                   parentPath As String,
                                   sb As Superblock)
            Dim entry As New DiagnosticEntry() With {
                .path = parentPath & "@" & attr.name,
                .kind = "attribute"
            }

            Try
                entry.dataType = If(attr.dataType Is Nothing, "", attr.dataType.type.ToString())
                If attr.dataType IsNot Nothing Then
                    entry.dataType &= " (" & attr.dataType.byteSize & "B)"
                End If
                If attr.dataSpace IsNot Nothing Then
                    entry.shape = "[" & String.Join(" x ", attr.dataSpace.dimensionLength.Select(Function(d) d.ToString())) & "]"
                End If

                entry.succeeded = True
                entry.sampleValues = "(attribute 元数据)"
            Catch ex As Exception
                entry.succeeded = False
                entry.errorType = ex.GetType().Name
                entry.errorMessage = ex.Message
                entry.errorFrame = firstFrame(ex)
            End Try

            report.Add(entry)
        End Sub


        Private Function firstFrame(ex As Exception) As String
            If ex.StackTrace Is Nothing Then
                Return ""
            End If
            Dim lines = ex.StackTrace.Split(New Char() {vbCr, vbLf}, StringSplitOptions.RemoveEmptyEntries)
            If lines.Length > 0 Then
                Return lines(0).Trim()
            End If
            Return ""
        End Function

        Private Function getDataspace(facade As DataObjectFacade) As DataspaceMessage
            If facade.dataObject Is Nothing Then Return Nothing
            For Each m In facade.dataObject.messages
                If m.headerMessageType Is ObjectHeaderMessageType.SimpleDataspace Then
                    Return m.dataspaceMessage
                End If
            Next
            Return Nothing
        End Function

        Private Function getDataType(facade As DataObjectFacade) As DataTypeMessage
            If facade.dataObject Is Nothing Then Return Nothing
            For Each m In facade.dataObject.messages
                If m.headerMessageType Is ObjectHeaderMessageType.Datatype Then
                    Return m.dataTypeMessage
                End If
            Next
            Return Nothing
        End Function
    End Module

End Namespace

