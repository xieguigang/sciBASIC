Imports System.Text
Imports Microsoft.VisualBasic.DataMining.Framework.ComponentModel
Imports Microsoft.VisualBasic.Extensions
Imports Microsoft.VisualBasic.Linq.Extensions

Namespace Kernel.Classifier

    ''' <summary>
    ''' 朴素贝叶斯分类器
    ''' </summary>
    ''' <remarks></remarks>
    Public Class Bayesian

        ''' <summary>
        ''' 原始的数据集合
        ''' </summary>
        ''' <remarks></remarks>
        Dim Entities As List(Of ComponentModel.Entity)
        Dim AllClass As Integer()
        Dim Width As Integer

        Public Shared Function Load(Data As Generic.IEnumerable(Of ComponentModel.Entity)) As Bayesian
            Dim Entities As List(Of ComponentModel.Entity) = Data.ToList
            Return New Bayesian With {
                .Entities = Entities,
                .AllClass = GetAllClass(Entities),
                .Width = Data.First.Length
            }
        End Function

        Public Overrides Function ToString() As String
            Return String.Format("Bayesian Classifier, {0} Entities and {1} properties.", Entities.Count, Width)
        End Function

        Public Function Classify(X As Integer()) As Integer
            Dim LQuery = From [Class] As Integer In AllClass Select P(X, Y:=[Class]) * P(Y:=[Class]) '
            Dim R As Double() = LQuery.ToArray
            Dim Index As Integer = Array.IndexOf(R, R.Max)
            Return AllClass(Index)
        End Function

        Private Function P(Y As Integer) As Double
            Dim LQuery = From Entity As Entity In Entities Where Entity.Class = Y Select 1 '
            Return LQuery.Count / Entities.Count
        End Function

        Private Shared Function GetAllClass(Entities As List(Of ComponentModel.Entity)) As Integer()
            If Entities Is Nothing OrElse Entities.Count = 0 Then Return New Integer() {}
            Dim LQuery = From Entity As Entity In Entities.AsParallel Select Entity.Class Distinct '
            Return LQuery.ToArray
        End Function

        ''' <summary>
        ''' P(X|Y=y)
        ''' </summary>
        ''' <param name="X"></param>
        ''' <param name="Y"></param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function P(X As ComponentModel.Entity, Y As Integer) As Double
            Dim LQuery = From Handle As Integer In Width.Sequence
                         Select (From Entity As Entity
                                 In Entities
                                 Where Entity(Handle) = X(Handle) AndAlso Entity.Class = Y
                                 Select 1).Count / Entities.Count '
            Return LQuery.π
        End Function

        ''' <summary>
        ''' P(X|Y=y)
        ''' </summary>
        ''' <param name="X">Subject Condition</param>
        ''' <param name="Y">Target Classify</param>
        ''' <returns></returns>
        ''' <remarks></remarks>
        Public Function P(X As Integer(), Y As Integer) As Double
            Dim LQuery = From Handle As Integer In Width.Sequence
                         Select (From Entity As Entity
                                 In Entities
                                 Where Entity(Handle) = X(Handle) AndAlso Entity.Class = Y
                                 Select 1).Count / Entities.Count '
            Return LQuery.π
        End Function
    End Class
End Namespace