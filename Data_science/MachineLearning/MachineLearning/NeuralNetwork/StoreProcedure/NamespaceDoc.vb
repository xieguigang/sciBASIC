Imports System.IO
Imports System.Text

Namespace NeuralNetwork.StoreProcedure

    ''' <summary>
    ''' 拥有两种类型的数据快照文件:
    ''' 
    ''' 1. 针对小网络模型的,所有的对象都被保存在同一个Xml/json文件之中
    ''' 2. 但是对于大型的网络而言,由于执行序列化的<see cref="StringBuilder"/>或者<see cref="MemoryStream"/>对象
    '''    具有自己的容量上限限制,所以大型网路是将部件分别保存在若干个文件之中完成的
    ''' </summary>
    Module NamespaceDoc
    End Module
End Namespace