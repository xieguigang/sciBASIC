#Region "Microsoft.VisualBasic::e667132a5d68449ea2f3f020836559b5, Data_science\DataMining\DataMining\ComponentModel\Normalizer\Methods.vb"

    ' Author:
    ' 
    '       asuka (amethyst.asuka@gcmodeller.org)
    '       xie (genetics@smrucc.org)
    '       xieguigang (xie.guigang@live.com)
    ' 
    ' Copyright (c) 2018 GPL3 Licensed
    ' 
    ' 
    ' GNU GENERAL PUBLIC LICENSE (GPL3)
    ' 
    ' 
    ' This program is free software: you can redistribute it and/or modify
    ' it under the terms of the GNU General Public License as published by
    ' the Free Software Foundation, either version 3 of the License, or
    ' (at your option) any later version.
    ' 
    ' This program is distributed in the hope that it will be useful,
    ' but WITHOUT ANY WARRANTY; without even the implied warranty of
    ' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    ' GNU General Public License for more details.
    ' 
    ' You should have received a copy of the GNU General Public License
    ' along with this program. If not, see <http://www.gnu.org/licenses/>.



    ' /********************************************************************************/

    ' Summaries:


    ' Code Statistics:

    '   Total Lines: 17
    '    Code Lines: 7 (41.18%)
    ' Comment Lines: 9 (52.94%)
    '    - Xml Docs: 100.00%
    ' 
    '   Blank Lines: 1 (5.88%)
    '     File Size: 572 B


    '     Enum Methods
    ' 
    '         NormalScaler, RangeDiscretizer, RelativeScaler
    ' 
    '  
    ' 
    ' 
    ' 
    ' 
    ' /********************************************************************************/

#End Region

Namespace ComponentModel.Normalizer

    Public Enum Methods
        ''' <summary>
        ''' 归一化到[0, 1]区间内
        ''' </summary>
        NormalScaler
        ''' <summary>
        ''' 直接 x / max 进行归一化, 当出现极值的时候, 此方法无效, 根据数据分布,可能会归一化到[0, 1] 或者 [-1, 1]区间内
        ''' </summary>
        RelativeScaler
        ''' <summary>
        ''' 通过对数据进行区间离散化来完成归一化
        ''' </summary>
        RangeDiscretizer
    End Enum
End Namespace
