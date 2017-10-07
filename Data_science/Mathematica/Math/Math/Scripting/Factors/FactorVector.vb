Imports Microsoft.VisualBasic.Math.SyntaxAPI.Vectors
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Scripting

    ''' <summary>
    ''' 提供和R之中的向量类似的行为：可以用两种方式来访问向量之中的成员，名字或者向量数组的下表
    ''' </summary>
    Public Class FactorVector : Inherits GenericVector(Of Object)

        Friend index As Dictionary(Of String, Integer)

        Default Public Overloads Property Item(name$) As Object
            Get
                If Not index.ContainsKey(name) Then
                    Return Nothing
                Else
                    Return buffer(index(name))
                End If
            End Get
            Set(value As Object)
                If Not index.ContainsKey(name) Then
                    Call buffer.Add(value)
                    Call index.Add(name, buffer.Length - 1)
                Else
                    buffer(index(name)) = value
                End If
            End Set
        End Property

        Public Overrides Function ToString() As String
            Return index.Keys.ToArray.GetJson
        End Function
    End Class
End Namespace