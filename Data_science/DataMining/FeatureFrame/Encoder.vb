Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.Math.DataFrame

Public Module Encoder

    ''' <summary>
    ''' encoding a feature dataframe to a new normalized dataframe automatically.
    ''' </summary>
    ''' <param name="data"></param>
    ''' <returns></returns>
    <Extension>
    Public Function Encoding(data As DataFrame) As DataFrame
        For Each name As String In data.features.Keys.ToArray
            Dim v As FeatureVector = data(name)
            Dim extends As DataFrame = FeatureEncoder.Encode(v, name)

            data.delete(featureName:=name)
            data = data.Union(extends)
        Next

        Return data
    End Function
End Module
