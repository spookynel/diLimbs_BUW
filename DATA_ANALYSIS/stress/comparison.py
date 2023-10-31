import os
import pandas as pd

# List of columns to exclude from comparison
exclude_columns = ['LegLength', 'StepLength', 'StrideWidth', 'StrideLengthRight', 'StrideLengthLeft', 'Velocity', 'Acceleration', 'RightFootGround', 'LeftFootGround']

# Path to the directory containing participant folders (P01, P02, ..., P09)
base_directory = "./"  # Change this to the path to your directory

# Iterate through each participant folder
for participant_folder in os.listdir(base_directory):
    if not participant_folder.startswith("P0"):
        continue

    # Iterate through each condition folder (Idle, Easy, Medium, Hard)
    for condition_name in ['Idle', 'Easy', 'Medium', 'Hard']:
        if condition_name == "Idle":
            continue  # Skip the Idle condition

        # Read the baseline (Idle) and current condition CSV files
        baseline_csv = os.path.join(base_directory, participant_folder, f"{participant_folder}-Idle.csv")
        condition_csv = os.path.join(base_directory, participant_folder, f"{participant_folder}-{condition_name}.csv")

        if not (os.path.isfile(baseline_csv) and os.path.isfile(condition_csv)):
            print(f"Missing files for {participant_folder} - {condition_name}. Skipping.")
            continue

        baseline_df = pd.read_csv(baseline_csv)
        condition_df = pd.read_csv(condition_csv)

        # Calculate the difference for selected columns and exclude specified columns
        comparison_df = condition_df.copy()
        for column in condition_df.columns:
            if column not in exclude_columns and column != "Timestamp":
                comparison_df[column] = condition_df[column] - baseline_df[column]

        # Create a new filename for the comparison CSV
        comparison_filename = os.path.join(base_directory, participant_folder, f"{participant_folder}-{condition_name}Comparison.csv")

        # Save the comparison data to the new CSV file, excluding specified columns
        comparison_df.drop(exclude_columns, axis=1, inplace=True)
        comparison_df.to_csv(comparison_filename, index=False)

        print(f"Saved {comparison_filename}")

print("All comparisons are saved.")
