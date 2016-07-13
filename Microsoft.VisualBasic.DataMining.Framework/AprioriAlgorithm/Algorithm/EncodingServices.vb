#Region "Microsoft.VisualBasic::9b92765059e18f5fe0a1a12a1ee4cc9b, ..\VisualBasic_AppFramework\Microsoft.VisualBasic.DataMining.Framework\AprioriAlgorithm\Algorithm\EncodingServices.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xieguigang (xie.guigang@live.com)
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

Imports System.Collections.ObjectModel
Imports Microsoft.VisualBasic.Linq

Namespace AprioriAlgorithm

    Public Class BinaryEncodingServices

        Dim _codesMappings As Dictionary(Of Char, String)
        Dim _mappingCodes As Dictionary(Of String, Char)

        Sub New(Codes As IEnumerable(Of String))
            Dim CodesChr = EncodingServices.GenerateCodes(Codes.Count)
            _codesMappings =
                IteratorExtensions.SeqIterator(Of Char, String)(CodesChr, Codes) _
                    .ToDictionary(Function(obj) obj.obj,
                                  Function(obj) obj.Follow)
            _mappingCodes = _codesMappings.ToDictionary(Function(obj) obj.Value,
                                                        Function(obj) obj.Key)
        End Sub

        ''' <summary>
        ''' ±àÂëÒ»¸öÊÂÎñ
        ''' </summary>
        ''' <param name="transaction"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function EncodingTransaction(transaction As String()) As String
            Dim Chars = (From s As String In transaction Select _mappingCodes(s)).ToArray
            Return New String(Chars)
        End Function

        ''' <summary>
        ''' ½âÂëÒ»¸öÊÂÎñ
        ''' </summary>
        ''' <param name="Transaction"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function DecodesTransaction(Transaction As String) As String()
            Dim LQuery = (From ch As Char In Transaction Select _codesMappings(ch)).ToArray
            Return LQuery
        End Function
    End Class

    ''' <summary>
    ''' ½«ÊÂ¼þ½øÐÐ±àÂëÎªµ¥¸ö×Ö·û
    ''' </summary>
    ''' <remarks></remarks>
    Public Class EncodingServices

        Dim _originals As String()

        Public ReadOnly Property CodeMappings As ReadOnlyDictionary(Of Char, KeyValuePair(Of String, Integer))

        ''' <summary>
        ''' ±àÂëÔ­Àí£¬Õâ¸öº¯ÊýÊÇÎª¶àÖµ½øÐÐ±àÂëµÄ£¬¼´<paramref name="items"></paramref>Ö®ÖÐµÄÃ¿Ò»¸öÔªËØÎªÈÎÒâÊµÊý½øÐÐÕ¹¿ª£¬È»ºó¶ÔÕ¹¿ªµÄÊý¾Ý½øÐÐ±àÂë´¦Àí
        ''' </summary>
        ''' <param name="Items">Value²¿·ÖÎªËùÓÐ¿ÉÄÜµÄÈ¡Öµ£¬Çë×¢Òâ£¬ValueÖ®ÖÐ²»ÄÜ¹»ÓÐÖØ¸´Öµ</param>
        ''' <remarks></remarks>
        Sub New(Items As String(), Levels As Integer())
            _originals = Items.ToArray
            Dim ItemLevels = (From itemName As String In _originals
                              Select (From n As Integer
                                      In Levels.Distinct
                                      Select New KeyValuePair(Of String, Integer)(itemName, n)).ToArray).MatrixToVector
            Dim Codes = GenerateCodes(ItemLevels.Length)
            _CodeMappings = New ReadOnlyDictionary(Of Char, KeyValuePair(Of String, Integer))(
                (From i As Integer
                 In ItemLevels.Sequence
                 Select ch = Codes(i),
                     dat = ItemLevels(i)).ToDictionary(Function(obj) obj.ch,
                                                       Function(obj) obj.dat))
        End Sub

        Public Shared Function GenerateCodes(Length As Integer) As Char()
            Dim ChunkBuffer = (From i As Integer In (Length + 200).Sequence.AsParallel Select ChrW(15000 + i) Distinct).ToArray
            Return ChunkBuffer
        End Function

        ''' <summary>
        ''' ÔÚ½øÐÐ¹ØÁª·ÖÎöÍê±ÏÖ®ºó£¬ÔÙÓ³Éä»ØÈ¥
        ''' </summary>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function MapRecovered(item As String) As KeyValuePair(Of String, Integer)()
            Dim LQuery = (From ch As Char In item Select _CodeMappings(ch)).ToArray
            Return LQuery
        End Function

        ''' <summary>
        ''' 
        ''' </summary>
        ''' <param name="data">Õâ¸öµÄË³ÐòÓëÊýÄ¿±ØÐëÒªÓë<see cref="_CodeMappings"></see>»òÕß<see cref="_originals"></see>ÏàÒ»ÖÂ</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function TransactionEncoding(data As Generic.IEnumerable(Of Transaction)) As String()
            Dim LQuery = (From item In data.AsParallel Select __transactionEncoding(item)).ToArray
            Return LQuery
        End Function

        Private Function __transactionEncoding(data As Transaction) As String
            Dim Encodes = (From idx As Integer In data.Values.Sequence
                           Let Token As Integer = data.Values(idx)
                           Let ID As String = Me._originals(idx)
                           Select (From item In Me._CodeMappings
                                   Where __equals(ID, Token, item.Value)
                                   Select item.Key).FirstOrDefault).ToArray
            Return New String(value:=Encodes)
        End Function

        Private Function __equals(ak As String, av As Integer, b As KeyValuePair(Of String, Integer)) As Boolean
            Return av = b.Value AndAlso String.Equals(ak, b.Key)
        End Function
    End Class

    Public Structure Transaction

        Public Property TransactionName As String
        ''' <summary>
        ''' Õâ¸öµÄË³ÐòÓëÊýÄ¿±ØÐëÒªÓë<see cref="EncodingServices.CodeMappings"></see>»òÕß<see cref="EncodingServices._originals"></see>ÏàÒ»ÖÂ
        ''' </summary>
        ''' <value></value>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Property Values As Integer()

        Sub New(ID As String, LevelValues As Integer())
            TransactionName = ID
            Values = LevelValues
        End Sub
    End Structure
End Namespace
