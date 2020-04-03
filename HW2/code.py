# import and solve data
from pandas import read_csv
import numpy as np
from matplotlib import pyplot as plt
from numpy import mean, min, max, median, quantile
from scipy.stats import binom, norm, t as student, expon
from math import sqrt, pow, floor, ceil, exp

# define corrected functions for std and variance
def variance(values):
    return np.var(values, ddof=1)

def std(values):
    return np.std(values, ddof=1)

data = 'HW2/data'
data_ex2 = read_csv(f'{data}\\data_ex2.csv', header=None)

# Exercise 2
# Load the data from the CSV file data_ex2.csv. These are samples from three different, independent Gaussian
# distributions, all mixed together.
# 1. Implement the Expectation-Maximization algorithm to fit a mixture of three Gaussian distributions to the data.
#    Try both with and without the prior update step. Discuss the results.
# 2. Give the parameters of the distributions thus found, and plot the corresponding PDFs on top of the
#    empirical PDFs of the data (e.g., the histogram).

values = data_ex2.to_numpy()[:,0]

from time import time
def expectation_maximization_normal(values, curve_n, iterations, curve_prob=None, prior=True):
    n = len(values)
    # use the same std for all of them
    deviations = [std(values)/curve_n]*curve_n
    # for 3 curves, use 1/4, 2/4, 3/4 quantiles as means
    means = [quantile(values, (i+1)/(curve_n+1)) for i in range(curve_n)]
    if curve_prob==None:
        curve_prob = [1/curve_n]*curve_n
    #print([(means[i], deviations[i], curve_prob[i]) for i in range(curve_n)])

    for iteration in range(iterations):
        denoms = np.zeros(n)
        for curve in range(curve_n):
            denoms += norm.pdf(values, means[curve], deviations[curve])*curve_prob[curve]
            
        for curve in range(curve_n):
            b = []
            m = means[curve]
            s = deviations[curve]
            b = norm.pdf(values,m,s)/denoms
            new_m = sum(b*values) / sum(b)
            new_s = sqrt( sum(b*((values-new_m)**2)) / sum(b) )
            means[curve] = new_m
            deviations[curve] = new_s
        #print([(means[i], deviations[i], curve_prob[i]) for i in range(curve_n)])

    return [(means[i], deviations[i], curve_prob[i]) for i in range(curve_n)]
    
start = time()
print("prior true")
curves = expectation_maximization_normal(values, 3, 100, prior=True)
print(curves)
plt.hist(values, bins=50, density=True, color='y', linewidth=0.1, edgecolor='b')
X_prob = np.arange(min(values), max(values), 1/100)
for c in curves:
    plt.scatter(X_prob, c[2]*norm.pdf(X_prob, c[0], c[1] ), s=1, zorder=2)


end = time()
print(end-start)
plt.show()

print("prior false")
curves = expectation_maximization_normal(values, 3, 100, prior=False)
print(curves)
plt.hist(values, bins=50, density=True, color='y', linewidth=0.1, edgecolor='b')
X_prob = np.arange(min(values), max(values), 1/100)
for c in curves:
    plt.scatter(X_prob, c[2]*norm.pdf(X_prob, c[0], c[1] ), s=1, zorder=2)
plt.show()

