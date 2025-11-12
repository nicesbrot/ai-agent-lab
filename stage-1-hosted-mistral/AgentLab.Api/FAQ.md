❓ What is Stage 1 – Hosted Model about?

Answer:
Stage 1 demonstrates how to replace the external OpenAI API with your own locally or cloud-hosted LLM.
Instead of sending requests to api.openai.com, your backend communicates with a self-hosted model such as Mistral 7B through an inference engine like Ollama, vLLM, or TGI.
This removes external dependencies and makes your system fully controllable.

❓ What is the difference between Ollama, vLLM, and TGI?
Feature	Ollama 🧑‍💻	vLLM ⚙️	TGI 🧠
Target users	Developers who want easy local testing	Research / Production servers	Enterprise / Hugging Face ecosystem
Installation	1-Click (Windows / macOS / Linux)	Python + GPU setup / Docker	Docker image from Hugging Face
API	OpenAI-compatible	OpenAI-compatible	OpenAI-like REST
GPU required	No (CPU possible)	Yes	Yes
Best for	Local prototyping	High-performance hosting	Scalable production services
❓ Which engine should I use for Stage 1?

Answer:
Start with Ollama — it runs locally, even on CPU-only machines.
Later you can switch to vLLM or TGI on a GPU server for better performance or production-grade setups.

❓ How can I implement all three engines in one backend?

Answer:
Create a common interface such as ILlmClient in your C# backend.
Then add one implementation per engine:

OllamaLlmClient

VllmLlmClient

TgiLlmClient

Select which one to use through an environment variable:

LLM_PROVIDER=ollama
LLM_API_BASEURL=http://localhost:11434


Your endpoint /api/chat stays identical — only the implementation behind the interface changes.

❓ Can I test Stage 1 without a GPU or server?

Answer:
Yes ✅ — Ollama works perfectly on CPU, just slower.
You can run the full Stage 1 locally and test your entire pipeline.
For vLLM or TGI, rent a temporary GPU instance (RunPod, Vast, Lambda) for a few USD/hour.

❓ How do I install Ollama?

Answer:

Go to https://ollama.com/download

Install for your OS

Run:

ollama pull mistral
ollama run mistral


Ollama starts a REST API at http://localhost:11434
.

❓ What does the data flow look like in Stage 1?
--------------                 -----------------                 -----------------
|  UserPrompt |  ───────────▶  |  REST API (C#) |  ───────────▶  |  Ollama/vLLM/TGI |
--------------                 -----------------                 -----------------
                                         ▲
                                         │
                                Project responsibility


The project owns the REST API and request logic.
The inference engine handles model execution.
OpenAI is no longer involved.

❓ How does Stage 1 differ from Stage 0?
Stage	Description
Stage 0	Calls the OpenAI API directly.
Stage 1	Hosts the model yourself (Ollama / vLLM / TGI).

Benefits: independence, privacy, and lower cost.

❓ What are the main benefits of Stage 1?

✅ No external API costs or rate limits
⚙️ Full control over model choice and hosting
🧠 Foundation for future fine-tuning (Stage 2)
🔒 Data stays private within your infrastructure

❓ What are the available options for customizing responses in Ollama API requests?

You can pass generation options in the request body under "options": { … }.

Option	Type	Typical Range	Description
temperature	float	0.0 – 1.5	Controls randomness. Low = deterministic (precise); high = creative.
top_p	float	0.5 – 1.0	Limits sampling to the top p cumulative probability mass (nucleus sampling).
num_predict	int	32 – 512	Maximum number of tokens the model may generate. Reducing it prevents long responses/timeouts.
repeat_penalty	float	1.0 – 2.0	Penalizes repetition of identical phrases; higher = fewer repeats.
seed	int	e.g. 42	Fixes the random seed for reproducible output.
stop	string / array	e.g. ["User:", "System:"]	Defines stop sequences to end generation early.
presence_penalty	float	0 – 2	Discourages reusing topics already mentioned.
frequency_penalty	float	0 – 2	Penalizes frequently used tokens.
mirostat	int	0, 1, 2	Enables dynamic temperature control for stable entropy (advanced).

Example usage:

{
  "model": "mistral",
  "messages": [
    { "role": "user", "content": "Explain photosynthesis briefly." }
  ],
  "stream": false,
  "options": {
    "temperature": 0.7,
    "top_p": 0.9,
    "num_predict": 128,
    "repeat_penalty": 1.1
  }
}