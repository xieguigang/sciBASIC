#Region "Microsoft.VisualBasic::4e460ae58064f52b4031d38acbaac7c1, Microsoft.VisualBasic.Core\ComponentModel\Settings\DataModels\ModelBase.vb"

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

    '     Interface IProfile
    ' 
    '         Function: Save
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Imports System.Text

Namespace ComponentModel.Settings

    ''' <summary>
    ''' 具备有保存数据功能的可配置数据文件的基本定义
    ''' </summary>
    Public Interface IProfile : Inherits IFileReference

        Function Save(Optional FilePath As String = "", Optional Encoding As Encoding = Nothing) As Boolean

    End Interface
End Namespace
