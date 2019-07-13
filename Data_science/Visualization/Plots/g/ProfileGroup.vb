#Region "Microsoft.VisualBasic::1f8ea136d1d0594faf98b3f75ae16ff3, Data_science\Visualization\Plots\g\ProfileGroup.vb"

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

    '     Class ProfileGroup
    ' 
    '         Properties: Serials
    ' 
    '         Function: ToString
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Drawing
Imports Microsoft.VisualBasic.ComponentModel.DataSourceModel
Imports Microsoft.VisualBasic.Serialization.JSON

Namespace Graphic

    ''' <summary>
    ''' The plot data group
    ''' </summary>
    Public MustInherit Class ProfileGroup

        ''' <summary>
        ''' The color profile of the plot elements
        ''' </summary>
        ''' <returns></returns>
        Public Overridable Property Serials As NamedValue(Of Color)()

        Public Overrides Function ToString() As String
            Return MyClass.GetJson
        End Function
    End Class
End Namespace
