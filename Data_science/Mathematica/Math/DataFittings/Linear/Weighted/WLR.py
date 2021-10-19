

def regress(x, y, w, deg = 2):
    
    m = len(y) # M = Number of data points
    n = deg  # N = Number of linear terms
    NDF = m - n  # Degrees of freedom
    ycal = []
    dy = []

    # If not enough data, don't attempt regression
    if NDF < 1:
        raise Exception("not enough data!")
