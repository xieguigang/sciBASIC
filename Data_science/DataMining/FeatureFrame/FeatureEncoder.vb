Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.DataFrame

''' <summary>
''' 
''' </summary>
Public Class FeatureEncoder

    ''' <summary>
    ''' auto encoder
    ''' </summary>
    ''' <param name="feature"></param>
    ''' <returns>
    ''' this will make all feature data type as the numeric value
    ''' </returns>
    Public Shared Function Encode(feature As FeatureVector, name As String) As DataFrame
        Select Case feature.type
            Case GetType(String) : Return EnumEncoder(feature, name)
            Case GetType(Boolean) : Return FlagEncoder(feature, name)
            Case GetType(Single), GetType(Double), GetType(Short), GetType(Integer), GetType(Long)
                Return New DataFrame With {
                    .features = New Dictionary(Of String, FeatureVector) From {{name, feature}},
                    .rownames = IndexNames(feature)
                }
            Case Else
                Throw New NotImplementedException(feature.type.Name)
        End Select
    End Function

    Private Shared Function IndexNames(feature As FeatureVector) As String()
        Return feature.size _
            .Sequence _
            .Select(Function(i) (i + 1).ToString) _
            .ToArray
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="feature">
    ''' the feature type should be the boolean type
    ''' </param>
    ''' <param name="name"></param>
    ''' <returns></returns>
    Public Shared Function FlagEncoder(feature As FeatureVector, name As String) As DataFrame
        Dim ints As Integer() = New Integer(feature.size - 1) {}
        Dim bools As Boolean() = feature.vector

        For i As Integer = 0 To ints.Length - 1
            ints(i) = If(bools(i), 1, 0)
        Next

        Return New DataFrame With {
            .features = New Dictionary(Of String, FeatureVector) From {{name, New FeatureVector(ints)}},
            .rownames = IndexNames(feature)
        }
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="feature">
    ''' the feature type should be the string type
    ''' </param>
    ''' <param name="name"></param>
    ''' <returns></returns>
    Public Shared Function EnumEncoder(feature As FeatureVector, name As String) As DataFrame
        Dim strs As String() = feature.TryCast(Of String)
        Dim str As String
        Dim extends As New Dictionary(Of String, Integer())
        Dim factors As String() = strs.Distinct.ToArray

        For Each key As String In factors
            Call extends.Add(key, New Integer(strs.Length - 1) {})
        Next

        For i As Integer = 0 To strs.Length - 1
            str = strs(i)
            extends(str)(i) = 1
        Next

        Return New DataFrame With {
            .features = extends _
                .ToDictionary(Function(v) $"{name}.{v.Key}",
                              Function(v)
                                  Return New FeatureVector(v.Value)
                              End Function),
            .rownames = IndexNames(feature)
        }
    End Function
End Class
