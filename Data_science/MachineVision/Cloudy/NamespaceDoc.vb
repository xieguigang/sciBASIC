''' <summary>
''' 3D point cloud processing library
'''
''' 工业三维点云处理流水线：
'''
'''  1. <c>Preprocessing</c>     数据采集与预处理——去噪、降采样、运动畸变补偿、坐标统一
'''  2. <c>Features</c>          特征提取与描述——法向/曲率、关键点（ISS/Harris-3D/SIFT-3D）、
'''                              描述子（FPFH/SHOT/ESF/VFH）
'''  3. <c>Registration</c>      配准与拼接——RANSAC+FPFH / 4PCS 粗配准，ICP/点到面/GICP/NDT 精配准，
'''                              位姿图优化与多站拼接
'''  4. <c>Segmentation</c>      分割与分类——RANSAC 基元（平面/球/圆柱/圆锥）、欧氏/FEC/DBSCAN 聚类、
'''                              区域生长、四级级联分割、语义分类
'''  5. <c>Reconstruction</c>    三维重建与建模——泊松重建、TSDF 融合、BPA、贪心三角化、Delaunay、
'''                              网格后处理（平滑/QEM/补洞）、B 样条曲面拟合、基元参数化拟合
'''  6. <c>Applications</c>      下游工业任务——CAD 偏差分析（色差图/统计/公差判定）、形位公差、
'''                              6-DoF 位姿估计与跟踪、抓取规划、手眼标定
'''
''' 支撑层 <c>Core</c> 提供 SoA 点云容器（<see cref="Core.PointCloudBuffer"/>）、包围盒、
''' 刚体变换与四元数、小规模线代（<see cref="Core.Linalg"/>）、三维 KD 树、稀疏体素哈希与
''' 邻域查询；所有批量算术都走 <c>Microsoft.VisualBasic.Math.SIMD</c> 的 SIMD + 并行内核。
'''
''' 选型建议：
'''  <list type="bullet">
'''  <item><description>计量级检测（航空航天/精密模具）：RANSAC + 点到面 ICP →
'''  泊松或 BPA 重建 → CAD 偏差分析 + 形位公差。</description></item>
'''  <item><description>机器人引导/无序抓取：ISS + FPFH → RANSAC 粗配准 → 局部 ICP 跟踪 →
'''  抓取规划（形态学网格搜索 / 对跖点采样）。</description></item>
'''  <item><description>大场景（车间/管道）：体素降采样 + NDT + 位姿图全局优化；
'''  需要流式处理时用 <see cref="Core.CloudyOptions"/> 控制分块与内存预算。</description></item>
'''  </list>
'''
''' 本库只做数学算法，不承担文件读写与界面渲染：点云可以直接来自外部三维文件读取模块
''' （例如 Landscape 的 PLY/PCD 读取器，其 <c>Ply.PointCloud</c> 实现
''' <c>Microsoft.VisualBasic.Imaging.Drawing3D.PointF3D</c>，可通过
''' <see cref="Core.PointCloudBuffer.From(IEnumerable(Of Microsoft.VisualBasic.Imaging.PointF3D))"/> 转入）。
''' </summary>
Friend Class NamespaceDoc
End Class
