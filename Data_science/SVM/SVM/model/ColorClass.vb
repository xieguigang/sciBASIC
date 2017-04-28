Namespace Model

    ''' <summary>
    ''' http://blog.csdn.net/v_july_v/article/details/7624837
    ''' 
    ''' ###### 线性分类
    ''' 
    ''' 现在有一个二维平面，平面上有两种不同的数据，分别用圈和叉表示。由于这些数据是线性可分的，
    ''' 所以可以用一条直线将这两类数据分开，这条直线就相当于一个超平面，超平面一边的数据点所
    ''' 对应的y全是-1 ，另一边所对应的y全是1
    ''' </summary>
    Public Enum ColorClass As Integer
        RED = 1
        BLUE = -1
    End Enum
End Namespace