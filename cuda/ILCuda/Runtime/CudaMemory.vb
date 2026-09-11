#Region "Microsoft.VisualBasic::3678a594b66f1ce1a5ee6e7c57c9ebaa, cuda\ILCuda\Runtime\CudaMemory.vb"

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

    '   Total Lines: 90
    '    Code Lines: 62 (68.89%)
    ' Comment Lines: 11 (12.22%)
    '    - Xml Docs: 72.73%
    ' 
    '   Blank Lines: 17 (18.89%)
    '     File Size: 3.11 KB


    '     Structure MemoryInfo
    ' 
    '         Properties: FreeBytes, FreeMB, TotalBytes, TotalMB, UsedBytes
    '                     UsedMB, UsedRatio
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString
    ' 
    '     Module CudaMemory
    ' 
    '         Function: CanAllocate, Query, TryQuery
    ' 
    ' 
    ' /********************************************************************************/

#End Region

' ------------------------------------------------------------------------
' 显存信息查询
' ------------------------------------------------------------------------

Namespace Runtime

    ''' <summary>当前上下文可见的显存使用情况</summary>
    Public Structure MemoryInfo
        ''' <summary>可用字节数</summary>
        Public Property FreeBytes As ULong
        ''' <summary>总字节数</summary>
        Public Property TotalBytes As ULong

        Public Sub New(freeBytes As ULong, totalBytes As ULong)
            Me.FreeBytes = freeBytes
            Me.TotalBytes = totalBytes
        End Sub

        Public ReadOnly Property UsedBytes As ULong
            Get
                Return If(TotalBytes > FreeBytes, TotalBytes - FreeBytes, 0UL)
            End Get
        End Property

        Public ReadOnly Property FreeMB As Double
            Get
                Return FreeBytes / 1024.0 / 1024.0
            End Get
        End Property

        Public ReadOnly Property TotalMB As Double
            Get
                Return TotalBytes / 1024.0 / 1024.0
            End Get
        End Property

        Public ReadOnly Property UsedMB As Double
            Get
                Return UsedBytes / 1024.0 / 1024.0
            End Get
        End Property

        ''' <summary>已用比例（0~1）；总量未知时为 0</summary>
        Public ReadOnly Property UsedRatio As Double
            Get
                If TotalBytes = 0UL Then Return 0
                Return UsedBytes / CDbl(TotalBytes)
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{UsedMB:N0} / {TotalMB:N0} MB 已用 ({UsedRatio:P1})，剩余 {FreeMB:N0} MB"
        End Function
    End Structure

    ''' <summary>显存查询助手</summary>
    Public Module CudaMemory

        ''' <summary>查询当前上下文的显存总量与可用量</summary>
        Public Function Query() As MemoryInfo
            Dim free As ULong = 0
            Dim total As ULong = 0

            CudaDriverApi.Check(CudaDriverApi.cuMemGetInfo_v2(free, total), "cuMemGetInfo_v2")

            Return New MemoryInfo(free, total)
        End Function

        ''' <summary>尝试查询显存；失败时返回 Nothing（例如尚未建立上下文）</summary>
        Public Function TryQuery(ByRef info As MemoryInfo) As Boolean
            info = Nothing

            Try
                info = Query()
                Return True
            Catch
                Return False
            End Try
        End Function

        ''' <summary>判断申请 sizeBytes 显存是否大概率可行（留 5% 余量）</summary>
        Public Function CanAllocate(sizeBytes As ULong) As Boolean
            Dim info As MemoryInfo = Nothing
            If Not TryQuery(info) Then Return False

            Dim margin = CULng(info.FreeBytes / 20UL) ' 5%
            Return info.FreeBytes > sizeBytes + margin
        End Function
    End Module
End Namespace

