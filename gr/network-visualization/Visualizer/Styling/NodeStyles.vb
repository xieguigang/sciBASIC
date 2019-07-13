#Region "Microsoft.VisualBasic::62c872f71e92e09406128ffcce24fe8d, gr\network-visualization\Visualizer\Styling\NodeStyles.vb"

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

    '     Module NodeStyles
    ' 
    '         Function: (+2 Overloads) DegreeAsSize
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices
Imports Microsoft.VisualBasic.ComponentModel
Imports Microsoft.VisualBasic.ComponentModel.Ranges
Imports Microsoft.VisualBasic.ComponentModel.Ranges.Model
Imports Microsoft.VisualBasic.Data.visualize.Network.FileStream.Generic
Imports Microsoft.VisualBasic.Data.visualize.Network.Graph

Namespace Styling

    Public Module NodeStyles

        <Extension> Public Function DegreeAsSize(nodes As IEnumerable(Of Node),
                                                 getDegree As Func(Of Node, Double),
                                                 sizeRange As DoubleRange) As Map(Of Node, Double)()
            Return nodes.RangeTransform(getDegree, sizeRange)
        End Function

        <Extension>
        Public Function DegreeAsSize(nodes As IEnumerable(Of Node), sizeRange As DoubleRange, Optional degree$ = NamesOf.REFLECTION_ID_MAPPING_DEGREE) As Map(Of Node, Double)()
            Dim valDegree = Function(node As Node)
                                Return node.data(degree).ParseDouble
                            End Function
            Return nodes.DegreeAsSize(
                getDegree:=valDegree,
                sizeRange:=sizeRange
            )
        End Function
    End Module
End Namespace
