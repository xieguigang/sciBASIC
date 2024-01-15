Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel.Collection
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Linq
Imports any = Microsoft.VisualBasic.Scripting

Public Module NumericDataSet

    <Extension>
    Private Function IndexGetter(v As Double()) As Func(Of Integer, Double)
        If v.IsNullOrEmpty Then
            Return Function(any) 0.0
        ElseIf v.Length = 1 Then
            Dim scalar As Double = v(0)
            Return Function(any) scalar
        Else
            Return Function(i) v(i)
        End If
    End Function

    <Extension>
    Public Function NumericGetter(v As FeatureVector) As Func(Of Integer, Double)
        Select Case v.type
            Case GetType(Double), GetType(Single), GetType(Integer),
                 GetType(Long), GetType(UInteger), GetType(ULong),
                 GetType(Short), GetType(UShort),
                 GetType(Boolean),
                 GetType(DateTime),
                 GetType(Byte), GetType(SByte)

                Return v.TryCast(Of Double)().IndexGetter
            Case GetType(String), GetType(Char)
                Dim factors As Index(Of String) = v.vector _
                    .AsObjectEnumerator _
                    .Select(Function(s) any.ToString(s)) _
                    .Indexing
                Dim vals As Double() = DirectCast(v.vector, String()) _
                    .Select(Function(f) CDbl(factors(f))) _
                    .ToArray

                Call $"cast the feature {v.name} in character type to numeric vector.".Warning

                Return vals.IndexGetter
            Case Else
                Throw New NotImplementedException($"could not cast object of type '{v.type.Name}' to numeric value!")
        End Select
    End Function

    <Extension>
    Public Iterator Function NumericMatrix(df As DataFrame) As IEnumerable(Of NamedCollection(Of Double))
        Dim colnames As String() = df.featureNames
        Dim fieldGetters As Func(Of Integer, Double)() = colnames _
            .Select(Function(s) df(s).NumericGetter) _
            .ToArray
        Dim nrow As Integer = df.nsamples
        Dim rownames As String() = df.rownames
        Dim offset As Integer
        Dim row As Double()

        For i As Integer = 0 To nrow - 1
            offset = i
            row = fieldGetters _
                .Select(Function(v) v(offset)) _
                .ToArray

            Yield New NamedCollection(Of Double)(rownames(i), row)
        Next
    End Function

End Module
