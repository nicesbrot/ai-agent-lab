# 🤖 AI Agent Lab

**AI Agent Lab** is a three-stage learning project that demonstrates how to evolve  
from a simple OpenAI-powered backend to a fully hosted and fine-tuned AI agent system.

Each stage builds upon the previous one — from basic API integration to hosting your own LLM  
and finally creating a specialized model adapted to your domain.

---

## 🧩 Project Overview

| Stage | Title | Description |
|-------|--------|-------------|
| **Stage 0 – OpenAI Proxy** | 🧠 Getting Started | A minimal backend (written in C#) that wraps the OpenAI API behind a clean `/api/chat` endpoint. This serves as the foundation for later stages. |
| **Stage 1 – Hosted Mistral** | ⚙️ Self-Hosted Model | Runs an open-weight model such as Mistral 7B using an inference engine (like vLLM). This stage removes external API dependency. |
| **Stage 2 – Fine-Tuned Agent** | 🎯 Specialized Intelligence | Uses fine-tuning or LoRA adaptation on custom datasets (e.g., sales or support conversations) to create domain-specific agents. |

---

## 🚀 Goals

- Understand how LLM-based systems are structured  
- Build an independent API layer compatible with any model  
- Learn the evolution from “simple API proxy” → “self-hosted model” → “fine-tuned agent”  
- Provide reproducible, open examples for educational purposes  

---

## 📁 Repository Structure

```text
ai-agent-lab/
├─ stage-0-openai-proxy/         # Stage 0 – C# API proxy using OpenAI
├─ stage-1-hosted-mistral/       # Stage 1 – Self-hosted open LLM (e.g. Mistral 7B)
├─ stage-2-finetuned-agent/      # Stage 2 – Fine-tuning or LoRA specialization
├─ LICENSE
└─ README.md                     # This overview file
