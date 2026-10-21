Namespace Hamming

    ''' <summary>
    ''' Hamming 聚类命名空间根：以「位级不一致」作为相似性判据的二值 / 定类数据分组技术。
    ''' 
    ''' 该命名空间提供两层能力：
    ''' 
    ''' 1. **度量层**：<see cref="HammingDistance"/> 与 <see cref="BitPattern"/>。
    '''    前者是纯函数的 Hamming 距离实现（支持二进制位串、类别行、加权距离），
    '''    后者把等长位串打包成机器字，使距离计算可以走「异或 + popcount」的硬件级快速路径；
    ''' 2. **算法层**：
    '''    - <see cref="KmodesEngine"/>（路线 A）：无监督 k-modes 聚类，
    '''      用 Hamming 距离替代欧氏距离、用逐位众数替代均值作为质心；
    '''    - <see cref="HammingClusteringEngine"/>（路线 B）：Muselli–Liberati 的监督式
    '''      规则提取，输出仅由 AND / OR / NOT 构成的可解释布尔判别网络。
    ''' 
    ''' 统一编码（<see cref="HammingEncoder"/>）把位串、字符/字符串类别行与数值矩阵
    ''' 归一到同一套类别码表示，因此两条算法路线共享同一份距离实现。
    ''' 二维表入口见 <see cref="HammingTableExtensions"/>。
    ''' 
    ''' **适用边界**：Hamming 距离要求等长串或同维度数据，连续变量需要先离散化；
    ''' 当数据极度稀疏（绝大多数位为 0）时「双零匹配」会主导距离，
    ''' 此时应改用 Jaccard 或余弦相似度；若各位的重要性不等，
    ''' 应使用 <see cref="HammingDistance.Weighted(String, String, Double())"/> 一类加权距离。
    ''' </summary>
    Module NamespaceDoc
    End Module
End Namespace
