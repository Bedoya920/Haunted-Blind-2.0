# 🎯 Descargar Modelos Whisper para Sentis

## ✅ **Enlaces de Descarga Alternativos:**

### **1. HuggingFace (Principal)**
- **Whisper Tiny**: https://huggingface.co/openai/whisper-tiny
- **Whisper Base**: https://huggingface.co/openai/whisper-base
- **Whisper Small**: https://huggingface.co/openai/whisper-small

### **2. Enlaces Directos (Si HuggingFace falla)**
- **Whisper Tiny**: https://huggingface.co/openai/whisper-tiny/resolve/main/pytorch_model.bin
- **Whisper Base**: https://huggingface.co/openai/whisper-base/resolve/main/pytorch_model.bin

### **3. GitHub (Modelos ONNX)**
- **Whisper ONNX**: https://github.com/onnx/models/tree/main/vision/body_analysis/whisper
- **Modelos pre-convertidos**: https://github.com/onnx/models/tree/main/vision/body_analysis/whisper

## 🚀 **Métodos de Descarga:**

### **Método 1: HuggingFace Hub (Recomendado)**
```bash
# Instalar huggingface-hub
pip install huggingface-hub

# Descargar modelo
huggingface-cli download openai/whisper-tiny --local-dir ./whisper-tiny
```

### **Método 2: Git LFS**
```bash
# Clonar repositorio
git clone https://huggingface.co/openai/whisper-tiny
cd whisper-tiny
git lfs pull
```

### **Método 3: Descarga Manual**
1. Ve a https://huggingface.co/openai/whisper-tiny
2. Click en **Files and versions**
3. Descarga los archivos:
   - `pytorch_model.bin` (modelo principal)
   - `config.json` (configuración)
   - `tokenizer.json` (tokenizador)

## 🔧 **Convertir a Formato Sentis:**

### **Opción 1: Unity Sentis Tools**
1. Abre Unity con Sentis instalado
2. Ve a **Window > Sentis > Model Import**
3. Selecciona el archivo `.pt` o `.onnx`
4. Convierte a `.sentis`

### **Opción 2: Python Script**
```python
import torch
from unity.sentis import ModelLoader

# Cargar modelo PyTorch
model = torch.load('whisper-tiny.pt')

# Convertir a Sentis
sentis_model = ModelLoader.convert_from_pytorch(model)
sentis_model.save('whisper-tiny.sentis')
```

### **Opción 3: ONNX Intermedio**
```python
import torch
import onnx

# PyTorch -> ONNX
torch.onnx.export(model, dummy_input, "whisper-tiny.onnx")

# ONNX -> Sentis (usar Unity Sentis tools)
```

## 📁 **Estructura de Archivos:**

Después de la descarga, deberías tener:
```
Assets/AI/Models/Whisper/
├── whisper-tiny-es.sentis          # Modelo principal
├── config.json                     # Configuración
├── tokenizer.json                  # Tokenizador
└── README.md                       # Documentación
```

## 🎯 **Modelos Recomendados:**

### **Para Desarrollo/Testing:**
- **Whisper Tiny** (~40MB)
  - Más rápido
  - Menor calidad
  - Ideal para pruebas

### **Para Producción:**
- **Whisper Base** (~150MB)
  - Balanceado
  - Buena calidad
  - Recomendado para juegos

### **Para Máxima Calidad:**
- **Whisper Small** (~500MB)
  - Alta calidad
  - Más lento
  - Para aplicaciones críticas

## 🆘 **Solución de Problemas:**

### **Error 404 en HuggingFace:**
1. Verifica que la URL sea correcta
2. Intenta con enlaces alternativos
3. Usa Git LFS para clonar el repositorio

### **Error de Descarga:**
1. Verifica tu conexión a internet
2. Intenta con VPN si estás en ciertos países
3. Usa herramientas como `wget` o `curl`

### **Error de Conversión:**
1. Asegúrate de tener Sentis instalado
2. Verifica que el modelo sea compatible
3. Usa ONNX como formato intermedio

## 🔗 **Enlaces Útiles:**

- **HuggingFace Whisper**: https://huggingface.co/openai/whisper-tiny
- **Unity Sentis Docs**: https://docs.unity3d.com/Packages/com.unity.sentis@latest/
- **ONNX Models**: https://github.com/onnx/models
- **Whisper Paper**: https://arxiv.org/abs/2212.04356

## 📝 **Notas Importantes:**

1. **Licencia**: Whisper es open source (MIT License)
2. **Idiomas**: Soporta múltiples idiomas, incluyendo español
3. **Tamaño**: Los modelos van de 40MB a 3GB
4. **Rendimiento**: Más grande = mejor calidad pero más lento

---

## 🎯 **Recomendación:**

Para empezar, descarga **Whisper Tiny** desde:
https://huggingface.co/openai/whisper-tiny

Si ese enlace no funciona, intenta con:
https://huggingface.co/openai/whisper-tiny/resolve/main/pytorch_model.bin

**¿Necesitas ayuda con algún método específico de descarga?**
