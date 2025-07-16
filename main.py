from fastapi import FastAPI, UploadFile, File
from fastapi.middleware.cors import CORSMiddleware
from langchain_openai import ChatOpenAI
from langchain_community.document_loaders import PyPDFLoader, TextLoader, UnstructuredWordDocumentLoader
import os
import shutil

app = FastAPI()

# CORS config (allows local frontend to talk to backend)
app.add_middleware(
    CORSMiddleware,
    allow_origins=["*"],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)

UPLOAD_DIR = "uploaded_files"
os.makedirs(UPLOAD_DIR, exist_ok=True)

@app.post("/upload")
async def upload_file(file: UploadFile = File(...)):
    try:
        file_path = os.path.join(UPLOAD_DIR, file.filename)
        with open(file_path, "wb") as buffer:
            shutil.copyfileobj(file.file, buffer)

        # Determine file extension
        ext = file.filename.lower().split(".")[-1]

        if ext == "pdf":
            loader = PyPDFLoader(file_path)
        elif ext == "docx":
            loader = UnstructuredWordDocumentLoader(file_path)
        elif ext == "txt":
            loader = TextLoader(file_path)
        else:
            return {"error": f"Unsupported file type: .{ext}"}

        documents = loader.load()
        full_text = "\n\n".join([doc.page_content for doc in documents])

        return {
            "message": "Parsed successfully",
            "filename": file.filename,
            "content": full_text
        }

    except Exception as e:
        return {"error": str(e)}
from fastapi.responses import JSONResponse

@app.get("/")
def root():
    return JSONResponse(content={"status": "CompliGenie backend is live."})
