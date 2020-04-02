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

data = 'HW2\\data'
data_ex1 = read_csv(f'{data}\\data_ex1.csv', header=None)
data_ex2 = read_csv(f'{data}\\data_ex2.csv', header=None)

# TODO: EXERCISE 1
# * Load the data from the CSV file data_ex1.csv. This data represents measurements of some quantity over a few days.
# * In each line: the 1st value refers to the time of the measurement; the 2nd value is the measurement output.
# * If you draw a scatter plot, you should see a clear trend in the data.
# * 1. Use least squares to remove the trend. You only need polynomial functions for this.
# * 2. After having verified that a good value for the maximum degree of the the polynomial is 5, remove the
# * trend from the data and fit a Gaussian distribution to the resulting dataset.
# * 3. Give the mean and variance of the distribution, and draw a QQ-plot to determine if the Gaussian approximation holds. Give a prediction interval for future samples from this Gaussian distribution.
# * 4. Discuss what would happen if you fit a polynomial of degree different than 5 to the data.
values = data_ex1.to_numpy()
# print(values)
X = values[:,0]
Y = values[:,1]
a, b, c, d, e, f = np.polyfit(X, Y, 5)
plt.scatter(X, Y, s=2)
plt.scatter(X, a*np.power(X,5)+b*np.power(X,4)+c*np.power(X,3)+d*np.power(X,2)+e*X +f, s=2)
plt.show()

def polyfit(X, Y, degree):
    # return X Y of fitted line
    pass
