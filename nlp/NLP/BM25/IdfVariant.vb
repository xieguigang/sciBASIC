Namespace BM25

    ''' <summary>
    ''' IDF 计算方式。
    ''' </summary>
    Public Enum IdfVariant
        ''' <summary>Lucene/Elasticsearch 变体: log(1 + (N-n+0.5)/(n+0.5))，避免负值。</summary>
        Lucene
        ''' <summary>原始 Okapi 变体: log((N-n+0.5)/(n+0.5))，可能产生负值。</summary>
        Okapi
    End Enum
End Namespace