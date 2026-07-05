---
name: save-note
description: Analyzes the recent conversation to extract key learnings, writes a beautiful markdown note, and deploys it automatically to the user's S3 notes website.
---

# Save Learning Note

When the user asks to save a learning note, or save what you just discussed as a note, execute this workflow.

## 1. Analyze and Draft
Review the conversation history (use your transcript files if needed) to extract:
1. **The Core Topic**: What was the main issue solved or concept learned?
2. **The "Why"**: The underlying reasoning, architecture choices, or debugging journey.
3. **The Solution**: Code snippets, commands, or diagrams showing how it works.

Draft a complete markdown document. It MUST include:
- A clear, descriptive H1 title (e.g., `# Fixing Azure AKS Persistent Volumes`)
- Metadata block at the top containing `**Date:** YYYY-MM-DD` and `**Tags:** tag1, tag2`
- Beautiful formatting using GitHub Flavored Markdown (GFM)
- Callout blocks `> [!NOTE]` or `> [!TIP]` for important gotchas.
- If architectural, include a `mermaid` diagram block!

## 2. Save the File
Save the drafted markdown file locally in `/home/prashant-kumar/Desktop/LeaveManagement/notes-app/content/` with a clean, lowercase hyphenated name (e.g., `azure-aks-pv-fix.md`).

## 3. Update the Index
Read the `/home/prashant-kumar/Desktop/LeaveManagement/notes-app/notes-index.json` file.
Add a new entry to the `notes` array for the newly created file. The object format must be:
```json
{
  "id": "unique-slug",
  "title": "Your H1 Title Without the #",
  "filename": "your-filename.md",
  "category": "learning",
  "tags": ["tag1", "tag2"],
  "date": "YYYY-MM-DD",
  "description": "A short 1-2 sentence summary of what this note is about."
}
```

## 4. Deploy to AWS
Run the following commands to deploy the new note and updated index to the live website:
```bash
aws s3 cp /home/prashant-kumar/Desktop/LeaveManagement/notes-app/content/your-filename.md s3://prashant-learning-notes-content/notes/
aws s3 cp /home/prashant-kumar/Desktop/LeaveManagement/notes-app/notes-index.json s3://prashant-learning-notes-content/
```

## 5. Report Success
Provide the user with the direct link to the live note by telling them to check their website and appending `#your-filename.md` to the URL.
