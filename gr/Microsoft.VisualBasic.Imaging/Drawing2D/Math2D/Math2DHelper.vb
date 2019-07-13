#Region "Microsoft.VisualBasic::b20d146fd0c8ab9b886d43673607cf34, gr\Microsoft.VisualBasic.Imaging\Drawing2D\Math2D\Math2DHelper.vb"

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

    '     Module Math2DHelper
    ' 
    '         Function: Rotate
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports System.Runtime.CompilerServices

Namespace Drawing2D.Math2D

    Public Module Math2DHelper

        ''' <summary>
        ''' 将目标多边型旋转指定的角度
        ''' </summary>
        ''' <param name="polygon"></param>
        ''' <param name="angle#">角度的单位在这里单位为度，不是弧度单位</param>
        ''' <returns></returns>
        <Extension>
        Public Function Rotate(polygon As IEnumerable(Of PointF), angle#) As PointF()
            Throw New NotImplementedException
        End Function
    End Module
End Namespace
