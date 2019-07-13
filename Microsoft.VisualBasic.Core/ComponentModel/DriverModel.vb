#Region "Microsoft.VisualBasic::adcad34ffc4db772e17534e416b8591c, Microsoft.VisualBasic.Core\ComponentModel\DriverModel.vb"

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

    '     Interface ITaskDriver
    ' 
    '         Function: Run
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel

    ''' <summary>
    ''' Driver abstract model
    ''' </summary>
    Public Interface ITaskDriver

        ''' <summary>
        ''' Start run this driver object.
        ''' </summary>
        ''' <returns></returns>
        Function Run() As Integer
    End Interface
End Namespace
