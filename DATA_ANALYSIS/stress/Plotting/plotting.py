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
    'StrideTimeRight',
    'StrideLengthLeftRatio',
    'StrideTimeLeft',
    'VelocityRatio',
    'AccelerationRatio',
    'Responses'
]

rotation_columns = [
    'RightThighRotationX', 'RightThighRotationY', 'RightThighRotationZ', 'RightThighRotationW',
    'LeftThighRotationX', 'LeftThighRotationY', 'LeftThighRotationZ', 'LeftThighRotationW',
    'RightShinRotationX', 'RightShinRotationY', 'RightShinRotationZ', 'RightShinRotationW',
    'LeftShinRotationX', 'LeftShinRotationY', 'LeftShinRotationZ', 'LeftShinRotationW',
    'RightShoulderRotationX', 'RightShoulderRotationY', 'RightShoulderRotationZ', 'RightShoulderRotationW',
    'LeftShoulderRotationX', 'LeftShoulderRotationY', 'LeftShoulderRotationZ', 'LeftShoulderRotationW',
    'HeadRotationX', 'HeadRotationY', 'HeadRotationZ', 'HeadRotationW'
]

# Define colors for the conditions
colors = ['lightgreen', 'lightblue', 'lightcoral']

# Iterate through the columns and create a separate figure for each
for column in columns_to_plot:
    fig, ax = plt.subplots(figsize=(6, 6))

    data = []  # List to store data for each condition

    # Iterate through the CSV files and extract data for the current column
    for i, csv_file in enumerate(csv_files):
        df = pd.read_csv(csv_file)
        
        if column == 'Responses':
            condition_data = df[column].iloc[1:]  # Skip data transformation for 'Responses'
        else:
            condition_data = df[column].iloc[1:].apply(
                lambda x: float(x.replace('+AC0-', '').replace(',', '')) if '+AC0-' in x else float(x)
            )  # Convert other columns to float
        
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

    # Save the figure as an image
    fig.savefig(f'{column}.png')

    # Show and save the figure (optional)
    plt.show()


    ### 3D Scatter Plot ###

    # Define colors for the conditions
colors = ['green', 'blue', 'red']
conditions = ['Easy', 'Medium', 'Hard']  # Define the conditions here

    # Initialize lists to store the mean values for each condition
mean_values = {condition: [] for condition in conditions}

# Iterate through the CSV files and extract the mean values for each joint
for condition, csv_file in zip(conditions, csv_files):
    df = pd.read_csv(csv_file)
    for column in rotation_columns:
        # Exclude the first row (labels) and calculate the mean
        mean_rotation = df[column].iloc[1:].apply(lambda x: float(x.replace('+AC0-', '').replace(',', '')) if '+AC0-' in x else float(x)).mean()
        mean_values[condition].append(mean_rotation)

# Create a 3D scatter plot
fig_3d = plt.figure(figsize=(10, 8))
ax_3d = fig_3d.add_subplot(111, projection='3d')



# Adjust the range of the 3D plot
ax_3d.set_xlim(0, 1)  # Set limits for the X-axis
ax_3d.set_ylim(0, 1)  # Set limits for the Y-axis
ax_3d.set_zlim(0, 1)  # Set limits for the Z-axis

# Scatter plot for the mean values of each joint with color mapping
for color, condition in zip(colors, conditions):
    ax_3d.scatter(
        mean_values[condition][::4],  # X values (every 4th element)
        mean_values[condition][1::4],  # Y values (every 4th element starting from index 1)
        mean_values[condition][2::4],  # Z values (every 4th element starting from index 2)
        color=color,
        label=condition
    )

ax_3d.set_xlabel('X')
ax_3d.set_ylabel('Y')
ax_3d.set_zlabel('Z')
ax_3d.set_title('Mean Rotation Values for Joints')
ax_3d.legend()

# Save the 3D scatter plot as an image
fig_3d.savefig('3D_Scatter_Plot.png')

# Show and save the 3D scatter plot (optional)
plt.show()

