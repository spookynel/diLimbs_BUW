import pandas as pd

# Read the CSV file into a DataFrame
df = pd.read_csv('Responses.csv')

# Calculate summary statistics for each column
summary_stats = df.describe()

# Save the summary statistics to a new CSV file
summary_stats.to_csv('ResponsesSummaryStatistics.csv')

print("Summary statistics saved to 'ResponsesSummaryStatistics.csv'")
