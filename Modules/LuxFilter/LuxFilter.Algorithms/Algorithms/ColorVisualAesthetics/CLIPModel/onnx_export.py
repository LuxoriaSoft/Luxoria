import torch
import numpy as np
from transformers import CLIPProcessor, CLIPModel
import os
import sys

# Config
model_id = "openai/clip-vit-base-patch32"
output_dir = (sys.argv[1] if len(sys.argv) > 1 else "./")

positive_prompt = "an outstanding picture"
negative_prompt = "a horrible picture"

# Setup
os.makedirs(output_dir, exist_ok=True)
device = "cuda" if torch.cuda.is_available() else "cpu"

print("Loading CLIP model...")
model = CLIPModel.from_pretrained(model_id).to(device)
processor = CLIPProcessor.from_pretrained(model_id)

# Export text prompts to numpy
def export_text_embedding(prompt, filename):
    inputs = processor(text=[prompt], return_tensors="pt", padding=True).to(device)
    with torch.no_grad():
        embedding = model.get_text_features(**inputs).cpu().numpy().squeeze()
    np.save(os.path.join(output_dir, filename), embedding)
    # Save as txt
    np.savetxt(os.path.join(output_dir, filename.replace('.npy', '.txt')), embedding)
    print(f"Saved {filename} & {filename.replace('.npy', '.txt')} with shape {embedding.shape}")

export_text_embedding(positive_prompt, "positive.npy")
export_text_embedding(negative_prompt, "negative.npy")

class ClipImageEncoder(torch.nn.Module):
    def __init__(self, clip_model):
        super().__init__()
        self.clip_model = clip_model

    def forward(self, pixel_values):
        return self.clip_model.get_image_features(pixel_values)

print("Exporting image encoder to ONNX...")

image_encoder = ClipImageEncoder(model).to(device)
dummy_input = torch.randn(1, 3, 224, 224).to(device)

torch.onnx.export(
    image_encoder,
    (dummy_input,),
    os.path.join(output_dir, "clip_image_encoder.onnx"),
    input_names=["pixel_values"],
    output_names=["image_embeds"],
    dynamic_axes={"pixel_values": {0: "batch_size"}, "image_embeds": {0: "batch_size"}},
    opset_version=14
)

print("ONNX exported !")
print("All files saved.", os.path.abspath(output_dir))