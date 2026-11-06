Option Strict On
Option Explicit On
Option Infer On

Namespace Thrift

    ''' <summary>
    ''' Thrift compact protocol 的数据类型标记。
    ''' <para>
    ''' 注意 <c>True</c> / <c>False</c> 只出现在<b>字段头</b>当中，用于编码布尔字段的值；
    ''' 在集合（list/map）中布尔值单独占用一个字节，字节值为 1（True）或者 2（False）。
    ''' </para>
    ''' </summary>
    Public Enum ThriftType As Byte

        [Stop] = 0
        [True] = 1
        [False] = 2
        [Byte] = 3
        I16 = 4
        I32 = 5
        I64 = 6
        [Double] = 7
        Binary = 8
        [List] = 9
        [Set] = 10
        [Map] = 11
        [Struct] = 12

    End Enum

End Namespace
