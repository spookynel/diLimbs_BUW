import pandas as pd
import matplotlib.pyplot as plt

# List of CSV file paths
csv_files = [
    'SummaryStatistics_Easy.csv',
    'SummaryStatistics_Medium.csv',
    'SummaryStatistics_Hard.csv'
]

# List of columns to create box plots for
columns_to_plot = [
    'StepLengthRatio',
    'StrideWidthRatio',
    'StrideLengthRightRatio',
    'StrideLengthLeftRatio',
    'VelocityRatio',
    'AccelerationRatio'
]

# Define colors for the conditions
colors = ['lightyellow', 'orange', 'lightcoral']

# Iterate through the columns and create a separate figure for each
for column in columns_to_plot:
    fig, ax = plt.subplots(figsize=(6, 6))

    data = []  # List to store data for each condition

    # Iterate through the CSV files and extract data for the current column
    for i, csv_file in enumerate(csv_files):
        df = pd.read_csv(csv_file)
        condition_data = df[column].iloc[1:].apply(lambda x: float(x.replace('+AC0-', '').replace(',', '')) if '+AC0-' in x else float(x))  # Exclude the first row (labels) and convert to float
        data.append(condition_data)

    # Create a box plot for the current column with custom box colors
    bplot = ax.boxplot(data, patch_artist=True, labels=['Easy', 'Medium', 'Hard'])

    # Set custom colors for the boxes
    for patch, color in zip(bplot['boxes'], colors):
        patch.set_facecolor(color)

    # Set the median line color to black
    for median in bplot['medians']:
        median.set_color('black')

    ax.set_title(column)
    ax.set_ylabel("Value")

    # Add gray horizontal lines with reduced opacity (alpha) and a straight linestyle
    ax.yaxis.grid(True, color='gray', linestyle='-', alpha=0.3)

    # Show and save the figure (optional)
    plt.show()

# If you want to save each figure, you can use plt.savefig("figure_name.png") within the loop.
