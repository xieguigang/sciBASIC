### Clustering SIMD + Parallel benchmark

| 用例 | SIMD+Parallel (ms) | 纯标量串行 (ms) | 加速比 | 分配 (MB) | 标量分配 (MB) |
| --- | ---: | ---: | ---: | ---: | ---: |
| distance-squared x500000 | 104.5 | 93.1 | 0.89x | 0.00 | 0.00 |
| distance-squared(d=1024) x20000 | 45.8 | 37.3 | 0.81x | 0.00 | 0.00 |
| square-distance(alloc path) x500000 | 106.4 | 90.4 | 0.85x | 0.00 | 0.00 |
| topk(select) n=200000 k=10 | 1.7 | 1.9 | 1.07x | 0.00 | 0.00 |
| topk(linq orderby) n=200000 k=10 | 2.3 | 2.3 | 1.00x | 3.05 | 3.05 |
| kmeans 10000x100 k=10 | 277.2 | 445.5 | 1.61x | 8.34 | 8.34 |
| fuzzy-cmeans 5000x50 c=8 | 565.1 | 505.4 | 0.89x | 26.52 | 26.52 |
| dbscan 5000x20 eps=3 minPts=5 | 306.3 | 282.2 | 0.92x | 100.33 | 100.66 |
| hdbscan 2000x10 minPts=5 (no cache) | 134.6 | 219.2 | 1.63x | 5.23 | 5.22 |
| pairwise-distance n=2000 (upper triangle) | 23.1 | 87.9 | 3.81x | 15.26 | 15.25 |

