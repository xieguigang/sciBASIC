#Region "Microsoft.VisualBasic::c886113e741a26d955e8d7562fed54bc, Data_science\DataMining\DataMining\Clustering\FuzzyCMeans\Entity.vb"

    ' Author:
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 
    ' 



    ' /********************************************************************************/

    ' Summaries:

    '     Class Entity
    ' 
    '         Properties: MarkClusterCenter, Memberships, ProbablyMembership
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.DataMining.KMeans
Imports Microsoft.VisualBasic.Language
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace FuzzyCMeans

    Public Class Entity : Inherits ClusterEntity

        ''' <summary>
        ''' ``Key``键名和数组的下标一样是从0开始的
        ''' </summary>
        ''' <returns></returns>
        Public Property Memberships As Dictionary(Of Integer, Double)
        Public Property MarkClusterCenter As Color

        ''' <summary>
        ''' Max probably of <see cref="Memberships"/> its key value.
        ''' </summary>
        ''' <returns></returns>
        Public ReadOnly Property ProbablyMembership As Integer
            Get
                Return Memberships _
                    .Keys _
                    .Select(Function(i) Memberships(i)) _
                    .MaxIndex
            End Get
        End Property

        Public Overrides Function ToString() As String
            Return $"{uid} --> {Memberships.GetJson}"
        End Function
    End Class
End Namespace
