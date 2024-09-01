#Region "Microsoft.VisualBasic::e644c072ff6e3fee21829a675434afa6, Data_science\DataMining\DataMining\AprioriRules\Helper\Encoding.vb"

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

    '   Total Lines: 65
    '    Code Lines: 37 (56.92%)
    ' Comment Lines: 19 (29.23%)
    '    - Xml Docs: 78.95%
    ' 
    '   Blank Lines: 9 (13.85%)
    '     File Size: 2.69 KB


    '     Class Encoding
    ' 
    '         Properties: AllItems, CodeMappings
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: ToString, (+2 Overloads) TransactionEncoding
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.AprioriRules.Entities
Imports Microsoft.VisualBasic.Linq

Namespace AprioriRules

    ''' <summary>
    ''' Transaction encoding helper.(对一个Transaction之中的独立部件编码为一个字符)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Encoding

        ' 一个事务是有多个关联的对象组成的，在这里一个事务就是一个字符串，一个字符就是事务里面有关联信息的对象

        Public ReadOnly Property CodeMappings As IReadOnlyDictionary(Of Integer, String)
        Public ReadOnly Property AllItems As Item()

        Dim itemCodes As Dictionary(Of String, Integer)

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="items"></param>
        ''' <remarks>
        ''' 因为<paramref name="items"/>的值可能会存在非常多种情况，所以在这构造函数之中会使用中文字符来进行编码
        ''' </remarks>
        Sub New(items As IEnumerable(Of String))
            CodeMappings = items _
                .Distinct _
                .OrderBy(Function(s) s) _
                .Select(Function(s, i)
                            Return (code:=i, raw:=s)
                        End Function) _
                .ToDictionary(Function(c) c.code,
                              Function(c)
                                  Return c.raw
                              End Function)
            itemCodes = CodeMappings.ToDictionary(Function(t) t.Value, Function(t) t.Key)
            AllItems = CodeMappings _
                .Select(Function(t) New Item(t.Key, t.Value)) _
                .ToArray
        End Sub

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Encoding(item As String) As Item
            Return _AllItems(itemCodes(item))
        End Function

        Public Overrides Function ToString() As String
            Return $"{AllItems.Length} codes = {CodeMappings.Keys.Take(5).JoinBy(", ")}..."
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="data">将事务编码为字符集和</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function TransactionEncoding(data As IEnumerable(Of Transaction)) As IEnumerable(Of ItemSet)
            Return From item In data Select TransactionEncoding(item)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function TransactionEncoding(data As Transaction) As ItemSet
            Return New ItemSet(data.Items.Select(Function(item) New Item(itemCodes(item), item)))
        End Function
    End Class
End Namespace
