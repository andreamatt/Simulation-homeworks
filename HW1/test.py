import pandas
import numpy as np
from matplotlib.pyplot import hist, show, scatter
from numpy import mean, std, var, percentile, min, max, median
from math import sqrt, pow

data = 'HW1\\data'
data_ex1 = f'{data}\\data_ex2.csv'


def CoV(values):
    # Coeff of variation (not defined on heavy tailed sets, because variance=>inf)
    return std(values)/mean(values)


def MAD(values):
    # mean absolute deviation, always defined
    n = len(values)
    m = mean(values)
    return sum([abs(x-m) for x in values])/n


def lorenz_gap(values):
    # always defined, the best
    return MAD(values)/(2*mean(values))


def JFI(values):
    # Jain Fairness Index (same problems as CoV?)
    return 1/(1+pow(CoV(values), 2))

def Gini(values):
    # useless
    return 0

def lorenz_curve(values):
    values = sorted(values)
    curve = []
    line = []
    tot = sum(values)
    for i in range(len(values)):
        line.append(i/len(values))
        curve.append(sum(values[:i])/tot)

    scatter(line, curve)




ex1 = pandas.read_csv(data_ex1, header=None)
values = ex1.to_numpy()[:, 0]

print("mean", mean(values))
print("std", std(values))
print("var (aka std^2)", var(values))
print("CoV", CoV(values))
print("MAD", MAD(values))
print("Lorenz gap", lorenz_gap(values))
print("JFI", JFI(values))

# lorenz_curve(values)


values = sorted(values)[:10]
#cum = np.cumsum(values)
Xq = np.arange(start=0, stop=1, step=1/len(values))
Xq2 = np.arange(start=0, stop=1, step=1/100)
scatter(Xq, values)
scatter(Xq, [np.quantile(values, q) for q in Xq])

samples_number = len(values)
# hist(values, bins=20)
show()
