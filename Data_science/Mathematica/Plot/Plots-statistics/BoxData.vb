Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Data.csv.IO
Imports Microsoft.VisualBasic.Linq
Imports Microsoft.VisualBasic.Math.LinearAlgebra
Imports Microsoft.VisualBasic.Serialization.JSON

''' <summary>
''' Group data model for box plot
''' </summary>
Public Class BoxData

    Public Property Groups As NamedValue(Of Vector)()
    Public Property SerialName As String

    Public ReadOnly Property GroupNames As String()
        Get
            Return Groups.Keys.ToArray
        End Get
    End Property

    Public Overrides Function ToString() As String
        Return $"({SerialName}) {GroupNames.GetJson}"
    End Function

    ''' <summary>
    ''' 
    ''' </summary>
    ''' <param name="path$"></param>
    ''' <param name="groupDesigner"></param>
    ''' <returns></returns>
    Public Shared Iterator Function Load(path$, groupDesigner As NamedCollection(Of String)()) As IEnumerable(Of BoxData)
        Dim datasets As Dictionary(Of DataSet) = DataSet _
            .LoadDataSet(path) _
            .ToDictionary
        Dim serials$() = datasets.PropertyNames

        For Each name As String In serials
            Dim data = groupDesigner _
                .Select(Function(x)
                            Dim values As Vector = datasets(x.Value).Vector([property]:=name)
                            Return New NamedValue(Of Vector) With {
                                .name = x.Name,
                                .Description = x.Description,
                                .Value = values
                            }
                        End Function) _
                .ToArray

            Yield New BoxData With {
                .SerialName = name,
                .Groups = data
            }
        Next
    End Function
End Class
