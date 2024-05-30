#Region "Microsoft.VisualBasic::1b650b9270f5529f4dda73bf314d4d0c, Microsoft.VisualBasic.Core\src\Data\Repository\SnowflakeIdGenerator.vb"

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

    '   Total Lines: 47
    '    Code Lines: 20 (42.55%)
    ' Comment Lines: 21 (44.68%)
    '    - Xml Docs: 95.24%
    ' 
    '   Blank Lines: 6 (12.77%)
    '     File Size: 2.52 KB


    '     Class SnowflakeIdGenerator
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: GenerateId
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Data.Repository

    ''' <summary>
    ''' a twitter unique Snowflake ID generator
    ''' </summary>
    Public Class SnowflakeIdGenerator

        Public Const DefaultEpoch As Long = 1288834974657L

        ReadOnly _epoch As Long
        ReadOnly _machineId As Long

        Dim _sequence As Long = 0L

        Public Sub New(machineId As Long, Optional epoch As Long = DefaultEpoch, Optional seqId As Long = 0L)
            _machineId = machineId
            _epoch = epoch
            _sequence = seqId
        End Sub

        ''' <summary>
        ''' generates a new snowflake id
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks>
        ''' Snowflake ID 通常是一个 64 位整数，由以下部分组成：41 位用于时间戳，提供毫秒级精度的自定义纪元；
        ''' 10 位用于机器/数据中心标识符，允许多达 1024 个唯一节点；12 位用于序列号，支持每个节点每毫秒生成
        ''' 多达 4096 个 ID。
        ''' 可扩展性：通过分散 ID 生成，Snowflake ID 允许系统水平扩展，而无需中央 ID 生成服务的瓶颈。
        ''' 唯一性：Snowflake ID 的结构确保了整个系统中每个 ID 的唯一性。可排序性：Snowflake ID 的时间戳组件
        ''' 允许它们可排序，这对于组织和索引数据可能有益。
        ''' 分布式数据库：Snowflake ID 在分布式数据库中被广泛使用，以确保从不同节点插入的记录具有唯一标识符。
        ''' 微服务架构：在微服务架构中，多个服务可能需要独立生成唯一标识符，Snowflake ID 提供了可靠的解决方案。
        ''' 
        ''' Snowflake ID 的概念起源于 Twitter，作为其对可扩展的唯一标识符生成系统的需求的解决方案。
        ''' 自那时起，它已被需要大规模唯一标识符的各种分布式系统和服务采用。演变：在 Twitter 推出后，
        ''' Snowflake ID 算法被开源，允许开发者社区对其进行演变和适应各种平台和技术的贡献。
        ''' </remarks>
        Public Function GenerateId() As Long
            SyncLock Me
                Dim currentTimestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                _sequence = _sequence + 1 And 4095
                Return currentTimestamp - _epoch << 22 Or _machineId << 12 Or _sequence
            End SyncLock
        End Function
    End Class
End Namespace
