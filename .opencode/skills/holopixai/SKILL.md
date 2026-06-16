---
name: holopixai
description: Use HolopixAI to generate or query game art images through the user's HolopixAI Access Key and Secret Key. Trigger when the user asks to use HolopixAI, Holopix AI, or HoloPix for image generation, character art, model queries, concept art, turnarounds, sprites, or Dragonfall assets.
---

# HolopixAI

Use this skill when the user wants to call HolopixAI APIs.

## Security

- Never print or store the user's full Access Key or Secret Key.
- Read credentials from `HOLOPIXAI_ACCESS_KEY` and `HOLOPIXAI_SECRET_KEY`.
- Do not put credentials in project files, prompts, logs, or generated artifacts.

## Official API Basics

- Base URL: `https://api.holopix.cn`
- Method: `POST`
- Content type: `application/json`
- Body format: `{"data": {...}}`
- Required headers:
  - `X-Access-Key`
  - `X-Signature`
  - `X-Timestamp`
  - `X-Nonce`

Signing:

```text
compact_json = json.dumps(body, ensure_ascii=False, separators=(",", ":"))
string_to_sign = X-Timestamp + "=" + compact_json
X-Signature = HMAC-SHA256(secret_key_utf8, string_to_sign_utf8).hexdigest()
```

`X-Timestamp` is milliseconds. `X-Nonce` must be unique; use a UUID.

## Script

Use:

```powershell
python .opencode/skills/holopixai/scripts/holopixai_generate.py <command>
```

Commands:

```powershell
# Query account rights / remaining points
python .opencode/skills/holopixai/scripts/holopixai_generate.py rights

# Query available model list
python .opencode/skills/holopixai/scripts/holopixai_generate.py models

# Query model detail
python .opencode/skills/holopixai/scripts/holopixai_generate.py model-detail --id 15D34DF2CZ

# Query image generation progress
python .opencode/skills/holopixai/scripts/holopixai_generate.py query-progress --client-id "<clientId>"

# Submit one-click text-to-image. This consumes account points.
python .opencode/skills/holopixai/scripts/holopixai_generate.py generate --mode t2i --prompt "..." --model-id 82751128 --aspect-ratios 1:1 --out C:\Users\Administrator\Desktop\Dragonfall\AIResult\holopixai-output.png --wait

# Submit one-click image-to-image. source-image must be a public URL or accepted base64.
python .opencode/skills/holopixai/scripts/holopixai_generate.py generate --mode i2i --source-image "https://..." --image-mode colorSketch --image-color true --image-weight 0.8 --prompt "..." --model-id 82751128 --out C:\Users\Administrator\Desktop\Dragonfall\AIResult\holopixai-i2i.png --wait

# Generic POST endpoint call
python .opencode/skills/holopixai/scripts/holopixai_generate.py call --path "/v1/model/apiList" --data-json "{}"
```

Known endpoints from the PDFs:

- `/v1/apiUser/rights`
- `/v1/model/apiList`
- `/v1/model/apiDetail`
- `/v1/images/generations/t2i`
- `/v1/images/generations/i2i`
- `/v1/images/generations/queryProgress`
- `/v1/images/generations/imagetoPrompt`
- `/v1/images/generations/poseprocess`
- `/v1/images/generations/remBg`

One-click draft generation fields confirmed from the PDFs:

- `modelDetailList`: list of `{modelId, strength}`; style model list max is 5.
- `prompt`: required; max 2000 chars.
- `negativePrompt`: optional; max 2000 chars; not supported for Holopix-V1.
- `seed`: required; `-1` means random.
- `aspectRatios`: text-to-image ratio, such as `16:9`, `9:16`, `1:1`, `4:3`, `3:4`, `3:2`, `2:3`, `21:9`.
- `imageGuidanceWeights`: optional/required for Holopix-V1; range from docs is generally `3-6`.
- `faceDetail`, `hdFix`, `hdScale`, `simpleBackground`, `enablePerturb`, `perturb`, `characterPose`, `batchSize`.
- `imageReference`, `referenceMode`, `referenceWeight` for character/style reference.
- For `i2i`: `sourceImage`, `imageMode` (`linerSketch` or `colorSketch`), `imageColor`, `imageWeight`.

Task creation returns `data.clientId`. Poll `/v1/images/generations/queryProgress` with `{"clientIds": ["..."]}` until `status` is `succeed`; image URLs are in `data.clientList[0].imgUrls`.

Do not run generation without user confirmation if the user has not explicitly asked to consume HolopixAI points.

## Dragonfall Usage

For Dragonfall assets, combine this skill with `dragonfall-art-gen` prompt guidance and save outputs under:

```text
C:\Users\Administrator\Desktop\Dragonfall\AIResult
```

Always report:

- output image path if an image was downloaded
- saved response JSON path if one was written
- prompt or payload used
- returned `clientId` for async tasks
