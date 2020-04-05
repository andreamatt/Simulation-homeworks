# import and solve data
from pandas import read_csv
import numpy as np
from matplotlib import pyplot as plt
from numpy import mean, min, max, median, quantile
from scipy.stats import binom, norm, t as student, expon
from math import sqrt, pow, floor, ceil, exp
from statsmodels.api import qqplot

# define corrected functions for std and variance
def variance(values):
	return np.var(values, ddof=1)

def std(values):
	return np.std(values, ddof=1)

data = 'HW2/data'
data_ex1 = read_csv(f'{data}\\data_ex1.csv', header=None)

# EXERCISE 1
# Load the data from the CSV file data_ex1.csv. This data represents measurements of some quantity over a few days.
# In each line: the 1st value refers to the time of the measurement; the 2nd value is the measurement output.
# If you draw a scatter plot, you should see a clear trend in the data.
# 1. Use least squares to remove the trend. You only need polynomial functions for this.
# 2. After having verified that a good value for the maximum degree of the the polynomial is 5, remove the
# trend from the data and fit a Gaussian distribution to the resulting dataset.
# 3. Give the mean and variance of the distribution, and draw a QQ-plot to determine if the Gaussian approximation holds. Give a prediction interval for future samples from this Gaussian distribution.
# 4. Discuss what would happen if you fit a polynomial of degree different than 5 to the data.
values = data_ex1.to_numpy()
# print(values)
X = values[:,0]
Y = values[:,1]

def polyfit(X, Y, degree):
	# return coefficients from high degree to low
	A = []
	for power in range(degree+1):
		A.append(np.power(X,power))
	A = np.array(A)
	A = np.transpose(A)
	A_t = np.transpose(A)
	b = np.linalg.inv(A_t @ A) @ A_t @ Y
	return np.flip(b)

# plt.scatter(X, Y, s=0.2)
for i in range(1,10):
	coeff = polyfit(X,Y,i)
	Y_fit = np.poly1d(coeff)(X)
	mean_square_error = sum(np.power(Y-Y_fit,2))/len(X)
	# print(i, mean_square_error)
	#plt.scatter(X, Y_fit, s=1)
	# plt.scatter(X, Y-np.poly1d(coeff)(X), s=1)
	# plt.show()
# plt.show()

coeff = polyfit(X,Y,5)
Y_detr = Y-np.poly1d(coeff)(X)
# plt.scatter(X, Y_detr, s=1)
# plt.show()


print(f'Std: {std(Y_detr)}, mean: {mean(Y_detr)}')
# plt.hist(Y_detr, bins=50, density=True, color='y', linewidth=0.1, edgecolor='b')
X_prob = np.arange(min(Y_detr), max(Y_detr), 1/100)
# plt.scatter(X_prob, norm.pdf(X_prob, mean(Y_detr), std(Y_detr) ), s=1, zorder=2)
# plt.show()


values = Y_detr
qqplot(values, dist=norm, loc=mean(values), scale=std(values), line='45')
plt.show()

def prediction_interval_bootstrap(values, gamma):
	values = sorted(values)
	n = len(values)
	alpha = 1-gamma
	j = floor(n*alpha/2)
	k = ceil(n*(1-alpha/2))
	return (values[j], values[k])

def prediction_interval_normal(values, gamma):
	values = sorted(values)
	n = len(values)
	alpha = 1-gamma
	m = mean(values)
	s = std(values)
	if n > 100:
		eta = norm.ppf(1-alpha/2)
		return (m-eta*s, m+eta*s)
	else:
		eta = student.ppf(1-alpha/2, n-1)
		rt = sqrt(1 + 1/n)
		return (m-eta*rt*s, m+eta*rt*s)


interval = prediction_interval_bootstrap(values, 0.95)
print(f"Bootstrap prediction interval at 0.95: [{interval[0]} , {interval[1]}]")
interval = prediction_interval_normal(values, 0.95)
print(f"Normal prediction interval at 0.95: [{interval[0]} , {interval[1]}]")