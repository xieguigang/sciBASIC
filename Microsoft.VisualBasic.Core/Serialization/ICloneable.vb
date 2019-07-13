#Region "Microsoft.VisualBasic::273da05431d41f4560295a7f7996af28, Microsoft.VisualBasic.Core\Serialization\ICloneable.vb"

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

    '     Interface ICloneable
    ' 
    '         Function: Clone
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace Serialization

    Public Interface ICloneable(Of T)

        Function Clone() As T
    End Interface
End Namespace
