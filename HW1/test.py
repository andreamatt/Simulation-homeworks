import pandas
import numpy as np
from matplotlib.pyplot import hist, show
from numpy import mean, std, var, percentile, min, max, median
from math import sqrt, pow

data = 'HW1\\data'
data_ex1 = f'{data}\\data_ex1.csv'


def CoV(values):
    # Coeff of variation (not defined on heavy tailed sets, because variance=>inf)
    return std(values)/mean(values)


def MAD(values):
    # mean absolute deviation, always defined
    n = len(values)
    m = mean(values)
    return sum([abs(x-m) for x in values])/n

def lorenz_gap(values):
    # always defined
    return MAD(values)/(2*mean(values))

ex1 = pandas.read_csv(data_ex1, header=None)
values = ex1.to_numpy()[:, 0]

print("mean", mean(values))
print("std", std(values))
print("var (aka std^2)", var(values))
print("CoV", CoV(values))
print("MAD", MAD(values))
print("Lorenz gap", lorenz_gap(values))


samples_number = len(values)
# hist(values, bins=20)
# show()
