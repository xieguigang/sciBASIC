#Region "Microsoft.VisualBasic::c2762ad3badc4447e689aa531abc5415, Microsoft.VisualBasic.Core\ApplicationServices\VBDev\XmlDoc\Extensions\VBSpecific.vb"

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

    '     Module VBSpecific
    ' 
    '         Function: IsMyResource
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Runtime.CompilerServices

Namespace ApplicationServices.Development.XmlDoc.Serialization

    Public Module VBSpecific

        <MethodImpl(MethodImplOptions.AggressiveInlining)>
        <Extension>
        Public Function IsMyResource([declare] As String) As Boolean
            Return InStr([declare], "My.Resources", CompareMethod.Binary) > 0
        End Function
    End Module
End Namespace
