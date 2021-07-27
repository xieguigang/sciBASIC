#Region "Microsoft.VisualBasic::215821029db9113be0d815793c42d62f, Data_science\DataMining\DataMining\AprioriRules\Helper\Encoding.vb"

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

    '     Class Encoding
    ' 
    '         Properties: AllCodes, AllItems, CodeMappings
    ' 
    '         Constructor: (+1 Overloads) Sub New
    '         Function: Decode, GenerateCodes, ToString, (+2 Overloads) TransactionEncoding
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.DataMining.AprioriRules.Entities
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Text

Namespace AprioriRules

    ''' <summary>
    ''' Transaction encoding helper.(对一个Transaction之中的独立部件编码为一个字符)
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Encoding

        ' 一个事务是有多个关联的对象组成的，在这里一个事务就是一个字符串，一个字符就是事务里面有关联信息的对象

        Public ReadOnly Property CodeMappings As IReadOnlyDictionary(Of Char, String)
        Public ReadOnly Property AllItems As String()
        Public ReadOnly Property AllCodes As Char()

        Dim itemCodes As Dictionary(Of String, Char)

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="items"></param>
        ''' <remarks>
        ''' 因为<paramref name="items"/>的值可能会存在非常多种情况，所以在这构造函数之中会使用中文字符来进行编码
        ''' </remarks>
        Sub New(items As IEnumerable(Of String))
            Static a% = GB2312.a

            CodeMappings = items _
                .Distinct _
                .OrderBy(Function(s) s) _
                .Select(Function(s, i)
                            Return (code:=a + i, raw:=s)
                        End Function) _
                .ToDictionary(Function(c) ChrW(c.code),
                              Function(c)
                                  Return c.raw
                              End Function)
            itemCodes = CodeMappings.ToDictionary(Function(t) t.Value, Function(t) t.Key)
            AllItems = CodeMappings.Values.ToArray
            AllCodes = CodeMappings.Keys.ToArray
        End Sub

        Public Shared Iterator Function GenerateCodes(length As Integer) As IEnumerable(Of Char)
            Dim a% = GB2312.a

            For i As Integer = 0 To length - 1
                Yield ChrW(a + i)
            Next
        End Function

        Public Overrides Function ToString() As String
            Return $"{AllItems.Length} codes = {CodeMappings.Keys.Take(5).JoinBy(", ")}..."
        End Function

        ''' <summary>
        ''' rule transaction string to item names
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function Decode(rule As String) As String()
            Return rule _
                .Select(Function(c) CodeMappings(c)) _
                .ToArray
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="data">将事务编码为字符集和</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        ''' 
        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Public Function TransactionEncoding(data As IEnumerable(Of Transaction)) As IEnumerable(Of String)
            Return From item In data Select TransactionEncoding(item)
        End Function

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        Private Function TransactionEncoding(data As Transaction) As String
            Return data.Items.Select(Function(item) itemCodes(item)).CharString
        End Function
    End Class
End Namespace
