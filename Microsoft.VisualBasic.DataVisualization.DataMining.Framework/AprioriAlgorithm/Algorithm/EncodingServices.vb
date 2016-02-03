Imports System.Collections.ObjectModel
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace AprioriAlgorithm

    Public Class BinaryEncodingServices

        Dim _InternalCodesMappings As Dictionary(Of Char, String)
        Dim _InternalMappingCodes As Dictionary(Of String, Char)

        Sub New(Codes As Generic.IEnumerable(Of String))
            Dim CodesChr = EncodingServices.InternalGenerateCodes(Codes.Count)
            _InternalCodesMappings = (From Idx As Integer In Codes.Sequence Select ch = CodesChr(Idx), Token_ID = Codes(Idx)).ToArray.ToDictionary(Function(obj) obj.ch, Function(obj) obj.Token_ID)
            _InternalMappingCodes = _InternalCodesMappings.ToDictionary(Function(obj) obj.Value, Function(obj) obj.Key)
        End Sub

        ''' <summary>
        ''' ±àÂëÒ»¸öÊÂÎñ
        ''' </summary>
        ''' <param name="TransactionTokens"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function EncodingTransaction(TransactionTokens As String()) As String
            Dim Chars = (From s As String In TransactionTokens Select _InternalMappingCodes(s)).ToArray
            Return New String(Chars)
        End Function

        ''' <summary>
        ''' ½âÂëÒ»¸öÊÂÎñ
        ''' </summary>
        ''' <param name="Transaction"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function DecodesTransaction(Transaction As String) As String()
            Dim LQuery = (From ch As Char In Transaction Select _InternalCodesMappings(ch)).ToArray
            Return LQuery
        End Function
    End Class

    ''' <summary>
    ''' ½«ÊÂ¼þ½øÐÐ±àÂëÎªµ¥¸ö×Ö·û
    ''' </summary>
    ''' <remarks></remarks>
    Public Class EncodingServices

        Dim _InternalOriginalItems As String()

        Public ReadOnly Property CodeMappings As ReadOnlyDictionary(Of Char, KeyValuePair(Of String, Integer))

        ''' <summary>
        ''' ±àÂëÔ­Àí£¬Õâ¸öº¯ÊýÊÇÎª¶àÖµ½øÐÐ±àÂëµÄ£¬¼´<paramref name="items"></paramref>Ö®ÖÐµÄÃ¿Ò»¸öÔªËØÎªÈÎÒâÊµÊý½øÐÐÕ¹¿ª£¬È»ºó¶ÔÕ¹¿ªµÄÊý¾Ý½øÐÐ±àÂë´¦Àí
        ''' </summary>
        ''' <param name="Items">Value²¿·ÖÎªËùÓÐ¿ÉÄÜµÄÈ¡Öµ£¬Çë×¢Òâ£¬ValueÖ®ÖÐ²»ÄÜ¹»ÓÐÖØ¸´Öµ</param>
        ''' <remarks></remarks>
        Sub New(Items As String(), Levels As Integer())
            _InternalOriginalItems = Items.ToArray
            Dim ItemLevels = (From itemName As String In _InternalOriginalItems Select (From n In Levels.Distinct Select New KeyValuePair(Of String, Integer)(itemName, n)).ToArray).ToArray.MatrixToVector 'Õ¹¿ª´¦Àí
            Dim Codes = InternalGenerateCodes(ItemLevels.Count) '±àÂë
            _CodeMappings = New ReadOnlyDictionary(Of Char, KeyValuePair(Of String, Integer))((From i As Integer In ItemLevels.Sequence Select ch = Codes(i), dat = ItemLevels(i)).ToArray.ToDictionary(Function(obj) obj.ch, Function(obj) obj.dat))
        End Sub

        Public Shared Function InternalGenerateCodes(Length As Integer) As Char()
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
        ''' <param name="data">Õâ¸öµÄË³ÐòÓëÊýÄ¿±ØÐëÒªÓë<see cref="_CodeMappings"></see>»òÕß<see cref="_InternalOriginalItems"></see>ÏàÒ»ÖÂ</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function TransactionEncoding(data As Generic.IEnumerable(Of Transaction)) As String()
            Dim LQuery = (From item In data.AsParallel Select __TransactionEncoding(item)).ToArray
            Return LQuery
        End Function

        Private Function __TransactionEncoding(data As Transaction) As String
            Dim Encodes = (From idx As Integer In data.Values.Sequence
                           Let Token As Integer = data.Values(idx)
                           Let ID As String = Me._InternalOriginalItems(idx)
                           Select (From item In Me._CodeMappings
                                   Where InternalEquals(ID, Token, item.Value)
                                   Select item.Key).ToArray.First).ToArray
            Return New String(value:=Encodes)
        End Function

        Private Function InternalEquals(ak As String, av As Integer, b As KeyValuePair(Of String, Integer)) As Boolean
            Return av = b.Value AndAlso String.Equals(ak, b.Key)
        End Function

        Public Structure Transaction
            Public Property TransactionName As String
            ''' <summary>
            ''' Õâ¸öµÄË³ÐòÓëÊýÄ¿±ØÐëÒªÓë<see cref="_CodeMappings"></see>»òÕß<see cref="_InternalOriginalItems"></see>ÏàÒ»ÖÂ
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
    End Class
End Namespace