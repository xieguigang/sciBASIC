Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.DataFrame

Public Class Encoder

    ReadOnly encodings As New Dictionary(Of String, FeatureEncoder)

    <MethodImpl(MethodImplOptions.AggressiveInlining)>
    Public Sub AddEncodingRule(field As String, encoder As FeatureEncoder)
        encodings(field) = encoder
    End Sub

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="data">
    ''' 不是所有的字段都需要进行编码操作的
    ''' </param>
    ''' <returns></returns>
    Public Function Encoding(data As DataFrame) As DataFrame
        For Each name As String In data.features.Keys.ToArray
            If Not encodings.ContainsKey(name) Then
                ' no needs for run data encoding
                Continue For
            End If

            Dim v As FeatureVector = data(name)
            Dim extends As DataFrame = encodings(name).Encode(v)

            data.delete(featureName:=name)
            data = data.Union(extends)
        Next

        Return data
    End Function

    ''' <summary>
    ''' encoding a feature dataframe to a new normalized
    ''' dataframe automatically.
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    Public Shared Function AutoEncoding(data As DataFrame) As DataFrame
        For Each name As String In data.features.Keys.ToArray
            Dim v As FeatureVector = data(name)
            Dim extends As DataFrame = Encode(v)

            data.delete(featureName:=name)
            data = data.Union(extends)
        Next

        Return data
    End Function

    ''' <summary>
    ''' auto encoder
    ''' </summary>
    ''' <param name="feature"></param>
    ''' <returns>
    ''' this will make all feature data type as the numeric value
    ''' </returns>
    Public Shared Function Encode(feature As FeatureVector) As DataFrame
        Select Case feature.type
            Case GetType(String) : Return New EnumEncoder().Encode(feature)
            Case GetType(Boolean) : Return New FlagEncoder().Encode(feature)
            Case GetType(Single),
                 GetType(Double),
                 GetType(Short),
                 GetType(Integer),
                 GetType(Long)

                Return New NumericEncoder().Encode(feature)
            Case Else
                Throw New NotImplementedException(feature.type.Name)
        End Select
    End Function
End Class
