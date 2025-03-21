# AnalytiQ - AI-Powered Feedback Analytics

## 🚀 Overview
AnalytiQ is a powerful AI-driven feedback analytics platform designed to process customer feedback, analyze sentiments, and generate actionable insights using **Azure AI Services, Power BI, and .NET technologies**. The project is structured into four main components:

1. **UI** - A modern WPF-based desktop app.
2. **API** - ASP.NET Core Web API for handling file uploads and authentication.
3. **ProcessUpload** - Azure Functions to process uploaded files.
4. **Power BI Embed** - Secure embedding of Power BI dashboards for data visualization.

## 📂 Repository Structure
```
AnalytiQ/
├── UI/                  # WPF Desktop App
├── API/                 # ASP.NET Core Web API
├── ProcessUpload/       # Azure Functions for file processing
└── PowerBIEmbed/        # Power BI report embedding
```

## 🛠️ Technologies Used
- **Frontend:** WPF (.NET 7), WebView2
- **Backend:** ASP.NET Core Web API
- **Cloud Services:** Azure Functions, Blob Storage, Event Grid, SQL Database
- **AI Services:** Azure OpenAI (GPT-4), Cognitive Services (Document Intelligence)
- **Data Visualization:** Power BI Embedded

---

# 🖥️ UI - AnalytiQ WPF App
The **AnalytiQ UI** is built in **WPF (VB.NET)** and provides a seamless user experience for managing feedback analysis.

### 🔹 Features:
- **Modern UI with WebView2** for Power BI embedding.
- **Dynamic navigation** between Dashboard, Analytics, and Settings.
- **Tenant-based authentication** (each company sees its own data).
- **File upload integration** with Azure Blob Storage.

### 📜 Main Components:
- **MainWindow.xaml & MainWindow.vb**: UI structure and logic.
- **WebView2 Integration**: Displays Power BI reports securely.
- **API Calls**: Fetches data from AnalytiQ's backend.

### ⚙️ How It Works:
1. **User logs in** → Token-based authentication.
2. **Uploads feedback data** (CSV, PDF, DOCX, etc.).
3. **AI analysis is triggered**, and results are stored in Azure SQL.
4. **Power BI visualizes** AI-driven insights.

---

# 🌐 API - File Upload & Authentication
The **AnalytiQ API** is built using **ASP.NET Core Web API** and is responsible for handling file uploads and authentication.

### 📌 Key Endpoints:
| Method | Endpoint                  | Description |
|--------|---------------------------|-------------|
| `POST` | `/api/FileUpload/upload`  | Uploads a file to Azure Blob Storage. |
| `GET`  | `/api/auth/login`         | Authenticates users. |
| `POST` | `/api/blobevents/handle-event` | Handles Azure Blob Storage event notifications. |

### ⚙️ How It Works:
1. User uploads a file → API validates and sends to Azure Blob Storage.
2. **Event Grid detects new upload** → Triggers **ProcessUpload Function**.
3. AI analysis is performed → Results saved in **Azure SQL**.

---

# 🏗️ ProcessUpload - AI-Powered File Processing
The **ProcessUpload Function** is an **Azure Function** that detects **new file uploads**, extracts text, and analyzes feedback.

### 🚀 Steps in Processing:
1. **Trigger:** Event Grid detects a new file in Blob Storage.
2. **Extract Text:** Document AI extracts content from PDFs, DOCX, etc.
3. **Analyze Sentiment:** Azure OpenAI (GPT-4) processes feedback.
4. **Save Results:** Insights are stored in **Azure SQL Database**.
5. **Trigger Power BI Update:** Dashboard refreshes with new insights.

### 🛠️ Supported File Types:
- **PDF** (Extracted using Azure Document Intelligence)
- **DOCX** (Extracted using Open XML SDK)
- **CSV** (Processed via CsvHelper)
- **TXT** (Raw text processing)
- **XLSX** (Handled with ExcelDataReader)

---

# 📊 Power BI Embedding
The **Power BI Embed Function** securely generates **embed tokens** for displaying reports inside the WPF app.

### 🔹 Features:
- **Secure access control** using Azure AD.
- **Tenant-based filtering** (each company sees only its own data).
- **Role-Level Security (RLS)** for enhanced data protection.

### 🔑 API Endpoint:
| Method | Endpoint                  | Description |
|--------|---------------------------|-------------|
| `GET`  | `/api/GetEmbedToken` | Generates an embed token for a Power BI report. |

### ⚙️ How It Works:
1. **User requests a report** → App calls `GetEmbedToken`.
2. **Power BI API generates** a secure token.
3. **Report is embedded** inside the WPF app.

---

# 🚀 Getting Started
## 🔧 Prerequisites
- **.NET 7 SDK** (for running API & UI)
- **Azure Subscription** (for cloud services)
- **Power BI Pro Account** (for embedding reports)

## 🛠️ Installation & Setup
### 1️⃣ Clone the Repository
```sh
git clone https://github.com/your-repo/AnalytiQ.git
cd AnalytiQ
```

### 2️⃣ Set Up the API
```sh
cd API
# Install dependencies
dotnet restore
# Run the API
dotnet run
```

### 3️⃣ Run the WPF UI
Open `UI/AnalytiQ.sln` in **Visual Studio** and hit **Run**.

### 4️⃣ Deploy Azure Functions
```sh
cd ProcessUpload
func azure functionapp publish analytiq-functions
```

### 5️⃣ Set Up Power BI Embedding
- Register an **Azure AD App**.
- Enable **Power BI API access**.
- Configure `appsettings.json` with **Power BI credentials**.

---

# 🌍 Deployment to Azure
### ☁️ Azure Services Used:
- **Azure Blob Storage** - Stores uploaded feedback files.
- **Azure SQL Database** - Stores AI-analyzed insights.
- **Azure Functions** - Automates file processing & analysis.
- **Power BI Embedded** - Secure, interactive dashboards.
- **Event Grid** - Detects file uploads and triggers analysis.

🔥 Deploy to Azure
```sh
az webapp up --name analytiq-api --resource-group AnalytiQ-Resources
az storage account create --name analytiqstorage290 --resource-group AnalytiQ-Resources --sku Standard_LRS
az sql db create --resource-group AnalytiQ-Resources --server analytiq-sql --name AnalytiQDB --service-objective S0
```

---

🔥 Future Improvements
- 🌟 **Real-time AI insights** with streaming analytics.
- 📊 **More detailed Power BI reports**.
- 🤖 **Custom AI models** for advanced feedback classification.
- 🏆 **Scalability improvements** for enterprise-level users.

---

 💡 Contributing
Got an idea? Found a bug? **Pull requests are welcome!** 🎉

- Fork the repo
- Create a feature branch (`git checkout -b feature-xyz`)
- Commit changes (`git commit -m 'Add feature XYZ'`)
- Push branch (`git push origin feature-xyz`)
- Submit a PR! 🚀

---

📜 License
MIT License © 2025 Victor Bash

---

 🤝 Credits
Developed by **Victor Bash** for **Microsoft Data + AI Kenya Hackathon**.

🚀 Happy coding!

