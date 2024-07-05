from flask import Flask, request, jsonify, make_response
from flask_cors import CORS, cross_origin
from sklearn.preprocessing import OneHotEncoder
import pandas as pd
from joblib import load
from sklearn.preprocessing import StandardScaler
app = Flask(__name__)
cors = CORS(app)
app.config['CORS_HEADERS'] = 'Content-Type'

# Load the trained model
with open('C:/Users/sarra/Desktop/ACPBackPrediction/best_rf_model.pkl', 'rb') as f:
    model = load(f)

@app.route('/predict', methods=['POST'])
@cross_origin()
def predict():
    # Check if file is present in the request
    if 'file' not in request.files:
        return jsonify({'error': 'No file part'})

    file = request.files['file']

    # Check if the file is an Excel file
    if file.filename == '':
        return jsonify({'error': 'No file selected'})
    if not file.filename.endswith('.xlsx'):
        return jsonify({'error': 'Only Excel files (.xlsx) are supported'})

    # Load the Excel file into a DataFrame
    try:
        df_origin = pd.read_excel(file)
        df=df_origin[['State','Response', 'Coverage',
        'Education', 'Effective To Date', 'EmploymentStatus', 'Gender',
        'Income', 'Location Code', 'Marital Status', 'Monthly Premium Auto',
        'Months Since Last Claim', 'Months Since Policy Inception',
        'Number of Open Complaints', 'Number of Policies', 'Policy Type',
        'Policy', 'Renew Offer Type', 'Sales Channel', 'Total Claim Amount',
        'Vehicle Class', 'Vehicle Size']]
        categorical_columns = ['State', 'Response', 'Coverage', 'Education',
        'EmploymentStatus', 'Gender',
        'Location Code', 'Marital Status',
        'Policy Type',
        'Policy', 'Renew Offer Type', 'Sales Channel',
        'Vehicle Class', 'Vehicle Size']

        # Extract categorical features
        categorical_data = df[categorical_columns]

        # Initialize OneHotEncoder
        encoder = OneHotEncoder(sparse_output=False, drop='first')

        # Fit and transform the categorical features
        encoded_features = encoder.fit_transform(categorical_data)

        # Create a DataFrame from the encoded features with column names
        encoded_df = pd.DataFrame(encoded_features, columns=encoder.get_feature_names_out(categorical_columns))

        # Concatenate the encoded DataFrame with the original DataFrame
        data_encoded = pd.concat([df.drop(columns=categorical_columns), encoded_df], axis=1)

        from datetime import datetime


        data_encoded['Effective To Date'] = pd.to_datetime(data_encoded['Effective To Date'], format='%m/%d/%y')

        # Calculate the difference between today's date and 'Effective To Date'
        today = pd.to_datetime(datetime.today().date())
        data_encoded['Effective To Date'] = (today - data_encoded['Effective To Date']).dt.days
        numerical_columns =["Effective To Date",
        "Income",
        "Monthly Premium Auto",
        "Months Since Last Claim",
        "Months Since Policy Inception",
        "Number of Open Complaints",
        "Number of Policies",
        "Total Claim Amount"]  
        # Initialize StandardScaler
        scaler = StandardScaler()

        # Fit and transform the numerical columns in the training set
        data_encoded[numerical_columns] = scaler.fit_transform(data_encoded[numerical_columns])
    except Exception as e:
        return jsonify({'error': f'Error reading Excel file: {str(e)}'})

    # Predict using the model
    try:
        predictions = model.predict(data_encoded)  
        df_origin['Predicted Customer Lifetime Value'] = predictions
    except Exception as e:
        print(e)
        return make_response(jsonify({'error': f'Error predicting: {str(e)}'}), 400)

    # Convert DataFrame to JSON and return
    response = make_response(df_origin.to_json(orient='records'), 200)
    response.headers['Content-Type'] = 'application/json'
    return response

if __name__ == '__main__':
    app.run(debug=True)
