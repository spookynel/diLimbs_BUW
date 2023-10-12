import pandas as pd
import matplotlib.pyplot as plt
from matplotlib.backends.backend_pdf import PdfPages
from datetime import datetime

# Read the CSV file into a DataFrame
df = pd.read_csv('D:\\Usuarios\\kerke\\Escritorio\\DiLimbs\\Pooja.csv', delimiter=';')

def plot_sensor_data(sensor_name, pdf):
    # Filter the DataFrame for the given sensor name
    sensor_data = df[df['SensorName'] == sensor_name]

    # Check if data is available for the sensor
    if sensor_data.empty:
        print(f"No data found for sensor: {sensor_name}")
        return

    # Extract the relevant columns for plotting
    frame_index = sensor_data['FrameIndex']
    theta = sensor_data['Theta']
    phi = sensor_data['Phi']

    # Convert decimal separator from comma to period
    theta = theta.str.replace(',', '.')
    phi = phi.str.replace(',', '.')

    # Convert the columns to numeric values
    theta = pd.to_numeric(theta, errors='coerce')
    phi = pd.to_numeric(phi, errors='coerce')

    # Print unique values for Theta and Phi
    print(f"Sensor: {sensor_name}")
    print(f"Unique Theta values: {theta.dropna().unique()}")
    print(f"Unique Phi values: {phi.dropna().unique()}")

    # Create the plot
    plt.figure(figsize=(10, 6))
    plt.plot(frame_index, theta, label='Theta')
    plt.plot(frame_index, phi, label='Phi')
    plt.xlabel('FrameIndex')
    plt.ylabel('Angle')
    plt.title(f"Sensor: {sensor_name}")
    plt.legend()
    plt.grid(True)

    # Save the plot to the PDF
    pdf.savefig()
    plt.close()

# Create a PDF file with the name of the current date and time
timestamp = datetime.now().strftime("%Y%m%d%H%M%S")
pdf_filename = f"sensor_plots_{timestamp}.pdf"

# Generate plots and save them in the PDF
sensor_names = df['SensorName'].unique()
with PdfPages(pdf_filename) as pdf:
    for sensor_name in sensor_names:
        plot_sensor_data(sensor_name, pdf)

print(f"Plots saved in {pdf_filename}")
