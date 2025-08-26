#Region "Microsoft.VisualBasic::91c0c7bf05f5cc78570c51304351cd2d, Microsoft.VisualBasic.Core\src\ApplicationServices\Parallel\Threads\Groups\GroupAPI.vb"

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

    '   Total Lines: 44
    '    Code Lines: 34 (77.27%)
    ' Comment Lines: 7 (15.91%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 3 (6.82%)
    '     File Size: 2.79 KB


    '     Module GroupAPI
    ' 
    '         Function: ParallelGroup
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace Parallel

    Public Module GroupAPI

        ''' <summary>
        ''' 貌似使用LINQ进行Group操作的时候是没有并行化的，灰非常慢，则可以使用这个拓展函数来获取较好的性能
        ''' </summary>
        ''' <typeparam name="T"></typeparam>
        ''' <typeparam name="T_TAG"></typeparam>
        ''' <param name="source"></param>
        ''' <returns></returns>
        <Extension> Public Function ParallelGroup(Of T, T_TAG)(source As IEnumerable(Of T), __getGuid As Func(Of T, T_TAG)) As GroupResult(Of T, T_TAG)()
            Call $"Generating guid index...".debug
            Dim TAGS = (From x As T In source.AsParallel Select guid = __getGuid(x), x).ToArray
            Call Console.WriteLine($"[DEBUG {Now.ToString}] Start to create lquery partitions...")
            Dim Partitions = TAGS.Split(TAGS.Length / Environment.ProcessorCount)
            Call Console.WriteLine($"[DEBUG {Now.ToString}] Invoke parallel group operations....")
            Call Console.WriteLine($"[DEBUG {Now.ToString}] First groups...")
            Dim FirstGroups = (From Partition In Partitions.AsParallel
                               Select (From obj In (From Token In Partition
                                                    Select Token.guid
                                                    Group guid By guid Into Group).ToArray Select obj.guid).ToArray).ToArray.Unlist
            Call Console.WriteLine($"[DEBUG {Now.ToString}] Unique group...")
            Dim UniqueGroup = (From TAG As T_TAG
                               In FirstGroups.AsParallel
                               Select TAG
                               Group TAG By TAG Into Group).ToArray
            Call Console.WriteLine($"[DEBUG {Now.ToString}] Generating group result....")
            Call Console.WriteLine(" * Cache data....")
            Dim Cache = (From TAG In UniqueGroup.AsParallel Select TAG, List = New List(Of T)).ToArray
            Call Console.WriteLine(" * Address data.....")
            Dim Addressing = (From obj In TAGS.AsParallel Select obj, List = (From item In Cache Where item.TAG.TAG.Equals(obj.guid) Select item.List).FirstOrDefault).ToArray
            For Each Address In Addressing
                Call Address.List.Add(Address.obj.x)
            Next
            Call Console.WriteLine(" * Generate result.....")
            Dim LQuery = (From TAG In Cache.AsParallel Select New GroupResult(Of T, T_TAG)() With {.Tag = TAG.TAG.TAG, .Group = TAG.List.ToArray}).ToArray
            Call Console.WriteLine($"[DEBUG {Now.ToString}] Parallel group operation job done!")
            Return LQuery
        End Function
    End Module
End Namespace
